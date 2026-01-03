# Phase 1: Azure Event Hubs with AKS

## Overview

This project demonstrates message processing using Azure Event Hubs and Azure Kubernetes Service.

**What it does:**
- Producer sends test messages to Azure Event Hubs
- Consumer runs in AKS, reads messages from Event Hubs, and logs them

---

## Architecture

```
Producer (Console) --> Azure Event Hubs --> Consumer (AKS Pod)
                                                 |
                                                 v
                                         Azure Blob Storage
                                           (Checkpoints)
```

---

## Prerequisites

### Azure Resources
- Event Hubs Namespace + Event Hub
- Storage Account
- Container Registry (ACR)
- AKS Cluster

### Local Tools
- .NET 8 SDK
- Docker Desktop
- kubectl
- Azure CLI

---

## Setup

### 1. Create Azure Resources

```bash
# Variables
RG="my-rg"
LOCATION="eastus"
EH_NS="myeventhub123"
EH_NAME="messages"
STORAGE="mystorage123"
ACR="myacr123"
AKS="myaks"

# Create Event Hubs
az eventhubs namespace create -n $EH_NS -g $RG -l $LOCATION --sku Standard
az eventhubs eventhub create -n $EH_NAME --namespace-name $EH_NS -g $RG --partition-count 2

# Get connection string
az eventhubs namespace authorization-rule keys list \
  --namespace-name $EH_NS -g $RG \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString -o tsv

# Create Storage Account
az storage account create -n $STORAGE -g $RG -l $LOCATION --sku Standard_LRS

# Get connection string
az storage account show-connection-string -n $STORAGE -g $RG --query connectionString -o tsv

# Create AKS and ACR
az acr create -n $ACR -g $RG --sku Basic
az aks create -n $AKS -g $RG --node-count 1 --node-vm-size Standard_B2s --generate-ssh-keys
az aks update -n $AKS -g $RG --attach-acr $ACR
az aks get-credentials -n $AKS -g $RG
```

### 2. Run Producer Locally

```bash
cd src/Producer

export EVENTHUB_CONNECTION_STRING="your-connection-string"
export EVENTHUB_NAME="messages"

dotnet run
```

Select option 2 to send 10 messages, or option 1 for single messages.

### 3. Deploy Consumer to AKS

```bash
# Build and push image
cd src/Consumer
az acr login -n $ACR
docker build -t $ACR.azurecr.io/eventhub-consumer:v1 .
docker push $ACR.azurecr.io/eventhub-consumer:v1

# Update k8s/configmap.yaml with your Event Hub name
# Update k8s/deployment.yaml with your ACR name

# Create secret with base64-encoded connection strings
echo -n "your-eventhub-connection" | base64
echo -n "your-storage-connection" | base64

# Edit k8s/secret.yaml with the base64 values

# Deploy
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Verify
kubectl get pods
kubectl logs -f deployment/eventhub-consumer
```

### 4. Test End-to-End

Terminal 1:
```bash
kubectl logs -f deployment/eventhub-consumer
```

Terminal 2:
```bash
cd src/Producer
dotnet run --count 5
```

You should see messages appearing in the consumer logs.

---

## Project Structure

```
ShivaProject/
├── README.md
├── azure-config.template.sh
├── ShivaProject.sln
├── src/
│   ├── Producer/
│   │   ├── Program.cs
│   │   ├── MessageGenerator.cs
│   │   └── Producer.csproj
│   └── Consumer/
│       ├── Program.cs
│       ├── Services/
│       │   └── EventHubsProcessor.cs
│       ├── Dockerfile
│       └── Consumer.csproj
└── k8s/
    ├── configmap.yaml
    ├── secret.yaml.template
    ├── deployment.yaml
    └── service.yaml
```

---

## Troubleshooting

### Producer
- Verify EVENTHUB_CONNECTION_STRING and EVENTHUB_NAME are set
- Check connection string has Send permissions

### Consumer
- Check logs: `kubectl logs <pod-name>`
- Verify connection strings in secret
- Ensure ACR is attached to AKS: `az aks check-acr -n $AKS -g $RG --acr $ACR`

### No Messages
- Verify Producer sends to correct Event Hub
- Check Event Hub metrics in Azure Portal
- Confirm Consumer Group matches (default: $Default)

---

## Key Concepts

**Event Hubs**: Azure messaging service for high-throughput event streaming

**Checkpointing**: Tracks which messages have been processed using Blob Storage

**Kubernetes Health Checks**: 
- Liveness probe: Restarts container if crashed
- Readiness probe: Controls traffic routing

**Configuration**:
- ConfigMap: Non-sensitive config
- Secret: Sensitive data (connection strings)

---

## Success Criteria

Phase 1 complete when:
1. Producer sends messages to Event Hubs
2. Consumer pod runs in AKS
3. Consumer logs show received messages
4. You can explain the architecture

---

## Resources

- [Azure Event Hubs](https://docs.microsoft.com/azure/event-hubs/)
- [Azure Kubernetes Service](https://docs.microsoft.com/azure/aks/)
- [.NET Background Services](https://docs.microsoft.com/dotnet/core/extensions/hosted-services)
