namespace ElectronicLibrary.Application.Interfaces;

public interface IAssetProcessor
{
    Task ProcessAsync(string blobName, CancellationToken ct = default);
}
