# ====================================================================================
# CONSUMER DOCKERFILE - Phase 1
# ====================================================================================
# Multi-stage build for a simple Event Hubs consumer console app.
# No web server needed - just logs messages to stdout.
# ====================================================================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY src/Consumer/Consumer.csproj ./Consumer/
RUN dotnet restore ./Consumer/Consumer.csproj

# Copy source and build
COPY src/Consumer/ ./Consumer/
RUN dotnet build ./Consumer/Consumer.csproj -c Release -o /app/build
RUN dotnet publish ./Consumer/Consumer.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Copy published app
COPY --from=build /app/publish .

# Run the console application
ENTRYPOINT ["dotnet", "Consumer.dll"]

