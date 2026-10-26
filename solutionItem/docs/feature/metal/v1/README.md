# Metal market — V1.0.0 product scope

This folder freezes the **Metal** bounded context for milestone **v1.0.0**. Implementation issues (#2–#12) must align with these micro-scopes; conflicts require an explicit scope amendment PR.

## Scope index

| ID | Document | Status |
|----|----------|--------|
| 01 | [Milestone and goals](scope/scope-01-milestone-and-goals.md) | Draft |
| 02 | [Symbol catalog (V1)](scope/scope-02-symbols-v1.md) | Draft |
| 03 | [Quote data model](scope/scope-03-quote-data-model.md) | Draft |
| 04 | [Ingest vendor and SLA](scope/scope-04-ingest-vendor-and-sla.md) | Draft |
| 05 | [Ingest runtime topology](scope/scope-05-ingest-runtime-topology.md) | Draft |
| 06 | [HTTP API (v1)](scope/scope-06-api-v1-contract.md) | Draft |
| 07 | [Persistence and databases](scope/scope-07-persistence-and-databases.md) | Draft |
| 08 | [Hosts and deployment (V1 prod)](scope/scope-08-hosts-and-deployment-v1.md) | Draft |
| 09 | [Explicitly out of scope](scope/scope-09-out-of-scope.md) | Draft |
| 10 | [Open decisions and sign-off](scope/scope-10-decisions-and-sign-off.md) | Draft |

## Repository alignment

- API route prefix: `/api/v1/markets/metals/...` via `MarketRoute(MarketType.Metals, ...)`.
- Development database: `core_quant_metal` / `MetalDbContext`.
- CQRS: `ICommandBus` / `IQueryBus`; validation in pipeline, not controllers.
- EF: configurations must be isolated per market DbContext before Metal entities ship (see scope 07).

## How to use

1. Read scopes **01 → 09** in order for V1 boundaries.
2. Resolve items in **10** before starting Domain (#2) or Persistence (#7).
3. When a micro-scope is agreed, set its **Status** in this table to **Approved** and record sign-off in scope 10.
