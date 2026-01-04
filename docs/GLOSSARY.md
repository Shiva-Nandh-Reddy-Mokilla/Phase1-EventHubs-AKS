# Glossary of Terms

A simple dictionary of all technical terms used in this project.

---

## General Terms

### API (Application Programming Interface)
**Simple:** A way for programs to talk to each other.
**Example:** When Producer talks to Event Hubs, it uses the Event Hubs API.
**Like:** A menu at a restaurant - tells you what you can order.

### Async/Await
**Simple:** "async" means a function can wait for something without blocking. "await" means "wait for this to finish."
**Example:** `await SendMessageAsync()` - wait for message to send before continuing.
**Why:** Lets your program do other things while waiting.

### Base64
**Simple:** A way to convert text/binary into letters and numbers only.
**Example:** "Hello" becomes "SGVsbG8="
**Why:** Some systems (like Kubernetes) need text-only format.
**Note:** NOT encryption - anyone can decode it!

### CLI (Command Line Interface)
**Simple:** A program you control by typing commands (not clicking buttons).
**Example:** `kubectl get pods` - typing command instead of clicking.
**Like:** Typing commands to your computer instead of using mouse.

### Container
**Simple:** A box that has your app + everything it needs to run.
**Example:** Your Consumer app in a Docker container.
**Why:** Works the same everywhere - your laptop, Azure, anywhere.
**Like:** A shipping container - same container works on ship, truck, or train.

### Environment Variable
**Simple:** A setting that exists outside your code.
**Example:** `EVENTHUB_NAME=my-event-hub` - available to your program.
**Why:** Change settings without changing code.
**How to set:** `export VARIABLE_NAME="value"` (Linux/Mac) or `set VARIABLE_NAME=value` (Windows)

### JSON (JavaScript Object Notation)
**Simple:** A way to write data in text format.
**Example:**
```json
{
  "name": "John",
  "age": 30
}
```
**Why:** Easy for humans to read, easy for computers to parse.
**Used:** Sending messages between Producer and Consumer.

### SDK (Software Development Kit)
**Simple:** A collection of tools to build software for a specific platform.
**Example:** Azure Event Hubs SDK - tools to work with Event Hubs.
**Contains:** Libraries, examples, documentation.
**Like:** A toolbox for building specific things.

---

## Azure Terms

### ACR (Azure Container Registry)
**Simple:** A place in Azure to store your Docker images.
**Example:** `shivaacr123.azurecr.io/eventhub-consumer:v1`
**Why:** Kubernetes can pull images from here.
**Like:** A private Docker Hub for your organization.

### AKS (Azure Kubernetes Service)
**Simple:** Microsoft runs Kubernetes for you in Azure.
**What you do:** Tell it what to run.
**What Microsoft does:** Manages servers, updates, security.
**Why:** You don't have to learn all the hard Kubernetes stuff.

### Azure Event Hubs
**Simple:** A super-fast messaging service in Azure.
**Purpose:** Temporarily hold messages between Producer and Consumer.
**Handles:** Millions of messages per second.
**Like:** A smart post office for data.

### Azure Portal
**Simple:** A website where you manage your Azure resources.
**URL:** portal.azure.com
**What you can do:** Create Event Hubs, see metrics, manage settings.
**Like:** Control panel for Azure.

### Connection String
**Simple:** A long string with information to connect to Azure.
**Example:** `Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...`
**Contains:** Where to connect + username + password (all in one string).
**Security:** Treat like a password - never put in code!

### Resource Group
**Simple:** A folder that holds related Azure resources.
**Example:** Put Event Hubs, AKS, Storage in same Resource Group.
**Why:** Easier to manage, can delete everything at once.
**Like:** A project folder containing all project files.

---

## Event Hubs Terms

### Consumer Group
**Simple:** A label that lets multiple consumers read the same Event Hub independently.
**Default:** `$Default` - the automatically created group.
**Example:** Group A reads all messages, Group B also reads all messages (independently).
**Why:** Different apps can process same events differently.
**Like:** Two people can read the same newspaper.

### Event
**Simple:** One message sent through Event Hubs.
**Example:** One JSON message with customer order data.
**Contains:** Bytes (usually text/JSON).
**Size limit:** 1 MB per event.

### Namespace
**Simple:** A container for Event Hubs (your Event Hubs account).
**Example:** `mynamespace.servicebus.windows.net`
**Contains:** One or more Event Hubs.
**Like:** A server that holds multiple mailboxes (Event Hubs).

### Partition
**Simple:** Event Hubs splits messages into multiple "lanes" for performance.
**Example:** 2 partitions = messages split into 2 groups.
**Why:** Allows parallel processing - multiple consumers can work simultaneously.
**Like:** Multiple checkout lanes at a grocery store.

### Partition Key
**Simple:** A value that determines which partition a message goes to.
**Example:** Send all messages for "Customer A" to same partition.
**Why:** Keeps related messages together and in order.
**Not used in Phase 1.**

### Throughput Unit (TU)
**Simple:** How much data Event Hubs can handle per second.
**1 TU:** 1 MB/sec input, 2 MB/sec output.
**Cost:** Pay per TU.
**Phase 1:** Usually 1 TU is enough.

---

## Kubernetes Terms

### Cluster
**Simple:** A group of servers that run Kubernetes.
**Contains:** Nodes (servers), Pods (containers), etc.
**Your AKS:** One cluster with 1-2 nodes.
**Like:** A team of computers working together.

### ConfigMap
**Simple:** A Kubernetes object that stores non-secret configuration.
**Example:** Event Hub name, API endpoints, feature flags.
**Why:** Separate configuration from code.
**Usage:** Injected as environment variables into pods.

### Container
**Simple:** A running instance of a Docker image.
**Contains:** Your app + .NET runtime + libraries.
**Runs:** Inside a pod.
**Like:** A running program in a sandbox.

### Deployment
**Simple:** Tells Kubernetes HOW to run your app.
**Specifies:** How many replicas, which image, resource limits.
**Manages:** Creating/updating/deleting pods.
**Example:** "Run 2 copies of Consumer using image v1."

### kubectl
**Pronunciation:** "kube-control" or "kube-cuttle"
**Simple:** Command-line tool to control Kubernetes.
**Commands:**
  - `kubectl get pods` - list pods
  - `kubectl logs <pod>` - view logs
  - `kubectl apply -f <file>` - deploy configuration
**Like:** The "remote control" for Kubernetes.

### Node
**Simple:** One server in your Kubernetes cluster.
**Runs:** Multiple pods.
**Your AKS:** Usually 1-2 nodes (virtual machines).
**Example:** A VM with 2 CPU cores, 8 GB RAM.

### Pod
**Simple:** The smallest unit in Kubernetes - runs one or more containers.
**Usually:** One container per pod (best practice).
**Your Consumer:** One pod running one container.
**Like:** A wrapper around your container.

### Replica
**Simple:** A copy of your app.
**Example:** `replicas: 2` means run 2 copies of Consumer.
**Why:** High availability, handle more traffic.
**Phase 1:** We use 1 replica.

### Secret
**Simple:** A Kubernetes object that stores sensitive data.
**Example:** Connection strings, passwords, API keys.
**Stored:** Base64-encoded (not encrypted by default).
**Security:** Access-controlled, not logged.

### Service
**Simple:** A stable network endpoint to reach your pods.
**Why:** Pod IPs change when they restart. Service IP stays the same.
**Types:**
  - **ClusterIP:** Internal only (default)
  - **LoadBalancer:** Public internet access
  - **NodePort:** Access via node IP

---

## Docker Terms

### Docker
**Simple:** A platform to build and run containers.
**What it does:** Packages your app + dependencies into a container.
**Why:** "Works on my machine" problem solved.

### Docker Image
**Simple:** A template for creating containers.
**Contains:** Your app code + runtime + dependencies.
**Like:** A cookie cutter (image) makes cookies (containers).
**Example:** `eventhub-consumer:v1`

### Dockerfile
**Simple:** A recipe for building a Docker image.
**Contains:** Instructions like "install .NET, copy code, run app."
**Example:**
```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
COPY . /app
CMD ["dotnet", "Consumer.dll"]
```

### Multi-stage Build
**Simple:** A Dockerfile with multiple "FROM" statements.
**Why:** Build with big image (SDK), run with small image (runtime).
**Result:** Final image is much smaller.
**Example:** Stage 1 compiles code (uses SDK). Stage 2 runs code (uses runtime only).

### Registry
**Simple:** A place to store Docker images.
**Examples:**
  - Docker Hub: public registry
  - ACR: your private registry in Azure
**Usage:** Push images to registry, pull to run.

### Tag
**Simple:** A version label for a Docker image.
**Example:** `eventhub-consumer:v1` - "v1" is the tag.
**Common tags:** `latest`, `v1.0`, `prod`, `dev`.
**Best practice:** Use specific tags, not `latest`.

---

## .NET Terms

### .csproj
**Simple:** XML file that defines a .NET project.
**Contains:** Target framework, dependencies (NuGet packages), settings.
**Like:** package.json for npm, pom.xml for Maven.

### async/await
**Simple:** C# keywords for asynchronous programming.
**async:** Marks a function as asynchronous.
**await:** Waits for an async operation without blocking.
**Example:**
```csharp
async Task<string> GetDataAsync()
{
    var result = await httpClient.GetAsync(url);
    return await result.Content.ReadAsStringAsync();
}
```

### Console Application
**Simple:** A program that runs in a terminal (no GUI).
**Input:** Keyboard.
**Output:** Text in terminal.
**Example:** Your Producer and Consumer.

### DLL (Dynamic Link Library)
**Simple:** A compiled .NET program.
**Example:** `Consumer.dll` - your compiled Consumer app.
**Run:** `dotnet Consumer.dll`

### NuGet
**Simple:** Package manager for .NET (like npm for Node.js).
**Contains:** Libraries you can use in your app.
**Example:** Azure.Messaging.EventHubs from NuGet.
**Install:** `dotnet add package PackageName`

### Target Framework
**Simple:** Which version of .NET to use.
**Example:** `net8.0` = .NET 8.0
**Why:** Different versions have different features.
**Choose:** Usually latest stable version.

---

## Networking Terms

### Endpoint
**Simple:** A URL or address where a service is available.
**Example:** `sb://mynamespace.servicebus.windows.net` - Event Hubs endpoint.
**Like:** A phone number or street address.

### Port
**Simple:** A number that identifies a specific service on a computer.
**Example:** Port 80 = HTTP, Port 443 = HTTPS, Port 8080 = our Consumer.
**Why:** One computer can run many services (each on different port).
**Like:** Apartment numbers in a building.

### Protocol
**Simple:** Rules for how computers communicate.
**Examples:**
  - **HTTP/HTTPS:** Web traffic
  - **AMQP:** Messaging (what Event Hubs uses)
  - **TCP/IP:** General internet communication

---

## Development Terms

### Build
**Simple:** Converting source code into a runnable program.
**Command:** `dotnet build`
**Output:** DLL files you can run.
**Like:** Compiling - turning recipe into actual meal.

### Compile
**Simple:** Converting human-readable code into computer-readable code.
**Example:** C# → IL (Intermediate Language) → Machine code.
**Done by:** Compiler (part of .NET SDK).

### Debug
**Simple:** Finding and fixing bugs (errors) in code.
**Tools:** Breakpoints, logging, step-through execution.
**Environment:** `Development` (more verbose logging).

### Deploy
**Simple:** Putting your app on a server where users can access it.
**Example:** `kubectl apply` deploys Consumer to AKS.
**Steps:** Build → Push to registry → Pull and run on server.

### Publish
**Simple:** Creating a deployment-ready version of your app.
**Command:** `dotnet publish`
**Result:** Only files needed to run (no source code, debug symbols, etc.).
**Smaller:** Than build output.

### Runtime
**Simple:** The software that runs your compiled code.
**Example:** .NET Runtime - runs .NET applications.
**Contains:** Virtual machine, libraries, JIT compiler.
**Smaller:** Than SDK (no compilers, etc.).

### SDK (Software Development Kit)
**Simple:** Tools to build applications.
**Contains:** Compiler, runtime, debugger, libraries.
**Example:** .NET 8 SDK.
**Larger:** Than runtime alone.

---

## Terms for Your Presentation

### Decoupling
**Simple:** Making Producer and Consumer independent of each other.
**Benefit:** Can update one without affecting the other.
**How:** Event Hubs sits in the middle.
**Like:** Using email instead of talking directly.

### High Availability
**Simple:** System keeps working even if parts fail.
**Example:** Run 2 Consumer replicas - if one crashes, other keeps working.
**Goal:** Minimize downtime.

### Scalability
**Simple:** Ability to handle more load by adding resources.
**Horizontal:** Add more replicas (scale out).
**Vertical:** Use bigger machines (scale up).
**Example:** 1 Consumer → 10 Consumers to handle 10x traffic.

### Observability
**Simple:** Being able to see what your system is doing.
**How:** Logs, metrics, traces.
**Example:** `kubectl logs` shows what Consumer is doing.

### Reliability
**Simple:** System works consistently and correctly.
**Features:** Retries, error handling, checkpointing.
**Goal:** Don't lose messages.

---

## Quick Reference

**Most Important Terms:**
- **Event Hubs**: Message queue
- **AKS**: Kubernetes in Azure
- **Pod**: Running container
- **kubectl**: Kubernetes CLI
- **ConfigMap**: Non-secret config
- **Secret**: Sensitive config
- **Deployment**: How to run your app

**Most Common Commands:**
- `dotnet run` - Run Producer
- `kubectl get pods` - List pods
- `kubectl logs <pod>` - View logs
- `kubectl apply -f <file>` - Deploy
- `docker build` - Build image
- `docker push` - Push to registry

---

**Print this glossary and keep it handy during your presentation!**

