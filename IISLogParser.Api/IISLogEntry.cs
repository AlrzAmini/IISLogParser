namespace IISLogParser.Api
{
    public sealed class IISLogEntry
    {
        public DateTime Date { get; set; }
        public string UriStem { get; set; }
        public string Ip { get; set; }
        public string UserAgent { get; set; }
        public int Status { get; set; }
        public double TimeTakenMs { get; set; }
    }
}
