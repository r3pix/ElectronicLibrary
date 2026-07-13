using ElectronicLibrary.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ElectronicLibrary.Persistence.EntityConfigurations;

public abstract class BaseEntityConfiguration<TKey, TEntity>(string table) : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity<TKey> where TKey : struct
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(table);
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => x.IsActive);
    }
}
