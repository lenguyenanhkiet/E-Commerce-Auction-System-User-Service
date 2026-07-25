using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.CreateAddress;

public sealed class CreateAddressCommandHandler : ICommandHandler<CreateAddressCommand, CreateAddressResponse>
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateAddressCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAddressCommandHandler(IAddressRepository addressRepository, IUserRepository userRepository, ICurrentUserService currentUserService, ILogger<CreateAddressCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAddressResponse> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information not found in JWT.");
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new UserNotFoundException($"User {userId} not found.");
        var address = new Address
            (
            userId,
            request.RecipientName,
            request.RecipientPhone,
            request.Province,
            request.Ward,
            request.Street,
            request.Type,
            request.IsDefault
            );
        await _addressRepository.AddAddressAsync(address, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Created Address {address.Id} for User {userId}");

        return new CreateAddressResponse
            (
            address.Id,
            address.RecipientName,
            address.RecipientPhone,
            address.Province,
            address.Ward,
            address.Street,
            address.Type,
            address.IsDefault,
            address.CreatedAt
            );
    }
}