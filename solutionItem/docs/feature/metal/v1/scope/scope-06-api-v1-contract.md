# Scope 06 — HTTP API (v1)

**Status:** Draft  
**Depends on:** [02 Symbols](scope-02-symbols-v1.md), [03 Quote](scope-03-quote-data-model.md)

## Purpose

Freeze **public read** endpoints for Metal v1. Presentation stays thin: bind → `IQueryBus` → envelope responses.

## Base path

```text
/CoreQuant/api/v1/markets/metals/...
```

(Path base includes service `Name` + `Component` per existing API host.)

## Endpoints (V1)

| Method | Route (relative to `markets/metals`) | Query / command | Auth | Response |
|--------|--------------------------------------|-----------------|------|----------|
| GET | `/symbols` | List active symbols (paged) | API key | `ApiResponse<PagedResponse<SymbolDto>>` |
| GET | `/symbols/{id}` | Get symbol by id | API key | `ApiResponse<SymbolDto>` or 404 |
| GET | `/symbols/{id}/quote` | Latest quote | API key | `ApiResponse<QuoteDto>` or 404 |

### Query parameters (list)

| Param | Default | Rules |
|-------|---------|-------|
| `page` | 1 | ≥ 1 |
| `pageSize` | 20 | Cap e.g. 100 |
| `status` | Active only | Optional filter if implemented |

### DTOs

- Live in **Application.Contracts** (no duplicate types in Api project).
- Include `CapturedAtUtc`, optional `IngestedAtUtc`, bid/ask/mid per scope 03.

### Not in V1 (unless scopes amended)

- `POST /symbols`, PATCH, activate/deactivate actions.
- `GET .../ohlc` or history.
- Public write ingest endpoints (ingest is internal via Worker only).

## HTTP semantics

| Case | Status |
|------|--------|
| Success read | 200 + envelope |
| Symbol not found | 404 Problem Details (central middleware) |
| Validation (query) | 422 via CQRS validation pipeline |

## Documentation deliverables

- Swagger summaries on controller actions.
- Postman folder **Metals** with the three GETs + environment variables for base URL and API key.

## Acceptance (scope 06 done)

- [ ] Endpoint list approved (read-only trio).
- [ ] Pagination defaults agreed.
- [ ] Admin/write endpoints explicitly deferred to V1.1.
