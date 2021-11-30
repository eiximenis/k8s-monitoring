# Prometheus monitoring using asp.net core

## Testing using Docker

> All commands from this section have to be run from `src` folder

In this scenario everything runs using docker:

* PostgreSQL Database
* Demo API

1. Build the _Hello API_ docker image: `docker compose build helloapi`
3. Start pgsql docker container: `docker compose up -d pgsql` 
2. Database needs to be seeded before api to run. To seed the database just type `docker compose --profile seed run hello-seeder`, then wait for the container to exit.
3. Run the system using `docker compose --profile api up`
4. Navigate 
    * To `http://localhost:5200/forecasts`. Sample json containing forecasts data should appear.
    * To `http://localhost:5200/metrics`. Prometheus metrics should appear, including HTTP requests data

## Deploy on k8s

### Deploy Prometheus using Helm

Use following commands to deploy prometheus using helm:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm install prometheus prometheus-community/prometheus -n prometheus --create-namespace
```

### Deploy the pgsql to Kubernetes

> This section must be performed from `/prometheus/k8s/pgsql` folder

1. Run `kubectl apply -f ./01-hello-ns.yaml` to create the namespace
2. Run `kubectl apply -k ./pgsql/` to deploy the postgtesql in the `hellomonitor` namespace

### Deploy the _Hello API_ to Kubernetes

> This section must be performed from  `/prometheus/k8s/hello-api/base` folder

1. Docker tag and push the `helloapi` image to a docker registry.
2. Go to `k8s/hello-api` folder and:
    * Edit the file `kustomization.yaml` and **uncomment** the line `# newName: Put your image name here` and use your image name
    * Run `kubecl apply -k .`

> Note: If you have `kustomize` installed you can run: `kustomize edit set image hellomonitor=<your-image-name>` instead of updating the `kustomization.yaml` manually

## Configure Prometheus

Edit the configmap `prometheus-server` on namespace `prometheus` to add a new job to scrape `hellomonitor.hellomonitor` service:

```yaml
    - job_name: hellomonitor
      static_configs:
      - targets: ['hellomonitor.hellomonitor.svc.cluster.local:80]
```

However **this is NOT a good configuration** because we are scrapping at service level, which is not correct if multiple replicas are enabled. We need a way to instruct Prometheus to "get all pods under a service and add them all" as a targets to scrape. This can be done using `kubernetes_sd` instead of `static_configs`:

```yaml
    - job_name: hellomonitor
      kubernetes_sd_configs:
      - role: pod
        selectors:
          - role: "pod"
            label: "app=hellomonitor"  
```

Now if deployment `hellomonitor` is scaled down or up, prometheus will automatically update its target list.

## Using the operator

The main drawback of installing Prometheus by using Helm Chart is that we have **a single point of configuration** which is not good. Would be good if we could "distribute" the Prometheus config across the cluster, and this is what Operator allows us to do.
First **uninstall the Helm release**:

``` 
helm delete  prometheus -n prometheus
```

### Install the operator

The [Prometheus Operator](https://github.com/prometheus-operator/prometheus-operator) handles all Prometheus installation and setup, and gives us a set of CRDs to define **which applications do we want to monitor**. This allows to deploy the monitorization configuration alongside the application definition.

To apply the operator with the default configuration just apply the [`bundle.yaml`](https://raw.githubusercontent.com/prometheus-operator/prometheus-operator/main/bundle.yaml) file that is on the github.

This file installs a set of CRDs in the cluster and all the components needed for the operator. However the operator is installed in the `default` namespace, and we don't want it, we wan't the operator to be installed in a custom namespace. For this to happen we need to update the `bundle.yaml`, because this file only allow its resources to be defined in the `default` namespace. To do this, you can download and edit the yaml file, or run the following command:

```
ns=monitoring
kubectl create ns $ns && wget -qO- https://raw.githubusercontent.com/prometheus-operator/prometheus-operator/main/bundle.yaml | sed "s/^\([[:blank:]]*\)namespace: default/\1namespace: $ns/" | kubectl create -f -
```

> This command creates the namespace, downloads the `bundle.yaml` file and changes all ocurrences of `namespace: default` to `namespace: $ns`  where `$ns` is the value of the namespace you choose (`monitoring` in the example).

After this we will have a pod running in the monitoring namespace:

```
> k get pods -n monitoring
NAME                                   READY   STATUS    RESTARTS   AGE
prometheus-operator-7579cd8569-2p8fc   1/1     Running   0          72s
```

### Deploy a Prometheus instance

> This section has to be performed from the `/prometheus/k8s/operator-crds/prometheus-resource/base` folder.

At this point **no prometheus is installed in the cluster, but we have all the infrastructure ready**. To deploy a Prometheus we need to create a CRD of kind `Prometheus` in the apiVersion `monitoring.coreos.com/v1`. The file `01-prometheus.yaml` contains a YAML file to deploy a 2-replicas Prometheus server. So apply it to the cluster:

```
kubectl apply -k .
```

This file creates following resources:
* A service account named `prometheus`
* A ClusteRole to give read access to various objects that we want to monitor
* A ClusteRoleBinding for the ClusterRole and the service account
* A `prometheus` CRD instance named `prometheus`

Once everything is created, the operator will notice that a new CRD appears and will deploy a _Prometheus_ in the namespace `monitoring`:

```
> kubectl get pods -n monitoring
NAME                                   READY   STATUS    RESTARTS   AGE
prometheus-operator-7579cd8569-2p8fc   1/1     Running   0          55m
prometheus-prometheus-1                2/2     Running   0          5s
prometheus-prometheus-0                2/2     Running   0          5s
```

### Configuring _Hello API_ to be monitored

> **Note**: This section assumes that the _Hello API_ is already deployed in the cluster

> This section has to be performed from `/prometheus/k8s/hello-api/prometheus-operator` folder

To monitor a pods exposed through a service, we use the `ServiceMonitor` CRD. If you look the file `servicemonitor.yaml` a _Service Monitor_ is defined. A crucial aspect is that the _Service Monitor_ defines a selector, to select which services will be selected by this _Service Monitor_. Our `hellomonitor` service did not had any label, so we apply a patch on it (the file `svc-hello.yaml`). Another issue is that the _Service Monitor_ expects a string in the port, not a number, so in the same patch we assign the name `web` to the `80` port.

If you access the prometheus service (`kubectl port-forward svc/prometheus-operated -n monitoring 9090:9090`) you will see how the pods of `hellomonitor` service have been added to the prometheus targets.

### Adding node metrics

By installing the operator this way, we started with an empty prometheus, so we are only scrapping our api, but not any of the existing targets (like nodes or kubelet). To do this we need to deploy additional things to the cluster. When we used Helm chart before, these things were already deployed automatically.

> This section has to be performed from `/prometheus/k8s/operator-crds` folder

To add node metrics we will install the [node-exporter for Prometheus](https://github.com/prometheus/node_exporter). Node Exporter collects metrics from the node, and exposes them in Prometheus format through port 9100. This needs **to be installed on any node** so we will use a DaemonSet for it. The easiest way to install it is using Helm:

```
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update
helm install node-exporter prometheus-community/prometheus-node-exporter -n monitoring
```

However adding node-exporter does not make its metrics scraped automatically by our Prometheus. Fortunately the chart creates a service in the `monitoring` namespace, named `node-exporter-prometheus-node-exporter`. So we can deploy a new `ServiceMonitor` CRD to monitor this service. Apply the file `02-smon-node-exporter.yaml` to the cluster, and that's all:

```
kubectl apply -f 02-smon-node-exporter.yaml -n monitoring
```

### Adding kube-state-metrics

> This section has to be performed from `/prometheus/k8s/operator-crds` folder

Let's deploy [kube-state-metrics](https://github.com/kubernetes/kube-state-metrics) which is another project that generate metrics about the state of kubernetes objects like pods, deployments, services and so on. To install it, we can use Helm because there is an [official Helm chart](https://artifacthub.io/packages/helm/prometheus-community/kube-state-metrics/):

```
helm install kube-state-metrics prometheus-community/kube-state-metrics -n monitoring
```

Helm chart created a service named `kube-state-metrics` in the namespace `monitoring`, so final step is to add an additional `ServiceMonitor` to scrape this new metrics endpoint.

```
kubectl apply -f 03-smon-kube-state-metrics.yaml -n monitoring
```

## Monitoring the Kubernetes Control Plane

> This sectiom assumes Prometheus is installed using the operator

The control plane is "the brain of the cluster", it is composed of different parts, and we can (and should) monitor them all.

### Monitoring api-server

> This section has to be performed from `/prometheus/k8s/operator-crds/prometheus-resource/api-server

API Server exposes a `/metrics` endpoint that exposes metrics in Prometheus format. But, there is a catch: this endpoint needs authentication. But, if you have a Prometheus running inside the cluster, the only thing required is to give permissions to the `/metrics` endpoint of the API Server to the `ClusterRole` assigned to the prometheus pods. To do that we need to add the following in the `rules` array of the `ClusterRole`:

```yaml
- nonResourceURLs:
  - /metrics
  verbs:
  - get
``` 

To apply this update to the cluster just type `kubectl apply -k .`

Once applied **go to the folder `/prometheus/k8s/operator-crds`** and apply the file `04-smon-api-server.yaml`. This file contains a _Service Monitor_ that adds the endpoints for ApiServer (the service named `kubernetes` in the `default` namespace):

```
kubectl apply -f 04-smon-api-server.yaml -n monitoring
``` 