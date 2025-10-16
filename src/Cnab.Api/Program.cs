using Cnab.Domain.Services;
using Cnab.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Allow large uploads
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; // 1GB
});

// SQLite file DB
builder.Services.AddDbContext<CnabDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("Default")
             ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
             ?? "Data Source=./data/cnab.db";
    o.UseSqlite(cs);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CnabParser>();

var app = builder.Build();

Directory.CreateDirectory("./data");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CnabDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/api/import",
[Consumes("multipart/form-data")]
async (IFormFile file, CnabParser parser, CnabDbContext db, CancellationToken ct) =>
{
    if (file is null || file.Length == 0)
        return Results.BadRequest("file is required");

    await using var s = file.OpenReadStream();
    var parsed = await parser.ParseAsync(file.FileName, s, ct);

    var exists = await db.ImportFiles.AnyAsync(x => x.Sha256 == parsed.File.Sha256, ct);
    if (exists) return Results.Conflict(new { message = "File already imported" });

    var cache = new Dictionary<(string owner, string name), Cnab.Domain.Entities.Store>();
    foreach (var tx in parsed.Transactions)
    {
        var key = (tx.Store.Owner, tx.Store.Name);
        if (!cache.TryGetValue(key, out var store))
        {
            store = await db.Stores.FirstOrDefaultAsync(s => s.Owner == key.Owner && s.Name == key.Name, ct)
                 ?? new Cnab.Domain.Entities.Store { Owner = key.Owner, Name = key.Name };
            cache[key] = store;
        }
        tx.Store = store;
        db.Transactions.Add(tx);
    }

    db.ImportFiles.Add(parsed.File);
    await db.SaveChangesAsync(ct);
    return Results.Ok(new { imported = parsed.Transactions.Count, errors = parsed.Errors });
})
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict)
.DisableAntiforgery();
// Stores list
app.MapGet("/api/stores", async (CnabDbContext db, CancellationToken ct) =>
{
    var rows = await db.Stores
        .Select(s => new
        {
            s.Id,
            s.Owner,
            s.Name,
            Balance = s.Transactions.Sum(t => t.Amount),
            Transactions = s.Transactions.Count
        })
        .OrderBy(s => s.Name)
        .ToListAsync(ct);

    return Results.Ok(rows);
});

// Store transactions
app.MapGet("/api/stores/{id:int}/transactions", async (int id, CnabDbContext db, CancellationToken ct) =>
{
    var store = await db.Stores.FindAsync(new object[] { id }, ct);
    if (store is null) return Results.NotFound();

    var txs = await db.Transactions
        .Where(t => t.StoreId == id)
        .OrderByDescending(t => t.OccurredAtUtc)
        .Select(t => new
        {
            t.Id,
            t.Kind,
            t.OccurredAtUtc,
            t.Amount,
            t.Cpf,
            t.Card
        })
        .ToListAsync(ct);

    return Results.Ok(new { Store = new { store.Id, store.Owner, store.Name }, Transactions = txs });
});

app.Run();
