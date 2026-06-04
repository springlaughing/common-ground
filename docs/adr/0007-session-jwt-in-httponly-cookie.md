# ADR 0007: Carry the session as a JWT in an HttpOnly cookie

## Status

Accepted

## Context

[ADR-0003](0003-use-private-links-and-access-codes.md) established accountless access: a
private result token delivered in the URL **fragment** (`/me#TOKEN`) and validated once
at `POST /api/session/start`. After that first validation the server must grant ongoing
access to the personal result page (`GET /api/me/reflection`, and future comparison
endpoints) **without** re-transmitting the token on every request and **without** user
accounts.

That requires a session mechanism. The choice and its threat model were previously
implied across ADR-0003, `research.md`, and the constitution but never recorded as a
single decision. This ADR consolidates it.

Threats to address:

- **Token leakage via logs** — tokens in the URL path/query land in server, proxy, and
  CDN logs. *(Handled by ADR-0003's fragment delivery.)*
- **Session theft via XSS** — any credential readable by browser JavaScript can be
  exfiltrated by injected script.
- **CSRF** — a cookie sent automatically on cross-site requests can be abused.
- **Transport interception** — credentials sent over plaintext HTTP.
- **Operational cost** — a server-side session store is overkill for the MVP.

## Decision

After the fragment token is validated, issue a **stateless JWT** (`sub = ResponseSetId`,
30-day expiry) carried in a cookie:

```text
Set-Cookie: cg_session=<JWT>; HttpOnly; Secure; SameSite=Strict; Max-Age=2592000; Path=/
```

- **JWT, not a server-side session record** — stateless and self-validating via its
  signature; no Redis or DB session table needed for the MVP.
- **`HttpOnly`** — browser JavaScript cannot read the cookie, so an XSS bug cannot
  exfiltrate the session token. *This is the core "don't expose the JWT in the browser"
  property.*
- **`Secure`** — sent only over HTTPS.
- **`SameSite=Strict`** — not sent on cross-site requests → CSRF mitigation.
- The JWT is read **from the cookie, not the `Authorization` header**: the JwtBearer
  `OnMessageReceived` handler pulls the token from `Request.Cookies["cg_session"]`
  (`Program.cs`). No JavaScript sets an auth header, so the token never lives in
  JS-reachable storage (e.g. `localStorage`).

When the cookie expires, the user re-validates by revisiting their saved `/me#TOKEN`
link (ADR-0003) — the URL stays a valid bookmark.

## Consequences

### Positive

- The session credential is never exposed to browser JS (XSS-resistant) and is never
  re-sent as a bare token after first validation.
- Stateless — no session store to run, back up, or scale for the MVP.
- CSRF and transport risks mitigated by `SameSite=Strict` + `Secure`.

### Negative / trade-offs

- **No instant revocation.** Stateless JWTs cannot be revoked before expiry without a
  denylist. Mitigated by the bounded 30-day lifetime and the response-deletion flow,
  which disables access by removing the underlying `ResponseSet`.
- **Startup key handling.** The JWT signing key must be present and non-empty. It is read
  **lazily** — inside the `AddJwtBearer` options callback — specifically so that
  `WebApplicationFactory` configuration overrides take effect in integration tests
  (an eager read at registration time misses the override). A related gap: the Privacy
  HMAC key is currently read *eagerly* and an **empty string** slips past its null check
  → a zero-length key with no startup error. Hardening (validate non-empty / read
  lazily) is a known follow-up. This subtlety matters when refactoring `Program.cs`.

## Alternatives considered

### JWT in `localStorage` + `Authorization` header

Rejected. `localStorage` and JS-set headers are readable by any script on the page, so an
XSS vulnerability can steal the token. An HttpOnly cookie removes that entire attack
class.

### Server-side session store (Redis / DB session table)

Rejected for the MVP — adds infrastructure and operational cost. It would allow instant
revocation, which a stateless JWT lacks; revisit if revocation becomes a requirement.

### `SameSite=Lax` or `SameSite=None`

Rejected. The flow is same-origin (SPA and API share an origin in deployment), so
`Strict` is sufficient and gives the strongest CSRF posture.

## Related

- [ADR-0003](0003-use-private-links-and-access-codes.md) — the fragment-token access
  model this session mechanism completes.
- Implemented in `backend/src/CommonGround.Api/Program.cs` (T011); session issuance in
  T032 (`POST /api/session/start`); protected read in T033 (`GET /api/me/reflection`).
