using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Entities.Users;
using ECommerceAuction.UserService.Domain.Repositories;
using ECommerceAuction.UserService.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _context;
    public AddressRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Address>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Addresses.Where(a => a.UserId == userId && a.DeletedAt == null).OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task AddAddressAsync(Address address, CancellationToken cancellationToken)
    {
        var existingAddresses = await _context.Addresses.Where(a => a.UserId == address.UserId && a.DeletedAt == null).ToListAsync(cancellationToken);

        if (!existingAddresses.Any() || address.IsDefault)
        {
            foreach(var existingAddress in existingAddresses.Where(a => a.IsDefault))
            {
                existingAddress.UnsetAsDefault();
            }
        }
        if (!existingAddresses.Any())
        {
            address.SetAsDefault();
        }
        _context.Addresses.Add(address);
    }

    public async Task DeleteAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        var address = await _context.Addresses.FindAsync([addressId], cancellationToken);
        if (address is null || address.UserId != userId)
        {
            throw new NotFoundException($"Address {addressId} not found.");
        }

        var wasDefault = address.IsDefault;
        address.Delete();
        // If delete this address is default, set other address is default
        if (wasDefault)
        {
            var nextDefault = await _context.Addresses.Where(a => a.UserId == userId && a.DeletedAt == null && a.Id != addressId).OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync(cancellationToken);

            if (nextDefault != null)
            {
                nextDefault.SetAsDefault();
            }
        }
    }

    public async Task<Address?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null, cancellationToken);
    }

    public async Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.DeletedAt == null, cancellationToken);
    }


    public async Task SetDefaultAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken)
    {
        // Get all active address this user
        var addresses = await _context.Addresses.Where(a => a.UserId == userId && a.DeletedAt == null).ToListAsync(cancellationToken);

        var targetAddress = addresses.FirstOrDefault(a => a.Id == addressId);
        if (targetAddress is null)
        {
            throw new NotFoundException($"Address {addressId} not found.");
        }

        foreach (var addr in addresses.Where(a => a.IsDefault && a.Id != addressId))
        {
            addr.UnsetAsDefault();
        }

        targetAddress.SetAsDefault();
    }
    /// <summary>
    /// Update Address
    /// </summary>
    public Task UpdateAddressAsync(Address address, CancellationToken cancellationToken)
    {
        _context.Addresses.Update(address);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Address>> GetDefaultAddressesByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0 )
        {
            return [];
        }
        return await _context.Addresses.AsNoTracking().Where(a => userIds.Contains(a.UserId) && a.IsDefault && a.DeletedAt == null).ToListAsync(cancellationToken);
    }
}
