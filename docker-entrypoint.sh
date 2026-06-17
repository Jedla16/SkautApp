#!/bin/sh
set -eu

DATA_DIR="/app/umbraco/Data"
DB_FILE="$DATA_DIR/Modry_zivot.sqlite.db"
SEED_FILE="$(find /seed-db -maxdepth 1 -type f 2>/dev/null | head -n 1 || true)"

mkdir -p "$DATA_DIR" /app/wwwroot/media

if [ ! -s "$DB_FILE" ] && [ -n "$SEED_FILE" ]; then
  cp "$SEED_FILE" "$DB_FILE"
fi

chown -R app:app /app/umbraco /app/wwwroot || true

exec dotnet SkautApp.dll