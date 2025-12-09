using IISLogParser.Api;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
builder.Services.AddEndpointsApiExplorer(); // needed for OpenAPI

var app = builder.Build();

const string LogPath = @"C:\inetpub\logs\LogFiles\W3SVC1";

// -----------------------
//      SCALAR UI
// -----------------------
app.MapScalarApiReference(options =>
{
    options.Title = "IIS Log Analyzer API";
    options.DarkMode = true;
});

// -----------------------
//      ENDPOINTS
// -----------------------

app.MapGet("/logs/scan", async (ILogAnalyzerService svc) =>
    {
        var logs = await svc.LoadLogsAsync(LogPath);
        return Results.Ok(new { Total = logs.Count });
    })
    .WithName("ScanLogs")
    .WithDescription("Scans the IIS log folder and returns the total number of parsed logs.");

app.MapGet("/logs/status-codes", async (ILogAnalyzerService svc) =>
    {
        var logs = await svc.LoadLogsAsync(LogPath);
        return Results.Ok(svc.GetStatusCodeStats(logs));
    })
    .WithName("StatusCodesStats");

app.MapGet("/logs/slow", async (ILogAnalyzerService svc, int top = 20) =>
    {
        var logs = await svc.LoadLogsAsync(LogPath);
        return Results.Ok(svc.GetTopSlowRequests(logs, top));
    })
    .WithName("SlowestRequests")
    .WithDescription("Returns top N slowest requests.");

app.MapGet("/logs/top-urls", async (ILogAnalyzerService svc, int top = 20) =>
    {
        var logs = await svc.LoadLogsAsync(LogPath);
        return Results.Ok(svc.GetTopUrls(logs, top));
    })
    .WithName("TopRequestedUrls");

app.Run();