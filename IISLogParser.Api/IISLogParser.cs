namespace IISLogParser.Api
{
    public static class IISLogParser
    {
        public static IEnumerable<IISLogEntry> Parse(string filePath)
        {
            foreach (var line in File.ReadLines(filePath))
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 10)
                    continue;

                yield return new IISLogEntry
                {
                    Date = DateTime.Parse($"{parts[0]} {parts[1]}"),
                    Ip = parts[2],
                    HttpMethod = parts[3],
                    UriStem = parts[4],
                    Status = int.Parse(parts[5]),
                    TimeTakenMs = long.Parse(parts[^1])
                };
            }
        }
    }
}
