# NZWalks

A two-part .NET 8 application for browsing and managing walking tracks in New Zealand: a REST API and an MVC web front end that consumes it.

I started this from a Udemy ASP.NET Core course and then extended it with refresh-token authentication, an MVC front end that calls the API, and an admin area for managing members.

## What it does

**API (`NZWalksAPI`)**
- RESTful endpoints for regions, walks, difficulties, images and members, built with ASP.NET Core 8 and Entity Framework Core on SQL Server.
- JWT access tokens and refresh tokens, with ASP.NET Identity for users and role-based authorisation (`Admin`, `Writer`, `Reader`).
- Repository pattern with DTOs and AutoMapper, separating data access from the controllers.
- Filtering, sorting and pagination on list endpoints.
- API versioning (v1 and v2) and Swagger documentation.
- Exception-handling middleware, model-validation filter and Serilog file logging.
- Image upload with file type and size checks.

**Web front end (`NZWalks.UI`)**
- ASP.NET Core MVC app that talks to the API through `HttpClient`.
- Login, registration and logout, with tokens kept in HTTP-only cookies and automatic token refresh.
- Admin area for viewing and updating members and their roles.

## Tech

C#, ASP.NET Core 8 (Web API and MVC), Entity Framework Core, SQL Server, ASP.NET Identity, JWT, AutoMapper, Serilog, Swagger.

## Run it locally

You need the .NET 8 SDK and SQL Server (the connection strings assume a local SQL Server Express instance called `SQLExpress`; change them in `NZWalksAPI/appsettings.json` if yours differs).

1. **Set the JWT signing key.** It is deliberately not stored in the repo. From the `NZWalksAPI` folder:

   ```
   dotnet user-secrets set "Jwt:Key" "<a long random string, at least 32 characters>"
   ```

2. **Create the databases.** The API uses two contexts, one for the walks data and one for authentication. From the `NZWalksAPI` folder:

   ```
   dotnet ef database update --context NZWalksDbContext
   dotnet ef database update --context NZWalksAuthDbContext
   ```

3. **Run the API** (`https://localhost:7020`). Swagger opens at `/swagger`.

4. **Run the UI** (`https://localhost:7152`). Its `ApiSettings:BaseUrl` in `NZWalks.UI/appsettings.json` must match the API address.

In Visual Studio you can set both projects as startup projects and press F5.

The migrations create the `Admin`, `Writer` and `Reader` roles. Register a user through the UI or Swagger (`POST /api/Auth/Register`) to get started.

## Project layout

```
NZWalksAPI/    Web API: controllers, repositories, models and DTOs, migrations, middleware
NZWalks.UI/    MVC front end: controllers, services, view models, views
```

## Known limitations

- There is no automated test project yet; I tested the endpoints manually through Swagger and the UI.
- This is a learning project. Production use would need tighter configuration (for example `AllowedHosts`, `TrustServerCertificate`, and a managed secret store for the JWT key).
