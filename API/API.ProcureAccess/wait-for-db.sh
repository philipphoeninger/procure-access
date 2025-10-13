#!/bin/bash
set -e

host="$1"
shift
cmd="$@"

echo "⏳ Waiting for SQL Server ($host:1433) to be available..."
until /opt/mssql-tools/bin/sqlcmd -S $host -U sa -P "$DB_SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1; do
  sleep 2
done

echo "✅ SQL Server is up! Running migrations and starting app..."
exec $cmd
