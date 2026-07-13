namespace ElectronicLibrary.Infrastructure.Models;

public class StorageOptions
{
    // Local dev (Aspire-provisioned Azurite emulator, shared-key auth). Takes precedence over BlobServiceUri.
    public string? ConnectionString { get; set; }

    // Production: managed identity via DefaultAzureCredential, no account keys.
    public string BlobServiceUri { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "assets";
}
