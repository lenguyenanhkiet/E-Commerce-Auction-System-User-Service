using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.Avatar.RemoveAvatar;

public sealed record RemoveAvatarCommand : ICommand<RemoveAvatarResponse>;
public sealed record RemoveAvatarResponse(string? PreviousKey, bool Removed);