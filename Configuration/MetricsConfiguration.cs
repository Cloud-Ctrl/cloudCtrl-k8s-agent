namespace CloudCtrl.Kubernetes.Agent.Configuration;

public sealed class MetricsConfiguration
{
    public const string SectionName = "Metrics";

    public string Host { get; set; } = "prometheus-kube-prometheus-prometheus.default.svc.cluster.local";

    public int Port { get; set; } = 9090;

    /// <summary>
    /// The number of days worth of data to query. Represents the timeframe
    /// of the query.
    /// </summary>
    public int BackFillDays { get; init; }

    /// <summary>
    /// How often the query should be executed. Expected values: Hourly, Daily
    /// </summary>
    public string Frequency { get; init; } = null!;
}