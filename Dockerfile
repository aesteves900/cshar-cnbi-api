# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ./src/Cnab.Domain/Cnab.Domain.csproj src/Cnab.Domain/
COPY ./src/Cnab.Infrastructure/Cnab.Infrastructure.csproj src/Cnab.Infrastructure/
COPY ./src/Cnab.Api/Cnab.Api.csproj src/Cnab.Api/
RUN dotnet restore src/Cnab.Api/Cnab.Api.csproj

COPY ./src/Cnab.Domain/ src/Cnab.Domain/
COPY ./src/Cnab.Infrastructure/ src/Cnab.Infrastructure/
COPY ./src/Cnab.Api/ src/Cnab.Api/
RUN dotnet publish src/Cnab.Api/Cnab.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Cnab.Api.dll"]