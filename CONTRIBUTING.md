# Contributing to CommonGround

CommonGround is a hobby and portfolio project, but it's run with a deliberate,
auditable process. The [project constitution](.specify/memory/constitution.md)
is the authoritative reference — when in doubt, it wins.

## Licensing of contributions

The source code is MIT-licensed ([LICENSE](LICENSE)). The questionnaire content
and product copy are **not** open — see [LICENSE-CONTENT.md](LICENSE-CONTENT.md).
By contributing code you agree it is contributed under MIT. Please don't add
third-party questionnaire content or copy unless its license clearly allows it.

## How work flows

Every change is traceable from an issue → a PR → a commit.

### Features (Spec Kit)

Sizeable features use [Spec Kit](https://github.com/github/spec-kit):

1. `/speckit.specify` — write the feature spec (`specs/NNN-name/spec.md`)
2. `/speckit.plan` — design and tech approach (`plan.md`)
3. `/speckit.tasks` — a dependency-ordered `tasks.md` (T0xx)
4. `/speckit.taskstoissues` — generate GitHub issues from the tasks
5. branch → PRs that `Closes #<issue>` → review → merge

### Ad-hoc work (fixes, chores, ops)

Smaller standalone work skips `tasks.md`:

1. open an issue (Bug or Chore template) with acceptance criteria
2. branch → PR that `Closes #<issue>` → review → merge

We don't backfill issues for work merged before this workflow was adopted.

## Branch, commit & PR conventions

- **Branches:** `feat/…`, `fix/…`, `chore/…`, `docs/…` (kebab-case); Spec Kit
  feature branches are `NNN-feature-name`.
- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/)
  (`feat:`, `fix:`, `chore:`, `docs:`, `test:`, `build:`, `ci:`). Commit per
  logical change and keep history readable.
- **PR titles:** same Conventional Commits format, with a **scope that names the
  area or feature** so the title stands alone without opening the spec — e.g.
  `feat(bilingual): switch language mid-flow (US2)`, not a bare `US2` (every spec
  has its own `US1`/`US2`/…, so the number only means something next to the
  feature). Use plain-word scopes (`bilingual`, `frontend`, `backend`,
  `reporting`); avoid insider abbreviations such as `i18n` or `a11y`.
- Prefer one issue per PR to keep the trail clear.

## Local development

You'll need the **.NET 10 SDK**, **Node 22**, and **Docker**.

```bash
# Start Postgres for local dev
docker compose up -d

# Backend — needs env vars (never commit secrets):
#   ConnectionStrings__DefaultConnection, Jwt__SecretKey, Privacy__HmacKey
dotnet run --project backend/src/CommonGround.Api      # http://localhost:5148

# Frontend
cd frontend && npm ci && npm run dev                   # http://localhost:5173
```

The whole stack also runs in containers: `docker compose --profile full up --build`.
See [`specs/001-questionnaire-completion/quickstart.md`](specs/001-questionnaire-completion/quickstart.md)
for the exact migration and run commands.

## Definition of Done

A change is done only when:

- its acceptance criteria are covered by tests,
- CI / the quality gate passes, and
- documentation (and an ADR, for significant decisions) is updated if behavior
  changed.

## Quality gates (enforced on every PR)

- backend + frontend tests pass
- ESLint clean; TypeScript compiles with no errors
- new-code coverage ≥ 80% (dev-only code may be excluded, with justification)
- no new vulnerabilities or secrets
- SonarCloud quality gate and CodeQL pass

## Architecture decisions

Significant technical decisions are recorded as ADRs in
[`docs/adr/`](docs/adr/). Add one when you make a decision worth remembering.
