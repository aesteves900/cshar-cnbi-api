# CNAB Importer (.NET 9 + MySQL)

Upload and parse CNAB fixed-width files, persist transactions, and expose a simple report per store with balances.

## Run with Docker

```bash
docker compose up --build
```
Swagger: http://localhost:8080/swagger

## Local run

```bash
dotnet restore && dotnet build
ASPNETCORE_URLS=http://localhost:8080 \
ConnectionStrings__Default="Server=localhost;Port=3306;Database=cnab;User=cnab;Password=cnab" \
dotnet run --project src/Cnab.Api
```

## API
- `POST /api/import` → multipart/form-data with a single file
- `GET /api/stores` → list stores with balances
- `GET /api/stores/{id}/transactions` → store transactions