using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ElectronicLibrary.Persistence.Identity;

// Adds firstName/lastName on top of the default claims. Tokens issued by MapIdentityApi are opaque to
// the client (CLAUDE.md §6), but this claims principal is still what HttpContext.User carries server-side
// on every authenticated request once the bearer token is validated.
public class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        ((ClaimsIdentity)principal.Identity!).AddClaims(
        [
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        ]);

        return principal;
    }
}
