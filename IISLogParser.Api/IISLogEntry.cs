namespace IISLogParser.Api
{
    public sealed class IISLogEntry
    {
        public DateTime Date { get; set; }
        public string Ip { get; set; }
        public string HttpMethod { get; set; }
        public string UriStem { get; set; }
        public int Status { get; set; }
        public long TimeTakenMs { get; set; }
    }
}
