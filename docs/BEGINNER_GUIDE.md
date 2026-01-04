# Complete Beginner's Guide to Phase 1

## Table of Contents
1. [What Is This Project?](#what-is-this-project)
2. [How Does It Work?](#how-does-it-work)
3. [Key Concepts Explained](#key-concepts-explained)
4. [File-by-File Explanation](#file-by-file-explanation)
5. [How to Run Everything](#how-to-run-everything)
6. [Troubleshooting](#troubleshooting)

---

## What Is This Project?

Imagine you have two applications that need to talk to each other:
- **Application A** (Producer) generates data
- **Application B** (Consumer) processes that data

But they can't talk directly. Why?
- They might be on different computers
- Application A might generate data faster than B can process it
- We want to make sure no data is lost if B crashes

**Solution: Azure Event Hubs**

Think of Event Hubs like a **post office**:
- Producer = Person mailing letters
- Event Hubs = Post office (holds letters temporarily)
- Consumer = Person receiving letters

### What This Project Does

```
┌──────────┐         ┌─────────────┐         ┌──────────┐
│ Producer │ ──────▶ │ Event Hubs  │ ──────▶ │ Consumer │
│ (Local)  │ sends   │  (Azure)    │ reads   │  (AKS)   │
└──────────┘         └─────────────┘         └──────────┘
```

**Producer (Your Computer):**
- A simple program you run on your laptop
- Generates fake messages (like "Order #123 processed")
- Sends them to Azure Event Hubs

**Event Hubs (Azure Cloud):**
- A "smart post office" in the cloud
- Holds messages until Consumer reads them
- Can handle millions of messages per second
- Never loses messages

**Consumer (Kubernetes in Azure):**
- A program running in the cloud (AKS = Azure Kubernetes Service)
- Continuously checks Event Hubs for new messages
- Reads messages and logs them
- Runs 24/7 without you doing anything

---

## How Does It Work?

### Step 1: Producer Sends a Message

```
You run the Producer:
  ↓
Producer creates a message:
  {
    "MessageId": "123",
    "Timestamp": "2026-01-04T10:30:00Z",
    "Payload": {
      "data": "Order processed",
      "value": 42
    }
  }
  ↓
Producer converts to JSON (text format)
  ↓
Producer sends to Event Hubs over the internet
  ↓
Event Hubs receives and stores the message
```

### Step 2: Event Hubs Holds the Message

```
Event Hubs:
  - Stores message in a "partition" (like a mailbox)
  - Keeps it for 24 hours (configurable)
  - Tracks which messages were read
  - Can handle millions of messages
```

### Step 3: Consumer Reads the Message

```
Consumer (running in AKS):
  ↓
Continuously checks Event Hubs:
  "Any new messages?"
  ↓
Event Hubs says: "Yes! Here's message #123"
  ↓
Consumer receives the message
  ↓
Consumer logs it:
  "RECEIVED MESSAGE: MessageId=123, ..."
  ↓
You see this in kubectl logs
```

---

## Key Concepts Explained

### 1. What is Azure Event Hubs?

**Simple Explanation:**
Event Hubs is like a smart queue. Imagine a line at a coffee shop:
- Customers (messages) join the line
- Barista (consumer) takes customers one by one
- The line can get long, but no customer is lost
- Multiple baristas can work together

**Technical Details:**
- **Partitions**: Event Hubs divides messages into multiple "lanes" (like multiple cash registers)
- **Throughput**: Can handle millions of events per second
- **Retention**: Keeps messages for 1-7 days
- **Protocol**: Uses AMQP (like HTTP but for messaging)

**Why Use It?**
- **Decoupling**: Producer and Consumer don't need to know about each other
- **Reliability**: If Consumer crashes, messages wait in Event Hubs
- **Scalability**: Can add more Consumers to process faster
- **Real-time**: Messages arrive in milliseconds

### 2. What is Kubernetes (AKS)?

**Simple Explanation:**
Kubernetes is like a super-smart robot that runs your applications:
- You tell it: "Run my Consumer app"
- It finds a computer (node) to run it on
- If the app crashes, it restarts it automatically
- If you need more apps, it creates copies
- It monitors health and fixes problems

**AKS = Azure Kubernetes Service:**
- Microsoft manages the Kubernetes "robot" for you
- You just tell it what to run
- It handles all the complicated stuff

**Key Parts:**
- **Pod**: A running instance of your app (like a box containing your program)
- **Deployment**: Instructions for how to run your app
- **Service**: A way for other apps to find your app
- **ConfigMap**: Non-secret configuration (like Event Hub name)
- **Secret**: Sensitive data (like passwords)

### 3. What is Docker?

**Simple Explanation:**
Docker is like a shipping container for software:
- You put your app + everything it needs in a container
- The container can run anywhere (your laptop, Azure, etc.)
- Everything inside works the same way everywhere

**Dockerfile:**
- A recipe for building your container
- Says: "Install .NET, copy my code, run this command"

**Why Use It?**
- **"Works on my machine" problem solved**: If it works in the container, it works everywhere
- **Isolation**: Your app doesn't interfere with other apps
- **Version control**: Can save different versions (v1, v2, etc.)

### 4. What is a Message?

In this project, a message is just JSON (text) with data:

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

**Parts:**
- **MessageId**: Unique identifier (like a tracking number)
- **Timestamp**: When it was created
- **Payload**: The actual data (can be anything)

### 5. How Does Authentication Work?

**Connection String:**
```
Endpoint=sb://mynamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=abc123...
```

This is like a **super-long password** that contains:
- **Endpoint**: Where Event Hubs is located
- **SharedAccessKeyName**: Username
- **SharedAccessKey**: Password

**Security:**
- Never put this in your code!
- Store in environment variables or Kubernetes Secrets
- It grants full access to your Event Hubs

---

## File-by-File Explanation

### Producer Files

#### 1. `src/Producer/Program.cs`

**Purpose:** The main Producer application that sends messages.

**What it does (in English):**

```
Step 1: Load Configuration
  - Read Event Hub connection string from environment variables
  - Read Event Hub name from environment variables
  - If not found, show error and exit

Step 2: Connect to Event Hubs
  - Create an "EventHubProducerClient" (the thing that sends messages)
  - Connect using the connection string

Step 3: Check Command Line Arguments
  - If you ran: dotnet run --count 10
    → Send 10 messages automatically
  - If you just ran: dotnet run
    → Show interactive menu

Step 4: Interactive Menu
  1 - Send single message
  2 - Send 10 messages
  3 - Send custom message
  q - Quit

Step 5: Sending a Message
  - Generate or get message data
  - Convert to JSON
  - Create an "EventDataBatch"
  - Add message to batch
  - Send batch to Event Hubs
  - Print confirmation
```

**Key Code Sections:**

```csharp
// This creates the connection to Event Hubs
var producerClient = new EventHubProducerClient(connectionString, eventHubName);
```
- **What**: Creates the "sender" object
- **Like**: Opening a mailbox to send letters

```csharp
// This creates a batch (group) of messages
using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
```
- **What**: Creates a container for messages
- **Like**: Getting an envelope to put your letter in
- **Why batch**: More efficient to send multiple messages at once

```csharp
// This converts your message to the format Event Hubs understands
var jsonMessage = JsonSerializer.Serialize(message);
var eventData = new EventData(jsonMessage);
```
- **What**: Turns your C# object into text (JSON)
- **Like**: Writing your letter
- **Why**: Event Hubs doesn't understand C# objects, only text/bytes

```csharp
// This sends the message
await producerClient.SendAsync(eventBatch);
```
- **What**: Sends the message to Event Hubs
- **Like**: Dropping the letter in the mailbox
- **async/await**: Means "wait for this to finish before continuing"

#### 2. `src/Producer/MessageGenerator.cs`

**Purpose:** Creates random messages for testing.

**What it does:**

```csharp
private static readonly string[] _sampleData = new[]
{
    "Temperature reading from sensor A",
    "Order processed successfully",
    "User login event",
    // ... more sample messages
};
```
- **What**: A list of fake messages to choose from
- **Like**: Having pre-written templates

```csharp
public static Message GenerateMessage()
{
    var messageId = _random.Next(1, 1000);  // Random number 1-1000
    var payloadIndex = _random.Next(_sampleData.Length);  // Pick random message
    
    return new Message
    {
        MessageId = messageId.ToString(),
        Timestamp = DateTime.UtcNow.ToString("O"),  // Current time in ISO format
        Payload = new
        {
            data = _sampleData[payloadIndex],
            value = randomValue,
            source = "producer-app"
        }
    };
}
```
- **What**: Creates a message with random data
- **Why**: So you can test without typing real data

#### 3. `src/Producer/Producer.csproj`

**Purpose:** Tells .NET what this project needs.

```xml
<TargetFramework>net8.0</TargetFramework>
```
- **What**: Use .NET version 8.0
- **Like**: Saying "I need Windows 10 or higher"

```xml
<PackageReference Include="Azure.Messaging.EventHubs" Version="5.11.5" />
```
- **What**: Download and use the Event Hubs library from NuGet (like npm for .NET)
- **Why**: You don't have to write Event Hubs code yourself

```xml
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
```
- **What**: Tools for reading configuration (appsettings.json, environment variables)
- **Why**: Makes it easy to configure the app without changing code

#### 4. `src/Producer/appsettings.json`

**Purpose:** Configuration file (like a settings menu).

```json
{
  "EventHub": {
    "EventHubName": "",
    "ConnectionString": ""
  }
}
```
- **What**: Can store settings here
- **When**: Usually leave empty and use environment variables instead
- **Why**: Don't want to accidentally commit secrets to git

---

### Consumer Files

#### 1. `src/Consumer/Program.cs`

**Purpose:** Sets up and starts the Consumer application.

**What it does (in English):**

```
Step 1: Load Configuration
  - Read Event Hub connection string from environment variable
  - Read Event Hub name from environment variable
  - Read Consumer Group (usually "$Default")

Step 2: Validate Configuration
  - Check if connection string exists
  - Check if Event Hub name exists
  - If missing, print error and exit

Step 3: Connect to Event Hubs
  - Create EventHubConsumerClient (the thing that reads messages)
  - Use $Default consumer group
  - Connect using connection string

Step 4: Start Reading Messages
  - Get list of partitions (Event Hubs splits messages into partitions)
  - For each partition, start a task to read messages
  - Each task runs in a loop: "Read message → Log it → Repeat"
  - Keep running until Ctrl+C is pressed

Step 5: When Ctrl+C is Pressed
  - Stop all reading tasks
  - Clean up connections
  - Exit gracefully
```

**Key Code Sections:**

```csharp
var consumer = new EventHubConsumerClient(
    consumerGroup,
    connectionString,
    eventHubName);
```
- **What**: Creates the "reader" object
- **Like**: Going to the post office to pick up mail
- **consumerGroup**: Like having multiple people picking up mail (they coordinate)

```csharp
string[] partitionIds = await consumer.GetPartitionIdsAsync();
```
- **What**: Gets list of partitions
- **Partitions**: Event Hubs splits messages into multiple "lanes" for performance
- **Example**: If you have 2 partitions, messages are split between them

```csharp
await foreach (var partitionEvent in consumer.ReadEventsFromPartitionAsync(
    partitionId,
    EventPosition.Latest,
    cancellationToken))
```
- **What**: Continuously reads messages from one partition
- **EventPosition.Latest**: Start reading from new messages (not old ones)
- **await foreach**: "For each message that arrives, do this..."
- **Like**: Standing at mailbox waiting for new mail

```csharp
var messageBody = Encoding.UTF8.GetString(partitionEvent.Data.EventBody.ToArray());
```
- **What**: Converts message from bytes to text
- **Why**: Event Hubs sends bytes, we want readable text
- **UTF8**: The encoding format (like choosing English vs Spanish)

```csharp
Console.WriteLine("========================================");
Console.WriteLine("RECEIVED MESSAGE");
Console.WriteLine($"Message ID:  {messageId}");
Console.WriteLine($"Body:        {messageBody}");
Console.WriteLine("========================================");
```
- **What**: Prints the received message to console
- **Why**: In Kubernetes, console output goes to logs
- **You see this**: When you run `kubectl logs`

#### 2. `src/Consumer/Consumer.csproj`

**Purpose:** Project configuration for Consumer.

```xml
<OutputType>Exe</OutputType>
```
- **What**: This creates a console application (not a web app)
- **Why**: We just want to read messages and log them

```xml
<PackageReference Include="Azure.Messaging.EventHubs" Version="5.11.5" />
```
- **What**: Event Hubs library (same as Producer)
- **Why**: Need this to read from Event Hubs

**Note:** We removed these packages for Phase 1:
- ~~Azure.Messaging.EventHubs.Processor~~ (Phase 2 - checkpointing)
- ~~Azure.Storage.Blobs~~ (Phase 2 - storage)

#### 3. `src/Consumer/Dockerfile`

**Purpose:** Recipe for building a Docker container.

**Line by Line:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```
- **What**: Start with Microsoft's .NET 8 SDK image
- **Like**: "I need a computer with .NET installed"
- **AS build**: Name this stage "build"

```dockerfile
WORKDIR /src
```
- **What**: Set working directory to /src
- **Like**: `cd /src`

```dockerfile
COPY src/Consumer/Consumer.csproj ./Consumer/
```
- **What**: Copy only the project file first
- **Why**: Docker caches each step. If code changes but dependencies don't, Docker reuses cached packages

```dockerfile
RUN dotnet restore ./Consumer/Consumer.csproj
```
- **What**: Download NuGet packages (like npm install)
- **Why**: Get all dependencies

```dockerfile
COPY src/Consumer/ ./Consumer/
```
- **What**: Copy all source code
- **Why**: Now we have everything needed to build

```dockerfile
RUN dotnet build ./Consumer/Consumer.csproj -c Release -o /app/build
```
- **What**: Compile the C# code
- **-c Release**: Optimized for production
- **-o /app/build**: Output to this folder

```dockerfile
RUN dotnet publish ./Consumer/Consumer.csproj -c Release -o /app/publish
```
- **What**: Create deployment-ready files
- **Why**: publish includes only what's needed to run (no source code)

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
```
- **What**: Start a NEW image with only .NET runtime (not SDK)
- **Why**: Final image is much smaller (SDK has compilers, etc. that we don't need)

```dockerfile
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser
```
- **What**: Create a non-root user and switch to it
- **Why**: Security best practice (don't run as root)

```dockerfile
COPY --from=build /app/publish .
```
- **What**: Copy compiled app from "build" stage to this stage
- **Why**: Final image only has the app, not the SDK

```dockerfile
ENTRYPOINT ["dotnet", "Consumer.dll"]
```
- **What**: Command to run when container starts
- **Like**: "When you turn this on, run this program"

---

### Kubernetes Files

#### 1. `k8s/configmap.yaml`

**Purpose:** Stores non-secret configuration.

```yaml
apiVersion: v1
kind: ConfigMap
```
- **What**: Tell Kubernetes this is a ConfigMap
- **Like**: Saying "This is a settings file"

```yaml
metadata:
  name: consumer-config
  namespace: default
```
- **name**: How you refer to this ConfigMap
- **namespace**: Which section of Kubernetes (default is... default)

```yaml
data:
  EVENTHUB_NAME: "my-event-hub"
```
- **What**: Key-value pairs of configuration
- **How used**: Becomes environment variable in your app
- **When Consumer starts**: It sees `EVENTHUB_NAME=my-event-hub`

**Why ConfigMap?**
- Easy to change without rebuilding Docker image
- Separate code from configuration
- Can use same code with different configs

#### 2. `k8s/secret.yaml.template`

**Purpose:** Template for storing sensitive data.

```yaml
kind: Secret
type: Opaque
```
- **What**: Tell Kubernetes this is a Secret
- **Opaque**: Generic secret (not SSL cert, etc.)

```yaml
data:
  eventhub-connection: <BASE64_ENCODED_CONNECTION_STRING>
```
- **What**: Store connection string
- **Base64**: Not encryption! Just encoding so it can be stored
- **How to create**: `echo -n "your-string" | base64`

**Why Secret?**
- Kubernetes restricts who can read Secrets
- Never shown in logs
- Can be encrypted at rest

**Process:**
1. Copy `secret.yaml.template` to `secret.yaml`
2. Replace `<BASE64_...>` with actual base64-encoded value
3. Apply to Kubernetes
4. Add `secret.yaml` to `.gitignore` (never commit!)

#### 3. `k8s/deployment.yaml`

**Purpose:** Tells Kubernetes how to run your app.

```yaml
kind: Deployment
```
- **What**: A Deployment manages your app
- **Does**: Creates pods, restarts them if they crash, updates them

```yaml
spec:
  replicas: 1
```
- **What**: Run 1 copy of the app
- **Can change**: Set to 2 to run 2 copies (for high availability)

```yaml
spec:
  template:
    spec:
      containers:
      - name: consumer
        image: shivaacr123.azurecr.io/eventhub-consumer:v1
```
- **What**: Use this Docker image
- **Where**: From your Azure Container Registry
- **:v1**: The version/tag

```yaml
env:
- name: EVENTHUB_NAME
  valueFrom:
    configMapKeyRef:
      name: consumer-config
      key: EVENTHUB_NAME
```
- **What**: Set environment variable EVENTHUB_NAME
- **From**: ConfigMap named "consumer-config", key "EVENTHUB_NAME"
- **In app**: You can read `Environment.GetEnvironmentVariable("EVENTHUB_NAME")`

```yaml
- name: EVENTHUB_CONNECTION_STRING
  valueFrom:
    secretKeyRef:
      name: azure-secrets
      key: eventhub-connection
```
- **What**: Set environment variable from Secret
- **From**: Secret named "azure-secrets", key "eventhub-connection"
- **Kubernetes**: Automatically decodes base64

```yaml
resources:
  requests:
    cpu: 100m
    memory: 128Mi
  limits:
    cpu: 500m
    memory: 512Mi
```
- **requests**: Minimum resources guaranteed
  - **100m**: 0.1 CPU cores
  - **128Mi**: 128 megabytes RAM
- **limits**: Maximum resources allowed
  - **500m**: 0.5 CPU cores
  - **512Mi**: 512 megabytes RAM
- **Why**: Prevents one app from using all resources

#### 4. `k8s/service.yaml`

**Purpose:** Creates a network endpoint for your app.

```yaml
kind: Service
```
- **What**: A Service makes your app accessible
- **Like**: Giving your app a phone number

```yaml
spec:
  type: ClusterIP
```
- **ClusterIP**: Only accessible inside Kubernetes
- **Other types**:
  - LoadBalancer: Public internet access
  - NodePort: Access via node IP

```yaml
selector:
  app: eventhub-consumer
```
- **What**: Find pods with this label
- **Why**: Service routes traffic to these pods

```yaml
ports:
- port: 80
  targetPort: 8080
```
- **port 80**: External port (what you connect to)
- **targetPort 8080**: Internal port (where app listens)
- **For Phase 1**: Not really used since Consumer is just logging

---

## How Everything Connects

### 1. Local Development (Producer)

```
Your Laptop
├── src/Producer/Program.cs
│   ├── Reads: appsettings.Development.json
│   ├── Reads: Environment variables
│   │   └── EVENTHUB_CONNECTION_STRING
│   │   └── EVENTHUB_NAME
│   └── Creates: EventHubProducerClient
│       └── Connects to: Azure Event Hubs
│           └── Sends: JSON messages
└── Internet → Azure Event Hubs
```

### 2. Azure Event Hubs

```
Azure Event Hubs
├── Receives messages from Producer
├── Stores in Partitions
│   ├── Partition 0: [ msg1, msg3, msg5, ... ]
│   └── Partition 1: [ msg2, msg4, msg6, ... ]
├── Tracks: Which messages were read
└── Waits for: Consumer to read
```

### 3. Kubernetes Deployment (Consumer)

```
Your Laptop
└── kubectl apply -f k8s/
    ├── Reads: configmap.yaml
    │   └── Creates: ConfigMap with EVENTHUB_NAME
    ├── Reads: secret.yaml
    │   └── Creates: Secret with connection string
    ├── Reads: deployment.yaml
    │   └── Creates: Deployment
    │       └── Creates: Pod
    │           ├── Pulls: Docker image from ACR
    │           ├── Injects: Environment variables from ConfigMap
    │           ├── Injects: Environment variables from Secret
    │           └── Starts: Consumer.dll
    │               ├── Reads environment variables
    │               ├── Creates: EventHubConsumerClient
    │               ├── Connects to: Azure Event Hubs
    │               ├── Reads: Messages from all partitions
    │               └── Logs: To stdout
    │                   └── Visible via: kubectl logs
    └── Reads: service.yaml
        └── Creates: Service (internal networking)

Azure AKS Cluster
└── Node (Virtual Machine)
    └── Pod (Container)
        └── Consumer App (Running)
```

### 4. Complete Flow

```
1. You run Producer on laptop
   ↓
2. Producer sends message to Event Hubs (via internet)
   ↓
3. Event Hubs stores message in a partition
   ↓
4. Consumer (in AKS) continuously checks Event Hubs
   ↓
5. Consumer sees new message
   ↓
6. Consumer reads message
   ↓
7. Consumer logs message to stdout
   ↓
8. Kubernetes captures stdout
   ↓
9. You see it with: kubectl logs
```

---

## How to Run Everything

### Prerequisites Checklist

```
□ Azure subscription (free trial OK)
□ Azure resources created:
  □ Event Hubs Namespace
  □ Event Hub
  □ Container Registry (ACR)
  □ AKS Cluster
□ Local tools installed:
  □ .NET 8 SDK
  □ Docker Desktop
  □ kubectl
  □ Azure CLI (az)
```

### Step-by-Step Process

#### Part 1: Run Producer Locally (5 minutes)

```bash
# 1. Go to Producer folder
cd src/Producer

# 2. Set your Event Hub connection details
export EVENTHUB_CONNECTION_STRING="Endpoint=sb://..."
export EVENTHUB_NAME="my-event-hub"

# 3. Run the producer
dotnet run

# 4. In the menu, choose option 2 (send 10 messages)
# You should see:
#   Sent: MessageId=123, Timestamp=...
#   Sent: MessageId=456, Timestamp=...
#   (10 times)

# 5. Type 'q' to quit
```

**What just happened:**
- Your laptop sent 10 messages to Azure Event Hubs
- Event Hubs is now holding those messages
- They're waiting for Consumer to read them

#### Part 2: Build and Push Docker Image (10-30 minutes)

```bash
# 1. Go back to project root
cd ../..

# 2. Build Docker image for AMD64 (AKS platform)
docker buildx build \
  --platform linux/amd64 \
  -t shivaacr123.azurecr.io/eventhub-consumer:v1 \
  -f src/Consumer/Dockerfile \
  --push \
  .

# This takes 10-30 minutes on Mac (cross-platform build)
# ☕ Go get coffee!

# 3. Verify image was pushed
az acr repository show \
  --name shivaacr123 \
  --repository eventhub-consumer
```

**What just happened:**
- Docker compiled your Consumer code
- Created a container image
- Pushed it to Azure Container Registry
- Now AKS can pull and run it

#### Part 3: Deploy to Kubernetes (5 minutes)

```bash
# 1. Make sure kubectl is connected
kubectl get nodes
# Should show your AKS node

# 2. Update ConfigMap with your Event Hub name
# Edit k8s/configmap.yaml, change EVENTHUB_NAME

# 3. Create Secret
kubectl create secret generic azure-secrets \
  --from-literal=eventhub-connection="Endpoint=sb://..."

# 4. Deploy everything
kubectl apply -f k8s/

# 5. Check if pod is running
kubectl get pods
# Should show: eventhub-consumer-xxx   1/1   Running

# 6. View logs
kubectl logs -f deployment/eventhub-consumer
```

**What just happened:**
- Kubernetes pulled your Docker image
- Started a pod running Consumer
- Consumer connected to Event Hubs
- Consumer is now reading messages

#### Part 4: Test End-to-End (2 minutes)

**Terminal 1: Watch Consumer Logs**
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

**Terminal 2: Send Messages**
```bash
cd src/Producer
export DOTNET_ENVIRONMENT=Development
dotnet run
# Choose option 2, send 10 messages
```

**Back to Terminal 1:**
You should now see:
```
========================================
RECEIVED MESSAGE
  Partition:   0
  Message ID:  123
  Timestamp:   2026-01-04T10:30:00Z
  Body:        {"MessageId":"123",...}
========================================

========================================
RECEIVED MESSAGE
  Partition:   1
  Message ID:  456
  Timestamp:   2026-01-04T10:30:01Z
  Body:        {"MessageId":"456",...}
========================================
```

**Success!** Messages are flowing from Producer → Event Hubs → Consumer!

---

## Troubleshooting

### Common Issues

#### Issue 1: "Connection string not configured"

**Symptom:**
```
ERROR: Missing Event Hub configuration
```

**Solution:**
```bash
# Make sure you set both variables
export EVENTHUB_CONNECTION_STRING="Endpoint=..."
export EVENTHUB_NAME="my-event-hub"

# Verify they're set
echo $EVENTHUB_CONNECTION_STRING
echo $EVENTHUB_NAME
```

#### Issue 2: "ImagePullBackOff"

**Symptom:**
```bash
kubectl get pods
# Shows: eventhub-consumer-xxx  0/1  ImagePullBackOff
```

**Causes:**
1. Image built for wrong platform (ARM vs AMD64)
2. ACR not attached to AKS
3. Wrong image name

**Solution:**
```bash
# 1. Rebuild for AMD64
docker buildx build --platform linux/amd64 ...

# 2. Attach ACR to AKS
az aks update --resource-group <RG> --name <AKS> --attach-acr <ACR>

# 3. Check image name in deployment.yaml matches ACR
kubectl get deployment eventhub-consumer -o yaml | grep image:
```

#### Issue 3: "No messages appearing"

**Symptom:**
- Producer sends successfully
- Consumer is running
- But no messages in logs

**Possible causes:**

**A. Consumer started before messages were sent**
- Consumer uses `EventPosition.Latest` (only new messages)
- Solution: Send new messages AFTER consumer is running

**B. Wrong Event Hub**
- Producer sending to different Event Hub than Consumer reading from
- Solution: Check both use same EVENTHUB_NAME

**C. Different namespaces**
- Producer connected to namespace A
- Consumer connected to namespace B
- Solution: Check connection strings match

**D. Consumer crashed**
```bash
# Check pod status
kubectl get pods

# Check for errors
kubectl logs deployment/eventhub-consumer

# Check events
kubectl describe pod <pod-name>
```

#### Issue 4: "kubectl: command not found"

**Symptom:**
```bash
kubectl get pods
# zsh: command not found: kubectl
```

**Solution:**
```bash
# Install kubectl
brew install kubectl

# Verify installation
kubectl version --client
```

#### Issue 5: "dial tcp localhost:8080: connect refused"

**Symptom:**
```bash
kubectl get pods
# error: dial tcp [::1]:8080: connect: connection refused
```

**Cause:**
kubectl not connected to AKS cluster

**Solution:**
```bash
# Connect to your AKS cluster
az aks get-credentials \
  --resource-group <RESOURCE_GROUP> \
  --name <AKS_CLUSTER_NAME>

# Verify connection
kubectl get nodes
```

---

## Key Takeaways

### What You Built

1. **Producer**: Console app that sends messages to Event Hubs
2. **Consumer**: Containerized app that reads messages from Event Hubs
3. **Event Hubs**: Reliable message queue in Azure
4. **AKS Deployment**: Consumer running 24/7 in Kubernetes

### What You Learned

1. **Azure Event Hubs**:
   - How to send/receive messages
   - Connection strings and authentication
   - Partitions and consumer groups

2. **Kubernetes**:
   - Deployments, Pods, Services
   - ConfigMaps and Secrets
   - kubectl commands
   - Container orchestration

3. **Docker**:
   - Dockerfile creation
   - Multi-stage builds
   - Cross-platform builds (ARM → AMD64)
   - Container registries

4. **.NET**:
   - Console applications
   - Async programming (async/await)
   - Configuration management
   - NuGet packages

### Phase 1 Complete When...

- [x] Producer sends messages successfully
- [x] Consumer receives messages in AKS
- [x] Logs clearly show message details
- [x] You can explain the architecture
- [x] You understand each component

---

## Next Steps

### For Presentation

1. **Practice the demo**:
   - Show Producer sending
   - Show Consumer logs receiving
   - Explain the flow

2. **Prepare to explain**:
   - Why Event Hubs (decoupling, reliability)
   - Why Kubernetes (scalability, auto-healing)
   - How it all connects

3. **Have screenshots ready**:
   - Azure Portal (Event Hubs, AKS)
   - Producer terminal
   - kubectl logs output

### For Learning More

1. **Try these experiments**:
   - Send 1000 messages at once
   - Stop Consumer, send messages, start Consumer (messages wait!)
   - Run 2 Producer instances simultaneously
   - Scale Consumer to 2 replicas (`kubectl scale deployment eventhub-consumer --replicas=2`)

2. **Phase 2 features** (future):
   - Add checkpointing (remember which messages processed)
   - Store messages in Blob Storage
   - Forward to Service Bus
   - Add monitoring and alerts

---

**Congratulations! You've built a complete event-driven system!** 🎉

