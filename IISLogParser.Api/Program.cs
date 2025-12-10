using IISLogParser.Api;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------
//      SERVICES
// -----------------------
builder.Services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(); // Required for Scalar

var app = builder.Build();

const string logPath = @"C:\inetpub\logs\LogFiles\Bck";

// -----------------------
//      SCALAR UI
// -----------------------
app.MapScalarApiReference(options =>
{
    options.Title = "IIS Log Analyzer API";
    options.DarkMode = true;
});

// Required for Scalar to read the spec
app.MapOpenApi();

app.MapGet("/hello", async (ILogAnalyzerService svc) => Results.Ok(new { Message = "Hello" }))
    .WithName("Hello");


// ======================================================================
// SECTION: BASIC LOG SCAN
// ======================================================================
app.MapGet("/logs/scan", async (ILogAnalyzerService svc) =>
{
    try
    {
        var logs = await LoadLogsAsync(svc);
        return Results.Ok(new { Total = logs.Count });
    }
    catch (Exception e)
    {
        return Results.InternalServerError(new { Message = e.Message });
    }
})
.WithName("ScanLogs")
.WithDescription("Scans the IIS log folder and returns the total number of parsed logs.");


// ======================================================================
// SECTION: GENERAL ANALYTICS
// ======================================================================
app.MapGet("/logs/status-codes", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetStatusCodeStats(logs));
})
.WithName("StatusCodeStats");

app.MapGet("/logs/slow", async (ILogAnalyzerService svc, int top = 20) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetTopSlowRequests(logs, top));
})
.WithName("SlowestRequests");

app.MapGet("/logs/top-urls", async (ILogAnalyzerService svc, int top = 20) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetTopUrls(logs, top));
})
.WithName("TopRequestedUrls");


// ======================================================================
// SECTION: IP TRAFFIC
// ======================================================================
app.MapGet("/logs/top-ips", async (ILogAnalyzerService svc, int top = 20) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetTopIps(logs, top));
})
.WithName("TopIps");

app.MapGet("/logs/high-traffic-ips", async (
    ILogAnalyzerService svc,
    int top = 10) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetHighTrafficIps(logs, top));
})
.WithName("HighTrafficIps");


// ======================================================================
// SECTION: CPU SPIKE CORRELATION
// ======================================================================
app.MapGet("/logs/analyze-cpu-spike", async (
    ILogAnalyzerService svc,
    DateTime from,
    DateTime to) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.AnalyzeCpuSpike(logs, from, to));
})
.WithName("AnalyzeCpuSpike");


// ======================================================================
// 🟦 SECTION: TRAFFIC RATE (RPS / RPM)
// ======================================================================
app.MapGet("/logs/rps", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetRequestsPerSecond(logs));
})
.WithName("RequestsPerSecond");

app.MapGet("/logs/rpm", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetRequestsPerMinute(logs));
})
.WithName("RequestsPerMinute");

app.MapGet("/logs/rps/raw", async (ILogAnalyzerService svc) =>
    {
        var logs = await LoadLogsAsync(svc);
        return Results.Ok(svc.GetRequestsPerSecond(logs));
    })
    .WithName("RequestsPerSecondRaw")
    .WithDescription("Returns raw RPS calculated using the IEnumerable<object> version.");


// ======================================================================
// SECTION: STATUS CODE TIMELINE
// ======================================================================
app.MapGet("/logs/status-codes/timeline", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetStatusCodeTimeline(logs));
})
.WithName("StatusCodeTimeline");


// ======================================================================
// SECTION: RESPONSE TIME & HEATMAP
// ======================================================================
app.MapGet("/logs/response-time-heatmap", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetResponseTimeHeatmap(logs));
})
.WithName("ResponseTimeHeatmap");


// ======================================================================
// SECTION: BOT DETECTION
// ======================================================================
app.MapGet("/logs/bots", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.DetectBots(logs));
})
.WithName("Bots");


// ======================================================================
// SECTION: ADVANCED PERF ANALYSIS
// ======================================================================
app.MapGet("/logs/expensive-endpoints", async (
    ILogAnalyzerService svc,
    int top = 10) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetExpensiveEndpoints(logs, top));
})
.WithName("ExpensiveEndpoints");

app.MapGet("/logs/error-rate", async (ILogAnalyzerService svc) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetErrorRateStats(logs));
})
.WithName("ErrorRateStats");

app.MapGet("/logs/recent-long-requests", async (
    ILogAnalyzerService svc,
    int lastMinutes = 10,
    int top = 20) =>
{
    var logs = await LoadLogsAsync(svc);
    return Results.Ok(svc.GetRecentLongRequests(logs, lastMinutes, top));
})
.WithName("RecentLongRequests");


// -----------------------
//      RUN
// -----------------------
app.Run();
return;

// Small helper for consistency
static async Task<List<IISLogEntry>> LoadLogsAsync(ILogAnalyzerService svc) =>
    await svc.LoadLogsAsync(logPath);
