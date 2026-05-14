# ADR 0003: Use private links and access codes instead of accounts

## Status

Accepted

## Context

CommonGround does not use user accounts in the MVP.

Participants still need a way to:

- access their personal reflection
- view their existing comparisons
- reuse a completed response in a new comparison
- avoid filling in the same questionnaire multiple times

The app also needs to preserve privacy. It should not automatically identify returning users, and it should not require email, passwords, or third-party login for the MVP.

Because there are no accounts, the system needs lightweight access mechanisms that are simple for users but still clear in purpose.

## Decision

Use two separate access mechanisms:

1. **Private result link**
2. **Access code**

The private result link is used to access a participant's personal result page:

```text
/me#TOKEN
```

The token is delivered in the URL fragment, not the path. Fragments are never transmitted to the server in HTTP requests, so the token does not appear in server access logs, CDN logs, or proxy logs.

When the participant navigates to `/me#TOKEN`, the React frontend reads the fragment, POSTs the token to `/api/session/start`, and the server validates it against the stored hash. On success, the server issues a short-lived session as a `HttpOnly; Secure; SameSite=Strict` cookie. Subsequent requests use the cookie — the token is not re-transmitted after the first validation.

The URL remains a valid bookmark. If the session cookie expires, the participant navigates back to their saved link to re-validate.

The personal result page may show:

- personal reflection
- existing comparisons
- pending comparisons
- ability to create new invite links
- access code management
- delete response option

The access code is used only to reuse an existing response in a new comparison.

Example:
```text
K7Q9-MP2D-W4T8
```
When an invited participant wants to reuse a previous response, they must choose "Use existing response" and enter their access code.

The application then:

- hashes the entered access code
- looks for a matching completed response
- checks that the questionnaire version matches the invite
- checks that the response has not been deleted
- asks the participant to explicitly confirm reuse for the current comparison

The app must not automatically attach an existing response to a new comparison.

## Security and privacy rules

Private result tokens and access codes must not be stored in plain text.

The system stores only hashes of:

- private result tokens
- access codes

The token is delivered in the URL fragment (`/me#TOKEN`) so it is never transmitted to the server in HTTP requests and does not appear in server or proxy logs.

The server must respond with `Referrer-Policy: no-referrer` on result pages to prevent the token leaking via the Referer header if the user navigates to an external link.

After the token is validated, the server issues a session as a `HttpOnly; Secure; SameSite=Strict` cookie. The token is not re-transmitted after the first validation. The cookie has a defined expiry; when it expires the participant re-validates using their saved link.

The access code is a portable credential. The UI must clearly explain that it should be kept private.

The private result link and access code are intentionally not interchangeable:

- the private result link gives access to the personal result page
- the access code supports reuse of an existing response in a new comparison

If a participant loses both their private result link and access code, the MVP provides no recovery path. Without accounts or email verification, the system cannot safely prove identity. The participant must complete a new response.

A participant may regenerate their access code from the private result page. When a new access code is generated, the old access code becomes invalid immediately.

## Consequences
### Positive consequences
- The MVP avoids user accounts, passwords, and email recovery.
- Users can reuse previous responses without filling in the questionnaire again.
- Returning-user recognition is user-initiated, not automatic.
- The app avoids silently identifying users across sessions or devices.
- The private result link and access code have clear, separate purposes.
- The design supports a privacy-first MVP with low operational complexity.
### Negative consequences
- Users must safely store their private result link and access code.
- If both are lost, the response cannot be recovered.
- Access code entry adds friction when reusing an existing response.
- The system must protect against access-code guessing through sufficient code entropy and rate limiting.
## Alternatives considered
### User accounts
User accounts would make recovery and cross-device access easier.
This was rejected for the MVP because accounts introduce additional complexity:
- authentication
- password or identity provider setup
- account recovery
- more personal data
- higher privacy and security responsibility

Accounts may be considered in a future version.

### Email magic links

Email magic links would provide accountless recovery and easier cross-device access.

This was rejected for the MVP because it requires:

- collecting email addresses
- integrating an email provider
- handling deliverability
- protecting additional personal data

Email magic links may be considered later as an optional recovery mechanism.

### Access code opens the full result page

This was rejected because it makes the access code too powerful.

The access code is intended only for response reuse. Full access to the personal reflection page and comparison dashboard should require the private result link.

### Token in URL path

Placing the token in the URL path (`/me/{token}`) was considered and rejected because the token would appear in server access logs, CDN logs, proxy logs, and browser history on every visit. The fragment approach (`/me#token`) avoids server-side log exposure while keeping the URL bookmarkable.

### Automatic returning-user detection

The app could try to recognize returning users through cookies, browser storage, or other signals.

This was rejected for the MVP because it is less transparent and may feel invasive. Reuse should be user-initiated by entering an access code.

## Future options

Future versions may add:

- optional accounts
- optional email magic links
- recovery flows
- organization/team spaces
- configurable access policies

These features should not change the core rule that response reuse requires explicit consent for each comparison.