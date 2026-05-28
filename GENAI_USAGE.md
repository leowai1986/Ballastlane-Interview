# GenAI Tool Usage Documentation

## Tool Used

**Cursor** (powered by Claude 4.7 Opus)

## Overview

The entire project was built incrementally using a **chunked prompt engineering strategy**. Instead of feeding one massive prompt, the development was split into 5 logical chunks to maintain control, quality, and traceability across the Clean Architecture layers.

---

## Chunk 1: Domain & Application Layer Foundation

### Prompt Strategy

Provided a constrained scaffold prompt focused on the inner layers of Clean Architecture:

- Explicitly prohibited Entity Framework Core, Dapper, and MediatR
- Requested pure Domain entities (`TaskItem`, `User`, `BaseEntity`, `TaskStatus` enum)
- Requested repository interfaces (`ITaskRepository`, `IUserRepository`)
- Requested Application DTOs, service interfaces, and FluentValidation validators
- Explicitly told the AI: "Do NOT generate Infrastructure, API, or Frontend yet"

### AI Output

Generated `TaskManager.Domain` and `TaskManager.Application` projects with:

- Entities, enums, and repository contracts
- DTOs (`TaskDto`, `CreateTaskRequest`, `UpdateTaskRequest`, etc.)
- Service interfaces (`ITaskService`, `IAuthService`, `IPasswordHasher`, `IJwtTokenGenerator`)
- FluentValidation validators with basic rules

### Validation & Corrections

- **Verified** that `Domain.csproj` had zero external dependencies (only `FluentValidation` in Application).
- **Verified** no prohibited libraries were referenced via global search in `.csproj` files.
- **Corrected** naming inconsistencies: ensured `TaskStatus` enum values matched frontend expectations (`Pending`, `InProgress`, `Completed`).
- **Added** `DateTime?` for `DueDate` to support optional dates.

---

## Chunk 2: Infrastructure & API Implementation

### Prompt Strategy

Requested raw ADO.NET SQLite implementation with strict security constraints:

- `SqliteConnection`, `SqliteCommand`, `SqliteParameter` ONLY
- `DatabaseInitializer` with `init.sql` for auto-migration and seeding
- JWT token generator and BCrypt password hasher
- Controllers: `TasksController` ([Authorize]), `AuthController` ([AllowAnonymous]), `HealthController` ([AllowAnonymous])
- Proper DI registration in `Program.cs`

### AI Output

Generated:

- `SqliteConnectionFactory`, `TaskRepository`, `UserRepository`
- `DatabaseInitializer` with SQL script execution
- `JwtTokenGenerator`, `PasswordHasher`
- Controllers with CRUD endpoints
- `Program.cs` with Swagger and DI setup

### Validation & Corrections

- **CRITICAL FIX**: The AI generated an **invalid BCrypt hash** in `init.sql` (truncated/placeholder format). This caused `SaltParseException` at runtime. Replaced with a real BCrypt hash generated via Python (`bcrypt.hashpw("Pa$$w0rd!", bcrypt.gensalt(rounds=11))`).
- **Verified** every SQL command uses `command.Parameters.AddWithValue()` — zero string concatenation in queries.
- **Corrected** middleware order in `Program.cs`: `UseRouting()` → `UseCors()` → `UseAuthentication()` → `UseAuthorization()` → `MapControllers()`. The AI initially placed `UseAuthentication` before `UseRouting`, which broke `[AllowAnonymous]` endpoints.
- **Added** `UseDeveloperExceptionPage()` temporarily to expose real errors instead of swallowed 500s.

### Edge Cases Handled

- **Empty database**: `DatabaseInitializer` runs on startup, creating tables and seeding demo data automatically.
- **Duplicate users**: `UNIQUE` constraint on `Email` in SQLite schema.
- **JWT expiration**: Configured `ValidateLifetime = true` in token validation parameters.

---

## Chunk 3: Unit Tests (TDD Approach)

### Prompt Strategy

Requested xUnit test projects for all 4 layers with specific scenarios:

- Domain: entity default behavior
- Application: service logic with Moq (create valid task, reject empty title, unauthorized access)
- Infrastructure: SQL parameterization verification, JWT generation, password hashing
- API: controller authorization (401 without token, 200 with token), anonymous access

### AI Output

Generated 4 test projects with xUnit, Moq, FluentAssertions covering:

- `TaskService` create/get/update/delete flows
- `AuthService` register/login flows
- Repository behavior mocks
- Controller action result assertions

### Validation & Corrections

- **Verified** all test projects reference the correct source projects.
- **Corrected** tests that used PascalCase JSON after Chunk 4's `JsonNamingPolicy.CamelCase` fix — updated test request objects to match.
- **Added** missing `WebApplicationFactory` setup for API integration tests where the AI generated only unit-level controller tests.
- **Ensured** `dotnet test` passes green before proceeding to next chunk.

---

## Chunk 4: React Frontend

### Prompt Strategy

Requested complete React 18 + TypeScript + Vite + Tailwind CSS frontend:

- Auth context with JWT storage in `localStorage`
- Axios interceptor attaching `Authorization: Bearer <token>`
- Task CRUD UI with responsive design
- Demo credentials pre-filled on login
- Modal form for create/edit with validation

### AI Output

Generated:

- `AuthContext`, `useAuth`, `useTasks` hooks
- `TaskList`, `TaskForm`, `LoginForm`, `Navbar` components
- `api.ts` service with Axios interceptors
- React Router setup with protected routes

### Validation & Corrections

- **CRITICAL FIX**: `TaskForm` did not sync state when switching between create and edit modes. The AI used `useState` with initial values only on first render. **Added `useEffect`** to reset/populate fields every time the modal opens or the task changes:
  ```typescript
  useEffect(() => {
    if (!open) return;
    if (props.mode === "edit" && props.initial) {
      setTitle(props.initial.title);
      // ... sync all fields
    } else {
      // reset to empty
    }
  }, [open, props.mode, props.initial?.id]);
  ```
- **CRITICAL FIX**: The AI generated backend DTOs with PascalCase properties, but React/Axios sends camelCase by convention. This caused **400 Bad Request** on every PUT/POST. **Configured `JsonNamingPolicy.CamelCase`** and `JsonStringEnumConverter` in `Program.cs`.
- **Corrected** `DueDate` validation: AI used `> DateTime.UtcNow` which rejected "today". Changed to `>= DateTime.UtcNow.Date` for demo usability.
- **Added** CORS policy in `Program.cs` to allow frontend origin (`localhost:5173`).
- **Verified** no console warnings in production build (`npm run build`).

### Edge Cases Handled

- **401 redirect**: Axios response interceptor catches 401, clears token, redirects to login.
- **Empty task list**: UI shows empty state message instead of blank screen.
- **Form validation**: Inline errors before submit (title required, max length).
- **Date formatting**: Frontend sends ISO 8601 (`toISOString()`), backend parses with `DateTime?`.

---

## Chunk 5: Documentation & Polish

### Prompt Strategy

Requested comprehensive README with:

- User story
- ASCII architecture diagram
- Setup instructions (backend + frontend)
- API endpoint table
- Demo credentials
- Test execution commands

### AI Output

Generated `README.md` with structured sections.

### Validation & Corrections

- **Fixed** ASCII diagram formatting: AI-generated Unicode box characters broke in GitHub markdown rendering. Replaced with clean list-based architecture description.
- **Fixed** project structure tree formatting: ensured proper code-block wrapping for consistent display.
- **Added** `.http` file examples for manual API testing.
- **Added** this `GENAI_USAGE.md` to satisfy the explicit GenAI documentation requirement.

---

## Critical Thinking Summary

| AI Suggestion                          | Human Validation              | Action Taken                                 |
| -------------------------------------- | ----------------------------- | -------------------------------------------- |
| EF Core scaffolding                    | ❌ Prohibited by requirements | Rejected, enforced ADO.NET raw               |
| Invalid BCrypt hash in seed data       | ❌ Runtime exception          | Replaced with real generated hash            |
| PascalCase JSON DTOs                   | ❌ Frontend incompatibility   | Added `JsonNamingPolicy.CamelCase`           |
| `TaskForm` without `useEffect`         | ❌ State desync on edit       | Added synchronization effect                 |
| `DueDate > UtcNow`                     | ❌ Rejects current day        | Relaxed to `>= UtcNow.Date`                  |
| Middleware order (Auth before Routing) | ❌ Breaks `[AllowAnonymous]`  | Reordered to correct ASP.NET pipeline        |
| Generic 500 responses                  | ❌ Hides real errors          | Added `DeveloperExceptionPage` for debugging |

---

## Security & Validation Checklist

- ✅ All SQL queries use parameterized commands (`SqliteParameter`)
- ✅ Passwords hashed with BCrypt (salted, never stored plain)
- ✅ JWT tokens contain user claims and expire
- ✅ CORS configured for development origin
- ✅ FluentValidation runs before service logic
- ✅ Users can only access their own tasks (authorization at service level)
- ✅ No secrets committed to repository (JWT key in `appsettings.json`)

---

## Conclusion

Generative AI accelerated scaffolding by approximately **60-70%**, but **human oversight was essential** for:

1. Enforcing hard constraints (no ORM, no Mediator)
2. Fixing runtime errors (invalid hashes, state desync)
3. Ensuring security (parameterized SQL, proper middleware order)
4. Validating test coverage and business logic isolation

The AI provided the skeleton; architectural decisions, security validation, and edge-case handling required critical human review.
