# Contributing to CoreQuant

Thanks for helping out! This document explains how we work with Git and GitHub so that `staging` and `production` stay stable.

## Branch model

```
feature/*  →  development  →  staging  →  production
```

| Branch | Purpose | Merge method | Approvals |
|---|---|---|---|
| `development` | Integration branch, all work lands here first | **Squash** | See "Reviews" below |
| `staging` | Pre-production testing | **Merge commit** | 1 |
| `production` | Live environment | **Merge commit** | 1 |

- Deployments follow these branches, so **never push directly** to `development`, `staging` or `production`. Direct pushes are blocked.
- `staging` and `production` are only ever updated by a **promotion PR** from the previous branch.
- Branches are protected by repository rulesets: no deletion, no force-push, and CI must pass.

## Working on a change

1. Branch from an up-to-date `development`:

   ```bash
   git checkout development
   git pull
   git checkout -b feature/short-description
   ```

2. Use one of these branch prefixes:

   | Prefix | Use for |
   |---|---|
   | `feature/` | New functionality |
   | `fix/` | Bug fixes |
   | `chore/` | Maintenance, dependencies, tooling |
   | `ci/` | Workflows and pipelines |
   | `docs/` | Documentation only |
   | `hotfix/` | Urgent production fixes (see below) |

3. Commit often, push, and open a PR **into `development`**.
4. Keep PRs small and focused. One concern per PR.

## Commit messages and PR titles

We use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>: <short summary in imperative mood>
```

Types: `feat`, `fix`, `chore`, `ci`, `docs`, `refactor`, `test`, `perf`.

Examples:

```
feat: add forex rate ingestion endpoint
fix: use relative project paths in solution file
ci: add build and unit test workflow
```

Because PRs into `development` are **squash-merged**, the **PR title becomes the commit message** on `development`. Write the PR title in the format above.

## Building and testing locally

The project targets **.NET 10**. From the repository root:

```bash
dotnet restore CoreQuant.slnx
dotnet build CoreQuant.slnx -c Release
dotnet test tests/UnitTests/<ProjectName>/<ProjectName>.csproj -c Release
```

Please run the build and the relevant unit tests before opening a PR. CI runs the same steps.

## Pull requests

Before requesting a review, check that:

- [ ] The PR targets the correct branch (`development` for normal work)
- [ ] CI (`build-and-test`) is green
- [ ] New or changed behavior is covered by tests where practical
- [ ] No secrets, passwords, tokens or real connection strings are included
- [ ] The PR description explains **what** changed and **why**

### Reviews

- Review comments must be **resolved** before merging.
- For `staging` and `production`, at least **one approval** from someone other than the author is required.
- For `development`, we still expect a teammate to look at non-trivial changes, even where GitHub does not enforce it.
- GitHub does not let authors approve their own PR.

## Promoting changes

Promotions move already-tested code forward. They are PRs, never direct pushes.

1. **`development` → `staging`**: open a PR with base `staging` and compare `development`. Merge with **Create a merge commit**.
2. Verify on staging.
3. **`staging` → `production`**: open a PR with base `production` and compare `staging`. Merge with **Create a merge commit**.

Things to know:

- After a promotion, the source branch shows "1 commit behind" because of the merge commit. This is expected, ignore it.
- **Do not click "Update branch"** on a promotion PR.
- Never commit directly to `staging` or `production`, otherwise the change will be lost or cause conflicts on the next promotion.

## Hotfixes

1. Branch `hotfix/<description>` from `development`, fix, and merge via the normal PR flow.
2. Promote quickly through `staging` to `production`.
3. In a real emergency, a repository admin may bypass the rules via a PR straight into `production`. In that case the **same fix must also be merged into `development`** right away, otherwise the next promotion will overwrite it.

## Dependencies

Dependabot opens weekly PRs against `development` for NuGet packages and GitHub Actions. Review them like any other PR: check the changelog for breaking changes and let CI confirm the build.

## Secrets and configuration

This repository is **public**. Never commit:

- Passwords, API keys, tokens, certificates (`.pfx`, `.pem`, `.key`)
- Real connection strings or internal hostnames and IP addresses
- Local settings such as `appsettings.Development.json` with real values or `secrets.json`

Use [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) locally and GitHub Secrets or Sealed Secrets for deployed environments. If you commit a secret by mistake, **revoke it immediately**. Deleting it in a later commit is not enough because it remains in Git history.

## Reporting security issues

Please do **not** open a public issue for vulnerabilities. Use GitHub's **Report a vulnerability** option on the repository's Security tab so it can be handled privately.
