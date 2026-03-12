# .NET Backend Template

An ASP.NET Core 8 Web API template with PostgreSQL, Entity Framework Core, ASP.NET Core Identity, JWT authentication, and 9 Claude sub-agents. Duplicate this repo when starting a new .NET backend project.

## Tech Stack

| Tool | Purpose |
|------|---------|
| ASP.NET Core 8 | HTTP framework |
| Entity Framework Core 8 | ORM + migrations |
| ASP.NET Core Identity | User management, roles, password hashing |
| JWT Bearer | Stateless API token authentication |
| PostgreSQL | Database |
| Serilog | Structured logging |
| Swashbuckle | Swagger/OpenAPI documentation |
| xUnit + Moq | Testing |

> **Prerequisite:** .NET 8 SDK required — download at https://dotnet.microsoft.com/download/dotnet/8.0

---

## Available Agents

Invoke by asking Claude: *"Use the [agent name] to..."*

| Agent | File | When to Use |
|-------|------|-------------|
| **API Architect** | `.claude/agents/api-architect.md` | Design controllers, services, DTOs, routes |
| **Database Specialist** | `.claude/agents/database-specialist.md` | EF Core schema, migrations, indexing, LINQ queries |
| **Auth Specialist** | `.claude/agents/auth-specialist.md` | Identity, JWT, roles, claims, policies |
| **Security Auditor** | `.claude/agents/security-auditor.md` | OWASP API Top 10, BOLA, mass assignment, secrets |
| **Performance Optimizer** | `.claude/agents/performance-optimizer.md` | N+1 queries, AsNoTracking, caching, async patterns |
| **Documentation Generator** | `.claude/agents/documentation-generator.md` | Swagger XML docs, ProducesResponseType, README |
| **Error Handler & Logger** | `.claude/agents/error-handler-logger.md` | Exception mapping, Serilog, structured logging |
| **Testing Specialist** | `.claude/agents/testing-specialist.md` | xUnit unit tests, WebApplicationFactory integration tests |
| **DevOps Assistant** | `.claude/agents/devops-assistant.md` | Docker, GitHub Actions CI/CD, Azure deployment |
| **Orchestrator** | `.claude/agents/orchestrator.md` | Coordinate multi-agent pipelines — use this to run a full workflow |

**Example invocations:**
- *"Use the API architect to design a product catalog feature"*
- *"Ask the database specialist to add a migration for the Orders table"*
- *"Have the security auditor review the auth controller before we ship"*
- *"Use the DevOps assistant to set up GitHub Actions CI"*

---

## Project Structure

```
src/
├── Controllers/         # Thin HTTP handlers — call services, return ActionResult
│   ├── BaseController.cs    # Shared response helpers
│   ├── HealthController.cs
│   └── AuthController.cs
├── Data/
│   ├── AppDbContext.cs      # EF Core DbContext with Identity
│   └── Migrations/          # Auto-generated — never edit manually
├── DTOs/
│   ├── Auth/                # Login, Register, AuthResponse DTOs
│   └── Common/              # ApiResponse<T>, PaginationMeta
├── Extensions/
│   └── ServiceExtensions.cs # Service registration, auth config, Swagger
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Models/
│   └── ApplicationUser.cs   # Extends IdentityUser
├── Services/
│   ├── Interfaces/          # IAuthService, ITokenService, etc.
│   ├── AuthService.cs
│   └── TokenService.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json

tests/Api.Tests/
├── Unit/Services/       # Service tests with Moq
├── Integration/         # HTTP tests via WebApplicationFactory
└── Helpers/
    └── TestWebApplicationFactory.cs
```

---

## Code Conventions

- **Controllers**: thin — validate model state, call service, return typed `IActionResult`
- **Services**: all business logic — no `HttpContext`, no EF queries in controllers
- **DTOs**: all input/output uses DTOs — never expose `ApplicationUser` or entity classes directly
- **Errors**: throw exceptions from services — `KeyNotFoundException` (404), `InvalidOperationException` (400), `UnauthorizedAccessException` (401). Middleware maps them.
- **Async**: all database operations must be `async`/`await` — never `.Result` or `.Wait()`
- **Queries**: use `AsNoTracking()` on all read-only EF Core queries
- **Naming**: PascalCase for everything C# — `ProductService`, `IProductService`, `CreateProductDto`

---

## Response Envelope Format

All responses use `ApiResponse<T>` from `DTOs/Common/ApiResponse.cs`:

```json
// Success
{ "status": "success", "data": { ... } }
{ "status": "success", "data": [...], "meta": { "total": 100, "page": 1, "limit": 20 } }

// Client error (4xx)
{ "status": "fail", "message": "User not found." }

// Server error (5xx)
{ "status": "error", "message": "Something went wrong. Please try again later." }
```

---

## Authentication

This template uses both Identity and JWT:

- **Register**: `POST /api/v1/auth/register`
- **Login**: `POST /api/v1/auth/login` → returns access token + sets `refreshToken` httpOnly cookie
- **Refresh**: `POST /api/v1/auth/refresh` → exchanges refresh cookie for new access token
- **Logout**: `POST /api/v1/auth/logout` → clears refresh token

Protect endpoints with `[Authorize]` and `[Authorize(Roles = "Admin")]`.

---

## Configuration

| Key | Source | Description |
|-----|--------|-------------|
| `ConnectionStrings:DefaultConnection` | User Secrets / Env | PostgreSQL connection string |
| `Jwt:Secret` | User Secrets / Env | Signing secret (32+ chars) |
| `Jwt:Issuer` | appsettings.json | Token issuer URL |
| `Jwt:Audience` | appsettings.json | Token audience URL |
| `Jwt:ExpiresInMinutes` | appsettings.json | Access token lifetime (default: 15) |
| `Jwt:RefreshExpiryDays` | appsettings.json | Refresh token lifetime (default: 7) |

---

## Available Commands

```bash
dotnet run --project src                          # Start development server
dotnet test                                       # Run all tests
dotnet test --collect:"XPlat Code Coverage"       # Run tests with coverage
dotnet ef migrations add <Name> --project src     # Create EF migration
dotnet ef database update --project src           # Apply migrations
dotnet ef migrations bundle --project src         # Create migration executable for production
dotnet list package --vulnerable                  # Security audit
```

---

## Starting a New Project from This Template

1. Click **"Use this template"** on GitHub (or clone and re-init git)
2. Install .NET 8 SDK if not already installed
3. Update `<AssemblyName>` and `<RootNamespace>` in `src/Api.csproj`
4. Set up User Secrets:
   ```bash
   dotnet user-secrets init --project src
   dotnet user-secrets set "Jwt:Secret" "your-secret-here" --project src
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;..." --project src
   ```
5. Apply the initial migration: `dotnet ef database update --project src`
6. Run: `dotnet run --project src`
7. Open Swagger UI at `https://localhost:5001/swagger`
8. Use the **API Architect** agent to plan your feature routes
9. Use the **Database Specialist** agent to design your domain schema

---

## Agent Workflows

Agents are aware of each other and will recommend handoffs when appropriate. For coordinated multi-agent pipelines, invoke the **Orchestrator** agent.

### Standard Pipelines

| Workflow | Agents Involved (in order) |
|----------|---------------------------|
| **New Project Setup** | Architect → Database → Auth → DevOps → Docs |
| **New Feature** | Architect → Database → Auth → Error Handler → Tester → Security → Docs |
| **API Integration** | Architect → Auth → Error Handler → Tester → Docs |
| **Database Migration** | Database → Architect → Performance → Tester → Docs |
| **Bug Fix** | Tester → Architect → Database → Security → Docs |
| **Performance Optimization** | Performance → Database → Architect → Tester → Docs |
| **Pre-Release Audit** | Security → Auth → Tester → DevOps → Docs |
| **Documentation Sprint** | Docs → Tester → Architect |

### How Agents Communicate

- Each agent's instructions include a **Handoffs** section listing which agents to recommend next
- When an agent completes work, it provides a summary for the next agent to pick up from
- The **Orchestrator** manages the full pipeline, passing context between agents and announcing each step
- You can run a full pipeline by saying: *"Use the Orchestrator to run the New Feature pipeline for [feature name]"*

---

## Architecture Decisions

> Record decisions here as the project evolves.

| Decision | Choice | Reason |
|----------|--------|--------|
| Framework | ASP.NET Core 8 Web API | Mature, high-performance, excellent tooling |
| ORM | Entity Framework Core 8 | First-party, type-safe, good migrations |
| Auth | Identity + JWT | Identity for user management, JWT for stateless API auth |
| Logging | Serilog | Structured logs, many sinks, production-ready |
| Testing | xUnit + Moq | Industry standard for .NET |
| Architecture | Simple layered | Controllers → Services → EF Core |
