# Cloud Ctrl Kubernetes Metrics Agent

Runs as a pod inside a cluster, running on a schedule. Each run makes several queries to Prometheus and exports the raw metric results to a Cloud Ctrl owned storage account. 

These raw metrics are then processed by the [CloudCtrl.Kubernetes.Functions](../CloudCtrl.Kubernetes.Functions/README.md) functions app.

## Dev Setup
You will need a Kubernetes cluster running. The easiest way to do this is from Azure by running a new Azure Kubernetes Cluster.

The following steps will:
- Set up Prometheus on the cluster
- Configure the metrics agent
- Port forward Prometheus API on localhost
- Run the agent locally to collect metrics from the Prometheus API

### Setup Prometheus
Deploy the [Prometheus Operator Helm chart](https://github.com/prometheus-community/helm-charts/tree/main/charts/kube-prometheus-stack) to your cluster:

Create a `values.yml` file:
```yaml
kube-state-metrics:
  metricLabelsAllowlist:
    - pods=[app],nodes=[kubernetes.azure.com/agentpool,kubernetes.azure.com/cluster,kubernetes.azure.com/nodepool-type]
```

```sh
helm install prometheus prometheus-community/kube-prometheus-stack -f values.yaml
```

If you had already deployed the Prometheus Operator you can upgrade it instead:

```sh
helm upgrade prometheus prometheus-community/kube-prometheus-stack -f values.yaml
```

### Obtain Cloud Ctrl API key
On startup the agent queries the CC API to obtain the SAS token for the storage account where it will upload the metrics. 

Obtain an API key from Cloud Ctrl staging for the following tenant and cloud account:

- Tenant: `2fda838f-be96-4cb1-bef2-7d614cf3915e`
- Cloud Account: `972c85ed-828f-4a56-8152-c34188a44e03`

Then set these values as dotnet `user-secrets`:

```sh
dotnet user-secrets set CloudCtrl:TenantId "2fda838f-be96-4cb1-bef2-7d614cf3915e"
dotnet user-secrets set CloudCtrl:CloudAccountId "972c85ed-828f-4a56-8152-c34188a44e03"
dotnet user-secrets set CloudCtrl:ApiKey "<api_key>"
```

Almost there! Grab the name of your cluster as it appears in the Azure portal and set it as a secret:

```sh
dotnet user-secrets set CloudCtrl:ClusterName "<cluster_name>"
```

### Port forward Prometheus API to localhost
```sh
kubectl port-forward service/prometheus-kube-prometheus-prometheus 9090
```

* The name of the service will depend on what name you gave the Helm install. If you used the name `prometheus` it should match the above. Otherwise run `kubectl get svc -A` to find the name of the service.

While you're at it, port forward Grafana to localhost. This will allow you to view the metrics in the Grafana dashboard.

```sh
kubectl port-forward deployment/prometheus-grafana 3000
```

* Again, the service name may be different for you

You can then access the Grafana dashboard from http://localhost:3000.

Default credentials are
- Username: `admin`
- Password: `prom-operator`

### Run the agent
Now when you run the agent (in your IDE, dotnet run, whatever) it will connect to the Prometheus API on localhost and upload the metrics to the storage account.

## Production deployment
Customers will deploy the agent following our instructions on the [Cloud Ctrl docs | Kubernetes Insights](https://docs.cloudctrl.com.au/Kubernetes/) page.

The agent image is pulled from DockerHub with the name [cloudctrlofficial/metrics-exporter](https://hub.docker.com/r/cloudctrlofficial/metrics-exporter).

### Releasing new agent
The Docker image is built and pushed from this [Azure DevOps release](https://dev.azure.com/cloudctrl/Cloud%20Ctrl/_release?view=all&_a=releases&definitionId=48).
- It runs manually, so you decide when to release a new version by running the release.

### Managing tags on DockerHub
If you are invited to the CloudCtrlOfficial organisation on DockerHub you can manage tags https://hub.docker.com/repository/docker/cloudctrlofficial/metrics-exporter/general.

## Testing agent on a cluster

The app is designed to be deployed to the cluster as a CronJob. You can do this using the [deployment manifest](deploy/deploy-agent.yaml). First replace any `<..>` placeholders in the file with the appropriate values.

Default values for `Prometheus__HostName` and `Prometheus__Port` are provided assuming you've installed Prometheus using the community helm chart suggested above. If you've installed it another way your setup may use a different endpoint for Prometheus and these may need to be changed.

```sh
kubectl apply -f deploy/deploy-agent.yaml
```

You can view the cron job with the command:

```sh
kubectl get cronjob
```

Note the default service discovery for Prometheus assumes the agent and Prometheus run in the same namespace. \
If this is not the case, you will need to update the service discovery config in the [deployment manifest](deploy/deploy-agent.yaml) by appending the namespace to the `Metrics__HostName`. 

E.g. if Prometheus runs in it's own `metrics` namespace, the service discovery config would be:

```yaml
- name: "Metrics__HostName"
  value: "prometheus-kube-prometheus-prometheus.metrics"
```