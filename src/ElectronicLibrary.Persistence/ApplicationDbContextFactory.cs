using ElectronicLibrary.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ElectronicLibrary.Persistence;

// Used only by `dotnet ef` design-time tooling (migrations add/update), which can't resolve
// ApplicationDbContext through the app's normal DI/Aspire-injected connection string.
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=ElectronicLibrary;Trusted_Connection=True;TrustServerCertificate=True;");

        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeCurrentUserService());
    }

    private class DesignTimeCurrentUserService : ICurrentUserService
    {
        public string? Email => null;
    }
}
