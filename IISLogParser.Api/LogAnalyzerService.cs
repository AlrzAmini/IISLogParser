namespace IISLogParser.Api
{
    public sealed class LogAnalyzerService : ILogAnalyzerService
    {
        public async Task<List<IISLogEntry>> LoadLogsAsync(string folderPath)
        {
            var result = new List<IISLogEntry>();
            var files = Directory.GetFiles(folderPath, "*.log");

            foreach (var file in files)
            {
                var logs = IISLogParser.Parse(file);
                result.AddRange(logs);
            }

            return await Task.FromResult(result);
        }

        public object GetStatusCodeStats(IEnumerable<IISLogEntry> logs)
        {
            return logs
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
        }

        public IEnumerable<IISLogEntry> GetTopSlowRequests(IEnumerable<IISLogEntry> logs, int count)
        {
            return logs.OrderByDescending(x => x.TimeTakenMs).Take(count);
        }

        public object GetTopUrls(IEnumerable<IISLogEntry> logs, int count)
        {
            return logs.GroupBy(x => x.UriStem)
                .Select(g => new { Url = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(count);
        }
    }
}
