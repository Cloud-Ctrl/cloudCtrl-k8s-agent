namespace CloudCtrl.Kubernetes.Agent.Configuration;

public sealed class CloudCtrlConfiguration
{
    public const string SectionName = "CloudCtrl";

    public string ClusterName { get; set; } = "";
    
    public string TenantId { get; set; } = "";
    public string CloudAccountId { get; set; } = "";
    public string ApiUrl { get; set; } = "http://localhost:5000";
    public string ApiKey { get; set; } = "";
}
