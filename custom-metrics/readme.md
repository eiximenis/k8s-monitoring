# HPA with Prometheus custom metrics

> This section requires a Prometheus and Hello API installed in the cluster. Please refer to [Prometheus setup](../prometheus/readme.md) to install it.

## Install Prometheus adapter

In order to being able to use HPA with custom metrics served by Prometheus, we need a way to serve the metrics handled by Prometheus in the format mandated by the Kubernetes custom metrics API. We need tp install the [prometheus-adapter](https://github.com/kubernetes-sigs/prometheus-adapter) for this, using Helm:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm install prometheus-adapter prometheus-community/prometheus-adapter -n monitoring --set prometheus.url=http://prometheus-operated
```

## Viewing metrics from _Hello API_

> This section assumes that _Hello API_ is installed and its being monitored by Prometheus.

> This section has to be performed from `/custom-metrics/jobs`

Ww need access to Prometheus, so open one terminal and type `kubectl port-forward svc/prometheus-operated -n monitoring 9090:9090`. Now we have Prometheus available on `localhost:9090`.

Now, generate some traffic to _Hello API_ by deploying the job `job-get-hello-api.yaml` in the cluster. This will perform some requests to _Hello API_. Wait until job is finished, and then let's query Prometheus the number of http_requests received by our pods:

```
curl -s http://localhost:9090/api/v1/query?query=http_requests_received_total | jq '.data.result[] | {pod: .metric.pod, value: .value[1]}'
```

This command invokes the Prometheus API by requesting the values of metric `http_requests_received_total` and shows the pod name and the metric value. As we had run the job, we should have some values here.

Now we can see the values that prometheus-adapter exposes using the kubernetes API. For that we can use `kubectl`:

```
kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1 | jq | grep http_requests_received_total
```

The result is empty which means, that **no http_requests_received_total metric is exposed**. However a metric called `http_requests_received` is exposed:

```
>kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1 | jq | grep http_requests_received
      "name": "namespaces/http_requests_received",
      "name": "pods/http_requests_received",
      "name": "services/http_requests_received",
      "name": "jobs.batch/http_requests_received",
```

But Prometheus do not have any metric named `http_requests_received`, so what is happening here?

The key point is that **prometheus-exporter performs some modifications in the metrics exposed by Prometheus**. Those modifications are stored in the config map `prometheus-adapter` (in `monitoring` namespace), and one of thism rules is that each metric ending in `_total` will be aggregated over 5 minutes values.

Let's view it. First we will aggregate the metric on Prometheus:

```
curl -s 'http://localhost:9090/api/v1/query?query=rate(http_requests_received_total[5m])' | jq '.data.result[] | {pod: .metric.pod, value: .value[1]}'
```

Now will compare this value with the value of the metric `http_requests_received` for the all pods in the namespace `hellomonitor`:

```
 kubectl get --raw /apis/custom.metrics.k8s.io/v1beta1/namespaces/hellomonitor/pods/*/http_requests_received | jq '.items[] | {pod: .describedObject.name, value: .value}'
```

As you can see both values are the same (note that the value in prometheus-adapter is given using mili units, so a value of 1234 in Prometheus will be exported as 1.234m).

>**Note** The values can differ if various HTTP Status codes are involved (Prometheus returns an entry for each http status code, while prometheus-adapter adds them all together).

## Create an HPA

>This section has to be performed from `/custom-metrics`

Now we have a metric in the metric server that we can use on our HPA. It is time to dinamically scale the workload based on this metric. The file `hpa-helloapi.yaml` contains an HPA definition. So apply it to the cluster:

```
kubectl apply -f .\hpa-helloapi.yaml -n hellomonitor
```

Now add some load to the pod and let's see how it scales. To add some "heavy" load, just deploy the file `./jobs/job-stress-hello-api.yaml` :)