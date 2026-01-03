// ====================================================================================
// PHASE 1: Event Hubs Consumer - AKS Application
// ====================================================================================
// This is the main entry point for the Consumer application.
// 
// What this does:
// 1. Creates a web application (needed for health checks in Kubernetes)
// 2. Registers the EventHubsProcessor service that reads messages
// 3. Sets up health check endpoints so Kubernetes knows if the app is running
// 4. Starts the application
// ====================================================================================

using EventHubConsumer.Services;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================================
// SERVICE REGISTRATION
// ====================================================================================
// Add the EventHubsProcessor as a background service.
// A "HostedService" runs continuously in the background reading from Event Hubs.
builder.Services.AddHostedService<EventHubsProcessor>();

// ====================================================================================
// HEALTH CHECKS (Required for Kubernetes)
// ====================================================================================
// Kubernetes uses these endpoints to check if the container is:
// - "Alive" (liveness probe): Can the process respond? If not, restart it.
// - "Ready" (readiness probe): Is it ready to handle work? If not, don't send traffic.
builder.Services.AddHealthChecks()
    .AddCheck("liveness", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("App is alive"));

var app = builder.Build();

// ====================================================================================
// CONFIGURE HTTP ENDPOINTS
// ====================================================================================

// Liveness endpoint: Returns 200 OK if the app can respond to requests
app.MapHealthChecks("/health/live");

// Readiness endpoint: Returns 200 OK if the app is ready
app.MapHealthChecks("/health/ready");

// Root endpoint: Simple message to confirm the app is running
app.MapGet("/", () => "Event Hub Consumer is running. Health endpoints: /health/live and /health/ready");

// ====================================================================================
// START THE APPLICATION
// ====================================================================================
Console.WriteLine("=== Phase 1: Azure Event Hubs Consumer ===");
Console.WriteLine("Application started successfully");
Console.WriteLine("Health endpoints available:");
Console.WriteLine("  - Liveness:  http://localhost:8080/health/live");
Console.WriteLine("  - Readiness: http://localhost:8080/health/ready");
Console.WriteLine();
Console.WriteLine("Event Hubs Processor is initializing...");
Console.WriteLine();

// Run the web application (this keeps the app running)
app.Run();
