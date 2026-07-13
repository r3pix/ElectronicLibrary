using ElectronicLibrary.Application.Interfaces;
using Microsoft.Azure.Functions.Worker;

namespace ElectronicLibrary.Functions.Triggers;

// Local-dev only: classic polling trigger, since Event Grid doesn't fire against Azurite.
// The isolated worker model has no [Disable] attribute, so this is disabled in prod purely via the
// app setting "AzureWebJobs.AssetBlobTrigger_Polling.Disabled" = true (set in prod app settings, not here).
// Connection = "blobs" matches the Aspire WithReference(blobs) name (ConnectionStrings:blobs) — the
// same storage account the Api uploads to. "AzureWebJobsStorage" is reserved for the Function host's
// own bookkeeping storage (a separate account Aspire auto-provisions) and would silently watch an
// empty container instead.
public class AssetBlobTriggerPolling(IAssetProcessor assetProcessor)
{
    [Function("AssetBlobTrigger_Polling")]
    public Task RunAsync(
        [BlobTrigger("assets/{name}", Connection = "blobs")] Stream blob,
        string name,
        CancellationToken cancellationToken) =>
        assetProcessor.ProcessAsync(name, cancellationToken);
}
