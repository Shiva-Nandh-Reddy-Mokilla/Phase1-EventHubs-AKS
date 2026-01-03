#!/bin/bash

# ====================================================================================
# AZURE CONFIGURATION TEMPLATE
# ====================================================================================
# This file contains placeholder values for Azure connection strings.
# 
# HOW TO USE:
# 1. Copy this file: cp azure-config.template.sh azure-config.sh
# 2. Fill in your actual Azure values in azure-config.sh
# 3. Source the file: source azure-config.sh
# 4. NEVER commit azure-config.sh to git (it's in .gitignore)
# ====================================================================================

# ====================================================================================
# EVENT HUBS & STORAGE (Required for Phase 1)
# ====================================================================================

# Event Hub Connection String
# Get from: Azure Portal > Event Hubs Namespace > Shared access policies > RootManageSharedAccessKey
export EVENTHUB_CONNECTION_STRING="Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY"

# Event Hub Name
# Get from: Azure Portal > Event Hubs Namespace > Event Hubs
export EVENTHUB_NAME="your-eventhub-name"

# Storage Account Connection String (for checkpointing)
# Get from: Azure Portal > Storage Account > Access keys > Connection string
export STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=YOUR-STORAGE;AccountKey=YOUR-KEY;EndpointSuffix=core.windows.net"

# ====================================================================================
# OPTIONAL: AZURE CLI DEFAULTS
# ====================================================================================

# Resource Group (optional, for convenience)
export AZURE_RESOURCE_GROUP="your-resource-group"

# Azure Container Registry (optional)
export ACR_NAME="your-acr-name"

# AKS Cluster (optional)
export AKS_CLUSTER="your-aks-cluster"

# ====================================================================================
# VERIFICATION
# ====================================================================================

echo "Azure configuration loaded"
echo ""
echo "Event Hub:"
echo "  - Name: $EVENTHUB_NAME"
echo "  - Connection: ${EVENTHUB_CONNECTION_STRING:0:30}..."
echo ""
echo "Storage:"
echo "  - Connection: ${STORAGE_CONNECTION_STRING:0:30}..."
echo ""
