# Security Policy

CommonGround handles consent and sensitive working-style data, so security and
privacy are first-class concerns — see Principle I (Privacy-First) and
Principle VII (Audit-Ready DevSecOps) of the
[constitution](.specify/memory/constitution.md).

## Supported versions

This is a hobby / portfolio project. Only the latest `main` — and the live
deployment it produces — is supported. There are no maintained release branches.

## Reporting a vulnerability

**Please do not open a public issue for security vulnerabilities.**

Report privately via GitHub's
[private vulnerability reporting](https://github.com/springlaughing/common-ground/security/advisories/new)
("Report a vulnerability" under the **Security** tab). Please include:

- a description of the issue and its impact;
- steps to reproduce, or a proof of concept;
- any suggested remediation.

## What to expect

As a solo hobby project, responses are best-effort. I aim to acknowledge a
valid report within about a week, confirm the issue, and address it on `main`
(which auto-deploys). Reporters who'd like to be credited will be, once a fix
is live.

## Scope

- **In scope:** the application code in this repository and its deployed instance.
- **Out of scope:** third-party platforms (Render, Neon, SonarCloud, GitHub) —
  report those to the respective vendor.
