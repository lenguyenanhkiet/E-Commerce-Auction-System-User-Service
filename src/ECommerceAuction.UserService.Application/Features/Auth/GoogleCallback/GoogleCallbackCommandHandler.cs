using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Entities.Users;

namespace ECommerceAuction.UserService.Application.Features.Auth.GoogleCallback;

/// <summary>
/// Handles the Google OAuth2 callback, verifies Google identity, links or creates a local user, and creates a frontend login code.
/// </summary>
public sealed class GoogleCallbackCommandHandler
    : ICommandHandler<GoogleCallbackCommand, GoogleCallbackResponse>
{
    private const string FailureCode = "google_login_failed";
    private const string EmailClaimConflictCode = "email_claim_conflict";

    private readonly IOAuthStateService _oauthStateService;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly IUserOAuthRepository _userOAuthRepository;
    private readonly ILoginCodeService _loginCodeService;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleCallbackCommandHandler(
        IOAuthStateService oauthStateService,
        IGoogleOAuthService googleOAuthService,
        IUserOAuthRepository userOAuthRepository,
        ILoginCodeService loginCodeService,
        IUnitOfWork unitOfWork)
    {
        _oauthStateService = oauthStateService;
        _googleOAuthService = googleOAuthService;
        _userOAuthRepository = userOAuthRepository;
        _loginCodeService = loginCodeService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes Google callback data and redirects the browser to either the frontend success route or login error route.
    /// </summary>
    public async Task<GoogleCallbackResponse> Handle(
        GoogleCallbackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(request.Error) ||
                string.IsNullOrWhiteSpace(request.Code) ||
                string.IsNullOrWhiteSpace(request.State))
            {
                return Failure();
            }

            var stateIsValid = await _oauthStateService.ValidateAndConsumeAsync(
                request.State,
                cancellationToken);

            if (!stateIsValid)
            {
                return Failure();
            }

            var token = await _googleOAuthService.ExchangeCodeAsync(
                request.Code,
                cancellationToken);

            var googleUser = await _googleOAuthService.ValidateIdTokenAsync(
                token.IdToken,
                cancellationToken);

            if (!googleUser.EmailVerified)
            {
                return Failure();
            }

            var externalLogin = await _userOAuthRepository.GetExternalLoginAsync(
                ExternalLoginProviders.Google,
                googleUser.ProviderUserId,
                cancellationToken);

            User user;
            string auditAction;

            if (externalLogin is not null)
            {
                if (!externalLogin.IsActive())
                {
                    return Failure();
                }

                user = externalLogin.User
                    ?? await _userOAuthRepository.GetUserByIdAsync(externalLogin.UserId, cancellationToken)
                    ?? throw new InvalidOperationException("Linked local user was not found.");

                if (user.IsBlockedFromAuthentication())
                {
                    return Failure();
                }

                ApplyVerifiedGoogleEmail(user);
                externalLogin.SyncProviderSnapshot(googleUser.Email, googleUser.FullName);

                if (!user.CanLogin())
                {
                    return Failure();
                }

                externalLogin.MarkLoggedIn();
                user.MarkLoggedIn();
                auditAction = UserAuditActions.GoogleLogin;
            }
            else
            {
                var existingUser = await _userOAuthRepository.GetUserByEmailAsync(
                    googleUser.Email,
                    cancellationToken);

                if (existingUser is null)
                {
                    user = User.CreateFromOAuth2(
                        googleUser.Email,
                        googleUser.FullName ?? googleUser.Email);

                    await _userOAuthRepository.AddUserAsync(user, cancellationToken);
                    auditAction = UserAuditActions.GoogleRegister;
                }
                else
                {
                    user = existingUser;
                    auditAction = UserAuditActions.GoogleLink;
                }

                var claimResult = ApplyGoogleClaimForExistingUser(user);

                if (claimResult == GoogleClaimResult.Conflict)
                {
                    await _userOAuthRepository.AddAuditLogAsync(
                        UserAuditLog.Create(
                            actorUserId: null,
                            targetUserId: user.Id,
                            action: UserAuditActions.GoogleEmailClaimConflict,
                            entityType: nameof(User),
                            entityId: user.Id.ToString()),
                        cancellationToken);

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return Failure(EmailClaimConflictCode);
                }

                if (claimResult == GoogleClaimResult.ClaimedUnverifiedAccount)
                {
                    auditAction = UserAuditActions.GoogleClaimUnverifiedAccount;
                }

                if (!user.CanLogin())
                {
                    return Failure();
                }

                var newExternalLogin = UserExternalLogin.CreateGoogle(
                    user.Id,
                    googleUser.ProviderUserId,
                    googleUser.Email,
                    googleUser.FullName);

                await _userOAuthRepository.AddExternalLoginAsync(newExternalLogin, cancellationToken);
                user.MarkLoggedIn();
            }

            await _userOAuthRepository.EnsureDefaultBuyerRoleAsync(user.Id, cancellationToken);

            await _userOAuthRepository.AddAuditLogAsync(
                UserAuditLog.Create(
                    actorUserId: user.Id,
                    targetUserId: user.Id,
                    action: auditAction,
                    entityType: nameof(User),
                    entityId: user.Id.ToString()),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var loginCode = await _loginCodeService.CreateAsync(user.Id, cancellationToken);
            var successUrl = _googleOAuthService.BuildFrontendSuccessUrl(loginCode);

            return new GoogleCallbackResponse(successUrl);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return Failure();
        }
    }

    /// <summary>
    /// Marks an existing linked user email as verified by Google and clears unsafe passwords only when no phone verification exists.
    /// </summary>
    private static void ApplyVerifiedGoogleEmail(User user)
    {
        if (user.IsEmailConfirmed)
        {
            return;
        }

        // ECA-6 OAuth2 Google: a previously linked Google account can safely verify the user's email.
        var shouldClearUnsafePassword = !user.IsPhoneConfirmed;

        user.VerifyEmailByGoogle();

        if (shouldClearUnsafePassword)
        {
            user.ClearPasswordAfterUnverifiedGoogleClaim();
        }
    }

    /// <summary>
    /// Applies account-claiming rules when Google returns an email that already exists in the local user table.
    /// </summary>
    private static GoogleClaimResult ApplyGoogleClaimForExistingUser(User user)
    {
        if (user.IsBlockedFromAuthentication())
        {
            return GoogleClaimResult.Rejected;
        }

        if (user.IsEmailConfirmed)
        {
            // ECA-6 OAuth2 Google: verified local accounts are already legitimate; never clear their password.
            return GoogleClaimResult.Success;
        }

        if (user.IsPhoneConfirmed)
        {
            // ECA-6 OAuth2 Google: phone-verified and email-unverified means two verified identities may conflict.
            // Do not auto-merge; FE should show a recovery/support flow for email_claim_conflict.
            return GoogleClaimResult.Conflict;
        }

        // ECA-6 OAuth2 Google: if this unverified account later has order/auction/reputation data,
        // add a policy/API check here before allowing auto-claim. UserService does not own those tables.
        // ECA-6 OAuth2 Google: no verified email/phone existed before Google, so Google can claim the account safely.
        user.VerifyEmailByGoogle();
        user.ClearPasswordAfterUnverifiedGoogleClaim();

        return GoogleClaimResult.ClaimedUnverifiedAccount;
    }

    /// <summary>
    /// Builds a safe frontend failure redirect without exposing backend exception details to the browser.
    /// </summary>
    private GoogleCallbackResponse Failure(string errorCode = FailureCode)
    {
        return new GoogleCallbackResponse(
            _googleOAuthService.BuildFrontendFailureUrl(errorCode));
    }
}

/// <summary>
/// Describes the result of attempting to link or claim an existing local account with Google OAuth2.
/// </summary>
internal enum GoogleClaimResult
{
    Success,
    ClaimedUnverifiedAccount,
    Conflict,
    Rejected
}
