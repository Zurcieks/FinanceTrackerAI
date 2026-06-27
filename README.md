# Finance Tracker

A personal finance tracker I built to manage my own spending. The backend is a
.NET Minimal API; the client is a native iOS app written in Swift. The interesting
part isn't the CRUD — it's the receipt scanning: snap a photo of a receipt and a
vision model pulls out the merchant, amount, date and category into a draft you
confirm before saving.

## What it does

- **Categories** with full CRUD, archived instead of hard-deleted so transaction
  history stays intact
- **Transactions** with filtering, search and pagination
- **Currency conversion** via the Polish National Bank (NBP) API.
- **Receipt scanning** — an OpenAI vision model extracts data from a photo into a
  draft; the user reviews and confirms before anything is written
- **Receipt storage** in object storage (S3 / MinIO), with short-lived presigned
  URLs generated on read

## Stack

.NET 10 · Minimal API · EF Core · PostgreSQL · Docker · OpenAI API

## Architecture

The project uses **Vertical Slice Architecture** — each feature lives in its own
folder with everything it needs (endpoint, request/response, validator) right next
to it, instead of being spread across horizontal `Services` / `Repositories` layers.
When I work on a feature, everything about it is in one place.

**Why no CQRS / MediatR?** At this scale a mediator pipeline is overhead without a
payoff — it adds indirection I'd have to justify, not remove. Validation runs through
FluentValidation wired in as an endpoint filter, which keeps it out of the handlers.

**Why no authentication?** It's a single-user app — there's no concept of a user, so
there's nothing to authorize. I'd rather leave it out cleanly than bolt on auth that
guards nothing.

## Running it locally

```bash
docker compose up -d                                  # PostgreSQL + MinIO
dotnet user-secrets set "OpenAI:ApiKey" "your-key"    # receipt scanning
dotnet ef database update                             # apply migrations + seed
dotnet run --project api
```

Then open the Scalar API UI at http://localhost:5204/scalar.

## Decisions worth explaining

**Exchange rate is a snapshot, not a live lookup.** When a transaction is saved, its
PLN value is computed once and stored. Reads never call NBP again. A purchase made
last month shouldn't change its recorded value just because today's rate moved —
the historical amount stays fixed.

**The AI is treated as an untrusted source.** A model can return well-formed JSON
with nonsense inside it — a currency that doesn't exist, a hallucinated category ID.
So its output is validated before use: enums are parsed safely (anything unrecognized
becomes null), and a suggested category is checked against the actual database before
it's accepted. The draft goes to the user to confirm regardless.

**Categories are archived, not deleted.** Hard-deleting a category would orphan every
transaction that referenced it. Archiving hides it from the active list while keeping
the history coherent.

## Tests

Unit tests cover the validators and the enum-parsing logic. Integration tests run
against a real PostgreSQL instance spun up in Docker via Testcontainers (not the EF
in-memory provider — that doesn't enforce unique indexes or foreign keys, so it would
pass tests that should fail). They exercise full request → database → response paths,
including cases like duplicate-name conflicts that only surface against a real database.
