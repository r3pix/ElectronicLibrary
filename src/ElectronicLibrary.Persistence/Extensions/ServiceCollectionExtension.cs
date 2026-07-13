using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Persistence.Identity;
using ElectronicLibrary.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicLibrary.Persistence.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("sqldb")));

        // No IEmailSender is registered yet (no mail provider configured), so confirmation links
        // would go nowhere — explicitly not requiring confirmation until that's built out.
        services.AddIdentityApiEndpoints<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Overrides the default claims factory AddIdentityApiEndpoints just registered above, so it
        // must come after — adds firstName/lastName to the claims principal on every authenticated request.
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IAssetRepository, AssetRepository>();

        return services;
    }
}
