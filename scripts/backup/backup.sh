#!/bin/sh
# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# One dump of the whole database, every schema (ARCHITECTURE §10.4, R-ADM-04; S6-29), then the
# retention. Runs in the `backup` compose service; the connection comes from the libpq variables
# PGHOST, PGPORT, PGDATABASE, PGUSER and PGPASSWORD, never from an argument.
#
#   BACKUP_RETENTION_DAYS  🔧 days a dump is kept (default in docker-compose.yml)
#   BACKUP_PATH            folder of the dumps (default /backups)
#
# A dump is written under a temporary name and gets its final name only after pg_dump succeeded and
# pg_restore can read it. Old dumps are removed only after that, so a failed run never costs one.
set -eu

backup_path=${BACKUP_PATH:-/backups}
retention_days=${BACKUP_RETENTION_DAYS:-}

: "${PGDATABASE:?PGDATABASE is required}"

# At most four digits and no leading zero: the shell would read 014 as an octal number.
case $retention_days in
  '' | 0* | *[!0-9]* | ?????*)
    echo "BACKUP_RETENTION_DAYS must be a whole number from 1 to 9999." >&2
    exit 2
    ;;
esac

if [ ! -d "$backup_path" ] || [ ! -w "$backup_path" ]; then
  echo "The backup folder $backup_path does not exist or is not writable." >&2
  exit 2
fi

# The dumps hold personal data: readable by their owner only.
umask 077

now=$(date -u +%s)
stamp=$(date -u -d "@$now" +%Y%m%dT%H%M%SZ)
final="$backup_path/procureaccess-$stamp.dump"
partial="$backup_path/.procureaccess-$stamp.dump.partial"

if [ -e "$final" ]; then
  echo "$final already exists." >&2
  exit 1
fi

trap 'rm -f -- "$partial"' EXIT
trap 'exit 143' TERM
trap 'exit 130' INT

# What a killed run left behind; a day old, so a run that is still writing keeps its file.
find "$backup_path" -maxdepth 1 -type f -name '.procureaccess-*.dump.partial' -mmin +1440 -delete

# --no-owner, --no-privileges: the dump restores under whichever role runs the restore.
pg_dump --format=custom --no-owner --no-privileges --file="$partial"
pg_restore --list "$partial" >/dev/null
mv -- "$partial" "$final"
echo "Backup written: $final"

# By the time in the file name: copying or moving a dump changes its modification time.
cutoff=$(date -u -d "@$((now - retention_days * 86400))" +%Y%m%d%H%M%S)
for file in "$backup_path"/procureaccess-*.dump; do
  name=${file##*/}
  file_stamp=${name#procureaccess-}
  file_stamp=${file_stamp%.dump}
  case $file_stamp in
    [0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]T[0-9][0-9][0-9][0-9][0-9][0-9]Z) ;;
    *) continue ;;
  esac
  digits=$(printf '%s' "$file_stamp" | tr -d 'TZ')
  if [ "$digits" -lt "$cutoff" ]; then
    rm -f -- "$file"
    echo "Backup removed after $retention_days days: $file"
  fi
done
