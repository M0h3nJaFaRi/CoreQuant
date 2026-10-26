# Scope 04 — Ingest vendor and SLA

**Status:** Draft  
**Depends on:** [03 Quote data model](scope-03-quote-data-model.md)

## Purpose

Lock **external data source**, **polling/stream behavior**, and **freshness** guarantees for V1.

## Vendor selection (TBD)

| Option | Type | Pros | Cons | V1 recommendation |
|--------|------|------|------|-------------------|
| A | TBD — e.g. metals REST API | Simple polling | Cost, rate limits | **Select one in scope 10** |
| B | TBD — alternate provider | Redundancy | Integration effort | Fallback `V1.1` |
| C | Manual / file fixture | Dev/test only | Not prod | Development only |

**Production vendor:** _not chosen in this draft — record in [scope 10](scope-10-decisions-and-sign-off.md)._

## Ingest mode (V1)

- **Polling** on a fixed interval (no WebSocket requirement for V1 unless vendor mandates).
- One HTTP request may fetch **multiple symbols** if vendor supports batch; otherwise one request per symbol.

## SLA (proposed defaults — confirm in scope 10)

| Parameter | Proposed V1 value |
|-----------|-------------------|
| Poll interval | 60 seconds |
| Max acceptable quote age (API) | 120 seconds (2× interval) |
| Vendor timeout | 15 s (align with `Framework:Resilience:Timeout`) |
| Retries | Use existing resilience (3 retries with jitter) |
| Behavior when vendor down | Keep last quote; expose `CapturedAtUtc` / age in API; log + metric; no synthetic prices |

## Rate limits and licensing

- Document vendor **requests/minute** and **monthly cost** in scope 10 when vendor is chosen.
- Ingest must respect rate limits (scheduler spacing or throttling in adapter).

## Normalization rules

- All timestamps stored and compared in **UTC**.
- Map vendor codes to catalog **Ticker** / optional **Vendor symbol** (scope 02).
- Missing or invalid price for a symbol: **skip symbol** for that cycle; do not fail entire batch unless all symbols fail.

## Acceptance (scope 04 done)

- [ ] Production vendor named and license/cost noted.
- [ ] Poll interval and max quote age approved.
- [ ] Failure behavior (stale last quote) approved.
