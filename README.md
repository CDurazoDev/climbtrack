# ClimbTrack

Monorepo base for the climbing and bouldering training app.

## Layout

- `apps/api` - ASP.NET Core 9 backend scaffold
- `apps/mobile` - Flutter 3.22 app scaffold
- `infra` - local infrastructure compose files
- `database` - database artifacts and SQL scripts

## Security notes

- Do not commit secrets in `appsettings*.json`.
- Use `apps/api/ClimbTrack.Api/appsettings.Development.json` for local credentials (ignored by git).
- Use `apps/api/ClimbTrack.Api/appsettings.Development.example.json` as the template.
- For design-time EF commands, prefer setting `CLIMBTRACK_PG` in your local environment.
