<div align="center">

# MonolithToMicro

### Enterprise Application Modernization · .NET 10 · Azure · Kubernetes · GitHub Actions

**An executable, Azure-hosted microservices and DevOps proof of concept**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Azure](https://img.shields.io/badge/Microsoft-Azure-0078D4?logo=microsoftazure&logoColor=white)
![Kubernetes](https://img.shields.io/badge/Kubernetes-AKS-326CE5?logo=kubernetes&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-2088FF?logo=githubactions&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Containers-2496ED?logo=docker&logoColor=white)
![Status](https://img.shields.io/badge/AKS-Stopped_after_validation-64748B)

[**AKS CI/CD workflow**](.github/workflows/aks-ci-cd.yml) · [**App Service CI/CD workflow**](.github/workflows/product-api-ci-cd.yml) · [**Verified AKS deployment**](https://github.com/Milind300/MonolithToMicro/actions/runs/42902156585)

</div>

> [!IMPORTANT]
> This is a **working architecture-focused proof of concept**, not a completed migration from an existing monolith or a production-ready commerce platform. AKS was **stopped after validation** to control costs. The latest Azure Function consumer invocation was **not independently verified**.

## At a glance

| **4 / 4 APIs** | **4 / 4 healthy pods** | **CI/CD deployed to AKS** | **Service Bus publisher verified** |
|:---:|:---:|:---:|:---:|
| Catalog · Product · Order · User | `1/1` ready each, zero restarts at verification | GitHub Actions Run #2 and Run #3 succeeded | Event ID correlated with SQL outbox |

| Engineering layer | Delivered and observed |
|---|---|
| Application | ASP.NET Core 10, C#, four business-oriented APIs |
| Delivery | .NET tests → Docker build → ACR → AKS rollout verification |
| Security | GitHub OIDC, Microsoft Entra ID, managed identities, namespace-scoped Kubernetes RBAC |
| Data | Azure SQL, EF Core, transactional outbox |
| Messaging | Azure Service Bus sender verified from AKS; Azure Function consumer deployed, invocation unconfirmed |
| Operations | Health checks, rollout status, image SHA verification, deliberate AKS shutdown |

## 1. Current architecture — deployment view

The vertical view below mirrors the **source → pipeline → registry → Kubernetes → data and messaging** design. Each stage is an actual component of the project.

```mermaid
flowchart TB
    GH["GitHub repository<br/>main branch · commit SHA"]
    CI["GitHub Actions CI/CD<br/>Build · test · Docker · OIDC"]
    ACR["Azure Container Registry<br/>Catalog · Product · Order · User images"]

    subgraph K8S["Azure Kubernetes Service · namespace: monolithtomicro"]
      direction LR
      CAT["Catalog API<br/>1/1 ready"]
      PROD["Product API<br/>1/1 ready"]
      ORD["Order API<br/>1/1 ready"]
      USER["User API<br/>1/1 ready"]
    end

    SQL["Azure SQL Database<br/>Product + orders schemas"]
    BUS["Azure Service Bus<br/>order-placed queue"]
    FUNC["Azure Function<br/>ProcessOrderFromQueue"]

    GH --> CI --> ACR --> K8S
    PROD --> SQL
    ORD --> SQL
    ORD -->|"Publisher verified"| BUS
    BUS -.->|"Trigger configured; processing unverified"| FUNC

    classDef source fill:#e0f2fe,stroke:#0284c7,color:#0c4a6e,stroke-width:1.5px;
    classDef delivery fill:#ede9fe,stroke:#7c3aed,color:#4c1d95,stroke-width:1.5px;
    classDef service fill:#ecfdf5,stroke:#059669,color:#065f46,stroke-width:1.5px;
    classDef data fill:#fff7ed,stroke:#ea580c,color:#7c2d12,stroke-width:1.5px;
    classDef pending fill:#f1f5f9,stroke:#64748b,color:#334155,stroke-dasharray: 5 4;
    class GH source;
    class CI,ACR delivery;
    class CAT,PROD,ORD,USER service;
    class SQL,BUS data;
    class FUNC pending;
    style K8S fill:#f8fafc,stroke:#2563eb,stroke-width:2px,color:#1e3a8a
```

> **Deployment-state note:** The readiness labels describe the **last successful running-state verification**, not live status while AKS is stopped. Only Product and Order Azure SQL paths were functionally tested; the diagram does not imply every service has a verified Azure SQL workload.

## 2. Architectural view — solution architect

This view separates **developer delivery**, **cloud execution**, **data/integration**, and **identity/security boundaries**.

```mermaid
flowchart LR
    subgraph DELIVERY["Delivery plane · GitHub"]
      SRC["Source control"] --> TEST[".NET 10 tests"] --> BUILD["Docker image builds"]
    end

    subgraph SECURITY["Identity and access"]
      OIDC["GitHub OIDC federation"]
      ENTRA["Microsoft Entra ID"]
      KRBAC["Kubernetes namespace RBAC"]
      WI["AKS workload identities"]
    end

    subgraph AZURE["Azure execution plane · Central India"]
      REG["ACR"] --> AKS["AKS · four microservices"]
      APP["Product API · App Service"]
      SQL["Azure SQL"]
      BUS["Service Bus queue"]
      FUN["Azure Function"]
    end

    BUILD --> REG
    SRC -.-> APP
    OIDC --> ENTRA --> KRBAC --> AKS
    WI --> SQL
    WI --> BUS
    AKS --> SQL
    AKS --> BUS
    BUS -.-> FUN

    classDef blue fill:#dbeafe,stroke:#2563eb,color:#1e40af;
    classDef violet fill:#ede9fe,stroke:#7c3aed,color:#4c1d95;
    classDef green fill:#dcfce7,stroke:#16a34a,color:#166534;
    classDef amber fill:#fef3c7,stroke:#d97706,color:#92400e;
    class SRC,TEST,BUILD blue;
    class OIDC,ENTRA,KRBAC,WI violet;
    class REG,AKS,APP green;
    class SQL,BUS,FUN amber;
    style DELIVERY fill:#f8fafc,stroke:#60a5fa,color:#1e40af
    style SECURITY fill:#faf5ff,stroke:#a78bfa,color:#4c1d95
    style AZURE fill:#f0fdf4,stroke:#4ade80,color:#166534
```

**Architectural decisions**

| Decision | Rationale | Trade-off |
|---|---|---|
| Business-aligned services | Independent application responsibilities and deployments | Increased distributed-system complexity |
| Logical SQL schemas | Service-oriented persistence models within one SQL deployment | Not full physical database isolation |
| Internal `ClusterIP` services | Avoid exposing each API publicly | External access needs ingress/gateway if introduced |
| Transactional outbox | Atomic persistence of order + integration event | Requires polling, retry, idempotency controls |
| GitHub OIDC + managed identities | Removes long-lived Azure credentials from application and CI workflows | Requires federation, RBAC, and credential configuration |
| Manual AKS deployment workflow | Controlled portfolio deployments and Azure spend | No automatic promotion on each commit |

### Azure infrastructure — where, how, and why each resource is used

The architecture separates **delivery**, **application hosting**, **data and messaging**, and **identity and access**. The table explains the **placement**, **actual integration**, and **purpose** of every Azure resource used by this project.

| Azure resource | Where it fits | How this project uses it | Why it is needed |
|---|---|---|---|
| **Azure App Service** | Alternative application-hosting path, separate from AKS | Hosts the Product API deployment from `product-api-ci-cd.yml`; the App Service health and product endpoints were validated | Demonstrates managed PaaS hosting and a simpler deployment model alongside containers |
| **Azure SQL Database** | Data layer, outside the AKS cluster | Product and Order APIs access the Azure SQL serverless database using Microsoft Entra authentication; the `product` and `orders` schemas hold domain data and outbox records | Durable relational persistence, transactions, and consistent order/outbox writes |
| **Azure Blob Storage** | Private object-storage layer | A private blob container was used for a verified upload/download operation; it is **not** a core dependency in the verified AKS order-processing flow | Demonstrates private cloud object storage for file-like content |
| **Azure Container Registry (ACR)** | Container image supply chain, between GitHub Actions and AKS | GitHub Actions builds and pushes four images tagged with the Git commit SHA; AKS pulls the published images | Central registry and traceability of deployed container versions |
| **Azure Kubernetes Service (AKS)** | Application execution and orchestration | Runs Catalog, Product, Order, and User Deployments in the `monolithtomicro` namespace, each with an internal Kubernetes Service | Independent deployments, scheduling, networking, health probes, and controlled rollouts |
| **Microsoft Entra ID** | Shared identity and authentication boundary | Authenticates GitHub's federated deployment identity, AKS administrators, and Product/Order workload identities | Removes dependence on long-lived Azure credentials and supports centrally managed identities |
| **User-assigned managed identities** | CI/CD identity and AKS workload identity | A dedicated GitHub identity pushes images and updates Deployments; separate Product and Order identities access Azure SQL; the Order identity also sends to Service Bus | Separates privileges by workload and enables passwordless Azure service access |
| **Azure Service Bus** | Asynchronous messaging layer, outside AKS | The Order API's `OutboxPublisher` sends `OrderPlaced.v1` events to the `order-placed` **queue** using managed identity | Decouples order persistence from downstream asynchronous processing |
| **Azure Functions** | Serverless event-consumer layer, outside AKS | `ProcessOrderFromQueue` is deployed with a Service Bus trigger; successful handling of the latest test message is **not independently verified** | Demonstrates an event-driven consumer without hosting a dedicated Kubernetes worker |

#### Resource interactions and boundaries

```mermaid
flowchart LR
    subgraph DELIVERY["Delivery plane"]
        GHA["GitHub Actions"] -->|"OIDC federation"| ENTRA["Microsoft Entra ID"]
        GHA -->|"Build / push images"| ACR["Azure Container Registry"]
    end

    subgraph COMPUTE["Application hosting"]
        APP["Azure App Service<br/>Product API alternate host"]
        subgraph AKS["AKS · monolithtomicro"]
            CAT["Catalog API"]
            PROD["Product API"]
            ORD["Order API"]
            USER["User API"]
        end
    end

    subgraph DATA["Data and integration"]
        SQL["Azure SQL Database"]
        BLOB["Private Azure Blob Storage"]
        SB["Service Bus queue<br/>order-placed"]
        FUN["Azure Function<br/>queue trigger"]
    end

    ACR -->|"Pull images"| AKS
    GHA -->|"AKS deployment"| AKS
    GHA -->|"Separate App Service workflow"| APP
    PROD -->|"Workload identity"| SQL
    ORD -->|"Workload identity"| SQL
    ORD -->|"Managed identity · publish"| SB
    APP -->|"Managed identity"| SQL
    SB -.->|"Consumer configured; success unverified"| FUN

    classDef delivery fill:#dbeafe,stroke:#2563eb,color:#1e40af;
    classDef compute fill:#dcfce7,stroke:#16a34a,color:#166534;
    classDef data fill:#ffedd5,stroke:#ea580c,color:#9a3412;
    classDef security fill:#ede9fe,stroke:#7c3aed,color:#5b21b6;
    class GHA,ACR delivery;
    class APP,CAT,PROD,ORD,USER compute;
    class SQL,BLOB,SB,FUN data;
    class ENTRA security;
    style DELIVERY fill:#f8fafc,stroke:#93c5fd,color:#1e40af
    style COMPUTE fill:#f0fdf4,stroke:#86efac,color:#166534
    style DATA fill:#fff7ed,stroke:#fdba74,color:#9a3412
```

The Blob Storage box is deliberately **not connected to the order-processing flow** because its validated use was a separate storage exercise. Likewise, the Function connection is dotted because consumer execution of the latest published message was not conclusively observed.

#### AKS configuration and network exposure

| Setting | Verified configuration | Operational meaning |
|---|---|---|
| Cluster | `aks-monolithtomicro-day13` | Managed Kubernetes control plane for the four APIs |
| Azure region | Central India | Region hosting the AKS environment |
| Resource group | `rg-monolithtomicro-aks-day13` | Azure resource-management boundary for this AKS deployment |
| Namespace | `monolithtomicro` | Logical workload boundary used for Deployments, Services, and RBAC |
| Authentication | Managed Microsoft Entra integration | Users and CI/CD authenticate through Entra identities |
| Authorization | Native Kubernetes RBAC | GitHub deployer receives namespace-scoped Deployment permissions rather than cluster-admin |
| API exposure | **Four internal `ClusterIP` Services on port `8080`** | Stable in-cluster networking; **no public AKS API endpoint or ingress was configured for these services** |
| Validation access | `kubectl port-forward` | Temporary access from the authenticated developer machine for `/health` and selected API requests |
| Runtime verification | Four Deployments `1/1` Ready, zero pod restarts | Last observed running-state health, **not live status after AKS shutdown** |
| Current AKS state | `Stopped` (provisioning `Succeeded`) | Worker compute stopped after validation to reduce charges; application endpoints are unavailable while stopped |

**Why `ClusterIP`?** It exposes each API **only within the Kubernetes cluster**, reducing accidental internet exposure during a proof of concept. Other pods can address a Service through Kubernetes DNS. Testing was performed through authenticated `kubectl port-forward`, which does **not** publish a permanent public endpoint. If external clients are required later, a separately designed ingress or API gateway, TLS, authentication, and network policies will be needed.

**Service boundary note:** `ClusterIP` controls Kubernetes Service exposure; it does **not** by itself guarantee pod-to-pod traffic isolation, authorize API callers, or make outbound Azure dependencies private. Those are separate network and application security concerns.

## 3. Four microservices

| Service | Responsibility | Verified in AKS |
|---|---|---|
| **Catalog API** | Catalog operations | `GET /health` → HTTP 200 |
| **Product API** | Product inventory / availability | `GET /health` and `GET /api/product` → HTTP 200; SQL-backed product returned |
| **Order API** | Orders, SQL transactions, outbox publisher | `GET /health`, `GET /api/order`, `POST /api/order` and Service Bus publishing verified |
| **User API** | Customer profile API | `GET /health` → HTTP 200 |

The codebase uses controller, application-service, repository, and EF Core patterns where implemented. Services have independent container build definitions. The solution does not claim complete feature parity with a legacy commerce platform.

## 4. CI/CD architecture — DevOps view

Two workflows coexist without overwriting each other:

- **Product API → Azure App Service:** [product-api-ci-cd.yml](.github/workflows/product-api-ci-cd.yml)
- **Four APIs → ACR → AKS:** [aks-ci-cd.yml](.github/workflows/aks-ci-cd.yml)

```mermaid
flowchart TD
    START["Manual dispatch · main branch"]
    CHECK["Checkout source"]
    DOTNET["Install .NET 10 · run Order tests"]
    LOGIN["Azure login · GitHub OIDC"]
    DOCKER["Build four Docker images"]
    PUSH["Push full Git-SHA tags to ACR"]
    ACCESS["kubelogin + AKS user context"]
    AUTH["Verify namespace deployment permission"]
    UPDATE["kubectl set image · four Deployments"]
    ROLLOUT["kubectl rollout status"]
    VERIFY["Verify expected SHA tags and deployments"]

    START --> CHECK --> DOTNET --> LOGIN --> DOCKER --> PUSH
    PUSH --> ACCESS --> AUTH --> UPDATE --> ROLLOUT --> VERIFY

    classDef step fill:#eff6ff,stroke:#3b82f6,color:#1e3a8a,stroke-width:1px;
    classDef security fill:#f5f3ff,stroke:#8b5cf6,color:#5b21b6,stroke-width:1px;
    classDef success fill:#ecfdf5,stroke:#10b981,color:#065f46,stroke-width:1.5px;
    class START,CHECK,DOTNET,DOCKER,PUSH,UPDATE step;
    class LOGIN,ACCESS,AUTH security;
    class ROLLOUT,VERIFY success;
```

### Verified GitHub Actions executions

| Run | Commit | Observed outcome |
|---|---|---|
| Initial image publishing | `940c04c` | Four SHA-tagged images pushed to ACR |
| [First end-to-end AKS deployment](https://github.com/Milind300/MonolithToMicro/actions/runs/42902156585) | `cdbe47f` | Both jobs successful; total **4m 11s** |
| [Updated publisher-capable Order API deployment](https://github.com/Milind300/MonolithToMicro/actions/runs/42907582391) | `e8693a3` | Both jobs successful; Service Bus publisher enabled in subsequent AKS configuration |

The workflow deploys images with `kubectl set image` and checks rollouts, preserving existing ServiceAccount and Secret references. It does **not** implement automatic rollback or full application acceptance testing.

## 5. Azure SQL — logical data architecture

The local solution uses SQL Server and domain-oriented schemas; the Azure test verified Product and Order operations against an Azure SQL database.

```mermaid
flowchart LR
    subgraph APIS["Application boundaries"]
      PRODUCT["Product API"]
      ORDER["Order API"]
    end

    subgraph DB["Azure SQL Database"]
      P["product schema<br/>Product data"]
      O["orders schema<br/>CustomerOrders"]
      X["orders schema<br/>OutboxMessages"]
    end

    PRODUCT --> P
    ORDER --> O
    ORDER --> X
    O -.- X

    classDef app fill:#dbeafe,stroke:#2563eb,color:#1d4ed8;
    classDef table fill:#ecfdf5,stroke:#059669,color:#065f46;
    class PRODUCT,ORDER app;
    class P,O,X table;
    style APIS fill:#eff6ff,stroke:#93c5fd,color:#1e40af
    style DB fill:#f0fdf4,stroke:#86efac,color:#166534
```

| Tested API call | Verified response |
|---|---|
| `GET /api/product` | HTTP 200 with `AKS-DEMO-001` product data |
| `GET /api/order` | HTTP 200 with previously stored orders |
| `POST /api/order` | New order inserted and outbox event created |

The first Product SQL-backed request timed out; a subsequent request succeeded while the database was online. The exact cause of the initial delay remains unconfirmed.

## 6. Service Bus — event-driven architecture

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant Order as AKS Order API
    participant DB as Azure SQL
    participant Publisher as OutboxPublisher
    participant Bus as Service Bus queue
    participant Function as Azure Function

    Client->>Order: POST /api/order
    Order->>DB: Insert CustomerOrder + OutboxMessage in one transaction
    DB-->>Order: Transaction committed
    Order-->>Client: HTTP 201 Created
    loop Poll unpublished messages
      Publisher->>DB: Find PublishedAtUtc IS NULL
      DB-->>Publisher: OrderPlaced.v1
      Publisher->>Bus: Send event using managed identity
      Bus-->>Publisher: Send completes
      Publisher->>DB: Set PublishedAtUtc
    end
    Bus-->>Function: Trigger configured
    Note over Function: Successful processing of this test event NOT independently confirmed
```

### Correlated publishing evidence

| Event field | Verified value |
|---|---|
| Order ID | `b6704cfe-3391-4b60-998a-5f2d6d02c86a` |
| Outbox message ID | `3e844ae6-52f1-47a0-943b-cece8a24289e` |
| Event type | `OrderPlaced.v1` |
| Created at (UTC) | `2026-11-10 21:50:52` |
| Published at (UTC) | `2026-11-10 21:51:03` |

The AKS Order API logged successful publication, and the SQL row had a non-null `PublishedAtUtc` for the **same message**. Service Bus reported **0 active** and **1 previously existing dead-letter** message after the test; the dead-letter count did not increase.

> [!NOTE]
> **Verified:** Order creation → SQL outbox → Azure Service Bus publication from AKS. **Not independently verified:** successful processing of this exact event by `ProcessOrderFromQueue`. The Function trigger is deployed, but its Invocations view required Application Insights, which was not configured. The outbox publication timestamp does **not** serve as consumer acknowledgment.

## 7. Identity, security, and authorization

```mermaid
flowchart TB
    GH["GitHub Actions · main"] -->|"OIDC token"| ID["Microsoft Entra federation"]
    ID --> DEPLOY["id-github-aks-deploy"]
    DEPLOY -->|"AcrPush · registry scope"| ACR["Azure Container Registry"]
    DEPLOY -->|"AKS Cluster User · cluster scope"| AKS["AKS credentials"]
    AKS -->|"Namespace-scoped RoleBinding"| ROLE["github-aks-deployer · monolithtomicro"]
    ROLE --> WORK["Patch/update Deployments"]

    subgraph RUNTIME["Runtime workload identities"]
      P["Product service account"] --> PID["Product managed identity"] --> SQL["Azure SQL"]
      O["Order service account"] --> OID["Order managed identity"]
      OID --> SQL
      OID --> BUS["Service Bus · Data Sender on order-placed"]
    end

    classDef identity fill:#ede9fe,stroke:#7c3aed,color:#4c1d95;
    classDef target fill:#dcfce7,stroke:#16a34a,color:#166534;
    classDef scope fill:#e0f2fe,stroke:#0284c7,color:#0c4a6e;
    class GH,ID,DEPLOY,PID,OID identity;
    class ACR,SQL,BUS,WORK target;
    class AKS,ROLE,P,O scope;
    style RUNTIME fill:#faf5ff,stroke:#a78bfa,color:#4c1d95
```

**Security posture:** No Service Bus connection string is needed in the AKS Order API: it uses `DefaultAzureCredential` with Workload Identity. SQL connection configuration is supplied using existing Kubernetes Secret references; the Secret value must **not** be committed to Git.

**Important boundary:** A namespace-scoped right to modify Deployments is still powerful and can indirectly expose credentials used by those pods. This is a portfolio proof of concept, not a claim of zero-risk production isolation.

## 8. Operational verification — evidence

| Check | Actual result |
|---|---|
| Kubernetes Deployments | Catalog, Product, Order, User all `1/1` Ready |
| Kubernetes pods | Four Running, zero restarts at verification |
| Internal services | Four `ClusterIP` services on port `8080` |
| HTTP health | All four `GET /health` endpoints returned **200 Healthy** |
| Product database | `GET /api/product` returned **200** with product data |
| Order database | `GET /api/order` returned **200** with order data |
| Order creation | `POST /api/order` succeeded; outbox row created |
| Outbox publisher | Explicit `OrderPlaced.v1` success log and SQL published timestamp |
| Azure Function consumer | Trigger configured; latest per-message invocation **not verified** |
| AKS shutdown | `PowerState: Stopped`, `ProvisioningState: Succeeded` |

<details>
<summary><strong>Read-only Kubernetes verification commands</strong></summary>

Run only when AKS is started and your `kubectl` context is authenticated:

```powershell
kubectl get deployments -n monolithtomicro -o wide
kubectl get pods -n monolithtomicro
kubectl get services -n monolithtomicro
kubectl rollout status deployment/order-api -n monolithtomicro
```

</details>

## 9. Repository layout and reproducibility

```text
MonolithToMicro/
├── .github/workflows/
│   ├── product-api-ci-cd.yml
│   └── aks-ci-cd.yml
├── src/
│   ├── MonolithToMicro.Catalog.Api/
│   ├── MonolithToMicro.Product.Api/
│   ├── MonolithToMicro.Order.Api/
│   └── MonolithToMicro.User.Api/
├── tests/MonolithToMicro.Order.Tests/
├── k8s/
│   └── aks/order-api.yaml
├── scripts/
├── Directory.Build.props
├── MonolithToMicro.slnx
└── README.md
```

The tree highlights the **known tracked core paths**, not every local file. Additional AKS manifests, a Function project, setup scripts and troubleshooting materials were present locally but **were not all committed** at the last Git status review. As a result, the entire Azure environment is **not yet reproducible from this repository alone**.

> [!CAUTION]
> Keep private `/docs/` files, local secrets, connection strings, SQL exports, backup snapshots, and other potentially sensitive artifacts out of Git. Review untracked scripts and Kubernetes snapshots individually before publishing. Do not use `git add .` indiscriminately.

### Build and tests

```powershell
dotnet build .\MonolithToMicro.slnx --configuration Release
dotnet test .\tests\MonolithToMicro.Order.Tests\MonolithToMicro.Order.Tests.csproj --configuration Release
```

### Docker build example

```powershell
docker build -f src/MonolithToMicro.Order.Api/Dockerfile -t order-api:local .
```

### CI/CD operation

The AKS workflow currently uses a manual `workflow_dispatch` trigger on `main`. It pushes Git-SHA-tagged images and updates the four existing Kubernetes Deployments. AKS must be running for the deployment stage.

## 10. Troubleshooting — real engineering lessons

<details>
<summary><strong>Microsoft Entra and kubelogin authentication</strong></summary>

After Entra integration was enabled, `kubectl` could not find the `kubelogin` executable. Azure CLI had installed it, but the active PowerShell session did not have its directory in `PATH`. Updating the session PATH and using `kubelogin convert-kubeconfig -l azurecli` restored access. Cloud authentication and Kubernetes authorization were checked separately.

</details>

<details>
<summary><strong>Azure SQL request timeout</strong></summary>

An initial database-backed Product API GET timed out. Azure SQL was subsequently confirmed online, and a retry succeeded with HTTP 200 and product data. An SQL serverless cold start is a possibility, not a proven root cause.

</details>

<details>
<summary><strong>Service Bus dead-letter troubleshooting</strong></summary>

An earlier Function consumer expected a different JSON property shape and a message entered the dead-letter queue. The consumer parser was corrected and republished. Later AKS publisher success was verified, but Function invocation logs were unavailable because Application Insights was not configured.

</details>

<details>
<summary><strong>GitHub Actions and PowerShell tooling</strong></summary>

Initial YAML-writing and local validation attempts failed because of PowerShell quoting/here-string handling and unavailable local parsers. The workflow was saved in VS Code and ultimately verified by real successful GitHub Actions executions. Git whitespace validation does not itself validate YAML semantics.

</details>

<details>
<summary><strong>Configuration drift</strong></summary>

The Service Bus publisher was initially enabled by updating the running Order Deployment environment variables. The corresponding configuration was then recorded in `k8s/aks/order-api.yaml` and committed. The CI/CD deployment uses image updates rather than applying every manifest file.

</details>

## 11. Limitations and next engineering steps

| Area | Current boundary | Next improvement |
|---|---|---|
| Azure Function consumer | No per-message success evidence | Add correlated consumer telemetry or a processed-events record |
| Testing | Existing Order tests run in CI | Expand service-level and integration coverage |
| Rollback | Sequential rollout checks; no automatic rollback | Add rollback strategy and deployment gates |
| Availability | Single-replica PoC | Multi-replica workloads, PDBs and capacity planning |
| Observability | Limited Function invocation history | Structured logs and distributed tracing with cost controls |
| Infrastructure | Some AKS resources provisioned manually | Move to reviewed Bicep/Terraform IaC |
| Security | Trusted `main` workflow and scoped roles | GitHub environment approvals, policy enforcement, image scanning |
| Reproducibility | Some local manifests and Function source untracked | Review and publish non-sensitive infrastructure assets |

## 12. Cost-aware operations

The AKS cluster was **stopped after successful validation**. This suspends worker-node compute but does not delete the cluster or necessarily eliminate charges for Azure Container Registry, storage, networking, SQL, Service Bus, Functions, and related resources.

The Azure promotional credit expiry was reported as **November 19, 2026** during the project. Review Azure Cost Management and remaining chargeable resources before that date. Do not delete AKS managed-resource-group components individually.

## 13. Portfolio outcome

**Verified:** Four independently deployed .NET APIs, real GitHub Actions deployments, Git-SHA-tagged images, AKS rollout and health verification, Azure SQL connectivity, and Azure Service Bus outbox publication from AKS.

**Not claimed:** A completed legacy migration, production readiness, comprehensive high availability, or independently verified processing of the latest test event by Azure Functions.

This project demonstrates **solution architecture, production-oriented .NET engineering practices, and hands-on Azure DevOps delivery**, with explicit evidence and stated limitations.

---

<div align="center">

**Source-controlled architecture · Passwordless CI/CD · Verified Azure deployments · Cost-aware operations**

</div>
