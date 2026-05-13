# ADR 0001: Use a modular monolith for the MVP

## Status

Accepted

## Context

CommonGround contains several domain areas: questionnaires, responses, comparisons, reporting, privacy, audit, and notifications.

The MVP does not require independent service deployment, independent team ownership, or separate scaling per component. Introducing microservices at this stage would add distributed-system complexity without clear product benefit.

## Decision

Use a modular monolith: one deployable backend application with clear internal module boundaries.

## Consequences

- Simpler local development, testing, deployment, and observability.
- Lower operational cost.
- Easier CI/CD setup for the MVP.
- Architecture tests will enforce boundaries between modules and layers.
- Future extraction remains possible if a module develops independent scaling, ownership, or deployment needs.

## Future options

Potential future extraction candidates include notifications, reporting, analytics, or identity.