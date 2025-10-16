namespace Cnab.Domain.Entities;

public class ImportFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalName { get; set; } = null!;
    public string Sha256 { get; set; } = null!; // for idempotency
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}