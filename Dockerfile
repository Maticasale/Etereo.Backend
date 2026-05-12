# ── Stage 1: Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Etereo.sln .
COPY src/Etereo.Api/Etereo.Api.csproj             src/Etereo.Api/
COPY src/Etereo.Application/Etereo.Application.csproj   src/Etereo.Application/
COPY src/Etereo.Domain/Etereo.Domain.csproj         src/Etereo.Domain/
COPY src/Etereo.Infrastructure/Etereo.Infrastructure.csproj src/Etereo.Infrastructure/
COPY src/Etereo.Shared/Etereo.Shared.csproj         src/Etereo.Shared/

RUN dotnet restore

COPY . .

RUN dotnet publish src/Etereo.Api/Etereo.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Etereo.Api.dll"]
