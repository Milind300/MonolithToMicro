# MonolithToMicro — .NET 10 E-commerce Microservices

A **four-service** local backend starter for Azure/.NET interview study. Built directly as microservices; despite its name, it does **not** include a migrated monolith.

## What is implemented

- Four ASP.NET Core 10 controller-based APIs: **Catalog**, **Product/Inventory**, **Order**, and **User**.
- Each API follows **Controller → Service → Repository → EF Core DbContext → SQL Server**.
- One local SQL Server database `ECommerceDb` with service-owned SQL schemas `catalog`, `product`, `orders`, `users`.
- Independent DbContexts and independent EF migrations history tables in each schema.
- Async CRUD, DI, GET/POST/PUT/DELETE endpoints, development Swagger and `/health` liveness routes.
- On Order create, a **local transactional outbox record** (`OrderPlaced.v1`) is stored in the same `SaveChanges` as the order.
- Multi-stage Dockerfile per API, plus SQL creation and migrations scripts.

## Scope limitations (important)

This is a **learning starter, not production-ready**. The current Order model is intentionally minimal; it does **not** yet contain order lines, stock reservations, payment processing, an outbox dispatcher or Azure Service Bus delivery. Authentication, Entra ID, authorization, input validation, test projects, distributed tracing, retry/idempotency controls, concurrency protection, and inter-service messaging are **planned later**. Never expose these unauthenticated CRUD endpoints publicly. No Azure resources are created.

### Prerequisites

- **Visual Studio 2026** (for .NET 10) or the .NET 10 SDK.
- Local SQL Server Developer Edition and optionally SSMS. Database-First scripts are only used to create the empty DB/schema; **tables are Code-First via EF Core migrations**.
- `dotnet-ef` global tool version 10.x for CLI migrations.
- NuGet network connectivity to restore packages on initial build.

### First build

Open `MonolithToMicro.slnx` in Visual Studio 2026; build the solution, or use PowerShell at the repository root:

```powershell
./scripts/build.ps1
```

### Prepare SQL Server and create Code-First tables

1. Run `scripts/create-database.sql` in SSMS against local SQL Server.
2. If necessary, change `ConnectionStrings:ECommerceDb` in **each service** using environment variables or user secrets; defaults to local Windows Auth on `localhost`.
3. Install the EF CLI tool: `./scripts/add-ef-tool.ps1`.
4. Generate initial migrations: `./scripts/ef-migrations.ps1`.
5. **Review and commit migrations** (they are generated against your real EF tool version).
6. Apply: `./scripts/apply-migrations.ps1`.

Note: This archive ships with EF models/configuration and migration **generation scripts**, but no pre-generated migrations. Migrations require `dotnet ef` and an installed .NET SDK; they must be generated before `database update`.

### Run with Visual Studio

Configure multiple startup projects or launch the four APIs individually using their HTTP launch profiles. Default local endpoints:

| API | URL | Swagger |
|---|---|---|
| Catalog | http://localhost:5101 | /swagger |
| Product | http://localhost:5102 | /swagger |
| Order | http://localhost:5103 | /swagger |
| User | http://localhost:5104 | /swagger |

Sample bodies (POST):

- Catalog `/api/catalog`: `{ "name": "Laptop", "description": "Demo", "category": "Electronics", "price": 45000, "isActive": true }`
- Product `/api/product`: `{ "sku": "LAP-001", "name": "Laptop", "availableQuantity": 5, "unitPrice": 45000 }`
- Order `/api/order`: `{ "customerId": "00000000-0000-0000-0000-000000000001", "totalAmount": 45000 }`
- User `/api/user`: `{ "displayName": "Demo User", "email": "demo@example.test", "city": "Mumbai" }`

**Database mapping:** This local single-database, multi-schema design has *logical ownership*, not physical database-per-service isolation. No cross-schema foreign keys or direct cross-service joins should be introduced. On Azure SQL we will check the free offer and deployment costs before creating anything.

### Docker

Each service contains a Linux Dockerfile; the initial Docker step will use your existing WSL Docker Engine. SQL Server LocalDB/Windows Authentication cannot be assumed to work from Linux containers: configure a reachable SQL Server TCP endpoint and an appropriately scoped SQL login or alternative local networking method **before** attempting containerized DB access. Do not store passwords in Git.
