namespace IISLogParser.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddLogAnalyzer(this IServiceCollection services)
        {
            services.AddSingleton<ILogAnalyzerService, LogAnalyzerService>();
            return services;
        }
    }
}
