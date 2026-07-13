using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ElectronicLibrary.Application.Extensions;
using ElectronicLibrary.Infrastructure.Extensions;
using ElectronicLibrary.Infrastructure.Middlewares;
using ElectronicLibrary.Infrastructure.Models;
using ElectronicLibrary.Persistence;
using ElectronicLibrary.Persistence.Extensions;
using ElectronicLibrary.Persistence.Identity;
using ElectronicLibrary.Persistence.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services
    .AddConfigurationModels(builder.Configuration)
    .AddInfrastructureServices()
    .AddCustomCors(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);

    var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
    var storageOptions = scope.ServiceProvider.GetRequiredService<StorageOptions>();

    // Minting a SAS for a blob doesn't create its container — without this, the client's direct PUT
    // 404s with "The specified container does not exist."
    await blobServiceClient.GetBlobContainerClient(storageOptions.ContainerName).CreateIfNotExistsAsync();

    // The SPA PUTs uploads directly to the blob, so the storage account itself (not just the Api)
    // needs CORS rules permitting the configured origins.
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (corsOrigins.Length > 0)
    {
        var properties = await blobServiceClient.GetPropertiesAsync();
        properties.Value.Cors =
        [
            new BlobCorsRule
            {
                AllowedOrigins = string.Join(",", corsOrigins),
                AllowedMethods = "GET,PUT,HEAD,OPTIONS",
                AllowedHeaders = "*",
                ExposedHeaders = "*",
                MaxAgeInSeconds = 3600
            }
        ];
        await blobServiceClient.SetPropertiesAsync(properties.Value);
    }
}

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapIdentityApi<ApplicationUser>();

app.Run();
