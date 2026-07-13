namespace ElectronicLibrary.Domain.Common;

public abstract class BaseEntity<TKey> : BaseEntity, ISoftDeletable where TKey : struct
{
    public TKey Id { get; set; }
    public bool IsActive { get; set; }

    protected BaseEntity() => IsActive = true;
}
