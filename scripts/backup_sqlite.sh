#!/usr/bin/env bash
set -euo pipefail

# Simple sqlite backup script — copies DB to backups directory with date suffix
APP_DIR="${1:-/app/umbraco/Data}"
BACKUP_DIR="${2:-/backups}"

DB_FILE="$APP_DIR/Modry_zivot.sqlite.db"
TIMESTAMP=$(date +"%F_%H%M%S")

if [ ! -f "$DB_FILE" ]; then
  echo "Database file not found: $DB_FILE" >&2
  exit 1
fi

mkdir -p "$BACKUP_DIR"
cp -v "$DB_FILE" "$BACKUP_DIR/Modry_zivot.$TIMESTAMP.sqlite.db"
echo "Backup saved to $BACKUP_DIR"

# Optional: you can add upload to object storage here (aws s3, rclone, etc.)
