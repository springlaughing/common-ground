# ADR 0002: Use a deterministic comparison engine

## Status

Accepted

## Context

CommonGround generates personal reflections and comparison reports from questionnaire answers.

Because the app deals with working preferences and interpersonal interpretation, outputs must be explainable, reproducible, neutral, and testable.

## Decision

Use a deterministic comparison engine for the MVP.

The system will use versioned questionnaire definitions, explicit scoring rules, dimensions, and predefined neutral insight templates.

The MVP will not call an LLM at runtime to interpret individual users or generate personalized psychological conclusions.

AI tools may be used during development to draft candidate wording for insight templates, but all user-facing interpretation templates must be human-reviewed, stored, versioned, tested, and served deterministically by the application.

## Consequences

- Results are reproducible for the same questionnaire version and answers.
- Insights can be tested with unit and mutation tests.
- Reports can explain which dimensions or questions contributed to an insight.
- Operating cost stays low.
- The app avoids black-box psychological interpretation.