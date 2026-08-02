using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Abstractions.Services;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Users;
using ECommerceAuction.UserService.Persistence.Context;
using ECommerceAuction.UserService.Persistence.Rbac;
using ECommerceAuction.UserService.Persistence.Repositories.Auditing;
using ECommerceAuction.UserService.Persistence.Repositories.Authentication;
using ECommerceAuction.UserService.Persistence.Repositories.IdentityVerifications;
using ECommerceAuction.UserService.Persistence.Repositories.PaymentMethods;
using ECommerceAuction.UserService.Persistence.Repositories.Reputation;
using ECommerceAuction.UserService.Persistence.Repositories.Roles;
using ECommerceAuction.UserService.Persistence.Repositories.Sellers;
using ECommerceAuction.UserService.Persistence.Repositories.Users;
using ECommerceAuction.UserService.Domain.PaymentMethods;
using ECommerceAuction.UserService.Domain.Reputation.Seller;
using ECommerceAuction.UserService.Domain.Reputation.Ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceAuction.UserService.Persistence;

/// <summary>
/// Registers persistence services and repositories for User Service.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds EF Core SQL Server and repository dependencies.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IUserPasswordHistoryRepository, UserPasswordHistoryRepository>();
        services.AddScoped<IUserOAuthRepository, UserOAuthRepository>();
        services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();
        services.AddScoped<ISellerProfileRepository, SellerProfileRepository>();
        services.AddScoped<IRbacRegistrar, RbacRegistrar>();
        services.AddScoped<
            ECommerceAuction.UserService.Application.Abstractions.Services.IInternalUserQueries,
            InternalUserQueries>();
        services.AddScoped<IIdentityVerificationRepository, IdentityVerificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAccountMaintenanceRepository, AccountMaintenanceRepository>();
        services.AddScoped<
            ECommerceAuction.UserService.Domain.IdentityVerifications.IBuyerVerificationRepository,
            BuyerVerificationRepository>();
        services.AddScoped<
            ECommerceAuction.UserService.Domain.Reputation.Buyer.IBuyerReputationRepository,
            BuyerReputationRepository>();
        services.AddScoped<
            ECommerceAuction.UserService.Domain.Reputation.Ledger.IReputationLedgerRepository,
            ReputationLedgerRepository>();
        services.AddScoped<ISellerReputationRepository, SellerReputationRepository>();
        services.AddScoped<IReputationRatingRepository, ReputationRatingRepository>();
        services.AddScoped<
            IBankAccountVerificationRepository,
            BankAccountVerificationRepository>();
        return services;
    }
}
