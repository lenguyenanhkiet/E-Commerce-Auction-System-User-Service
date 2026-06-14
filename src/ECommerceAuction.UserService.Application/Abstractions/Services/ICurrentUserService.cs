namespace ECommerceAuction.UserService.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}

