Coolify deployment notes

1) Create new application in Coolify and connect your Git repo.

2) Build settings
- Build command: leave blank if you use Dockerfile. If using Compose, set Compose file path to `docker-compose.prod.yml`.
- Dockerfile path: `./Dockerfile` (if using single-container build).

3) Volumes / Persistent paths
- Map a host path to `/app/umbraco/Data` (persist SQLite, media). Example host path `/opt/skautapp/umbraco/Data`.
- Map a host path to `/app/wwwroot` for media/static files.

4) Environment variables
- `ASPNETCORE_ENVIRONMENT=Production`
- Optional overrides: `ConnectionStrings__umbracoDbDSN` to change DB location.
- If using registry image: set `SKAUTAPP_IMAGE` to the image name in `docker-compose.prod.yml`.

5) Healthcheck
- Coolify can use `http://<app-host>/health` to determine readiness.

6) Backups
- Use the included `scripts/backup_sqlite.sh` as a cron job on the server or a Coolify scheduled job to copy `/app/umbraco/Data/Modry_zivot.sqlite.db` to backups.
