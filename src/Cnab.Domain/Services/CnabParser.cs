using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Cnab.Domain.Entities;
using Cnab.Domain.Enums;

namespace Cnab.Domain.Services;

public sealed class CnabParser
{
    public sealed record ParseResult(ImportFile File, List<Transaction> Transactions, List<string> Errors);

    public async Task<ParseResult> ParseAsync(string originalName, Stream stream, CancellationToken ct = default)
    {
        using var sha = SHA256.Create();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        ms.Position = 0;
        var hash = Convert.ToHexString(sha.ComputeHash(ms));
        ms.Position = 0;

        var file = new ImportFile { OriginalName = originalName, Sha256 = hash };
        var txs = new List<Transaction>();
        var errors = new List<string>();

        using var reader = new StreamReader(ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 8192, leaveOpen: true);
        string? line;
        int lineNo = 0;
        while ((line = await reader.ReadLineAsync()) is not null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                if (line.Length == 80)
                {
                    line = line.PadRight(81);
                }

                if (line.Length < 80)
                {
                    throw new FormatException($"Line {lineNo} length {line.Length} < 80");
                }

                var kind = (TransactionKind)int.Parse(line[0..1]); // 1
                var dateStr = line[1..9];                           // 2-9 (YYYYMMDD)
                var valueStr = line[9..19];                         // 10-19
                var cpf = line[19..30].Trim();                      // 20-30
                var card = line[30..42].Trim();                     // 31-42
                var timeStr = line[42..48];                         // 43-48 (HHmmss) UTC-3
                var owner = line[48..62].Trim();                    // 49-62
                var storeName = line[62..81].Trim();                // 63-81

                var local = DateTime.ParseExact(dateStr + timeStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                // Source is UTC-3 per spec; normalize to UTC
                var occurredUtc = TimeZoneInfo.ConvertTimeToUtc(local, TimeZoneInfo.CreateCustomTimeZone("UTC-3", TimeSpan.FromHours(-3), "UTC-3", "UTC-3"));

                var rawCents = long.Parse(valueStr);
                var magnitude = rawCents / 100.00m; // normalize per spec

                var sign = TransactionClassifier.Sign(kind);
                var amount = sign * magnitude;

                var tx = new Transaction
                {
                    Kind = kind,
                    OccurredAtUtc = occurredUtc,
                    Amount = amount,
                    Cpf = cpf,
                    Card = card,
                    Store = new Store { Name = storeName, Owner = owner },
                    SourceLine = lineNo,
                    ImportFile = file
                };
                txs.Add(tx);
            }
            catch (Exception ex)
            {
                errors.Add($"Line {lineNo}: {ex.Message}");
            }
        }

        return new ParseResult(file, txs, errors);
    }
}
