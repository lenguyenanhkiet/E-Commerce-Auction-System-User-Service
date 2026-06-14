using ECommerceAuction.UserService.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using ECommerceAuction.UserService.Domain.Entities.Users;
namespace ECommerceAuction.UserService.Persistence.Context;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<ReputationProfile> ReputationProfiles => Set<ReputationProfile>();

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

