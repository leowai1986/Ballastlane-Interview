# Task Manager — Ballastlane Technical Interview

## User Story

&gt; "As a registered user, I want to manage my personal tasks (create, view, edit, delete) so that I can organize my daily work efficiently. Each task has a title, description, status, and due date. Tasks belong to me and no other user should see them."

---

## Tech Stack

| Layer          | Technology                                                      |
| -------------- | --------------------------------------------------------------- |
| Backend        | .NET 8, ASP.NET Web API                                         |
| Data Access    | Raw ADO.NET (Microsoft.Data.Sqlite) — **No EF Core, No Dapper** |
| Authentication | JWT Bearer + BCrypt.NET                                         |
| Validation     | FluentValidation                                                |
| Frontend       | React 18, TypeScript, Vite, Tailwind CSS, Axios                 |
| Testing        | xUnit, Moq, FluentAssertions                                    |
| Database       | SQLite (file-based)                                             |

---

## Clean Architecture

Dependencies point inward. Outer layers depend on inner layers, never the reverse.

- **TaskManager.API** (Presentation)
  - Controllers, Middleware, DI, JWT, Swagger
  - Depends on → Application & Infrastructure

- **TaskManager.Infrastructure** (Data & External)
  - ADO.NET Repositories, SQLite, BCrypt, JWT Generator
  - Depends on → Application

- **TaskManager.Application** (Business Logic)
  - Services, DTOs, FluentValidation, Business Rules
  - Depends on → Domain

- **TaskManager.Domain** (Core)
  - Entities, Enums, Repository Interfaces
  - Zero dependencies on any framework

**Dependency Rule:** Dependencies point inward. Domain knows nothing about Infrastructure or API.

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- Git

### 1. Clone & Backend

```bash
cd src/TaskManager/TaskManager.API
dotnet run
```

- API will be available at http://localhost:5257
- Swagger UI at http://localhost:5257/swagger/index.html
- Database (taskmanager.db) is auto-created and seeded on startup

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

- Frontend will be available at http://localhost:5173

### 3. Demo Credentials

| Field    | Value                  |
| -------- | ---------------------- |
| Email    | `demo@ballastlane.com` |
| Password | `Pa$$w0rd!`            |

Pre-filled on the login screen for convenience.

## API Endpoints

| Method   | Endpoint             | Auth Required | Description               |
| -------- | -------------------- | ------------- | ------------------------- |
| `POST`   | `/api/auth/register` | No            | Create new user           |
| `POST`   | `/api/auth/login`    | No            | Login, returns JWT        |
| `GET`    | `/api/health`        | No            | Health check (public)     |
| `GET`    | `/api/tasks`         | Yes           | List current user's tasks |
| `GET`    | `/api/tasks/{id}`    | Yes           | Get specific task         |
| `POST`   | `/api/tasks`         | Yes           | Create new task           |
| `PUT`    | `/api/tasks/{id}`    | Yes           | Update task               |
| `DELETE` | `/api/tasks/{id}`    | Yes           | Delete task               |

Auth Header: Authorization: Bearer <jwt_token>

## Running Tests

### Backend Tests

```bash
cd src
dotnet test
```

## Key Design Decisions

Raw ADO.NET over EF/Dapper: Required by exercise constraints. Used parameterized SqliteCommand to prevent SQL injection while maintaining full control over queries.
JWT Statelessness: No server-side session storage required; token contains user claims.
Clean Architecture: Business logic is isolated in Application layer, testable without web server or database.
Responsive Frontend: Mobile-first Tailwind design with loading states and empty states for full UX.
TDD Approach: Unit tests written for Domain, Application, Infrastructure, and API layers.

## Seeded Data

On first run, the database is initialized with:
- 1 Demo User: demo@ballastlane.com
- 3 Demo Tasks: Pending, InProgress, Completed

To reset the database, simply delete src/TaskManager.API/taskmanager.db and restart the API.

## Project Structure

```bash
Ballastlane-Interview/
├── src/
│   ├── TaskManager.sln
│   ├── TaskManager.Domain/
│   ├── TaskManager.Application/
│   ├── TaskManager.Infrastructure/
│   └── TaskManager.API/
├── tests/
│   ├── TaskManager.Domain.Tests/
│   ├── TaskManager.Application.Tests/
│   ├── TaskManager.Infrastructure.Tests/
│   └── TaskManager.API.Tests/
├── frontend/
│   ├── src/
│   ├── package.json
│   └── vite.config.ts
├── README.md
└── init.sql
```

## License
This project was built for interview evaluation purposes.
