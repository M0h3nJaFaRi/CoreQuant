# Scope 09 — Explicitly out of scope (v1.0.0)

**Status:** Draft  

Items below are **not** part of milestone v1.0.0 unless a scope amendment PR is merged.

## Product / markets

- Forex, Crypto, Stocks, Commodity, Indices market features and production databases.
- News product features (News DB may stay unconfigured in prod).
- Cross-market aggregation or comparison APIs.

## Metal data

- OHLC / candle history and `GET .../ohlc`.
- Tick storage and replay.
- Multi-vendor quote blending or failover (single vendor in V1 — scope 04).
- Historical quote retention (V1 overwrites latest only).

## API / admin

- Public CRUD for symbols (create, update, activate via HTTP).
- User-facing authentication product (beyond API key middleware).
- WebSocket streaming to clients.

## Platform

- Eight separate production MySQL databases for all markets.
- Consumer host in production (default).
- Read replica routing and read-after-write consistency policy (document as future; not blocking V1 if reading from primary for quotes).

## Technical debt explicitly deferred

- Fixing Commodity domain sample code (not copied into Metal).
- Filling all eight DbContexts with domain models.
- Complete Postman coverage for non-Metal markets.

## Amendment process

1. Add or update a micro-scope file under `solutionItem/docs/feature/metal/v1/scope/`.
2. Update index status in [README](../README.md).
3. Record decision in [scope 10](scope-10-decisions-and-sign-off.md).
4. Squash PR to `development` with `docs:` prefix.
