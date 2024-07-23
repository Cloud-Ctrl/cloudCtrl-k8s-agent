using CloudCtrl.Kubernetes.Agent.Configuration;
using CloudCtrl.Kubernetes.Agent.Helpers;

namespace CloudCtrl.Kubernetes.Agent.Services;

public sealed class MetricsReader
{
    // Represents the time between data points in the query (i.e. the granularity).
    // This needs to match the time interval used in all rate queries, e.g.
    // "rate(container_cpu_usage_seconds_total[24h])"
    private const int StepsInSeconds = 86400;

    private readonly MetricsConfiguration _metricsConfig;

    public MetricsReader(MetricsConfiguration metricsConfig)
    {
        _metricsConfig = metricsConfig;
    }

    public async Task<string> GetMetrics(string query)
    {
        var endTime = GetEndTime(_metricsConfig.Frequency);
        var startTime = endTime.AddDays(-1 * _metricsConfig.BackFillDays);

        using var httpClient = new HttpClient();
        var queryParams = new KeyValuePair<string, string>[]
        {
            new("query", query),
            new("start", startTime.ToString("o")),
            new("end", endTime.ToString("o")),
            new("step", StepsInSeconds.ToString())
        };
        var url = $"http://{_metricsConfig.Host}:{_metricsConfig.Port}/api/v1/query_range";
        using var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(queryParams));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public static DateTime GetEndTime(string frequency)
    {
        if (frequency.Equals("hourly", StringComparison.OrdinalIgnoreCase))
        {
            return DateTime.UtcNow.RoundToNearestInterval(TimeSpan.FromHours(1));
        }

        // Assume daily
        return DateTime.UtcNow.Date;
    }
}