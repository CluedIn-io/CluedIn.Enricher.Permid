# Migrating a Connector/Enricher to Multi-Version Targeting

This document tracks the migration of `CluedIn.Enricher.Permid` (published NuGet package:
`CluedIn.ExternalSearch.Providers.PermId.Provider`) from a single-version build to the
multi-version targeting pattern.

Prior art consulted before starting:
- `CluedIn.Crawling.MasterDataServices/docs/multi-version-targeting-migration.md`,
  `CluedIn.Connector.AzureEventHubs/docs/multi-version-targeting-migration.md`,
  `CluedIn.Enricher.GoogleMaps/docs/multi-version-targeting-migration.md`,
  `CluedIn.Connector.Dataverse.V2/docs/multi-version-targeting-migration.md` — GoogleMaps' and
  Dataverse.V2's docs are the most recent/accurate; GoogleMaps' doc already corrected the
  `multiVersionCluedInTargets` schema and package-suffix table against the live template, so that's
  treated as ground truth here rather than re-deriving it.

Branch: `feature/multi-version-targeting` (off `origin/develop`).

---

## Overview

| CluedIn version | .NET TFM | Package suffix |
|---|---|---|
| 4.7.0 | net6.0 | `.470` |
| 4.8.0 | net6.0 | `.480` |
| 5.0.0-beta.* | net10.0 | `.500` |

**Verified 2026-09-10** (not assumed from an older doc): restored a throwaway project against this
repo's own `NuGet.config`/`Nuget.config` feeds with `-p:_CluedIn=5.0.0-*` — resolves to
`5.0.0-beta.575`. Beta is the current prerelease channel for the CluedIn 5.0 line (matches the
finding in both GoogleMaps' and Dataverse.V2's docs from the day before/of this one).

**4.6.0 excluded.** This enricher's source has no CluedIn-version-sensitive dependency found (no
`IStreamRepository`/stream-repository usage — this is a plain `ExternalSearchProvider`, a much
smaller API surface than a connector). Matches the MasterDataServices/GoogleMaps precedent of
excluding 4.6.0 rather than AzureEventHubs/AzureDataLake's wider window; there was no concrete
reason found to include it.

**No active test projects.** `test/Directory.Build.props` and `test/unit/Directory.Build.props`
both exist but neither is consumed by any actual `.csproj` — `find src test -iname "*.csproj"`
only turns up the two `src/` projects. Both test `Directory.Build.props` files were left as-is
(unlike GoogleMaps, which deleted its equivalent dead `test/unit/Directory.Build.props` — that one
is *not* touched here since it costs nothing to leave inert scaffolding alone, and deleting it is
out of scope for this migration). Note for whoever adds real tests to this repo later: the
top-level `test/Directory.Build.props` unconditionally references `xunit` v2 packages alongside
`AutoFixture.Xunit3` (a v3 package) — that mismatched pairing will need fixing the same way
MasterDataServices'/GoogleMaps' docs describe before it can be used.

---

## Step 1 — Pipeline template (`azure-pipelines.yml`)

Status: **Done**

Replaced the single-version `crawler.build.yml` steps-template with the multi-version
`crawler.build.jobs.yml` jobs-template. Switched pool from `windows-latest` to `ubuntu-22.04`
(matching every other migrated repo; each job in the jobs-template installs its own SDK per
`global.json`). Kept the `nuget` variable group and the existing branch trigger list (`develop`,
`master`, `release/*`, `ama/*`) unchanged. `pipelineTemplateRef` set to
`refs/heads/feature/multi-version-packaging`.

**First push (PR #41, build 151863) failed all three legs**, not on this repo's code at all —
purely a pipeline-authoring mistake: the original single-version call
(`- template: crawler.build.yml@templates` with zero parameters) had never set
`useGitVersionDotNetTool`, and the first version of this file carried that omission forward
instead of matching Dataverse.V2/GoogleMaps' explicit `useGitVersionDotNetTool: true`. Left at its
default (`false`), the template's `UseGitVersion` step falls back to the legacy marketplace
`GitVersionTask@5` extension, which runs on a retired Node6 runtime and failed outright on this
org's current hosted agents (`Error: EINVAL: invalid argument, readlink
'/opt/hostedtoolcache/dotnet/dotnet'` during its own tool-caching step) - identical failure, same
log line, on all three legs. Nothing to do with GitVersion.yml's schema (that part was already
verified fine in Step 6 below) or this repo's code. Fixed by adding the same explicit parameter
set Dataverse.V2 and every other migrated repo uses:

```yaml
jobs:
  - template: crawler.build.jobs.yml@templates
    parameters:
      pool:
        vmImage: 'ubuntu-22.04'
      githubReleaseInMaster: true
      publicReleaseForMaster: true
      publishCodeCoverage: true
      useGitVersionDotNetTool: true
      publishToDevFeed: true
      probeCluedInVersion: ${{ parameters.probeCluedInVersion }}
      multiVersionCluedInTargets:
        - cluedInVersion: '4.7.0'
        - cluedInVersion: '4.8.0'
        - cluedInVersion: '5.0.0-beta.*'
```

---

## Step 2 — `Directory.Build.props`

Status: **Done**

Honours `CluedInMultiVersionTargetFramework` with `net10.0` local-dev fallback; derives
`CLUEDIN_V47`/`V48`/`V50` `DefineConstants`. Pinned `LangVersion` to `13.0` defensively (GoogleMaps'
doc flagged a net6.0 `CS8936` trap from an unpinned `LangVersion`; didn't actually bite here, but
cheap to guard against).

---

## Step 3 — `Packages.props`

Status: **Done**

Already correctly cased. Guarded `_CluedIn` so the pipeline's per-leg override wins.

---

## Step 4 — `NuGet.config`

Status: **Done**

Renamed `Nuget.config` → `NuGet.config` (two-step `git mv`, Windows filesystem is
case-insensitive). No `public` feed needed here (unlike AzureEventHubs) — verified directly:
`CluedIn.Core` restores cleanly at `4.7.0`, `4.8.0`, and `5.0.0-*` against this repo's existing
`nuget.org`/`develop`/`release`/`AzurePipelines` feeds.

---

## Step 5 — API compatibility audit across 4.7.0 / 4.8.0 / 5.0.0-beta.*

Status: **Done**

Built both `src/` projects for real (`dotnet build -p:_CluedIn=<v>
-p:CluedInMultiVersionTargetFramework=<tfm>`) against all three targets.

**Finding: same RestSharp 106-vs-114 break GoogleMaps' repo hit** (not a CluedIn.Core API break —
`CluedIn.Core`/`CluedIn.ExternalSearch` bring RestSharp in transitively, and the concrete version
differs sharply by CluedIn generation, exactly as GoogleMaps' doc describes). `4.7.0`/`4.8.0`
(net6.0) resolve **RestSharp 106.x** (legacy API: `Method.GET` uppercase enum,
`IRestResponse<T>`); `5.0.0-beta.*` (net10.0) resolves **RestSharp 114.x** (rewritten API:
`Method.Get` PascalCase, `RestResponse<T>` concrete class, no `IRestResponse<T>`).
`PermIdExternalSearchProvider.cs` was written against the newer API and failed to compile on
4.7.0/4.8.0 with `CS0117`/`CS1503`. Fixed with `#if CLUEDIN_V50` guards at 3 call sites — 2×
`Method.Get`/`Method.GET`, and made `ConstructVerifyConnectionResponse` generic
(`ConstructVerifyConnectionResponse<T>(RestResponse<T> response)` for V50,
`ConstructVerifyConnectionResponse<T>(IRestResponse<T> response)` otherwise) since both call sites
pass the same `T` (`PermIdSearchResponse`) — identical pattern to
`GoogleMapsExternalSearchProvider.cs`'s `ConstructVerifyConnectionResponse<T>`.

Verified: both `src/` projects build cleanly (0 errors) against all three targets (4.7.0/net6.0,
4.8.0/net6.0, 5.0.0-beta.*/net10.0), and the local-dev default (net10.0/`5.0.0-*`) still builds
clean too.

No test-project package selection step was needed (see Overview — no active test projects).

---

## Step 6 — Reset the semantic version (`GitVersion.yml`)

Status: **Done**

```yaml
next-version: 1.0
ignore:
  commits-before: 2026-06-18T00:00:00
```

Highest pre-existing tag is `4.6.2` at `2026-06-17T17:20:26+10:00` (checked every `4.x`/`v4.x` tag
candidate's real commit date via `git log -1 --format=%aI <tag>`, not tag-name sort order — several
beta tags share the same commit as their non-beta counterpart, and creation order doesn't always
match version order). `2026-06-18T00:00:00` sits safely after it.

Verified with the pipeline's actual pinned `GitVersion.Tool 5.9.0` (installed to a scratch
tool-path, not the globally-installed version — that one fails on a schema mismatch against this
org's `pull-request: tag: pr` config, a pre-existing issue unrelated to this migration, same as
found in Dataverse.V2):

```
SemVer: 1.0.0-multi-version-targeting.100
BranchName: feature/multi-version-targeting
```

---

## Step 7 — Push and confirm CI

Status: **Pending**

---

## Checklist

- [x] `azure-pipelines.yml` — switched to `crawler.build.jobs.yml` with `multiVersionCluedInTargets` (4.7.0, 4.8.0, 5.0.0-beta.*); pool switched to ubuntu-22.04
- [x] `Directory.Build.props` — honours `CluedInMultiVersionTargetFramework` with net10.0 local fallback; `DefineConstants` derived; `LangVersion` pinned to 13.0
- [x] `Packages.props` — `_CluedIn` guarded (already correctly cased)
- [x] `NuGet.config` — renamed from `Nuget.config`; verified sufficient as-is otherwise
- [x] Source — `#if CLUEDIN_V50` guards added for the RestSharp 106↔114 API break (3 call sites in `PermIdExternalSearchProvider.cs`); verified 0 errors on all three legs
- [x] `GitVersion.yml` — `next-version: 1.0`; `ignore.commits-before: 2026-06-18T00:00:00`; verified with the pipeline's actual pinned GitVersion.Tool 5.9.0
- [ ] Push branch and confirm the Azure DevOps pipeline is green end-to-end (all three legs + `Multi-version: publish`)
