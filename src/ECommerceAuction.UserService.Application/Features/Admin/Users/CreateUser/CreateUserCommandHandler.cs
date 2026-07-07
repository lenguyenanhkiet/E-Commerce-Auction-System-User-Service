using System.Text.Json;
using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Entities.Reputation;
using ECommerceAuction.UserService.Domain.Entities.Roles;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using MassTransit;
using Nexus.Shared.Contracts.Events.User;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.CreateUser;

/// <summary>
/// Admin User Management: creates a verified user account, assigns roles, and records the action.
/// </summary>
public sealed class CreateUserCommandHandler
    : ICommandHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleManagementRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IRoleManagementRepository roleRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateUserResponse> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var actorUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("Authenticated user identifier is missing.");

        var email = request.Email.Trim().ToLowerInvariant();
        var phoneNumber = request.PhoneNumber.Trim();

        if (await _userRepository.CheckEmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("The email already exists.");
        }

        if (await _userRepository.CheckPhoneExistsAsync(phoneNumber, cancellationToken))
        {
            throw new ConflictException("The phone number already exists.");
        }

        // Resolve the roles to assign before touching the user so an invalid code aborts early.
        var roles = await ResolveRolesAsync(request.RoleCodes, cancellationToken);

        var user = User.CreateByAdmin(
            email: email,
            passwordHash: _passwordHasher.HashPassword(request.Password),
            fullName: request.FullName,
            phoneNumber: phoneNumber,
            gender: request.Gender,
            dateOfBirth: request.DateOfBirth,
            address: request.Address);

        foreach (var role in roles)
        {
            user.AssignRole(UserRole.Assign(user.Id, role.Id, actorUserId));
        }

        await _userRepository.AddAsync(user, cancellationToken);

        // A verified email grants the first reputation point, matching the self-registration flow.
        await _userRepository.AddReputationProfileAsync(
            ReputationProfile.CreateForVerifiedEmail(user.Id),
            cancellationToken);

        var assignedRoleCodes = roles.Select(role => role.Code).ToList();

        await _userRepository.AddAuditLogAsync(
            UserAuditLog.Create(
                actorUserId,
                targetUserId: user.Id,
                UserAuditActions.UserCreated,
                nameof(User),
                user.Id.ToString(),
                oldValue: null,
                newValue: JsonSerializer.Serialize(new { Roles = assignedRoleCodes })),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Announce the new account so other services (e.g. Notification) can react.
        await _publishEndpoint.Publish(
            new UserRegisteredEvent
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SourceService = "UserService"
            },
            cancellationToken);

        return new CreateUserResponse(
            Id: user.Id,
            Email: user.Email,
            PhoneNumber: user.PhoneNumber,
            FullName: user.FullName,
            Status: user.Status,
            Roles: assignedRoleCodes);
    }

    /// <summary>
    /// Resolves and validates the requested role codes, defaulting to the BUYER role when none are supplied.
    /// </summary>
    private async Task<IReadOnlyList<Role>> ResolveRolesAsync(
        IReadOnlyList<string>? requestedRoleCodes,
        CancellationToken cancellationToken)
    {
        var normalizedCodes = (requestedRoleCodes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        // No explicit selection falls back to the default buyer role, like self-registration.
        if (normalizedCodes.Count == 0)
        {
            normalizedCodes.Add(RoleCodes.Buyer);
        }

        var query = _roleRepository.QueryRoles()
            .Where(role =>
                role.Status == RoleStatuses.Active &&
                role.DeletedAt == null &&
                normalizedCodes.Contains(role.Code));

        var roles = await _roleRepository.ListRolesAsync(query, cancellationToken);

        if (roles.Count != normalizedCodes.Count)
        {
            var foundCodes = roles.Select(role => role.Code).ToHashSet(StringComparer.Ordinal);
            var invalidCodes = normalizedCodes.Where(code => !foundCodes.Contains(code));

            throw new BusinessRuleException(
                $"One or more roles are invalid or inactive: {string.Join(", ", invalidCodes)}.");
        }

        return [.. roles];
    }
}
