using CloudCtrl.Kubernetes.Agent.Configuration;
using CloudCtrl.Kubernetes.Agent.Helpers;
using CloudCtrl.Kubernetes.Agent.Services;
using Microsoft.Extensions.Configuration;

var config = ConfigurationHelper.GetConfiguration();
var prometheusConfig = config.GetRequiredSection(MetricsConfiguration.SectionName).Get<MetricsConfiguration>();
var cloudCtrlConfig = config.GetRequiredSection(CloudCtrlConfiguration.SectionName).Get<CloudCtrlConfiguration>();
var queries = await QueryFileReader.GetQueries("queries.json");

var apiClient = new HttpClient
{
    BaseAddress = new Uri(cloudCtrlConfig.ApiUrl),
    DefaultRequestHeaders =
    {
        { "Api-Key", cloudCtrlConfig.ApiKey },
        { "X-Tenant-Id", cloudCtrlConfig.TenantId }
    }
};

Console.WriteLine($"Calling CloudCtrl API to get SAS token {cloudCtrlConfig.ApiUrl}");
var response = await apiClient.PostAsync($"/api/k8s/cloudaccounts/{cloudCtrlConfig.CloudAccountId}/agent/{cloudCtrlConfig.ClusterName}", new StringContent(""));
response.EnsureSuccessStatusCode();
var sasToken = await response.Content.ReadAsStringAsync();


var endTime = MetricsReader.GetEndTime(prometheusConfig.Frequency);
var startTime = endTime.AddDays(-1 * prometheusConfig.BackFillDays);

Console.WriteLine("Querying metrics from {0} to {1}", startTime, endTime);

var prometheusReader = new MetricsReader(prometheusConfig);

foreach (var query in queries)
{
    var json = await prometheusReader.GetMetrics(query.Value);
    await BlobStorageWriter.Write(sasToken, json, BlobNamingHelper.GetName(cloudCtrlConfig, query.Key));
}