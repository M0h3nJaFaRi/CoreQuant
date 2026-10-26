# Scope 08 — Hosts and deployment (V1 prod)

**Status:** Draft  
**Depends on:** [05 Ingest topology](scope-05-ingest-runtime-topology.md), [07 Persistence](scope-07-persistence-and-databases.md)

## Purpose

Define which **processes** run in production for Metal V1 and minimum deploy expectations.

## Hosts required in production (V1)

| Host | Required V1 | Metal-related responsibility |
|------|-------------|------------------------------|
| **Api** | Yes | Read API (scope 06) |
| **Worker** | Yes | Vendor fetch + ingest commands |
| **Scheduler** | Yes | Poll trigger (scope 05 default) |
| **Consumer** | No | Omit from V1 prod unless queue topology chosen in scope 10 |

## Shared infrastructure

| Service | Required V1 |
|---------|-------------|
| MySQL 8 | Yes (Core + Metal) |
| Redis | Yes (cache optional for quotes; lock for ingest; existing logical DB layout) |
| Ingress / TLS | Yes (Api only externally) |

## Configuration highlights

- `ASPNETCORE_ENVIRONMENT=Production`
- `Infrastructure:Databases:Metal:*` and `Core:*` populated from secrets
- API key clients for external consumers
- Timezone: `Asia/Tehran` in containers (existing Docker pattern)

## Deployment artifacts (existing repo)

- Dockerfiles: `solutionItem/deployment/docker/production/{api,worker,scheduler}/`
- K8s ConfigMap/Secret stubs exist; **Deployment/Service/Ingress** completion tracked in infra epic (#11)

## Health

- Api readiness: **Metal** + **Core** DB checks when configured; Redis when configured.
- Worker/Scheduler: readiness includes Metal DB if EF registered on those hosts.

## Acceptance (scope 08 done)

- [ ] Prod host list: Api + Worker + Scheduler (Consumer excluded unless amended).
- [ ] External exposure only for Api confirmed.
- [ ] Aligns with datacenter BOM issue (#9) when available.
