using ECommerceAuction.UserService.Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Domain.Repositories
{
    public interface IAddressRepository
    {
        Task<List<Address>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddAddressAsync(Address address, CancellationToken cancellationToken = default);
        Task UpdateAddressAsync(Address address, CancellationToken cancellationToken = default);
        Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
        Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default );
        Task<IReadOnlyList<Address>> GetDefaultAddressesByUserIdsAsync(
       IReadOnlyCollection<Guid> userIds,
       CancellationToken cancellationToken = default);
    }
}
