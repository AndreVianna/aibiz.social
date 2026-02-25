# ── Stage 1: Restore ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /repo

# Copy only project files first so NuGet restore is cached as a layer
COPY source/Directory.Build.props source/
COPY source/AiBiz.Domain/AiBiz.Domain.csproj source/AiBiz.Domain/
COPY source/AiBiz.Infrastructure/AiBiz.Infrastructure.csproj source/AiBiz.Infrastructure/
COPY source/AiBiz.Web/AiBiz.Web.csproj source/AiBiz.Web/
COPY source/AiBiz.Tests/AiBiz.Tests.csproj source/AiBiz.Tests/
COPY source/AiBiz.IntegrationTests/AiBiz.IntegrationTests.csproj source/AiBiz.IntegrationTests/
COPY AiBiz.slnx ./

RUN dotnet restore AiBiz.slnx

# ── Stage 2: Build ────────────────────────────────────────────────────────────
FROM restore AS build
COPY . .
RUN dotnet build AiBiz.slnx --no-restore -c Release

# ── Stage 3: Test (unit tests only; integration tests need a running DB) ──────
FROM build AS test
RUN dotnet test source/AiBiz.Tests/AiBiz.Tests.csproj --no-build -c Release \
    --logger "console;verbosity=normal"

# ── Stage 4: Publish ──────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish source/AiBiz.Web/AiBiz.Web.csproj --no-build -c Release \
    -o /app/publish

# ── Stage 5: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AiBiz.Web.dll"]
