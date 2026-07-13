using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator _mediator = mediator;

    protected async Task<ActionResult> ExecuteCommand(Func<Task> command)
    { await command(); return Accepted(); }

    protected async Task<ActionResult> ExecuteQuery<TResponse>(Func<Task<TResponse>> query)
    { var r = await query(); return Ok(r); }

    protected async Task<ActionResult> ExecuteCommandWithResult<TResponse>(Func<Task<TResponse>> command)
    { var r = await command(); return Accepted(r); }
}
