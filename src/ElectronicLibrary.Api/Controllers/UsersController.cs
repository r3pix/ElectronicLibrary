using System.Net;
using ElectronicLibrary.Api.Models;
using ElectronicLibrary.Domain.Models;
using ElectronicLibrary.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(Response<CurrentUserModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new Response<CurrentUserModel>(new CurrentUserModel
        {
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList()
        }));
    }
}
