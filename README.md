# Spendly — Finance Tracker API

Osobisty tracker finansów. Backend w .NET (Minimal API),
aplikacja kliencka w Swift (iOS).

## Funkcje

- Zarządzanie kategoriami (CRUD + archiwizacja zamiast usuwania)
- Transakcje z filtrowaniem, wyszukiwaniem i paginacją
- Przeliczanie walut przez API NBP
- Skanowanie paragonów: vision AI (OpenAI) wyciąga dane do draftu
- Przechowywanie paragonów w object storage (S3/MinIO)

## Stack

.NET 10, Minimal API, EF Core, PostgreSQL, Docker, OpenAI API

## Architektura

Vertical Slice Architecture — każdy feature samowystarczalny
(endpoint + request/response + walidator w jednym miejscu).

**Dlaczego bez CQRS/MediatR:** przy tej skali pipeline MediatR
to zbędny narzut. Walidacja przez FluentValidation + endpoint filter.

**Dlaczego bez autentykacji:** aplikacja jednoosobowa

## Uruchomienie

1. `docker compose up -d` (PostgreSQL + MinIO)
2. Ustaw klucz OpenAI: `dotnet user-secrets set "OpenAI:ApiKey" "..."`
3. `dotnet ef database update`
4. `dotnet run --project api`
5. Scalar UI: http://localhost:5204/scalar

## Decyzje techniczne

- **Migawka kursu** — AmountInPLN liczony przy zapisie, nie przy
  odczycie (kwota historyczna nie "tańczy" przy zmianie kursu)
- **AI jako niezaufane źródło** — odpowiedź modelu jest walidowana
  (enumy parsowane bezpiecznie, sugerowana kategoria sprawdzana
  względem bazy) przed użyciem
- **Soft delete kategorii** (archiwizacja) — chroni historię transakcji
