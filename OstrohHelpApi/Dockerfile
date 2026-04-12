# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution and project files
COPY OstrohHelpApi.sln .
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj src/Api/
COPY tests/Tests.Common/Tests.Common.csproj tests/Tests.Common/
COPY tests/Tests.Data/Tests.Data.csproj tests/Tests.Data/
COPY tests/Api.Tests.Integration/Api.Tests.Integration.csproj tests/Api.Tests.Integration/

# Restore NuGet packages
RUN dotnet restore OstrohHelpApi.sln

# Copy source code
COPY . .

# Build the solution
RUN dotnet build OstrohHelpApi.sln -c Release --no-restore

# Publish API
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish --no-restore --no-build

# Runtime stage for API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api-runtime

WORKDIR /app

COPY --from=builder /app/publish .

EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Api.dll"]

# Test stage
FROM builder AS test-runtime

WORKDIR /src

# Run tests
RUN dotnet test tests/Tests.Common/Tests.Common.csproj \
    --logger "console;verbosity=normal" \
    --logger "trx;LogFileName=/testresults/test-results.trx" \
    --results-directory /testresults \
    -c Release

# Dummy runtime for test output
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS test-final

WORKDIR /testresults
COPY --from=test-runtime /testresults /testresults

ENTRYPOINT ["cat", "/testresults/test-results.trx"]
