using ElectronicLibrary.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace ElectronicLibrary.Functions.Triggers;

public class AssetBlobTriggerEventGrid(IAssetProcessor assetProcessor)
{
    [Function("AssetBlobTrigger_EventGrid")]
    public Task RunAsync(
        [BlobTrigger("assets/{name}", Source = BlobTriggerSource.EventGrid, Connection = "blobs")] Stream blob,
        string name,
        CancellationToken cancellationToken) =>
        assetProcessor.ProcessAsync(name, cancellationToken);
}
