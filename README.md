Beanstalk 🌱
============

A self-hosted link page build with dotnet, blazor, and PostgreSQL.

Features
--------

- Username-only authentication (no email).
- Usernames ≥ 5 characters, unique, URL-safe `[a-zA-Z0-9_-]`.
- First registered user becomes Admin.
- Admin can open/close registrations.
- Users can:
  - Enable/disable their page (disabled or missing → Profile Not Found).
  - Customize profile: 
    - Title and bio.
    - Links (add/remove/reorder/toggle). 
    - Display picture.
    - Theme.
- Profile pages at `/{username}`.

Local Development
-----------------

### Pre-requisites

- .NET SDK 9.0+.
- Docker (optional but recommended).

### Building

1. Restore and build:
   ```bash
   dotnet restore src/Beanstalk.sln
   dotnet build src/Beanstalk.sln
   ```
2. Start the development database instance:
   ```bash
   docker compose -f ./docker-compose.dev.yaml up -d --force-recreate
   ```
3. Run the app (requires Postgres running):
   ```bash
   export ConnectionStrings__Default="Host=localhost;Database=beanstalk;Username=postgres;Password=postgres"
   dotnet run --project src/Beanstalk.App/Beanstalk.App.csproj
   ```
   - App listens on `http://localhost:5264`.
   - On startup, EF Core migrations are applied automatically.

Docker
------

### Build image

```bash
docker build -t beanstalk-app -f src/Beanstalk.App/Dockerfile .
```

### Run with docker-compose

An example compose file is provided at `docker-compose.yml`:
```bash
docker compose up -d --build
```
- App: `http://localhost:8080`

Structure
---------

- `/home`
  - Home page
  - Login page
  - Register page
  - Logout
- `/site`
  - Error
  - Profile not found
- `/view`
  - `/{username}` - View public profile.
- `/edit`
  - Dashboard
