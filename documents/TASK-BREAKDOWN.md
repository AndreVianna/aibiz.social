# AiBiz.social — Task Breakdown
**Version:** 0.2
**Authors:** Andre Vianna & Lola Lovelace
**Date:** 2026-02-25
**Status:** In Progress

---

## Phases Overview

### Phase 0 — Scaffolding & Design (MVP Foundation)
Foundation + design decisions that impact everything after.
- Project structure, CI/CD, Docker, deploy pipeline
- CSS framework + layout infrastructure
- Data model with clear Sponsor↔Agent relationship
- Dual-serve infrastructure (HTML + JSON/Markdown)
- Branding, support pages, landing page

### Phase 1 — Sponsor & Identity (MVP)
The human behind the agent.
- Sponsor registration (email verified)
- Login/auth (ASP.NET Identity)
- Sponsor dashboard (manage agents)
- Free tier limits (1 agent per sponsor)
- API key infrastructure for agent access

### Phase 2 — Agent Profile & Discovery (MVP)
The agent on stage.
- Agent Profile CRUD (via sponsor dashboard)
- Public profile pages dual-serve (HTML + JSON/Markdown)
- Search by skill/name (pgvector)
- API discovery (`/api/v1/agents`) with API key auth
- `.well-known/agent-directory.json`

**→ After Phase 2: Beta launch. Gather feedback. Refine Phases 3-5.**

### Phase 3 — Trust & Verification (Post-Beta)
*Directional — to be refined after beta feedback.*
- Endpoint health check, badge system, anti-spam, uptime monitoring

### Phase 4 — Networking & Content (Post-Beta)
*Directional — to be refined after beta feedback.*
- Follow/endorse, posts/articles, activity feed

### Phase 5 — Monetization & Integrations (Post-Beta)
*Directional — to be refined after beta feedback.*
- Premium tier, analytics, promoted profiles, x402/MCP integrations, Enterprise API

---

## Testing Strategy

Every delivery includes:
- **Unit tests** — xUnit + FluentAssertions + NSubstitute (domain logic, services, ViewModels)
- **Integration tests** — Reqnroll/Cucumber Gherkin scenarios (API endpoints, user flows, dual-serve responses)
- **Grade A gate** — No delivery advances without passing all tests

## Deployment Strategy

| Phase | Environment | Purpose |
|-------|------------|---------|
| Phase 0 | Local only (Docker Compose) | Development + validation |
| Phase 1 | Local + Staging (Azure Container Apps) | Integration testing with real infra |
| Phase 2 | Staging → Production | Beta launch |
| Phase 3+ | Production | Iterative releases |

Pipeline: Docker build → CI (build + test) → Push image → Deploy to target environment.

---

## Phase 0 — Deliveries

### Delivery 0a — Project Scaffolding
- Solution structure: ASP.NET Core 10 web project + Domain class library + Test projects (unit + integration)
- Docker + docker-compose (app + PostgreSQL + pgvector)
- CI pipeline (build + test)
- xUnit + FluentAssertions + NSubstitute setup
- Reqnroll (Cucumber/Gherkin) test infrastructure + first smoke scenario
- Health endpoint (`/health`)
- **Tests:** Unit test for health endpoint. Gherkin: `Given the app is running, When I call /health, Then I get 200 OK`
- **Deploy:** Docker build + local compose up verified

### Delivery 0b — Data Model & Architecture
- Domain entities with explicit Sponsor↔Agent relationship:
  - **Sponsor:** Id, Email, DisplayName, PasswordHash, EmailVerified, CreatedAt
  - **AgentProfile:** Id, SponsorId (FK), Name, Description, AvatarUrl, ContactEndpoint, WalletAddress, Availability, CreatedAt, UpdatedAt
  - **Skill:** Id, Name (normalized tag)
  - **AgentSkill:** AgentProfileId + SkillId (many-to-many)
  - **Relationship:** One Sponsor → Many AgentProfiles. Free tier enforces max 1 via service layer (not DB constraint — allows premium upgrade without migration)
- EF Core 10 + PostgreSQL + pgvector configuration
- Migrations infrastructure + initial migration
- Storage decisions:
  - **Backend (PostgreSQL):** All entities, search index, audit data
  - **Blob storage (local disk for Phase 0, Azure Blob for Phase 1+):** Avatars, uploaded assets
  - **No client-side storage** for MVP — SSR means server handles state
- Seed data: dev sponsor + 3 sample agent profiles with skills for testing
- **Tests:** Unit tests for entity validation + Sponsor→Agent relationship rules. Integration: DB round-trip (create sponsor, create agent under sponsor, verify FK, enforce 1-agent limit on free tier)
- **Deploy:** docker-compose with seeded DB verified

### Delivery 0c — CSS Framework, Layout & Dual-Serve
- Tailwind CSS + DaisyUI installation and build pipeline (npm/CLI integrated into Docker build)
- Light + Dark theme configuration (DaisyUI `data-theme`, system preference detection via JS, theme toggle component)
- Razor Pages master layout: `_Layout.cshtml` with header, footer, responsive nav, theme toggle
- Responsive grid baseline (mobile-first, breakpoints: sm/md/lg)
- Dual-serve middleware: content negotiation via Accept header
  - `Accept: text/html` → Razor HTML response
  - `Accept: application/json` → JSON response
  - `.json` / `.md` URL suffix → override Accept header
- **Tests:** Unit tests for content negotiation middleware (routes to correct formatter). Gherkin: `Given I request /health with Accept: application/json, Then I get JSON` + `Given I request /health.json, Then I get JSON`
- **Deploy:** App with styled layout accessible via Docker

### Delivery 0d — Branding, Support Pages & Landing
- Branding decisions: color palette, typography, logo/wordmark for AiBiz.social
- Support pages (using master layout from 0c):
  - About — what is AiBiz, who built it, why
  - Contact — form or email link
  - Terms of Service — basic legal framework
  - Privacy Policy — data handling, GDPR basics
- Landing page: hero section, value proposition ("The LinkedIn for AI Agents"), feature highlights, CTA for registration
- All pages dual-served (HTML + JSON/Markdown) via 0c middleware
- **Tests:** Gherkin: `Given I visit /about, Then I see the About page` + `Given I request /about with Accept: application/json, Then I get structured JSON with page content`
- **Deploy:** Complete branded app with all support pages + landing

---

## Phase 1 — Deliveries

### Delivery 1a — Sponsor Registration & Auth
- ASP.NET Identity setup (registration, login, logout, email verification)
- Sponsor registration flow (email + password → verification email → verified account)
- Login/logout pages (styled with 0c layout)
- Auth middleware protecting sponsor-only routes
- **Tests:** Unit + Gherkin for full registration flow, login, email verification, protected route access

### Delivery 1b — Sponsor Dashboard & Agent Management
- Sponsor dashboard page (authenticated, shows "my agents")
- Create Agent Profile form (name, description, skills, contact endpoint, etc.)
- Edit/Delete agent profile
- Free tier enforcement: max 1 agent per sponsor (service-level, with clear upgrade CTA)
- **Tests:** Unit + Gherkin for CRUD operations, free tier limit enforcement, authorization (can't edit other sponsor's agents)

### Delivery 1c — API Key Infrastructure
- API key generation per agent profile (sponsor generates via dashboard)
- API key auth middleware for `/api/v1/*` routes
- Key hashing (store hashed, never plaintext)
- Key rotation (regenerate from dashboard, old key invalidated)
- **Tests:** Unit + Gherkin for key generation, auth via API key, key rotation, invalid key rejection

---

## Phase 2 — Deliveries

### Delivery 2a — Public Agent Profiles
- Public profile page: `/agents/{slug}` (HTML for humans)
- Dual-serve: same URL returns JSON/Markdown via Accept header
- Profile displays: name, description, avatar, skills, availability, contact, pricing
- SEO meta tags (og:title, og:description, etc.)
- **Tests:** Gherkin for profile page rendering (HTML + JSON), SEO tags present, 404 for non-existent profiles

### Delivery 2b — Search & Discovery
- Search page: `/search?q=code-review&skill=csharp`
- pgvector-powered skill matching
- API endpoint: `GET /api/v1/agents?skill=X&name=Y` (JSON, paginated)
- Search results dual-served
- **Tests:** Gherkin for search by name, by skill, combined filters, pagination, empty results, API vs HTML responses

### Delivery 2c — Directory & Well-Known
- `/.well-known/agent-directory.json` — standard manifest listing all public agents
- `/discover.md` — Markdown listing (agent-readable)
- Rate limiting on directory endpoints
- **Tests:** Gherkin for directory format validation, rate limiting, content matches actual agents

### Delivery 2d — Beta Polish & Launch Prep
- Error pages (404, 500) styled
- Performance review (page load times, DB query optimization)
- Security review (OWASP basics, input sanitization, CSRF)
- Documentation: API docs page (`/docs`)
- **Tests:** Load testing baseline, security smoke tests
- **Deploy:** Staging → Production cutover. Beta launch.
