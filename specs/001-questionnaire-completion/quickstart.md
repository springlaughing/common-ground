# Quickstart: Questionnaire Completion

**Feature**: 001-questionnaire-completion

## Prerequisites

- Docker Desktop running
- .NET 9 SDK installed
- Node.js 20+ installed

## Start local database

```bash
docker compose up -d
```

PostgreSQL available at `localhost:5432`, database `commonground_dev`.

## Run backend

```bash
cd backend
dotnet restore
dotnet ef database update --project src/CommonGround.Api
dotnet run --project src/CommonGround.Api
```

API available at `https://localhost:5001`.

## Run frontend

```bash
cd frontend
npm install
npm run dev
```

App available at `http://localhost:5173`.

## Run tests

```bash
# Backend unit + integration + architecture tests
cd backend
dotnet test

# Frontend component tests
cd frontend
npm run test

# E2E tests (requires backend + frontend running)
cd frontend
npm run test:e2e
```

## Verify the feature works

1. Open `http://localhost:5173`
2. Read the consent explanation and click acknowledge
3. Answer all 12 questions (one at a time, use Next/Back)
4. Submit — you should see your personal reflection with 6 insights
5. Copy your private result link and access code
6. Open a new tab, paste the private result link
7. Confirm your reflection page loads correctly
