using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using ECommerceAuction.UserService.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAuction.UserService.Persistence.Context;

/// <summary>
/// EF Core database context for User Service owned tables.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
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
}
