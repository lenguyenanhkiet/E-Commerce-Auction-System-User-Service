using ECommerceAuction.UserService.Application.Abstractions.Messaging;
using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerceAuction.UserService.Application.Features.Users.DeleteAddress
{
    public sealed class DeleteAddressCommandHandler : ICommandHandler<DeleteAddressCommand>
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteAddressCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAddressCommandHandler(IAddressRepository addressRepository, ICurrentUserService currentUserService, ILogger<DeleteAddressCommandHandler> logger, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _currentUserService = currentUserService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User information not found in JWT.");
            await _addressRepository.DeleteAddressAsync(userId, request.AddressId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Deleted Address {request.AddressId} for User {userId}");
        }
    }
}