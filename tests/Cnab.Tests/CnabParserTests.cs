using Cnab.Domain.Enums;
using Cnab.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Cnab.Tests;

public class CnabParserTests
{
    [Fact]
    public async Task Parses_Valid_Line()
    {
        // 1 + 8 + 10 + 11 + 12 + 6 + 14 + 19 = 81 chars
        var line =
            "1" +              // type (1)
            "20190301" +       // date (8)
            "0000014200" +     // value (10) => 142.00
            "00123456789" +    // cpf (11)
            "1234****5678" +   // card (12)  <<-- keep this EXACTLY 12 chars
            "123456" +         // time (6)
            "OWNER NAME    " + // owner (14)  ("OWNER NAME" + 4 spaces)
            "STORE NAME         "; // store (19) ("STORE NAME" + 9 spaces)

        using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(line + "\n"));
        var parser = new CnabParser();
        var result = await parser.ParseAsync("test.txt", ms);

        result.Errors.Should().BeEmpty();
        result.Transactions.Should().HaveCount(1);
        var t = result.Transactions.Single();
        t.Kind.Should().Be(TransactionKind.Debit);
        t.Amount.Should().BePositive();
        t.Store.Name.Should().NotBeNull();
    }

}
