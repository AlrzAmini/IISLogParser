namespace IISLogParser.Api;

public interface ILogAnalyzerService
{
    Task<List<IISLogEntry>> LoadLogsAsync(string folderPath);
    object GetStatusCodeStats(IEnumerable<IISLogEntry> logs);
    IEnumerable<IISLogEntry> GetTopSlowRequests(IEnumerable<IISLogEntry> logs, int count);
    object GetTopUrls(IEnumerable<IISLogEntry> logs, int count);
}