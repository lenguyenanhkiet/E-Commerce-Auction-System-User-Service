using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.UserAddress.SetDefaultAddress;

public sealed class SetDefaultAddressCommandHandler : ICommandHandler<SetDefaultAddressCommand>
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDefaultAddressCommandHandler> _logger;

    public SetDefaultAddressCommandHandler(IAddressRepository addressRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger<SetDefaultAddressCommandHandler> logger)
    {
        _addressRepository = addressRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information not found in JWT.");
        await _addressRepository.SetDefaultAddressAsync(userId, request.AddressId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Set Address {request.AddressId} as default for User {userId}");
    }
}