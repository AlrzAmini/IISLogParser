using System.Globalization;

namespace IISLogParser.Api;

public class LogAnalyzerService : ILogAnalyzerService
{
    // ============================================
    // LOAD LOGS
    // ============================================
    public async Task<List<IISLogEntry>> LoadLogsAsync(string folder)
    {
        var logs = new List<IISLogEntry>();

        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"Log folder not found: {folder}");
        }

        var files = Directory.GetFiles(folder, "*.log", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var lines = await File.ReadAllLinesAsync(file);
            logs.AddRange(ParseIisLines(lines));
        }

        return logs;
    }

    private static IEnumerable<IISLogEntry> ParseIisLines(string[] lines)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith("#")) continue;

            var cols = line.Split(' ');
            if (cols.Length < 10) continue;

            yield return new IISLogEntry
            {
                Date = DateTime.Parse($"{cols[0]} {cols[1]}", CultureInfo.InvariantCulture),
                Ip = cols[2],
                UriStem = cols[4],
                Status = int.Parse(cols[6]),
                TimeTakenMs = double.Parse(cols[9]),
                UserAgent = cols.Length > 10 ? string.Join(" ", cols.Skip(10)) : ""
            };
        }
    }

    // ============================================
    // BASIC STATS
    // ============================================
    public object GetStatusCodeStats(IEnumerable<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => x.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    public IEnumerable<IISLogEntry> GetTopSlowRequests(IEnumerable<IISLogEntry> logs, int top)
    {
        return logs
            .OrderByDescending(x => x.TimeTakenMs)
            .Take(top)
            .ToList();
    }

    public object GetTopUrls(IEnumerable<IISLogEntry> logs, int top)
    {
        return logs
            .GroupBy(x => x.UriStem)
            .Select(g => new UrlCountDto(
                Url: g.Key,
                Count: g.Count(),
                AvgTimeMs: g.Average(x => x.TimeTakenMs)
            ))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    // ============================================
    // NEW BASIC MONITORING
    // ============================================
    public object GetRequestsPerSecond(IEnumerable<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => new DateTime(
                x.Date.Year,
                x.Date.Month,
                x.Date.Day,
                x.Date.Hour,
                x.Date.Minute,
                x.Date.Second))
            .Select(g => new { Time = g.Key, Count = g.Count() })
            .OrderBy(x => x.Time)
            .ToList();
    }

    public object GetHighTrafficIps(IEnumerable<IISLogEntry> logs, int top)
    {
        return logs
            .GroupBy(x => x.Ip)
            .Select(g => new
            {
                Ip = g.Key,
                Count = g.Count(),
                Errors = g.Count(x => x.Status >= 400)
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    public object GetExpensiveEndpoints(IEnumerable<IISLogEntry> logs, int top)
    {
        return logs
            .GroupBy(x => x.UriStem)
            .Select(g => new
            {
                Url = g.Key,
                AvgTimeMs = g.Average(x => x.TimeTakenMs),
                Count = g.Count()
            })
            .OrderByDescending(x => x.AvgTimeMs)
            .Take(top)
            .ToList();
    }

    public object GetErrorRateStats(IEnumerable<IISLogEntry> logs)
    {
        var total = logs.Count();
        var errors = logs.Count(x => x.Status >= 500);
        var clientErrors = logs.Count(x => x.Status >= 400 && x.Status < 500);

        return new
        {
            Total = total,
            Errors = errors,
            ClientErrors = clientErrors,
            ErrorRate = total == 0 ? 0 : (errors / (double)total) * 100,
            ClientErrorRate = total == 0 ? 0 : (clientErrors / (double)total) * 100
        };
    }

    public IEnumerable<IISLogEntry> GetRecentLongRequests(IEnumerable<IISLogEntry> logs, int lastMinutes, int top)
    {
        var since = DateTime.UtcNow.AddMinutes(-lastMinutes);

        return logs
            .Where(x => x.Date >= since)
            .OrderByDescending(x => x.TimeTakenMs)
            .Take(top)
            .ToList();
    }

    // ============================================
    // ADVANCED MONITORING
    // ============================================
    public List<IpTrafficDto> GetTopIps(List<IISLogEntry> logs, int top)
    {
        return logs
            .GroupBy(x => x.Ip)
            .Select(g => new IpTrafficDto(
                Ip: g.Key,
                Count: g.Count(),
                ErrorCount: g.Count(x => x.Status >= 400)
            ))
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    public CpuSpikeAnalysisDto AnalyzeCpuSpike(List<IISLogEntry> logs, DateTime from, DateTime to)
    {
        var window = logs
            .Where(x => x.Date >= from && x.Date <= to)
            .ToList();

        return new CpuSpikeAnalysisDto(
            TopUrls: GetTopUrls(window, 10) as List<UrlCountDto>,
            TopIps: GetTopIps(window, 10),
            SlowRequests: GetTopSlowRequests(window, 20).ToList(),
            TotalRequests: window.Count
        );
    }

    // ============================================
    // RPS / RPM
    // ============================================
    public List<TimeCountDto> GetRequestsPerSecond(List<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => new DateTime(
                x.Date.Year, x.Date.Month, x.Date.Day,
                x.Date.Hour, x.Date.Minute, x.Date.Second))
            .Select(g => new TimeCountDto(g.Key, g.Count()))
            .OrderBy(x => x.Time)
            .ToList();
    }

    public List<TimeCountDto> GetRequestsPerMinute(List<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => new DateTime(
                x.Date.Year, x.Date.Month, x.Date.Day,
                x.Date.Hour, x.Date.Minute, 0))
            .Select(g => new TimeCountDto(g.Key, g.Count()))
            .OrderBy(x => x.Time)
            .ToList();
    }

    // ============================================
    // STATUS CODES OVER TIME
    // ============================================
    public Dictionary<int, List<TimeCountDto>> GetStatusCodeTimeline(List<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => x.Status)
            .ToDictionary(
                g => g.Key,
                g => g.GroupBy(x =>
                        new DateTime(
                            x.Date.Year, x.Date.Month, x.Date.Day,
                            x.Date.Hour, x.Date.Minute, 0))
                    .Select(x => new TimeCountDto(x.Key, x.Count()))
                    .OrderBy(x => x.Time)
                    .ToList());
    }

    // ============================================
    // HOURLY HEATMAP
    // ============================================
    public List<HeatmapEntryDto> GetResponseTimeHeatmap(List<IISLogEntry> logs)
    {
        return logs
            .GroupBy(x => x.Date.Hour)
            .Select(g => new HeatmapEntryDto(
                Hour: g.Key,
                AverageTimeMs: g.Average(x => x.TimeTakenMs)
            ))
            .OrderBy(x => x.Hour)
            .ToList();
    }

    // ============================================
    // BOT DETECTION
    // ============================================
    public List<IpTrafficDto> DetectBots(List<IISLogEntry> logs)
    {
        return logs
            .Where(x =>
                x.UserAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
                x.UserAgent.Contains("crawler", StringComparison.OrdinalIgnoreCase) ||
                x.UserAgent.Contains("spider", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.Ip)
            .Select(g => new IpTrafficDto(
                Ip: g.Key,
                Count: g.Count(),
                ErrorCount: g.Count(x => x.Status >= 400)
            ))
            .OrderByDescending(x => x.Count)
            .ToList();
    }
}