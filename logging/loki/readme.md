# Deploy Loki

## Install Loki using Helm

To deploy Loki using Helm:

```
helm repo add grafana https://grafana.github.io/helm-charts 
helm repo update
helm upgrade --install loki grafana/loki-stack -n loki --create-namespace -f values-loki.yaml
```

### How Loki and Promtail work together

* Promtail is a daemonset **who scraps the logs of containers from the nodes** and send them all to Loki.
* Loki is a log aggregator

## Install Grafana

To view data in Loki we need to use Grafana, as Loki do not have any kind of UI. Let's start installing grafana with no persistence at this moment:

```
helm install grafana grafana/grafana -n grafana --create-namespace
```

An admin password is randomly generated, you can view it by typing:

```
kubectl get secret -n grafana grafana -o jsonpath="{.data.admin-password}" | base64 --decode ; echo
```

Finally use `port-forward` to access grafana UI through `localhost:3000`:

```
kubectl port-forward svc/grafana -n grafana 3000:80
```  





