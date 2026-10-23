# CoreQuant

Market-data / quant backend on **.NET 10**. This repository is the production foundation for the service: Clean Architecture, CQRS, four isolated hosts, and shared infrastructure for API, Worker, Consumer, and Scheduler.

The shared pipeline, persistence, Redis, health, and auth wiring is in place. Domain and public HTTP endpoints are still being filled in.

---

## Architecture

Dependencies point inward. Presentation consumes `Application.Contracts`. It does not reach Domain internals, repositories, or `DbContext`.

```text
HTTP / Host
    ↓
Presentation (controllers, middleware)
    ↓
Application.Contracts  →  Application (handlers + CQRS pipeline)
    ↓
Domain
    ↓
Infrastructure (EF / Redis / ACL)
```

All layers are composed in `CoreQuant.Root`. Each host keeps a thin `Program.cs` and calls its own composition method.

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Shared domain | `Common.Domain` | `Entity`, `AggregateRoot`, `ValueObject`, `Ensure`, domain events, soft delete, audit |
| Product domain | `CoreQuant.Domain` | Market aggregates (starting with Commodity / Symbol) |
| Shared application | `Common.Application` | `ICommandBus` / `IQueryBus` and pipeline behaviors |
| Product application | `CoreQuant.Application` + `.Contracts` | Product contracts and handlers |
| Shared presentation | `Common.Presentation` | Versioning, Swagger, response envelope, middleware |
| Framework | `CoreQuant.Framework` | Serilog/Syslog, health, metrics, encryption, resilience, clock |
| Persistence | `CoreQuant.Infrastructure.Persistence.EF` | MySQL, multiple DbContexts, unit of work, interceptors |
| Redis | `CoreQuant.Infrastructure` | Cache, idempotency, rate limit, token, lock, event store, queue |
| ACL | `CoreQuant.Infrastructure.Acl` | HTTP current-user accessor (API host only) |
| Composition | `CoreQuant.Root` | Service registration and HTTP pipeline |

### CQRS

Commands and queries stay separate. Controllers do not instantiate handlers; they dispatch through the buses.

Default pipeline (registration order):

Logging → Metrics → Validation → Authorization → RateLimit → Idempotency → Caching → Handler

Validation belongs in the CQRS pipeline, not in controllers.

---

## Hosts

Four processes, each identified by `Name` / `Component`:

| Host | Project | Kind | Role |
|------|---------|------|------|
| **Api** | `CoreQuant.Api` | ASP.NET Core | Public HTTP, versioning, API key, ACL |
| **Worker** | `CoreQuant.Worker` | Generic Host | Background work |
| **Consumer** | `CoreQuant.Consumer` | Generic Host | Queue / event consumption |
| **Scheduler** | `CoreQuant.Scheduler` | Generic Host | Scheduled jobs |

Composition:

- API: `AddApiDependency` + `UseApiPipeline` + `MapApiEndpoints`
- Others: `AddWorkerDependency` / `AddConsumerDependency` / `AddSchedulerDependency`

Worker, Consumer, and Scheduler do not register HTTP ACL. They keep the null user accessor and allow-all authorization defaults.

---

## Repository layout

```text
CoreQuant.slnx
src/
  Application/          CoreQuant.Application[.Contracts]
  Common/               Domain, Application, Framework, Presentation, Infrastructure
  Domain/               CoreQuant.Domain
  Framework/            CoreQuant.Framework
  Infrastructure/       Redis, EF, ACL
  Hosts/
    Root/               composition root
    Api | Worker | Consumer | Scheduler
tests/
  UnitTests/
  IntegrationTests/     per host
solutionItem/
  deployment/           Docker + Kubernetes (development / stage / production)
  docs/                 per-host README stubs
  postman/
  scripts/MySql/        migrations and seed
```

Versioned controllers live under `Controllers/V1` (then `V2`) and inherit `ControllerV1`. Resource URLs are declared with `[ApiRoute]` or `[MarketRoute]`, not derived from the controller type name.

---

## Stack

- **.NET 10** (`net10.0`), nullable reference types, implicit usings
- **ASP.NET Core** with URL versioning (`Asp.Versioning`) and Swagger
- **EF Core 10** + **MySQL 8** (`Microting.EntityFrameworkCore.MySql`)
- **Redis** (multiple logical databases)
- **FluentValidation**, **Serilog** (console / syslog RFC5424)
- **xUnit** for unit and integration tests
- Docker and Kubernetes per host and environment

### MySQL databases

Each market has its own database. Configuration keys live under `Infrastructure:Databases`:

| Key | DbContext | Development database name |
|-----|-----------|---------------------------|
| `Core` | `CoreQuantDbContext` | `core_quant_core` |
| `News` | `NewsDbContext` | `core_quant_news` |
| `Forex` | `ForexDbContext` | `core_quant_forex` |
| `Index` | `IndexDbContext` | `core_quant_index` |
| `Metal` | `MetalDbContext` | `core_quant_metal` |
| `Stock` | `StockDbContext` | `core_quant_stock` |
| `Crypto` | `CryptoDbContext` | `core_quant_crypto` |
| `Commodity` | `CommodityDbContext` | `core_quant_commodity` |

Endpoints without Host/Name are not registered in readiness.

### Redis logical databases

| DB | Use |
|----|-----|
| 0 | Cache |
| 1 | Idempotency |
| 2 | Rate limit |
| 3 | Token |
| 4 | Distributed lock |
| 5 | Event store |
| 6 | Queue |

---

## Prerequisites

- **.NET 10** SDK
- **MySQL 8.0** (mapped server version: `8.0.36`)
- **Redis** at `127.0.0.1:6379` for Development
- Schema scripts in `solutionItem/scripts/MySql/`

Do not commit real passwords. Use `launchSettings.json`, user secrets, or environment ConfigMaps.

---

## Run locally

```bash
dotnet restore CoreQuant.slnx
dotnet build CoreQuant.slnx
```

API (Development profile):

```bash
dotnet run --project src/Hosts/Api/CoreQuant.Api/CoreQuant.Api.csproj --launch-profile Development
```

Default URLs: `https://localhost:7001` and `http://localhost:5001`.

`UsePathBase` is taken from `Name` (`CoreQuant`), so HTTP paths sit under that prefix:

| Path | Purpose |
|------|---------|
| `/CoreQuant/health/live` | Liveness |
| `/CoreQuant/health/ready` | Readiness (DB + Redis when configured) |
| `/CoreQuant/health/startup` | Startup |
| `/CoreQuant/health/details` | Check details (requires authorization) |
| `/CoreQuant/swagger` | OpenAPI UI (when enabled) |
| `/CoreQuant/api/v1/{resource}` | REST v1 |

Market routes:

```text
/api/v{version}/markets/{forex|crypto|stocks|metals|commodity|indices}/...
```

Other hosts:

```bash
dotnet run --project src/Hosts/Worker/CoreQuant.Worker/CoreQuant.Worker.csproj --launch-profile Development
dotnet run --project src/Hosts/Consumer/CoreQuant.Consumer/CoreQuant.Consumer.csproj --launch-profile Development
dotnet run --project src/Hosts/Scheduler/CoreQuant.Scheduler/CoreQuant.Scheduler.csproj --launch-profile Development
```

---

## Configuration

Layered: `appsettings.json` → `appsettings.{Environment}.json` → environment variables → `launchSettings`.

| Section | Contents |
|---------|----------|
| `Name` / `Component` | Service identity and path base |
| `Framework:Serilog` / `Syslog` | Logging |
| `Framework:Health` / `Metrics` | Health and metrics paths |
| `Framework:Resilience` | Retry, timeout, circuit breaker |
| `Framework:Encryption` | RSA / AES |
| `Infrastructure:Databases` | MySQL endpoints |
| `Infrastructure:Redis` | Connection and logical DBs |
| `Infrastructure:Persistence:SlowQuery` | Slow-query thresholds |
| `Infrastructure:Acl:DevAuthEnabled` | DevUser scheme (Development only) |
| `Middlewares:ApiKey` | `X-API-Key` header and clients |
| `Middlewares:Maintenance` | 503 during maintenance |
| `Middlewares:RequestTimeouts` | Request timeout policies |

API-key exclusions default to `/health`, `/swagger`, and `/openapi`.

---

## HTTP contract

- Thin controllers: bind, dispatch to the bus, map the result to HTTP.
- No business rules, domain validation, transactions, or generic exception handling in controllers.
- Success uses `ApiResponse` / `ApiResponse<T>`. Errors become Problem Details from central middleware.
- URL versioning (`ControllerV1`, `ControllerV2`). The Application layer is not versioned.
- Propagate `CancellationToken` through handlers and persistence.

API pipeline (order): PathBase → Correlation → Observability → Request logging → Exception handling → Routing → CORS → Maintenance → Request timeouts → API key → Authentication → Authorization.

---

## Tests

```bash
dotnet test CoreQuant.slnx
```

- Unit: Domain, Application, Common, Framework, Infrastructure, ACL, EF, and each host.
- Integration: one project per Api / Worker / Consumer / Scheduler.

---

## Deployment

Per host and environment (`development` / `stage` / `production`):

- Docker: `solutionItem/deployment/docker/{env}/{host}/Dockerfile`
- Kubernetes ConfigMap / Secret: `solutionItem/deployment/k8s/{env}/{host}/`
- NuGet config: `solutionItem/deployment/packages/{env}/{host}/`

Runtime image: `mcr.microsoft.com/dotnet/aspnet:10.0`. The process runs as a non-root user. Container timezone is `Asia/Tehran`.

---

## Current status

Wired and ready:

- Composition for all four hosts
- CQRS pipeline
- API middleware, versioning, health
- Redis adapters
- Eight MySQL DbContexts
- ACL on the API host
- Docker / Kubernetes / Postman scaffolding

Still thin:

- Domain starts at Commodity Symbol session
- Application registers the pipeline without product handlers yet
- `CoreQuant.Api/Controllers` is empty; the Postman collection has no requests yet

Add market capabilities on this contract. Do not bypass the layers.

---

## Host docs

- [API](solutionItem/docs/api/README.md)
- [Worker](solutionItem/docs/worker/README.md)
- [Consumer](solutionItem/docs/consumer/README.md)
- [Scheduler](solutionItem/docs/scheduler/README.md)
