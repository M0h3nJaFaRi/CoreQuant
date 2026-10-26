# Scope 03 — Quote data model

**Status:** Draft  
**Depends on:** [02 Symbol catalog](scope-02-symbols-v1.md)

## Purpose

Define what **price data** is stored for V1 and how it relates to symbols (by id, not aggregate navigation).

## V1 default: latest quote only

**Approved path for v1.0.0 (recommended):** persist and serve **one latest quote row per active symbol**. No OHLC bars, no tick history in V1.

### Latest quote fields

| Field | Required | Type / rules |
|-------|----------|----------------|
| SymbolId | Yes | FK to catalog; same id type as symbol. |
| Bid | No* | Decimal; precision per persistence contract (e.g. 18,4). |
| Ask | No* | Decimal. |
| Mid | Yes** | Decimal; required if bid/ask not both present. |
| Source | Yes | Vendor identifier string (max length per persistence rules). |
| CapturedAtUtc | Yes | Vendor timestamp (UTC); `DateTimeOffset`. |
| IngestedAtUtc | Yes | When CoreQuant stored the quote (UTC). |

\* At least one of: (bid **and** ask) or **mid** must be present.  
\** Domain invariant: reject quote with no usable price.

### Uniqueness and updates

- **One current quote per symbol** (upsert by `SymbolId` or equivalent aggregate design).
- Historical quotes **not** retained in V1 (each ingest overwrites latest).

## Optional amendment: OHLC (not V1 default)

If milestone must include bars, add a separate amendment file `scope-03b-ohlc.md` with timeframe, retention, and API pagination. Until then, OHLC is **out of scope** (see [09](scope-09-out-of-scope.md)).

## Domain boundaries

| Concern | Owner |
|---------|--------|
| Symbol metadata | Catalog aggregate (scope 02) |
| Latest price | Separate write model (quote aggregate or entity + ingest command) |
| Cross-reference | `SymbolId` only |

## Acceptance (scope 03 done)

- [ ] Confirmed: **latest quote only** for V1 (or OHLC amendment merged).
- [ ] Field list and invariants agreed.
- [ ] Overwrite semantics (no history) accepted for V1.
