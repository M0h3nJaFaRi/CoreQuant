# Scope 07 — Persistence and databases

**Status:** Draft  
**Depends on:** [02 Symbols](scope-02-symbols-v1.md), [03 Quote](scope-03-quote-data-model.md)

## Purpose

Define **MySQL** usage, EF mapping rules, and prerequisites for `core_quant_metal`.

## Databases by environment

| Environment | MySQL databases required for Metal V1 |
|-------------|--------------------------------------|
| Development | `core_quant_metal` (+ `core_quant_core` if shared cross-cutting data needed later) |
| Staging | Same as prod-like: **Core** + **Metal** |
| Production V1 | **`core_quant_core` + `core_quant_metal` only** for readiness |

Other market DbContexts (Forex, Crypto, …) remain **unconfigured** in production ConfigMaps (`Host`/`Name` empty) so readiness does not require them.

## Metal DbContext

- Context: `MetalDbContext` → configuration key `Infrastructure:Databases:Metal`.
- Dev database name: `core_quant_metal` (per README).

## EF prerequisites (blocking)

1. **Isolate configurations per market DbContext** before adding Metal `IEntityTypeConfiguration` types (prevent Commodity/Forex configs applying to Metal).
2. One configuration class **per entity**; every property explicitly configured (MySQL 8, TIMESTAMP, UNSIGNED ids, decimal precision, indexes, FKs).

## Tables (V1 logical model)

| Table / entity | Purpose |
|----------------|---------|
| Metal symbol (catalog) | Scope 02 fields |
| Metal latest quote | Scope 03 fields; FK to symbol |

## Migrations and scripts

- Incremental migration or SQL under `solutionItem/scripts/MySql/Migrations/` (replace empty placeholder when implementation lands).
- **No physical delete**; soft delete via `DeletedAt` global filter on `Entity<TId>`.

## Read vs write

- Commands (ingest, seed): `MetalDbContext` with UoW / SaveChanges.
- API queries: projection + `AsNoTracking()` on read side (same context acceptable for V1 if no read replica yet; document replica routing as V1.1 if needed).

## Repository

- One repository per aggregate root (`IMetalSymbolRepository`; quote side per domain design).
- No generic `IRepository<T>`; no `IQueryable` exposed.

## Acceptance (scope 07 done)

- [ ] Prod DB list: Core + Metal only for V1 readiness.
- [ ] Entity list matches scopes 02–03.
- [ ] EF isolation task acknowledged as prerequisite before first Metal migration.
