using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Infrastructure.Models;
using ElectronicLibrary.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicLibrary.Infrastructure.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddConfigurationModels(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();

        // Aspire's WithReference(blobs) injects the ASP.NET Core-style ConnectionStrings:blobs for
        // regular projects, but the Azure Functions worker resolves its own bindings via the bare
        // "blobs" app setting instead — check both so this works from Api and Functions alike.
        options.ConnectionString ??= configuration.GetConnectionString("blobs") ?? configuration["blobs"];

        if (string.IsNullOrEmpty(options.ConnectionString) && string.IsNullOrEmpty(options.BlobServiceUri))
        {
            throw new InvalidOperationException(
                "Blob storage is not configured (Storage:ConnectionString/BlobServiceUri, or " +
                "ConnectionStrings:blobs). Run via the Aspire AppHost, or set it manually in config.");
        }

        services.AddSingleton(options);
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();
        services.AddScoped<IAssetProcessor, AssetProcessor>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<StorageOptions>();
            return string.IsNullOrEmpty(options.ConnectionString)
                ? new BlobServiceClient(new Uri(options.BlobServiceUri), new DefaultAzureCredential())
                : new BlobServiceClient(options.ConnectionString);
        });

        // Azurite (local dev via a connection string) has no AAD support at all, so a user-delegation
        // key can't be issued against it — only present when there's a shared key to sign SAS with
        // directly; BlobStorageService falls back to the (managed-identity) user-delegation flow when
        // this is null, which is the only path real Azure/prod ever takes.
        services.AddSingleton<StorageSharedKeyCredential>(sp =>
        {
            var options = sp.GetRequiredService<StorageOptions>();
            return string.IsNullOrEmpty(options.ConnectionString)
                ? null!
                : ParseSharedKeyCredential(options.ConnectionString)!;
        });

        return services;
    }

    private static StorageSharedKeyCredential? ParseSharedKeyCredential(string connectionString)
    {
        var parts = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('=', 2))
            .Where(part => part.Length == 2)
            .ToDictionary(part => part[0], part => part[1], StringComparer.OrdinalIgnoreCase);

        return parts.TryGetValue("AccountName", out var accountName) && parts.TryGetValue("AccountKey", out var accountKey)
            ? new StorageSharedKeyCredential(accountName, accountKey)
            : null;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
            options.AddPolicy("Default", policy =>
                policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

        return services;
    }
}
