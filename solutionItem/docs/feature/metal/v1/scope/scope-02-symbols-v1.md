# Scope 02 — Symbol catalog (V1)

**Status:** Draft  
**Depends on:** [01 Milestone and goals](scope-01-milestone-and-goals.md)

## Purpose

Define which **Metal instruments** exist in V1 and what **metadata** the catalog aggregate owns (not live prices).

## V1 symbol set (proposed default)

| Ticker | Metal | Quote currency | Unit | Initial status | Notes |
|--------|-------|----------------|------|----------------|-------|
| XAU | Gold | USD | troy oz | Active | Spot / LBMA-style quote |
| XAG | Silver | USD | troy oz | Active | |
| XPT | Platinum | USD | troy oz | Active | Optional for first prod cut; can defer to `V1.1` |
| XPD | Palladium | USD | troy oz | Active | Optional for first prod cut; can defer to `V1.1` |

**Minimum for milestone G1:** **XAU** and **XAG** must be active in production. XPT/XPD: confirm in [scope 10](scope-10-decisions-and-sign-off.md).

## Catalog fields (domain / persistence)

| Field | Required V1 | Description |
|-------|-------------|-------------|
| Id | Yes | Application-assigned snowflake `ulong` (align with project id strategy). |
| Ticker | Yes | Unique business key (e.g. `XAU`); DB unique index with soft-delete semantics. |
| Display name | Yes | Human-readable (English ASCII for V1 API). |
| Metal type | Yes | Enum (Gold, Silver, …) with Persian `[Description]` in Domain. |
| Quote currency | Yes | ISO 4217 (e.g. `USD`). |
| Unit | Yes | e.g. troy oz. |
| Status | Yes | Draft / Active / Inactive (exact enum names in Domain). |
| Vendor symbol | No | External provider code if different from ticker; add if vendor requires. |

## Catalog behaviors (V1)

| Behavior | In V1 API? | In V1 ingest? |
|----------|------------|---------------|
| Create symbol | **No** (seed / migration / one-off command internal) | N/A |
| Activate / deactivate | **No** public API in minimal V1 | N/A |
| Update metadata | **No** public API in minimal V1 | N/A |
| Read symbol by id / list | **Yes** (scope 06) | N/A |

Admin CRUD for symbols is **V1.1** unless scope 06 is amended.

## Seeding

- V1 symbols are loaded via **SQL seed** or **idempotent startup seed job** (implementation choice in persistence epic).
- Seed list must match the table above once approved.

## Acceptance (scope 02 done)

- [ ] Final ticker list confirmed (min XAU, XAG).
- [ ] Field list agreed for Domain + DB.
- [ ] Decision recorded: seed-only vs internal admin command for catalog changes in V1.
