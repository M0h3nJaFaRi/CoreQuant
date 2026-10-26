# Scope 05 — Ingest runtime topology

**Status:** Draft  
**Depends on:** [04 Ingest vendor and SLA](scope-04-ingest-vendor-and-sla.md)

## Purpose

Define **which hosts** run ingest in V1 and how work is scheduled.

## V1 recommended topology: Scheduler + Worker

| Component | Role in V1 |
|-----------|------------|
| **Scheduler** | Triggers ingest on poll interval (scope 04); enqueues work or invokes job abstraction. |
| **Worker** | Calls vendor adapter, normalizes payload, dispatches **ingest quote** command(s). |
| **Consumer** | **Not required** for V1 minimal path (no Redis stream processing mandatory). |
| **Api** | Read-only; no ingest. |

### Rationale

- Scheduler keeps timing out of the API process.
- Worker already exists as a generic host; register **EF + Application** on Worker for V1 (today Worker does not call `AddInfrastructureEf` — required implementation note).

## Alternative (defer unless chosen in scope 10)

| Topology | When to use |
|----------|-------------|
| Worker only (`BackgroundService` + timer) | Fastest MVP; acceptable if Scheduler host not deployed in prod V1 |
| Scheduler → Redis Queue → Consumer | Higher decoupling; choose if ops standard is queue-based ingest |

**Default for this scope draft:** Scheduler fires → Worker executes ingest (direct invocation or shared job interface). Queue-based path is **optional V1.1**.

## Concurrency

- **Distributed lock** (Redis DB 4) around ingest cycle per environment to prevent overlapping runs if multiple Worker replicas exist.
- Idempotent ingest command (scope Application epic) for same vendor payload / correlation.

## Configuration

- Poll interval from config (key name TBD in implementation; document in Worker/Scheduler appsettings).
- Vendor URL and secrets via environment / K8s Secret — never committed.

## Acceptance (scope 05 done)

- [ ] Chosen topology: Scheduler + Worker (or documented alternative).
- [ ] Confirmed V1 prod deploy includes Scheduler + Worker (see scope 08).
- [ ] Decision: queue vs direct Worker execution.
