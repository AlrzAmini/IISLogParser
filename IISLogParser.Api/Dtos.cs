namespace IISLogParser.Api
{
    public record IpTrafficDto(string Ip, int Count, int ErrorCount);

    public record TimeCountDto(DateTime Time, int Count);

    public record HeatmapEntryDto(int Hour, double AverageTimeMs);

    public record UrlCountDto(string Url, int Count, double? AvgTimeMs = null);

    public record CpuSpikeAnalysisDto(
        List<UrlCountDto> TopUrls,
        List<IpTrafficDto> TopIps,
        List<IISLogEntry> SlowRequests,
        int TotalRequests
    );

}
