#!/bin/sh
# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# Restores a dump into a NEW database (ARCHITECTURE §10.4, R-ADM-04; S6-29). It never writes to a
# database that exists, so the live database stays untouched until the operator switches DB_NAME
# (docs/TECHNICAL.md, „Backup and restore“). The connection comes from the libpq variables.
#
#   restore.sh <dump file> <new database name>
set -eu

if [ "$#" -ne 2 ]; then
  echo "Usage: restore.sh <dump file> <new database name>" >&2
  exit 2
fi

dump=$1
target=$2

# Lower-case letters, digits and underscores only: the name goes into a SQL literal below.
case $target in
  '' | [0-9]* | *[!a-z0-9_]*)
    echo "The database name may hold lower-case letters, digits and underscores, and starts with a letter or underscore." >&2
    exit 2
    ;;
esac
if [ "${#target}" -gt 63 ]; then
  echo "The database name is longer than 63 characters." >&2
  exit 2
fi

if [ ! -f "$dump" ]; then
  echo "The dump $dump does not exist." >&2
  exit 2
fi

if ! pg_restore --list "$dump" >/dev/null; then
  echo "The dump $dump cannot be read." >&2
  exit 1
fi

exists=$(psql --dbname=postgres --no-psqlrc --tuples-only --no-align --set=ON_ERROR_STOP=1 \
  --command="SELECT 1 FROM pg_database WHERE datname = '$target'")
if [ -n "$exists" ]; then
  echo "The database $target already exists: nothing was changed. Choose a new name." >&2
  exit 1
fi

created=false
restored=false
remove_partial_database() {
  if [ "$created" = true ] && [ "$restored" = false ]; then
    dropdb --maintenance-db=postgres "$target" || echo "Could not remove the incomplete database $target." >&2
  fi
}
trap remove_partial_database EXIT
trap 'exit 143' TERM
trap 'exit 130' INT

# template0: the dump brings its own objects (ICU collation, tables of every schema).
createdb --maintenance-db=postgres --template=template0 --encoding=UTF8 "$target"
created=true

pg_restore --dbname="$target" --no-owner --no-privileges --exit-on-error --single-transaction "$dump"
restored=true

echo "Restored $dump into the new database $target."
echo "Next (docs/TECHNICAL.md): set the key-ring certificate of this backup, raise CONCURRENCY_ETAG_EPOCH, set DB_NAME=$target, start the API."
