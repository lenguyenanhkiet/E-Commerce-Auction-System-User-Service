using ECommerceAuction.UserService.Application.Abstractions.Messaging;

namespace ECommerceAuction.UserService.Application.Features.Admin.Sellers.ApproveSeller;

public sealed record ApproveSellerCommand(Guid SellerProfileId, Guid AdminUserId) : ICommand<ApproveSellerResponse>;

public sealed record ApproveSellerResponse(Guid Id, string Status, DateTime ReviewAt);