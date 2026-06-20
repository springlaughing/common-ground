# Implementation Plan: One-to-One Comparison (Core Happy-Path)

**Branch**: `003-one-to-one-comparison` | **Date**: 2026-06-20 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/003-one-to-one-comparison/spec.md`

## Summary

Let one person (the **inviter**) compare their working style with one other person (the **invitee**) via a single-use, time-limited **invite**; the invitee **explicitly consents**, completes the questionnaire, and becomes a full participant with their own `/me` results. Once both responses exist on the **same questionnaire version**, a **deterministic** pair comparison is produced and shown to each participant on **their own `/me` page**, which becomes a hub listing all of their comparisons. The report is **second-person for both people**, ordered **differences-then-similarities**, neutral (no compatibility score), explainable (traceable to dimensions), and **bilingual (EN/DE)** — reusing feature 002's localized insight text and feature 001's accountless `/me#TOKEN` access. A person can create **several** invites, each reusing their own response (no re-taking the questionnaire).

The comparison content is **computed on read** from the two responses' existing per-dimension strengths and the existing localized insight snippets — no new insight text is authored, and nothing about the report is stored, so determinism is automatic. Invitee **reuse-by-access-code** is explicitly deferred to feature 004; the invitee join step is built with a clean seam for it.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (backend), TypeScript 5 / React 19 (frontend) — unchanged from 001/002.

**Primary Dependencies**: ASP.NET Core 10, EF Core 10 + Npgsql, xUnit + FluentAssertions + Moq, Testcontainers.NET, NetArchTest (backend); React 19, Vite, React Testing Library, Playwright (frontend). **No new runtime dependency** — the comparison engine is in-house deterministic logic; invite tokens reuse the existing `TokenService` (HMAC-SHA256 hashing, `RandomNumberGenerator` entropy).

**Storage**: PostgreSQL 16 (Neon prod; Testcontainers for integration). New `Invite` table; `ComparisonParticipant` gains a display-name column; `ComparisonSession`/`ComparisonParticipant` (currently entities only) get EF configurations + a migration. No comparison **result** table — reports are computed on read.

**Testing**: xUnit + FluentAssertions + Moq (unit, incl. the deterministic engine), Testcontainers.NET (integration: full invite→consent→join→generate→view), NetArchTest (module boundaries), React Testing Library (component), Playwright (E2E: create invite, invitee consent+complete, both view report, multi-comparison list). Mutation testing on the comparison engine is **mandated by the constitution** but Stryker.NET remains blocked on .NET 10 (issue #18) — compensated by exhaustive unit + property-style tests over the classification rules (see Constitution Check).

**Target Platform**: Web service on Linux container (backend), modern browser (frontend). Same-origin SPA-in-API container; Render Blueprint, `RunMigrationsOnStartup=true`.

**Project Type**: Web application — ASP.NET Core REST API + React SPA (unchanged from 001/002).

**Performance Goals**: Creating/sharing an invite < ~1 min of user effort (SC-001). Comparison computed on read from two small score sets — negligible cost; report renders in well under a second.

**Constraints**: Deterministic & explainable (same two responses ⇒ identical report); no LLM at runtime; no compatibility score; never expose raw answers; invite/result tokens and access codes stored only as hashes; invite single-use + time-limited with a mid-flow grace period; consent explicit with equal-weight accept/decline and no double negatives; bilingual with English field-level fallback; WCAG 2.1 AA incl. German text wrapping; rate limiting on invite validation and (future) access-code entry; same-version comparison only.

**Scale/Scope**: Pairwise comparisons; one person may hold many comparisons. Data model already supports group comparisons (constitution IV) — not built here. Two locales (en, de), reusing 002's content; no new comparison text authored.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Gate | Status | Realized by |
|-----------|------|--------|-------------|
| I. Privacy-First | Raw answers never shown/logged; invite + result tokens + access codes hashed only; invite time-limited + single-use; invite link never exposes inviter's results; audit records lifecycle without sensitive content | ✅ Pass | Invite token in URL **fragment** (`/invite#TOKEN`), validated server-side like `/me`; `Invite.TokenHash`; report computed from scores, not raw answers |
| II. Neutral Outputs | No overall compatibility score / "fit"; per-dimension alignment shown descriptively; every line an observation; prompts open-ended | ✅ Pass | Engine emits per-dimension strengths + classification only; optional neutral "you align on N of M dimensions" count (allowed by II), never a score |
| III. Deterministic Engine | Same version + same two responses ⇒ identical report; no runtime LLM; every insight traceable to dimensions | ✅ Pass | `ComparisonEngine` is pure over the two responses' `DimensionScore`s; report computed on read; reuses versioned insight snippets |
| IV. Modular Monolith | Comparison logic in `Comparisons`; insight text from `Reporting`; cross-module via SharedKernel interfaces; arch tests; group-ready data model | ✅ Pass | `IComparisonService`/`IReportingService` interfaces; NetArchTest cases; `ComparisonSession` already group-shaped (N participants) |
| V. Explicit Consent | Invitee consents before any sharing; equal-weight accept/decline; specific copy (what, with whom, why); no urgency/guilt/double-negatives; consent audited | ✅ Pass | Invitee `ConsentStep` (bilingual, reviewed against §V); `comparison_joined` audit records consent; decline creates nothing |
| VI. Accountless Identity | No accounts; `/me#TOKEN` fragment + cookie reused; invite is a separate single-use credential; access code stays reuse-only (and reuse is deferred) | ✅ Pass | Session cookie identifies the participant's `ResponseSet`; `/me` hub lists that response's comparisons; access code untouched here |
| VII. DevSecOps | TDD; CI quality gate; ADR for the comparison/invite design; PR linked to issues; **mutation testing on the engine** | ⚠️ Pass w/ noted gap | ADRs (engine + invite token) to be written; coverage ≥ 80% new code; **Stryker blocked on .NET 10 (issue #18)** — engine covered by exhaustive unit/property tests as compensation |

**Consent revisit (memory flag resolved here)**: feature 002 deliberately collected **no name**; comparison now introduces a **display label per participant** (the inviter labels themselves at invite creation; the invitee labels themselves at join). These are short, user-provided, non-verified labels shown only to the other participant — the invitee's consent copy explicitly states their chosen label is shown to the inviter. This is the first personal-ish field collected and is recorded in research/ADR.

**Mutation-testing gap (Principle VII)**: the constitution requires Stryker on the comparison engine. Stryker.NET does not yet support .NET 10 (tracked in issue #18). The engine is therefore isolated as a pure function and covered by **exhaustive case + property-style unit tests** (every classification branch; symmetry: comparing A↔B equals B↔A from each viewpoint; determinism: repeated runs identical). Recorded in Complexity Tracking; revisit when Stryker unblocks.

No unjustified violations. Architecture decisions indexed in [`docs/adr/`](../../docs/adr/README.md).

## Project Structure

### Documentation (this feature)

```text
specs/003-one-to-one-comparison/
├── plan.md              ← this file
├── research.md          ← Phase 0 output (engine rules, invite-token model, display labels, compute-on-read)
├── data-model.md        ← Phase 1 output (Invite, ComparisonSession/Participant, audit events)
├── quickstart.md        ← Phase 1 output (run + exercise the comparison flow; SC mapping)
├── contracts/
│   └── api.md           ← Phase 1 output (invite, join, /me comparisons endpoints)
├── checklists/
│   └── requirements.md  ← spec quality checklist (from /speckit-specify)
└── tasks.md             ← Phase 2 output (/speckit-tasks — NOT created here)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── CommonGround.Modules.Comparisons/
│   │   ├── Entities/Invite.cs                     # NEW: TokenHash, InviterResponseSetId, ComparisonSessionId, InviteeLabel?, ExpiresAt, Status, CreatedAt
│   │   ├── Entities/ComparisonSession.cs          # CHANGED: (status/version already present) used as-is
│   │   ├── Entities/ComparisonParticipant.cs      # CHANGED: + DisplayLabel (each participant's self-label)
│   │   ├── Services/ComparisonEngine.cs           # NEW: pure deterministic per-dimension classification (similar/differ) from two score sets
│   │   ├── Services/ComparisonService.cs          # NEW: create invite, validate invite, join (consent+complete), list, get report
│   │   ├── Services/InviteTokenService.cs         # NEW (thin): generate/hash/expiry via Privacy.TokenService
│   │   └── ComparisonsModuleExtensions.cs         # CHANGED: register services
│   ├── CommonGround.SharedKernel/Interfaces/
│   │   └── IComparisonService.cs                  # NEW: DTOs (ComparisonSummaryDto, ComparisonReportDto, ComparisonInsightDto) + ops
│   ├── CommonGround.Modules.Reporting/
│   │   └── (expose per-dimension localized title+text for reuse by the comparison assembler via interface)
│   └── CommonGround.Api/
│       ├── Controllers/ComparisonsController.cs   # NEW: POST /api/comparisons (create invite), GET/POST /api/invite (validate/join)
│       ├── Controllers/MeController.cs            # CHANGED: GET /api/me/comparisons, GET /api/me/comparisons/{id}?locale=
│       └── Persistence/
│           ├── Configurations/InviteConfiguration.cs · ComparisonSessionConfiguration.cs · ComparisonParticipantConfiguration.cs  # NEW
│           └── Migrations/<ts>_AddComparisons.*    # NEW: Invite + ComparisonSession + ComparisonParticipant(+label) tables
└── tests/
    ├── CommonGround.UnitTests/                     # ComparisonEngine: every classification branch, symmetry, determinism; invite expiry/single-use
    ├── CommonGround.IntegrationTests/              # invite→consent→join→generate→view end-to-end; multi-comparison; same-version guard; audit written
    └── CommonGround.ArchitectureTests/             # Comparisons↔Reporting only via interfaces; no raw-answer leakage to Comparisons

frontend/
├── src/
│   ├── main.tsx                                   # CHANGED: add /invite route (alongside /me, /)
│   ├── pages/
│   │   ├── InvitePage/InvitePage.tsx              # NEW: validate invite (fragment), show inviter label + consent, then questionnaire, then own credentials
│   │   ├── ReflectionPage/MeReflection.tsx        # CHANGED: hub — lists comparisons (pending/ready) + "invite someone" entry
│   │   └── ComparisonPage/ComparisonPage.tsx      # CHANGED: wire to API, per-viewer, bilingual (replaces demo data)
│   ├── components/
│   │   ├── InviteCreate/                          # NEW: inviter enters their display label, gets a shareable link
│   │   ├── ComparisonList/                        # NEW: the /me hub list of comparisons
│   │   └── ConsentStep/                           # REUSED/extended: invitee comparison-consent variant
│   ├── services/comparisonApi.ts                  # NEW: create invite, validate, join, list, get report (locale-aware)
│   └── i18n/messages.en.ts · messages.de.ts       # CHANGED: invite/consent/comparison/hub chrome (both languages)
└── tests/
    ├── components/                                 # invite-create, comparison list, consent variant, report renders differ-then-similar
    └── e2e/                                        # create invite; invitee consent+complete; both view report; second comparison listed
```

**Structure Decision**: Unchanged web-app layout. The empty `Comparisons` module gains the engine + services; comparison **report text reuses `Reporting`'s localized insight snippets** through a SharedKernel interface (no duplicated content, boundaries preserved). Frontend adds an `/invite` entry point and turns `/me` into a small hub; the existing `ComparisonPage` is wired to the real API.

## Complexity Tracking

| Note | Why | Why the simpler alternative was not taken |
|------|-----|-------------------------------------------|
| Comparison computed **on read**, no result table | Deterministic over the two responses' versioned scores; storing nothing guarantees consistency and avoids cache-invalidation | A `ComparisonResult` table would cache derived data that must never diverge from its inputs — pure overhead and a staleness risk for no user benefit |
| Invite token in **URL fragment** (`/invite#TOKEN`), POSTed to validate | Keeps the invite token out of server/proxy logs, consistent with Principle VI's fragment pattern for `/me` | The product concept sketched `/invite/{token}` (path); using the path would log a live credential — the fragment is a strict privacy improvement at no UX cost |
| **Display label per participant** (each self-labels) | The report names "the other person" from each viewpoint; both sides need a label, and it matches the existing `ComparisonDto.theirName` | A single inviter-only label leaves the inviter's view of the invitee unnamed; collecting an opaque, optional-feeling label from each side is the minimal symmetric solution and is consent-disclosed |
| Mutation testing deferred on the engine | Stryker.NET unsupported on .NET 10 (issue #18) | Cannot run Stryker; compensated with exhaustive branch + property (symmetry/determinism) unit tests until it unblocks |
