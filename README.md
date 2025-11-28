# Apprentice Program App

ASP.NET Core 8 + EF Core + SignalR + Angular dashboard for tracking apprentices, mentors, and assignments.

## Backend (ApprenticeApp.Api)
- Prereqs: .NET 8 SDK, SQLite (bundled), `dotnet-ef` tool.
- Restore/build: `dotnet build ApprenticeApp.sln`
- Apply migrations (creates `apprentice.db` in the API folder): `cd ApprenticeApp.Api && dotnet ef database update`
- Run API: `dotnet run --project ApprenticeApp.Api` (defaults to `http://localhost:5000`, Swagger at `/swagger`)
- Admin UI: MVC views at `/Admin/Apprentices` (create/edit/delete apprentices; list/create/edit/delete assignments).
- SignalR hub: `/hubs/apprentices` broadcasts `ApprenticeChanged` and `AssignmentChanged` events.
- Connection string: `appsettings.Development.json` uses SQLite. Override with `APPRENTICEAPP_CONNECTION` or `ConnectionStrings__DefaultConnection`.
- CORS: allows `http://localhost:4200` for the Angular dev server (allowing credentials).

## API surface (JSON)
- Apprentices: `GET /api/apprentices?status=&track=&pageNumber=&pageSize=`, `GET /api/apprentices/{id}`, `POST /api/apprentices`, `PUT /api/apprentices/{id}`, `DELETE /api/apprentices/{id}`
- Assignments: `GET /api/apprentices/{id}/assignments`, `POST /api/apprentices/{id}/assignments`, `PUT /api/assignments/{id}`, `DELETE /api/assignments/{id}`
- Mentors: `GET /api/mentors`
- Swagger + sample HTTP file: `/swagger` and `ApprenticeApp.Api/ApprenticeApp.Api.http`

## Data/EF Core
- DbContext: `ApprenticeApp.Core/Data/ApprenticeDbContext.cs`
- Entities: Apprentice, Mentor, Assignment with enums for track/status.
- Seeding: `DbInitializer.SeedAsync` runs at startup after `context.Database.Migrate()`.
- Migrations: `ApprenticeApp.Api/Data/Migrations` (design-time factory included).

## Frontend (Angular, SignalR)
- Location: `apprentice-dashboard/`
- Install: `cd apprentice-dashboard && npm install`
- Run dev server: `npm start` (defaults to http://localhost:4200). Expects API at http://localhost:5000.
- SignalR client: `src/app/services/apprentice-hub.service.ts` connects to `/hubs/apprentices` and exposes observable streams.
- Simple live events view: `src/app/app.component.*` displays incoming push events.

## Switching to SQL Server later
1. Add package: `dotnet add ApprenticeApp.Api package Microsoft.EntityFrameworkCore.SqlServer`
2. Update `Program.cs`/`ApprenticeDbContextFactory` to use `UseSqlServer(connectionString, b => b.MigrationsAssembly("ApprenticeApp.Api"))`.
3. Set `ConnectionStrings:DefaultConnection` (or `APPRENTICEAPP_CONNECTION`) to a SQL Server connection string.
4. Regenerate/apply migrations: `dotnet ef migrations add SqlServerInit --project ApprenticeApp.Api --startup-project ApprenticeApp.Api` then `dotnet ef database update`.

## Project layout
- `ApprenticeApp.Core`: entities, enums, EF configurations, DbContext, seed, repositories.
- `ApprenticeApp.Api`: API controllers, SignalR hub, MVC admin views, DTOs, migrations.
- `apprentice-dashboard`: Angular app with SignalR dashboard. 
