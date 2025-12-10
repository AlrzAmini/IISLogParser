namespace IISLogParser.Api;

public interface ILogAnalyzerService
{
    Task<List<IISLogEntry>> LoadLogsAsync(string folder);
    object GetStatusCodeStats(IEnumerable<IISLogEntry> logs);
    IEnumerable<IISLogEntry> GetTopSlowRequests(IEnumerable<IISLogEntry> logs, int top);
    object GetTopUrls(IEnumerable<IISLogEntry> logs, int top);

    // NEW CPU & TRAFFIC MONITORING
    object GetRequestsPerSecond(IEnumerable<IISLogEntry> logs);
    object GetHighTrafficIps(IEnumerable<IISLogEntry> logs, int top);
    object GetExpensiveEndpoints(IEnumerable<IISLogEntry> logs, int top);
    object GetErrorRateStats(IEnumerable<IISLogEntry> logs);
    IEnumerable<IISLogEntry> GetRecentLongRequests(IEnumerable<IISLogEntry> logs, int lastMinutes, int top);

    List<IpTrafficDto> GetTopIps(List<IISLogEntry> logs, int top);
    CpuSpikeAnalysisDto AnalyzeCpuSpike(List<IISLogEntry> logs, DateTime from, DateTime to);
    List<TimeCountDto> GetRequestsPerSecond(List<IISLogEntry> logs);
    List<TimeCountDto> GetRequestsPerMinute(List<IISLogEntry> logs);
    Dictionary<int, List<TimeCountDto>> GetStatusCodeTimeline(List<IISLogEntry> logs);
    List<HeatmapEntryDto> GetResponseTimeHeatmap(List<IISLogEntry> logs);
    List<IpTrafficDto> DetectBots(List<IISLogEntry> logs);
}
