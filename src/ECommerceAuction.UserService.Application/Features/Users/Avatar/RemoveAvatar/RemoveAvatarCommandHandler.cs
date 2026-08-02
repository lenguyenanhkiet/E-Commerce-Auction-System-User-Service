using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.Avatar.RemoveAvatar;

public sealed class RemoveAvatarCommandHandler : ICommandHandler<RemoveAvatarCommand, RemoveAvatarResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAvatarCommandHandler(ICurrentUserService currentUserService, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RemoveAvatarResponse> Handle(
        RemoveAvatarCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException(
                "User information not found in JWT.");

        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        var previousKey = user.AvatarKey;

        if (string.IsNullOrWhiteSpace(previousKey))
        {
            return new RemoveAvatarResponse(null, false);
        }

        user.SetAvatar(null, null);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RemoveAvatarResponse(previousKey, true);
    }
}