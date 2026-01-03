# ====================================================================================
# CONSUMER DOCKERFILE - Multi-Stage Build
# ====================================================================================
# This Dockerfile creates a containerized version of the Consumer application.
# 
# MULTI-STAGE BUILD:
# - Stage 1 (build): Compiles the .NET application
# - Stage 2 (runtime): Creates a minimal container with only the compiled app
# 
# Benefits:
# - Smaller final image (no SDK, only runtime)
# - Faster deployment to Kubernetes
# - More secure (fewer tools in production image)
#
# HOW TO BUILD (from project root):
# docker build -t eventhub-consumer:v1 -f src/Consumer/Dockerfile .
# ====================================================================================

# ====================================================================================
# STAGE 1: BUILD
# ====================================================================================
# Use .NET SDK image to compile the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore NuGet packages
# (Doing this first allows Docker to cache the packages layer)
COPY src/Consumer/Consumer.csproj ./Consumer/
RUN dotnet restore ./Consumer/Consumer.csproj

# Copy source code and build the application
COPY src/Consumer/ ./Consumer/
RUN dotnet build ./Consumer/Consumer.csproj -c Release -o /app/build

# Publish the application (creates deployment-ready files)
RUN dotnet publish ./Consumer/Consumer.csproj -c Release -o /app/publish

# ====================================================================================
# STAGE 2: RUNTIME
# ====================================================================================
# Use .NET Runtime image (much smaller than SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
# Running as non-root is a Kubernetes best practice
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose port 8080 (used for HTTP and health checks)
EXPOSE 8080

# Health check (Docker will periodically check if the container is healthy)
# This is separate from Kubernetes health checks but provides similar functionality
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health/live || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Consumer.dll"]
