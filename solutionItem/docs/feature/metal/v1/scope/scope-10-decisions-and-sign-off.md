# Scope 10 — Open decisions and sign-off

**Status:** Draft  

## Open decisions (fill before Domain / Persistence coding)

| ID | Question | Options | Decision | Owner | Date |
|----|----------|---------|----------|-------|------|
| D1 | V1 symbol set beyond XAU/XAG | XPT/XPD in V1 vs V1.1 | **TBD** | | |
| D2 | Production data vendor | Provider name + contract | **TBD** | | |
| D3 | Poll interval / max quote age | Default 60s / 120s (scope 04) | **TBD** | | |
| D4 | Ingest topology | Scheduler+Worker (default) vs Worker-only vs Queue+Consumer | **TBD** | | |
| D5 | Consumer in prod V1 | No (default) vs Yes | **TBD** | | |
| D6 | OHLC in V1 | No (default) vs Yes → add scope-03b | **TBD** | | |
| D7 | Public admin API for symbols | No (default) vs Yes | **TBD** | | |
| D8 | Production datacenter / cloud | See infra issue #9 | **TBD** | | |

## Micro-scope approval

When stakeholders approve a scope, update [README](../README.md) table **Status** from `Draft` → `Approved`.

| Scope | Approved by | Date |
|-------|-------------|------|
| 01 | | |
| 02 | | |
| 03 | | |
| 04 | | |
| 05 | | |
| 06 | | |
| 07 | | |
| 08 | | |
| 09 | | |
| 10 | | |

## Milestone sign-off (v1.0.0 scope freeze)

- [ ] All scopes 01–09 **Approved** or explicitly deferred with `V1.1` label in scope 09.
- [ ] All **D1–D8** resolved or deferred with written rationale.
- [ ] Product / tech lead sign-off below.

**Signed off by:** _TBD_  
**Role:** _TBD_  
**Date:** _TBD_

---

## Changelog

| Date | Author | Change |
|------|--------|--------|
| 2026-03-26 | — | Initial micro-scope split (draft defaults for minimal V1). |
