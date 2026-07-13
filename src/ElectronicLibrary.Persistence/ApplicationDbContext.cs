using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Common;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElectronicLibrary.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
{
    private readonly string AzureFunctionAuditName = "Azure Function";
    public DbSet<Asset> Assets => Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(ct);
    }

    public Task<int> SaveChangesAsyncWithoutUser(CancellationToken ct = default) =>
        base.SaveChangesAsync(ct);

    private void StampAuditFields()
    {
        var email = currentUserService.Email ?? AzureFunctionAuditName;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreateDate = now;
                    entry.Entity.CreateEmail = email;
                    entry.Entity.LMDate = now;
                    entry.Entity.LMEmail = email;
                    break;
                case EntityState.Modified:
                    entry.Entity.LMDate = now;
                    entry.Entity.LMEmail = email;
                    break;
            }
        }
    }
}
