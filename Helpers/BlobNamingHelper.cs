using CloudCtrl.Kubernetes.Agent.Configuration;

namespace CloudCtrl.Kubernetes.Agent.Helpers;

public static class BlobNamingHelper
{
    public static string GetName(CloudCtrlConfiguration config, string queryName)
    {
        var utcNow = DateTime.UtcNow;
        return $"{utcNow:yyyy-MM-dd}/{config.ClusterName}/{queryName}.json";
    }
}