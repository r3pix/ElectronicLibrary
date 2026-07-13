using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Application.Models;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Infrastructure.Models;

namespace ElectronicLibrary.Infrastructure.Services;

public class BlobStorageService(
    BlobServiceClient blobServiceClient,
    StorageOptions options,
    StorageSharedKeyCredential? sharedKeyCredential) : IBlobStorageService
{
    private static readonly TimeSpan SasLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ClockSkewMargin = TimeSpan.FromMinutes(5);

    public async Task<AssetUploadSasModel> GetUploadSasAsync(string fileName, AssetType type, CancellationToken ct = default)
    {
        var blobName = $"{PrefixFor(type)}/{Guid.NewGuid()}-{fileName}";
        var blob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(blobName);

        var uploadUrl = await BuildSasUrlAsync(blob, BlobSasPermissions.Write | BlobSasPermissions.Create, ct);

        return new AssetUploadSasModel { UploadUrl = uploadUrl, BlobName = blobName };
    }

    public async Task<string> GetDownloadSasAsync(string blobName, string fileName, CancellationToken ct = default)
    {
        var blob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(blobName);

        // Overrides the response Content-Disposition for just this SAS-authenticated request (doesn't
        // touch the blob's own stored metadata) — makes the browser download the file straight away
        // instead of navigating to/rendering it (e.g. a PDF opening in-tab).
        var contentDisposition = $"attachment; filename=\"{fileName.Replace('"', '\'')}\"";

        return await BuildSasUrlAsync(blob, BlobSasPermissions.Read, ct, contentDisposition);
    }

    public async Task<string> GetThumbnailSasAsync(string thumbnailBlobName, CancellationToken ct = default)
    {
        // No Content-Disposition override — this needs to render inline in an <img>, not force a download.
        var blob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(thumbnailBlobName);
        return await BuildSasUrlAsync(blob, BlobSasPermissions.Read, ct);
    }

    private async Task<string> BuildSasUrlAsync(
        BlobClient blob,
        BlobSasPermissions permissions,
        CancellationToken ct,
        string? contentDisposition = null)
    {
        var startsOn = DateTimeOffset.UtcNow.Subtract(ClockSkewMargin);
        var expiresOn = DateTimeOffset.UtcNow.Add(SasLifetime);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b",
            StartsOn = startsOn,
            ExpiresOn = expiresOn,
            ContentDisposition = contentDisposition
        };
        sasBuilder.SetPermissions(permissions);

        SasQueryParameters sasQuery;
        if (sharedKeyCredential is not null)
        {
            // Local dev against Azurite: it has no AAD support, so GetUserDelegationKeyAsync would
            // 403 ("Only authentication scheme Bearer is supported") — sign with the shared key instead.
            sasQuery = sasBuilder.ToSasQueryParameters(sharedKeyCredential);
        }
        else
        {
            var userDelegationKey = await blobServiceClient.GetUserDelegationKeyAsync(startsOn, expiresOn, ct);
            sasQuery = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, blobServiceClient.AccountName);
        }

        // blob.Uri.ToString() (via plain interpolation) can hand back spaces un-escaped instead of
        // "%20" for certain inputs — AbsoluteUri is the one guaranteed to be wire-safe.
        return $"{blob.Uri.AbsoluteUri}?{sasQuery}";
    }

    private static string PrefixFor(AssetType type) => type switch
    {
        AssetType.Score => "scores",
        AssetType.Diploma => "diplomas",
        AssetType.Poster => "posters",
        AssetType.Recording => "recordings",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
