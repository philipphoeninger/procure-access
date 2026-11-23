#!/bin/sh
# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# The main process of the `backup` compose service (S6-29): one backup a night at
# BACKUP_TIME_UTC (🔧 HH:MM, UTC). At the start it catches up when the newest dump is older than a
# day, so a host that was off at that time still gets its backup. A failed run is logged, and the
# service's health check turns unhealthy when no dump is younger than 25 hours.
#
#   schedule.sh                       runs the schedule
#   schedule.sh seconds-until <now>   prints the seconds from <now> (Unix time) to the next run
set -eu

script_dir=$(dirname -- "$0")
backup_path=${BACKUP_PATH:-/backups}
backup_time=${BACKUP_TIME_UTC:-02:00}

case $backup_time in
  [01][0-9]:[0-5][0-9] | 2[0-3]:[0-5][0-9]) ;;
  *)
    echo "BACKUP_TIME_UTC must be a time from 00:00 to 23:59." >&2
    exit 2
    ;;
esac

# Unix time has exactly 86400 seconds a day, so the time of day in UTC is a remainder.
seconds_until() {
  hour=${backup_time%:*}
  minute=${backup_time#*:}
  target=$((${hour#0} * 3600 + ${minute#0} * 60))
  seconds=$(((target - $1 % 86400 + 86400) % 86400))
  if [ "$seconds" -eq 0 ]; then
    seconds=86400
  fi
  echo "$seconds"
}

if [ "${1:-}" = seconds-until ]; then
  seconds_until "$2"
  exit 0
fi

run_backup() {
  if ! sh "$script_dir/backup.sh"; then
    echo "Backup failed at $(date -u +%Y-%m-%dT%H:%M:%SZ)." >&2
  fi
}

# PID 1 of the container: without these traps `docker stop` waits for its timeout.
sleeper=
stop() {
  if [ -n "$sleeper" ]; then
    kill "$sleeper" 2>/dev/null || true
  fi
  exit "$1"
}
trap 'stop 143' TERM
trap 'stop 130' INT

if ! find "$backup_path" -maxdepth 1 -type f -name 'procureaccess-*.dump' -mmin -1440 | grep -q .; then
  run_backup
fi

while :; do
  # In the background with `wait`, because a trap runs only after a foreground command has ended.
  sleep "$(seconds_until "$(date -u +%s)")" &
  sleeper=$!
  wait "$sleeper"
  sleeper=
  run_backup
done
