using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Entities;
using ElectronicLibrary.Domain.Enums;
using ElectronicLibrary.Persistence;
using ElectronicLibrary.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace ElectronicLibrary.Tests.Repositories;

// Regression coverage for the soft-delete fix called out in CLAUDE.md §5.3: Repository<TEntity>.DeleteAsync
// must set IsActive = false for ISoftDeletable entities instead of hard-removing the row, and the global
// query filter (BaseEntityConfiguration) must then hide it from normal queries.
public class RepositorySoftDeleteTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.Email.Returns("user@example.com");
        return new ApplicationDbContext(options, currentUser);
    }

    [Fact]
    public async Task DeleteAsync_OnSoftDeletableEntity_SetsIsActiveFalse_InsteadOfRemovingRow()
    {
        await using var context = CreateContext();
        var repository = new AssetRepository(context);
        var asset = new Asset { Id = Guid.NewGuid(), Type = AssetType.Score, BlobName = "scores/example.pdf" };
        await repository.AddAsync(asset);

        await repository.DeleteAsync(asset);

        var stored = await context.Assets.IgnoreQueryFilters().SingleAsync(a => a.Id == asset.Id);
        stored.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_OnSoftDeletableEntity_ExcludesItFromSubsequentQueries()
    {
        await using var context = CreateContext();
        var repository = new AssetRepository(context);
        var asset = new Asset { Id = Guid.NewGuid(), Type = AssetType.Score, BlobName = "scores/example.pdf" };
        await repository.AddAsync(asset);

        await repository.DeleteAsync(asset);

        var remaining = await repository.GetAllAsync();
        remaining.Should().BeEmpty();
    }
}
