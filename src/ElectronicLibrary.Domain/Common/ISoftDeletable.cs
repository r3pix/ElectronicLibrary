namespace ElectronicLibrary.Domain.Common;

public interface ISoftDeletable
{
    bool IsActive { get; set; }
}
