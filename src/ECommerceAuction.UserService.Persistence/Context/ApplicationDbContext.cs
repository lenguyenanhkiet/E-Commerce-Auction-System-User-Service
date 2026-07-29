using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Auditing;
using ECommerceAuction.UserService.Domain.Authentication;
using ECommerceAuction.UserService.Domain.Roles;
using ECommerceAuction.UserService.Domain.IdentityVerifications;
using ECommerceAuction.UserService.Domain.PaymentMethods;
using ECommerceAuction.UserService.Domain.Reputation;
using ECommerceAuction.UserService.Domain.Reputation.Buyer;
using ECommerceAuction.UserService.Domain.Reputation.Ledger;
using ECommerceAuction.UserService.Domain.Sellers;
using ECommerceAuction.UserService.Domain.Sellers.Applications;
using ECommerceAuction.UserService.Domain.Users;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Context;

/// <summary>
/// EF Core database context for User Service owned tables.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserAuditLog> UserAuditLogs => Set<UserAuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<SellerApplicationHistory> SellerApplicationHistories => Set<SellerApplicationHistory>();
    public DbSet<IdentityVerification> IdentityVerifications => Set<IdentityVerification>();
    public DbSet<BuyerVerificationProfile> BuyerVerificationProfiles => Set<BuyerVerificationProfile>();
    public DbSet<BuyerReputationProfile> BuyerReputationProfiles => Set<BuyerReputationProfile>();
    public DbSet<ReputationLedgerEntry> ReputationLedgerEntries => Set<ReputationLedgerEntry>();
    public DbSet<BankAccountVerification> BankAccountVerifications => Set<BankAccountVerification>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveColumnType("datetimeoffset(3)");

        configurationBuilder.Properties<DateTimeOffset?>()
            .HaveColumnType("datetimeoffset(3)");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            // RBAC duplicate requests should become HTTP 409 instead of a generic HTTP 500.
            throw new ConflictException(
                "A record with the same unique value already exists.",
                exception);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException &&
               sqlException.Number is 2601 or 2627;
    }
}