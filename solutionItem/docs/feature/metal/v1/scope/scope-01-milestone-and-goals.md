# Scope 01 — Milestone and goals

**Status:** Draft  
**Milestone:** `v1.0.0`  
**Market:** Metals only (first product vertical on CoreQuant)

## Goal

Deliver a **production-ready** Metal market capability:

1. Symbol catalog stored in `core_quant_metal`.
2. **Latest quote** ingested from an external vendor and exposed via HTTP API.
3. Operational path: staging validation → production cutover with documented checklist.

## Non-goals for v1.0.0

- Other markets (Forex, Crypto, Stocks, Commodity, Indices, News product features).
- Full admin/identity product (beyond existing API key gate).
- Historical OHLC/tick storage (deferred unless scope 03 is amended to **Approved** for bars).

## Success criteria (milestone level)

| # | Criterion |
|---|-----------|
| G1 | At least one configured Metal symbol has a fresh quote in production within ingest SLA (scope 04). |
| G2 | Public read API returns symbol list and latest quote under `/api/v1/markets/metals/...` (scope 06). |
| G3 | Only **Core** + **Metal** MySQL databases and Redis are required for production readiness (scope 07–08). |
| G4 | Scope documents 01–09 approved; open decisions in 10 closed or deferred with label `V1.1`. |

## Downstream epics (reference)

| Epic | Issue theme | Blocked until |
|------|-------------|---------------|
| Domain catalog | `feat: add Metals bounded context` | 02 approved |
| Quote domain | `feat: add Metals quote` | 03 approved |
| Application / API | commands, queries, controllers | 02–06 approved |
| Persistence | `core_quant_metal` mapping | 07 + EF isolation fix |
| Ingest | live vendor → DB | 04–05 approved |
| Prod | datacenter, deploy, cutover | 08 + infra issues |
