using System.Text.Json;

namespace CloudCtrl.Kubernetes.Agent.Helpers;

public static class QueryFileReader
{
    public static async Task<IDictionary<string, string>> GetQueries(string jsonFilePath)
    {
        var json = await File.ReadAllTextAsync(jsonFilePath);

        return JsonSerializer.Deserialize<IDictionary<string, string>>(json)!;
    }
}