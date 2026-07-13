using Azure.Storage.Blobs;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using PDFtoImage;

namespace ElectronicLibrary.Infrastructure.Services;

public class AssetProcessor(IAssetRepository assets, BlobServiceClient blobServiceClient, StorageOptions options)
    : IAssetProcessor
{
    private const string ThumbnailsPrefix = "thumbnails";

    public async Task ProcessAsync(string blobName, CancellationToken ct = default)
    {
        var type = ResolveType(blobName);
        if (type is null)
            return;

        var blob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(blobName);

        var existing = await assets.GetByBlobNameAsync(blobName, ct);
        if (existing is not null && existing.Status == AssetStatus.Ready)
            return; // Already processed — avoid redoing (potentially expensive) thumbnail work.

        var properties = await blob.GetPropertiesAsync(cancellationToken: ct);
        var thumbnailBlobName = await TryGenerateThumbnailAsync(type.Value, blobName, properties.Value.ContentType, ct);

        if (existing is not null)
        {
            // Title is already the original filename from upload time (CreateAssetUploadSasCommandHandler)
            // — only content-derived fields get filled in here.
            existing.ContentType = properties.Value.ContentType;
            existing.SizeBytes = properties.Value.ContentLength;
            existing.ThumbnailBlobName = thumbnailBlobName;
            existing.Status = AssetStatus.Ready;
            await assets.UpdateAsync(existing, ct);
            return;
        }

        try
        {
            // No pre-created stub (e.g. a file dropped straight into the container) — no separately
            // tracked original filename exists, so fall back to the blob name's suffix.
            var asset = new Asset
            {
                Type = type.Value,
                Status = AssetStatus.Ready,
                BlobName = blobName,
                Title = blobName[(blobName.IndexOf('/') + 1)..],
                ContentType = properties.Value.ContentType,
                SizeBytes = properties.Value.ContentLength,
                ThumbnailBlobName = thumbnailBlobName
            };
            await assets.AddAsync(asset, ct);
        }
        catch (DbUpdateException)
        {
            // Duplicate BlobName from a concurrent/at-least-once Event Grid delivery — already processed.
        }
    }

    // Score PDFs only — MusicXML scores have no page to render, and other asset types are out of scope
    // for previews. Never lets a bad/corrupt PDF fail the asset itself; a missing thumbnail just falls
    // back to a placeholder icon client-side.
    private async Task<string?> TryGenerateThumbnailAsync(AssetType type, string blobName, string contentType, CancellationToken ct)
    {
        if (type != AssetType.Score)
            return null;

        var isPdf = string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            || blobName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdf)
            return null;

        try
        {
            var sourceBlob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(blobName);
            await using var pdfStream = await sourceBlob.OpenReadAsync(cancellationToken: ct);

            using var pngStream = new MemoryStream();
            // PDFtoImage's [SupportedOSPlatform] list is exactly Windows/Linux/macOS — the analyzer just
            // can't see that because this project targets net10.0, not a platform-suffixed TFM.
#pragma warning disable CA1416
            Conversion.SavePng(
                pngStream,
                pdfStream,
                page: 0,
                leaveOpen: false,
                password: null,
                options: new RenderOptions(Width: 300, WithAspectRatio: true));
#pragma warning restore CA1416
            pngStream.Position = 0;

            var thumbnailBlobName = $"{ThumbnailsPrefix}/{Path.ChangeExtension(blobName[(blobName.IndexOf('/') + 1)..], ".png")}";
            var thumbnailBlob = blobServiceClient.GetBlobContainerClient(options.ContainerName).GetBlobClient(thumbnailBlobName);
            await thumbnailBlob.UploadAsync(pngStream, overwrite: true, cancellationToken: ct);

            return thumbnailBlobName;
        }
        catch
        {
            return null;
        }
    }

    private static AssetType? ResolveType(string blobName) => blobName switch
    {
        _ when blobName.StartsWith("scores/", StringComparison.OrdinalIgnoreCase) => AssetType.Score,
        _ when blobName.StartsWith("diplomas/", StringComparison.OrdinalIgnoreCase) => AssetType.Diploma,
        _ when blobName.StartsWith("posters/", StringComparison.OrdinalIgnoreCase) => AssetType.Poster,
        _ => null
    };
}
