using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Domain.Repositories;

namespace ECommerceAuction.UserService.Application.Features.Admin.Users.GetUsers;

/// <summary>
/// Handles Admin user list queries.
/// </summary>
public sealed class GetUsersQueryHandler
    : IQueryHandler<GetUsersQuery, PagedUsersResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityVerificationRepository _identityVerificationRepository;
    private readonly IAddressRepository _addressRepository;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        IIdentityVerificationRepository identityVerificationRepository,
        IAddressRepository addressRepository)
    {
        _userRepository = userRepository;
        _identityVerificationRepository = identityVerificationRepository;
        _addressRepository = addressRepository;
    }

    public async Task<PagedUsersResponse> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1)
        {
            throw new InvalidOperationException(
                "Page must be greater than or equal to 1.");
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            throw new InvalidOperationException(
                "PageSize must be between 1 and 100.");
        }

        var result = await _userRepository.GetPagedAsync(
            search: request.Search,
            gender: request.Gender,
            status: request.Status,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var userIds = result.Items.Select(user => user.Id).ToList();
        var identityVerifications = await _identityVerificationRepository.GetByUserIdsAsync(userIds, cancellationToken);
        var identityNumbersByUserId = identityVerifications.ToDictionary(iv => iv.UserId, iv => iv.IdentityNumber);

        var defaultAddresses = await _addressRepository.GetDefaultAddressesByUserIdsAsync(userIds, cancellationToken);
        var addressByUserId = defaultAddresses.ToDictionary(a => a.UserId, a => a);
        var items = result.Items
            .Select(user =>
            {
                var address = addressByUserId.GetValueOrDefault(user.Id);
                return new AdminUserItem(
                    Id: user.Id,
                    FullName: user.FullName,
                    Email: user.Email,
                    PhoneNumber: user.PhoneNumber,
                    IdentityNumber: identityNumbersByUserId.GetValueOrDefault(user.Id),
                    Gender: user.Gender,
                    Address: address is null
                        ? null
                        : $"{address.Street},{address.Ward}, {address.Province}",
                    DateOfBirth: user.DateOfBirth);
            })
            .ToList();

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(
                result.TotalCount / (double)request.PageSize);

        return new PagedUsersResponse(
            Items: items,
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: result.TotalCount,
            TotalPages: totalPages);
    }
}