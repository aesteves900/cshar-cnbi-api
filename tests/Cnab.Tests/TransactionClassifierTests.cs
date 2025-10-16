using Cnab.Domain.Enums;
using Cnab.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Cnab.Tests;

public class TransactionClassifierTests
{
    [Theory]
    [InlineData(TransactionKind.Debit, 1)]
    [InlineData(TransactionKind.Boleto, -1)]
    [InlineData(TransactionKind.Credit, 1)]
    [InlineData(TransactionKind.Rent, -1)]
    public void Sign_Is_Correct(TransactionKind kind, int expected)
    {
        TransactionClassifier.Sign(kind).Should().Be(expected);
    }
}
