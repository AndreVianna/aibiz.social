# Delivery 0a — Project Scaffolding

**Phase:** 0
**Status:** Not Started
**Depends On:** Nothing (first delivery)

---

## Existing State

The repo already has:
- Solution (`AiBiz.slnx`) with 4 projects: Web, Domain, Infrastructure, Tests
- Basic entities: `Sponsor.cs`, `AgentProfile.cs` (to be refined in 0b)
- xUnit test project (with placeholder `UnitTest1.cs`)
- `.gitignore` (basic .NET)
- `README.md` (good, keep as-is)
- `Program.cs` (minimal Hello World)

---

## Tasks

### TASK-0a.1 — Update Solution Structure & Config

**What:** Clean up existing structure, add missing test project, update configs.

**Changes:**
1. Create `source/AiBiz.IntegrationTests/` project:
   - Reqnroll + xUnit + FluentAssertions
   - Reference to AiBiz.Web (for WebApplicationFactory)
   - `Features/` folder for `.feature` files
   - `StepDefinitions/` folder
2. Add to `AiBiz.Tests`:
   - FluentAssertions package
   - NSubstitute package
3. Delete `source/AiBiz.Tests/UnitTest1.cs` (placeholder)
4. Add `.runsettings` at solution root:
   - Code coverage settings
   - Test parallelization config
5. Update `.gitignore`:
   - Add Docker: `docker-compose.override.yml`
   - Add Node/npm: `node_modules/`, `package-lock.json`
   - Add Tailwind: `wwwroot/css/output.css` (generated)
   - Add IDE: `.idea/`, `.vscode/`
6. Add `Directory.Build.props` at `source/` root (if not exists):
   - Common properties: `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`
   - Remove duplicated props from individual `.csproj` files
7. Update `AiBiz.slnx` to include new IntegrationTests project

**Tests:** N/A (infrastructure task)

### TASK-0a.2 — Docker & Compose

**What:** Containerize the app + database.

**Changes:**
1. Create `Dockerfile` at repo root:
   - Multi-stage: restore → build → test → publish → runtime
   - Base: `mcr.microsoft.com/dotnet/aspnet:10.0`
   - Build: `mcr.microsoft.com/dotnet/sdk:10.0`
2. Create `docker-compose.yml`:
   - `app` service: builds from Dockerfile, exposes port 8080, depends on db
   - `db` service: PostgreSQL 16 with pgvector extension (`pgvector/pgvector:pg16`)
   - Volume for DB persistence
   - Environment variables for connection string
3. Create `docker-compose.override.yml.example` (dev overrides template)
4. Create `.dockerignore`

**Verification:** `docker-compose up --build` starts app + DB, app responds on port 8080

### TASK-0a.3 — Health Endpoint

**What:** Implement health check endpoint with DB connectivity check.

**Changes:**
1. Update `Program.cs`:
   - Add ASP.NET Health Checks middleware
   - Add PostgreSQL health check (NuGet: `AspNetCore.HealthChecks.NpgSql`)
   - `app.MapHealthChecks("/health")`
2. Health response format: `{ "status": "Healthy", "checks": { "database": "Healthy" } }`
3. Wire connection string from `appsettings.json` / environment variables

**Tests:**
- Unit: Health endpoint returns 200 when DB is healthy
- Unit: Health endpoint returns 503 when DB is unreachable

### TASK-0a.4 — Reqnroll Smoke Test

**What:** First Gherkin scenario proving the integration test infrastructure works.

**Changes:**
1. Create `Features/Health.feature`:
```gherkin
Feature: Health Check
  As a DevOps engineer
  I want to verify the application is healthy
  So that I can monitor its status

  Scenario: Application health check returns healthy
    Given the application is running
    When I send a GET request to "/health"
    Then the response status code should be 200
    And the response should contain "Healthy"
```
2. Create `StepDefinitions/HealthSteps.cs`:
   - Uses `WebApplicationFactory<Program>` + `HttpClient`
   - Testcontainers for PostgreSQL (or in-memory fallback)
3. Create `Support/WebAppFixture.cs` — shared fixture for all integration tests

**Tests:** The Gherkin scenario above + step definitions

### TASK-0a.5 — CI Pipeline

**What:** GitHub Actions workflow for automated build + test.

**Changes:**
1. Create `.github/workflows/ci.yml`:
   - Trigger: push to `main` + PRs
   - Steps: checkout → setup .NET 10 → restore → build → test (unit) → test (integration) → Docker build
   - Docker service for PostgreSQL in CI
   - Fail-fast on any test failure
   - Test results published as artifacts

**Tests:** N/A (pipeline task — verified by pipeline itself)

---

## Task Order

```
0a.1 (structure) → 0a.2 (Docker) → 0a.3 (health endpoint) → 0a.4 (Reqnroll smoke) → 0a.5 (CI)
```

Linear — each builds on the previous.

## Acceptance Criteria

- [ ] Solution builds with `dotnet build`
- [ ] `docker-compose up --build` starts app + PostgreSQL
- [ ] `GET /health` returns 200 with DB status
- [ ] Gherkin smoke scenario passes
- [ ] All tests pass: `dotnet test`
- [ ] CI pipeline green on push
