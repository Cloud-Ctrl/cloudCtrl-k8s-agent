using Microsoft.Extensions.Configuration;

namespace CloudCtrl.Kubernetes.Agent.Helpers;

public static class ConfigurationHelper
{
    public static IConfiguration GetConfiguration()
    {
        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true);

        if (env is not null)
        {
            config = config.AddJsonFile($"appsettings.{env}.json", true);
        }

        return config
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}