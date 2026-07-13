using System.Net;
using ElectronicLibrary.Application.CQRS.Assets.Commands.CreateAssetUploadSas;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicLibrary.Api.Controllers;

[Authorize(Roles = "Admin")]
public class UploadsController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost("sas")]
    [ProducesResponseType(typeof(Response<AssetUploadSasModel>), (int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> CreateSas([FromBody] CreateAssetUploadSasRequest request) =>
        await ExecuteCommandWithResult(async () => await _mediator.Send(CreateAssetUploadSasCommand.Create(request)));
}
