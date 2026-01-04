# Quick Reference Card

One-page cheat sheet for everything you need.

---

## Architecture (One Sentence)

Producer (local) sends JSON messages to Azure Event Hubs (cloud queue), which are read by Consumer (AKS container) and logged.

---

## Key Commands

### Producer (Local)
```bash
cd src/Producer
export EVENTHUB_CONNECTION_STRING="Endpoint=sb://..."
export EVENTHUB_NAME="my-event-hub"
dotnet run
```

### Consumer (Build & Deploy)
```bash
# Build for AMD64
docker buildx build --platform linux/amd64 \
  -t shivaacr123.azurecr.io/eventhub-consumer:v1 \
  -f src/Consumer/Dockerfile --push .

# Deploy to AKS
kubectl apply -f k8s/

# View logs
kubectl logs -f deployment/eventhub-consumer
```

### Kubernetes Basics
```bash
kubectl get pods                    # List pods
kubectl get pods -w                 # Watch pods
kubectl logs <pod-name>             # View logs
kubectl logs -f deployment/NAME     # Follow logs
kubectl describe pod <pod-name>     # Detailed info
kubectl delete pod <pod-name>       # Delete pod
kubectl get nodes                   # List nodes
kubectl get deployments             # List deployments
kubectl get services                # List services
kubectl get configmaps              # List ConfigMaps
kubectl get secrets                 # List Secrets
```

### Azure CLI
```bash
az login                            # Login to Azure
az aks get-credentials --resource-group RG --name AKS  # Connect kubectl
az aks list --output table          # List AKS clusters
az acr login --name ACR             # Login to ACR
az eventhubs eventhub list --namespace NAME -g RG  # List Event Hubs
```

---

## File Purposes

| File | Purpose |
|------|---------|
| `src/Producer/Program.cs` | Sends messages to Event Hubs |
| `src/Producer/MessageGenerator.cs` | Creates random test messages |
| `src/Consumer/Program.cs` | Reads messages from Event Hubs |
| `src/Consumer/Dockerfile` | Container build instructions |
| `k8s/configmap.yaml` | Non-secret configuration |
| `k8s/secret.yaml` | Sensitive data (connection strings) |
| `k8s/deployment.yaml` | How to run Consumer in K8s |
| `k8s/service.yaml` | Network endpoint for Consumer |

---

## Message Format

```json
{
  "MessageId": "123",
  "Timestamp": "2026-01-04T10:30:00Z",
  "Payload": {
    "data": "Order processed successfully",
    "value": 42,
    "source": "producer-app"
  }
}
```

---

## Key Concepts

**Event Hubs**: Cloud message queue (like a smart post office)
**Partitions**: Event Hubs splits messages into lanes for performance
**Consumer Group**: Lets multiple consumers read independently (default: `$Default`)
**AKS**: Azure Kubernetes Service (managed Kubernetes)
**Pod**: Running container in Kubernetes
**ConfigMap**: Non-secret configuration in Kubernetes
**Secret**: Sensitive data in Kubernetes (base64-encoded)
**Deployment**: Kubernetes object that manages pods

---

## Flow Diagram

```
1. You run Producer
   ↓
2. Producer creates JSON message
   ↓
3. Producer sends to Event Hubs (HTTPS)
   ↓
4. Event Hubs stores in partition
   ↓
5. Consumer (in AKS) reads from partition (AMQP)
   ↓
6. Consumer logs message to stdout
   ↓
7. You see it: kubectl logs
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Connection string not found | `export EVENTHUB_CONNECTION_STRING="..."` |
| kubectl connection refused | `az aks get-credentials --resource-group RG --name AKS` |
| ImagePullBackOff | Rebuild with `--platform linux/amd64` |
| No messages appearing | Send AFTER consumer starts (uses EventPosition.Latest) |
| Pod CrashLoopBackOff | `kubectl logs <pod-name>` to see error |
| Build too slow | Cross-platform (ARM→AMD64) takes 15-30 min |

---

## Environment Variables

**Producer needs:**
- `EVENTHUB_CONNECTION_STRING` - How to connect to Event Hubs
- `EVENTHUB_NAME` - Name of the Event Hub

**Consumer gets from Kubernetes:**
- `EVENTHUB_CONNECTION_STRING` (from Secret)
- `EVENTHUB_NAME` (from ConfigMap)

---

## Success Checklist

Phase 1 complete when:
- [ ] Producer sends messages successfully
- [ ] Consumer pod runs in AKS (STATUS: Running)
- [ ] Consumer logs show received messages
- [ ] Message IDs match between Producer and Consumer
- [ ] You can explain the architecture

---

## Key Metrics

**Typical values:**
- End-to-end latency: < 1 second
- Message size: Usually < 10 KB
- Event Hubs can handle: Millions messages/sec
- Consumer reads from: All partitions simultaneously
- Docker build time (cross-platform): 15-30 minutes
- Docker build time (same platform): 2-3 minutes

---

## Important URLs

- Azure Portal: https://portal.azure.com
- Event Hubs docs: https://docs.microsoft.com/azure/event-hubs/
- Kubernetes docs: https://kubernetes.io/docs/
- .NET SDK: https://dotnet.microsoft.com/download

---

## Presentation Talking Points

**Opening:** "I built an event-driven messaging system using Azure Event Hubs and Kubernetes that demonstrates modern cloud architecture."

**Why Event Hubs:** Decouples Producer and Consumer, handles speed mismatches, ensures reliability

**Why Kubernetes:** Auto-healing, easy scaling, industry standard

**Demo:** Show Producer sending → Consumer logging in real-time

**Challenges:** Cross-platform builds, Kubernetes authentication, debugging containers

**Learned:** Event-driven architecture, container orchestration, Azure services, async programming

---

## Quick Tests

**Test Producer:**
```bash
cd src/Producer
export EVENTHUB_CONNECTION_STRING="..."
export EVENTHUB_NAME="..."
dotnet run --count 5
# Should see: "Sent: MessageId=..."
```

**Test Consumer:**
```bash
kubectl get pods
# Should see: eventhub-consumer-xxx  1/1  Running

kubectl logs deployment/eventhub-consumer
# Should see: "Connected to Event Hub successfully"
```

**Test End-to-End:**
```bash
# Terminal 1:
kubectl logs -f deployment/eventhub-consumer

# Terminal 2:
cd src/Producer && dotnet run
# Choose option 2

# Terminal 1 should show received messages!
```

---

## Common Mistakes

1. **Forgetting to set environment variables**
   - Producer won't know how to connect

2. **Not connecting kubectl to AKS**
   - Get error: "connection refused"

3. **Building for wrong platform (ARM instead of AMD64)**
   - Pod gets: ImagePullBackOff

4. **Sending messages before Consumer starts**
   - Consumer uses EventPosition.Latest (new messages only)

5. **Hardcoding secrets in code**
   - Security risk! Use environment variables

6. **Not checking if ACR is attached to AKS**
   - Can't pull images

---

## Useful Aliases (Optional)

Add to ~/.zshrc or ~/.bashrc:

```bash
# Kubernetes shortcuts
alias k='kubectl'
alias kgp='kubectl get pods'
alias kgd='kubectl get deployments'
alias kl='kubectl logs'
alias kd='kubectl describe'

# Azure shortcuts
alias azaks='az aks get-credentials'
alias azacr='az acr login'

# Project shortcuts
alias prod='cd ~/ShivaProject/src/Producer && dotnet run'
alias cons='kubectl logs -f deployment/eventhub-consumer'
```

---

**Print this page and keep it next to you while working!**

