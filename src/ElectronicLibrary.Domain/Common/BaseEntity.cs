namespace ElectronicLibrary.Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreateDate { get; set; }
    public DateTime LMDate { get; set; }
    public string CreateEmail { get; set; } = string.Empty;
    public string LMEmail { get; set; } = string.Empty;
}
