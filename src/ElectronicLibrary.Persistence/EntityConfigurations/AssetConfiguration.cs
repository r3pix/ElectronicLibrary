using ElectronicLibrary.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicLibrary.Persistence.EntityConfigurations;

public class AssetConfiguration() : BaseEntityConfiguration<Guid, Asset>("Assets")
{
    public override void Configure(EntityTypeBuilder<Asset> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.BlobName).IsRequired().HasMaxLength(1024);
        builder.HasIndex(x => x.BlobName).IsUnique();

        builder.Property(x => x.Title).HasMaxLength(512);
        builder.Property(x => x.ContentType).HasMaxLength(256);
        builder.Property(x => x.UploadedBy).HasMaxLength(256);
        builder.Property(x => x.ThumbnailBlobName).HasMaxLength(1024);
    }
}
