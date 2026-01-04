# Fast Docker Build for AKS

## The Problem

Your Mac uses ARM64 (Apple Silicon), but AKS uses AMD64 (Intel). Building locally takes 40-60 minutes because Docker has to emulate a different CPU architecture.

## The Solution: Build on Azure

Let Azure build the image directly on AMD64 servers. Takes **2-3 minutes** instead of 40-60 minutes!

## Steps

### 1. Cancel Current Build (if running)
Press `Ctrl+C` in your terminal

### 2. Make Sure You're Logged In
```bash
az login
az acr login --name shivaacr123
```

### 3. Build Directly in Azure Container Registry
```bash
cd /Users/shivanandh/Downloads/ShivaProject

az acr build \
  --registry shivaacr123 \
  --image eventhub-consumer:v1 \
  --file src/Consumer/Dockerfile \
  --platform linux/amd64 \
  .
```

**This uploads your code to Azure and builds there (on AMD64 servers).**

### 4. Verify Image Exists
```bash
az acr repository show \
  --name shivaacr123 \
  --repository eventhub-consumer
```

### 5. Force Kubernetes to Pull New Image
```bash
# Delete old pod (forces new pod with new image)
kubectl delete pod -l app=eventhub-consumer

# Watch it come back up
kubectl get pods -w
```

### 6. Check Logs
```bash
kubectl logs -f deployment/eventhub-consumer
```

You should see:
```
================================================
   PHASE 1: Azure Event Hubs Consumer
================================================
Configuration:
  Event Hub: my-event-hub
  Consumer Group: $Default

Connected to Event Hub successfully
Event Hub has 2 partition(s)
Listening for messages...
```

## Comparison

| Method | Time | Platform | Notes |
|--------|------|----------|-------|
| Local build (Mac) | 40-60 min | ARM→AMD64 | QEMU emulation (very slow) |
| **az acr build** | **2-3 min** | **AMD64 native** | **Builds on Azure servers** ✅ |

## Why This Works

- Azure builds on AMD64 servers (same as AKS)
- No emulation needed
- Fast network in Azure datacenter
- Automatic push to ACR

---

**Run the `az acr build` command above now!**

