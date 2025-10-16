using System.ComponentModel.DataAnnotations;

namespace Cnab.Domain.Entities;

public class Store
{
    public int Id { get; set; }

    [MaxLength(14)] public string Owner { get; set; } = null!;
    [MaxLength(19)] public string Name { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}