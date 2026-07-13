using ElectronicLibrary.Api.Models;
using ElectronicLibrary.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicLibrary.Api.Controllers;

// Registration lives here instead of MapIdentityApi's built-in /register endpoint, which only accepts
// { email, password } — no room for first/last name. Login/refresh are unaffected and keep using
// MapIdentityApi's built-in endpoints.
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
            return Ok();

        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);

        return ValidationProblem(ModelState);
    }
}
