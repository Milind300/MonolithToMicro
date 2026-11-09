# MonolithToMicro

**Enterprise Application Modernization | .NET 10 | Microservices Architecture | Microsoft Azure | DevOps**

MonolithToMicro is an architecture-focused Proof of Concept (PoC) exploring the design and implementation patterns required to modernize a monolithic ASP.NET Core application into independently deployable microservices.

The solution demonstrates service-oriented application design, domain-aligned persistence, distributed transaction patterns, containerization, cloud deployment, identity-based security, and deployment automation using the Microsoft technology stack.

The repository provides a working microservices reference implementation. It does not currently include the source code of a legacy monolith or a completed migration from an existing monolithic application.

## 1. Architecture Overview

The solution is organized into four business-oriented APIs:

| Service | Responsibility |
|---|---|
| Catalog API | Catalog item management |
| Product API | Product inventory and availability |
| Order API | Customer order processing and integration workflows |
| User API | Customer profile management |

Each service implements a layered structure:

`Controller → Application Service → Repository → EF Core DbContext → SQL Server`

Services maintain their own persistence models and Entity Framework Core migrations.

### Architectural Principles

- Business-capability-based service boundaries
- Independent service build and deployment
- Explicit API contracts
- Service ownership of persistence models
- Dependency injection and separation of concerns
- Asynchronous data access
- Transactional consistency within service boundaries
- Secure cloud authentication without embedded credentials
- Automated, repeatable deployment processes

## 2. Technology Stack

| Area | Technology |
|---|---|
| Application framework | ASP.NET Core 10 |
| Language | C# |
| Persistence | Entity Framework Core 10 |
| Database | Microsoft SQL Server / Azure SQL Database |
| Containerization | Docker and Docker Compose |
| Cloud hosting | Azure App Service |
| Object storage | Azure Blob Storage |
| Cloud identity | Microsoft Entra managed identities |
| Source control | Git and GitHub |
| CI/CD | GitHub Actions — integration in progress |
| API documentation | Swagger / OpenAPI in development |

## 3. Data Architecture

The local implementation uses a shared SQL Server database, `ECommerceDb`, with service-specific schemas:

| Service | Schema |
|---|---|
| Catalog | `catalog` |
| Product | `product` |
| Order | `orders` |
| User | `users` |

Each service maintains its own `DbContext` and migration history.

This approach provides **logical data ownership**, not full physical database isolation. Separate databases per service remain an architectural option when operational, scaling, or organizational requirements justify that change.

The design avoids introducing direct cross-service database dependencies as an integration mechanism.

## 4. Distributed Transaction Pattern

The Order API demonstrates the transactional outbox pattern.

When an order is created, the order entity and a corresponding `OrderPlaced.v1` outbox message are persisted as part of the same database transaction.

This establishes the persistence foundation for reliable event publication.

An automated outbox dispatcher, message broker integration, consumer processing, and end-to-end delivery guarantees are not yet implemented.

## 5. Containerization

The APIs include Dockerfiles for containerized deployment.

Docker Compose is configured for the Product and Order services, supporting local development and integration scenarios.

Container-to-database connectivity is configured through local environment settings. Credentials are excluded from source control.

## 6. Microsoft Azure Deployment

The Product API has been deployed and validated using Microsoft Azure.

### Implemented Cloud Components

| Component | Implementation |
|---|---|
| Azure App Service | Windows Free F1 hosting for Product API |
| Azure SQL Database | Serverless database within configured free usage limits |
| Microsoft Entra identity | System-assigned managed identity for Product API |
| Database authorization | Schema-scoped SQL permissions |
| Azure Blob Storage | Private container with verified upload/download |
| Database schema management | EF Core migrations applied to Azure SQL |
| Network controls | SQL firewall restricted to configured application outbound IPs |

### Deployment Validation

The deployed Product API has been verified through the following endpoints:

| Endpoint | Verified result |
|---|---|
| `/health` | HTTP 200 — `Healthy` |
| `/api/product` | HTTP 200 — `[]` |

The Product API accesses Azure SQL using passwordless managed identity authentication.

The current `/health` endpoint is an application liveness check; the successful `/api/product` request also validates the database-backed execution path.

Only the Product API has been verified on Azure. The other services remain part of the local implementation.

## 7. CI/CD Strategy

GitHub Actions is the selected CI/CD platform.

The intended pipeline includes:

1. Source checkout and .NET SDK initialization
2. Dependency restoration and Release build
3. Automated test execution
4. Product API-specific publishing
5. Artifact-based deployment to Azure App Service
6. Federated Azure authentication using OpenID Connect
7. Post-deployment endpoint verification

The workflow is under implementation and will be validated before continuous deployment is enabled.

## 8. Repository Structure

```text
MonolithToMicro/
├── src/
│   ├── MonolithToMicro.Catalog.Api/
│   ├── MonolithToMicro.Product.Api/
│   ├── MonolithToMicro.Order.Api/
│   └── MonolithToMicro.User.Api/
├── tests/
├── scripts/
├── compose.order-product.yaml
├── MonolithToMicro.slnx
├── Directory.Build.props
└── README.md
```

Personal planning material, local environment secrets, and generated deployment artifacts are excluded from the public repository.

## 9. Build and Deployment

### Prerequisites

- .NET 10 SDK
- SQL Server for local persistence
- Docker Engine for containerized execution
- EF Core CLI tools for migration management

### Build

From the repository root:

```powershell
dotnet build .\MonolithToMicro.slnx --configuration Release
```

### Run automated tests

```powershell
dotnet test .\MonolithToMicro.slnx --configuration Release
```

### Publish Product API

```powershell
dotnet publish .\src\MonolithToMicro.Product.Api\MonolithToMicro.Product.Api.csproj --configuration Release --output .\publish
```

For database initialization and local environment configuration, refer to the scripts and service configuration files in the repository. Provide credentials using environment variables or another secure local configuration mechanism.

## 10. Architecture Considerations and Scope

The implementation demonstrates selected modernization patterns, but is not presented as a production-ready commerce platform.

Areas requiring further implementation or validation include:

- Centralized authentication and authorization
- Comprehensive automated testing
- Message broker integration and outbox dispatch
- Distributed observability and tracing
- Retry policies, idempotency, and concurrency controls
- Production-grade API validation and security hardening
- Deployment rollback and operational resilience
- Independent cloud deployment of the remaining services

These concerns represent explicit architecture and engineering considerations rather than completed implementation claims.

## 11. Purpose

The purpose of this PoC is to demonstrate practical competency in evaluating modernization approaches, establishing service boundaries, implementing .NET microservices, integrating Azure platform capabilities, and designing secure, repeatable delivery processes.

The emphasis is on **architecture decisions supported by executable implementation and verifiable deployment evidence**.