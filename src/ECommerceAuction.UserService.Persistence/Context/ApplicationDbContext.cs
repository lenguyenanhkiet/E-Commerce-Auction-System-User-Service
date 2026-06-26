using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Application.Common.Exceptions;
using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Context;

/// <summary>
/// EF Core database context for User Service owned tables.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<RolePrivilege> RolePrivileges => Set<RolePrivilege>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ReputationProfile> ReputationProfiles => Set<ReputationProfile>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserAuditLog> UserAuditLogs => Set<UserAuditLog>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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
