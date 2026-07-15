using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Users.SetAvatar;

/// <summary>
/// Loads the authenticated user, records the new avatar (URL + key) on their profile, and returns
/// the previous storage key so the API layer can delete the orphaned old file.
/// </summary>
public sealed class SetAvatarCommandHandler
    : ICommandHandler<SetAvatarCommand, SetAvatarResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetAvatarCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SetAvatarResponse> Handle(
        SetAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User information not found in JWT.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User information not found.");

        // Capture the key being replaced before overwriting it.
        var previousKey = user.AvatarKey;

        user.SetAvatar(request.AvatarUrl, request.AvatarKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Only report the old key when it actually differs from the new one.
        var replacedKey = string.Equals(previousKey, request.AvatarKey, StringComparison.Ordinal)
            ? null
            : previousKey;

        return new SetAvatarResponse(user.AvatarUrl ?? string.Empty, replacedKey);
    }
}
