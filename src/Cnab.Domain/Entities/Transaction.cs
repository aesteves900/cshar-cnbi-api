using Cnab.Domain.Enums;

namespace Cnab.Domain.Entities;

public class Transaction
{
    public long Id { get; set; }

    public TransactionKind Kind { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public decimal Amount { get; set; } // signed value

    public string Cpf { get; set; } = null!;
    public string Card { get; set; } = null!;

    public int StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid ImportFileId { get; set; }
    public ImportFile ImportFile { get; set; } = null!;

    public int SourceLine { get; set; }
}