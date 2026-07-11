using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UpdateAddress;

public sealed class UpdateAddressCommandHandler : ICommandHandler<UpdateAddressCommand, UpdateAddressResponse>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAddressCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    public UpdateAddressCommandHandler(IAddressRepository addressRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<UpdateAddressCommandHandler> logger)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<UpdateAddressResponse> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        
        // Verify user exists
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User information not found in JWT.");

        _logger.LogInformation("Handling UpdateAddressCommand for UserId: {UserId}, AddressId: {AddressId}",
            userId,
            request.AddressId);

        // Get address to update
        var address = await _addressRepository.GetByIdAsync(request.AddressId, cancellationToken);
        if (address is null)
        {
            _logger.LogWarning($"Address {request.AddressId} not found");
            throw new NotFoundException($"Address {request.AddressId} not found");
        }

        // Verify address belong to the user
        if (address.UserId != userId)
        {
            _logger.LogWarning($"Address {request.AddressId} does not belong to User {userId}");
            throw new UnauthorizedAccessException("Address does not belong to User ");
        }

        // Update Address fields
        address.Update(
            request.RecipientName,
            request.RecipientPhone,
            request.Street,
            request.Province,
            request.City,
            request.Ward,
            request.Type
            );
        // Handle default address logic
        if (request.IsDefault && !address.IsDefault)
        {
            _logger.LogInformation($"Setting Address {request.AddressId} as default for User {userId}");
            await _addressRepository.SetDefaultAddressAsync(userId, request.AddressId, cancellationToken);
        }
        else if (!request.IsDefault && address.IsDefault)
        {
            // If unset default, there must be at least one other address.
            var otherAddresses = await _addressRepository.GetUserAddressesAsync(userId, cancellationToken);
            if (!otherAddresses.Any(a => a.Id != request.AddressId && a.DeletedAt is null))
            {
                _logger.LogWarning($"Cannot unset default address for User {userId} - no other address exists.");
                throw new InvalidOperationException("Cannot unset the only address as default.");
            }
            address.UnsetAsDefault();
            await _addressRepository.UpdateAddressAsync(address, cancellationToken);
        }
        else
        {
            // Only update address fields
            await _addressRepository.UpdateAddressAsync(address, cancellationToken);
        }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation($"Successfully updated Address {request.AddressId} for User {userId}");
        return new UpdateAddressResponse
            (
                address.Id,
                address.RecipientName,
                address.RecipientPhone,
                address.Province,
                address.City,
                address.Ward,
                address.Street,
                address.Type,
                address.IsDefault,
                address.UpdatedAt ?? DateTime.UtcNow
            );
    }
}
