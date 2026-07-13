using System.Security.Claims;
using ElectronicLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ElectronicLibrary.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? Email => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
}
