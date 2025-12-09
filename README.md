# HmctsTasksNet8

Small ASP.NET Core Web API (NET 8, C# 12) that uses EF Core with SQLite and exposes Swagger/OpenAPI documentation.  
It allows caseworkers to create tasks and includes:

- A backend API to create tasks  
- A lightweight HTML + Alpine.js frontend  
- SQLite persistence  
- MSTest unit tests

## Contents
- `Program.cs` — app startup, service registration, Swagger configuration (includes robust XML docs lookup).  
- `HmctsTasksNet8.csproj` — target framework and XML doc generation enabled.  
- `Data/` — EF Core `AppDbContext` and entity definitions.  
- `wwwroot/` — static files served by the app (`index.html` is the frontend).  
- `tests/` — MSTest unit tests for API and/or services.

## Requirements
- .NET 8 SDK  
- Visual Studio 2022 or `dotnet` CLI

## Build & run (CLI)
1. Clean and restore:
   - `dotnet clean`
   - `dotnet restore`
2. Build:
   - `dotnet build`
3. Run:
   - `dotnet run`
   - Or use Visual Studio: F5 / Run (Debug profile sets `ASPNETCORE_ENVIRONMENT=Development` by default)

When running you should see the app URL(s) printed to the console, e.g. `https://localhost:5001`.

## Run tests
From the solution root:
- `dotnet test`
This runs the MSTest unit tests found in the `tests/` project(s).

## View API docs (Swagger / OpenAPI)
- Swagger UI: `https://<host>:<port>/swagger` (or `/swagger/index.html`)  
- OpenAPI JSON: `https://<host>:<port>/swagger/v1/swagger.json`

If Swagger UI is not visible, ensure the app is running and `UseSwagger()` / `UseSwaggerUI()` are enabled (by default this project enables them in Development).

## API endpoints

POST `/api/tasks`

Creates a new task.

Request JSON fields and validation:
- `title` — required, max 200 characters  
- `description` — optional, max 1000 characters  
- `status` — must be one of: `New`, `InProgress`, `Completed`  
- `dueAt` — must be a future date/time

Responses:
- On success: `201 Created` with the created task in the response body.  
- On validation error: `400 Bad Request` with a standard `ProblemDetails` body describing errors.

Use Swagger UI or the raw OpenAPI JSON to inspect request/response schemas.

## Frontend

Located at `wwwroot/index.html`.

- Simple form built with Alpine.js.  
- Sends JSON using `fetch` to `POST /api/tasks`.  
- Shows validation errors above the form when the server returns `400`.  
- After a successful request shows a “Task created” card with the new task details.

## XML documentation & Swagger comments
- Project includes `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in the `.csproj`. Rebuild to generate the XML comments file in `bin\<Configuration>\net8.0\`.  
- The app searches for the XML doc next to the running assembly at startup; if missing it logs a console warning and continues without XML comments.

## Database
- Uses EF Core with SQLite; connection string is in `appsettings.json`.  
- On startup the app calls `db.Database.EnsureCreated()` to create the SQLite DB if missing. Use EF Core Migrations for schema evolution when needed.

## Logging and diagnostics
- Startup writes a console warning if the XML comments file can't be found.  
- To view logs, run the app (Visual Studio or `dotnet run`) and watch the console output.  
- For richer logging configure providers in `Program.cs` (e.g., `builder.Logging.AddConsole()` or via `appsettings.json`).

## Troubleshooting
- `Metadata file ... obj\Debug\net8.0\ref\<Project>.dll could not be found`: fix compile errors, then run `dotnet clean && dotnet build`.  
- Missing XML comments in Swagger: ensure XML generation is enabled in the `.csproj` and the XML file is present in the build output.  
- If Swagger UI not visible: confirm the app is running, correct port, and that `UseSwagger()` / `UseSwaggerUI()` are called for the current environment.

If you want, I can add example request/response payloads, a sample `curl` command for `POST /api/tasks`, or a section describing the tests in more detail.
