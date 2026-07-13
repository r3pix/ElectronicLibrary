using System.Net;
using ElectronicLibrary.Application.CQRS.Assets.Commands.DeleteAsset;
using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetById;
using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssetDownloadSas;
using ElectronicLibrary.Application.CQRS.Assets.Queries.GetAssets;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicLibrary.Api.Controllers;

[Authorize]
public class AssetsController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet]
    [ProducesResponseType(typeof(Response<List<AssetModel>>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetAll([FromQuery] GetAssetsQuery query) =>
        await ExecuteQuery(async () => await _mediator.Send(query));

    [HttpGet("{Id}")]
    [ProducesResponseType(typeof(Response<AssetModel>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetById([FromRoute] GetAssetByIdQuery query) =>
        await ExecuteQuery(async () => await _mediator.Send(query));

    [HttpGet("{Id}/download-sas")]
    [ProducesResponseType(typeof(Response<string>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult> GetDownloadSas([FromRoute] GetAssetDownloadSasQuery query) =>
        await ExecuteQuery(async () => await _mediator.Send(query));

    [HttpDelete("{Id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.Accepted)]
    public async Task<ActionResult> Delete([FromRoute] DeleteAssetCommand command) =>
        await ExecuteCommand(async () => await _mediator.Send(command));
}
