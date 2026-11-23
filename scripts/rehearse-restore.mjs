#!/usr/bin/env node
// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Restore rehearsal (R-ADM-04, ARCHITECTURE §10.4; S6-29; docs/TECHNICAL.md). On a throwaway stack
// (docker-compose.rehearsal.yml) it backs up a database with a user and a project, replaces the
// server with an empty one, restores into a new database, starts the API with a raised ETag epoch,
// signs in and opens the project. Every secret is generated for this run; the real .env is not read.
// Needs Docker and openssl; not part of CI.
//
//   node scripts/rehearse-restore.mjs
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { randomBytes } from 'node:crypto';
import { mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, statSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { setTimeout as delay } from 'node:timers/promises';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const RESTORED_DATABASE = 'procureaccess_restored';
const OWNER = 'e2e-org-owner@example.invalid'; // TestUserSeedExtensions.OrganizationOwnerEmail
const ORGANIZATION = '0e2e0000-0000-4000-8000-000000000001'; // TestUserSeedExtensions.OrganizationId
const RETENTION_DAYS = 14;
const DAY_MS = 86_400_000;

// Row counts of every table; the log table is only listed, because the API keeps writing to it.
const TABLE_COUNTS_SQL = `
  SELECT n.nspname || '.' || c.relname || '=' || CASE WHEN n.nspname = 'logging' THEN 'not counted' ELSE
    (xpath('/row/c/text()', query_to_xml(format('SELECT count(*) AS c FROM %I.%I', n.nspname, c.relname), false, true, '')))[1]::text END
  FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
  WHERE c.relkind IN ('r', 'p') AND n.nspname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
  ORDER BY 1`;

const work = mkdtempSync(join(tmpdir(), 'pa-rehearsal-'));
const backups = join(work, 'backups');
const envFile = join(work, 'rehearsal.env');
const password = `Rehearsal-${randomBytes(12).toString('hex')}`;
const user = `${process.getuid()}:${process.getgid()}`;

// Only what Docker needs, so no variable of the caller's shell overrides the env file.
const dockerEnv = Object.fromEntries(
  ['PATH', 'HOME', 'DOCKER_HOST', 'DOCKER_CONFIG', 'XDG_RUNTIME_DIR']
    .filter((name) => process.env[name] !== undefined)
    .map((name) => [name, process.env[name]]),
);

function run(command, args, { allowFailure = false } = {}) {
  const result = spawnSync(command, args, { cwd: ROOT, env: dockerEnv, encoding: 'utf8', maxBuffer: 128 * 1024 * 1024 });
  if (!allowFailure && result.status !== 0) {
    throw new Error(`${command} ${args.slice(0, 12).join(' ')} failed:\n${(result.stderr ?? '').slice(-3000)}`);
  }
  return result;
}

const compose = (args, options) =>
  run('docker', ['compose', '--project-directory', ROOT, '--env-file', envFile, '-f', 'docker-compose.yml', '-f', 'docker-compose.rehearsal.yml', ...args], options);

const backupTool = (args, options) => compose(['run', '--rm', '--no-deps', '--user', user, 'backup', 'sh', ...args], options);

function writeEnv(values) {
  const lines = Object.entries(values).map(([name, value]) => `${name}=${value}`);
  writeFileSync(envFile, `${lines.join('\n')}\n`, { mode: 0o600 });
}

function keyRingCertificate() {
  const key = join(work, 'keyring-key.pem');
  const certificate = join(work, 'keyring-cert.pem');
  const pfx = join(work, 'keyring.pfx');
  run('openssl', ['req', '-x509', '-newkey', 'rsa:3072', '-sha256', '-days', '1', '-nodes', '-subj', '/CN=rehearsal', '-keyout', key, '-out', certificate]);
  run('openssl', ['pkcs12', '-export', '-inkey', key, '-in', certificate, '-passout', 'pass:', '-out', pfx]);
  return readFileSync(pfx).toString('base64');
}

const tableCounts = (database, dbUser) =>
  compose(['exec', '-T', 'db', 'psql', '-U', dbUser, '-d', database, '-At', '-c', TABLE_COUNTS_SQL]).stdout.trim().split('\n');

function stampOf(date) {
  return date.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z$/, 'Z');
}

/** A minimal browser session: keeps the cookies and sends the antiforgery token. */
class Session {
  cookies = new Map();

  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async request(method, path, { json, ifMatch } = {}) {
    const headers = {};
    if (this.cookies.size > 0) {
      headers.Cookie = [...this.cookies].map(([name, value]) => `${name}=${value}`).join('; ');
    }
    if (method !== 'GET' && this.cookies.has('XSRF-TOKEN')) {
      headers['X-XSRF-TOKEN'] = this.cookies.get('XSRF-TOKEN');
    }
    if (ifMatch) {
      headers['If-Match'] = ifMatch;
    }
    if (json) {
      headers['Content-Type'] = 'application/json';
    }
    const response = await globalThis.fetch(`${this.baseUrl}${path}`, {
      method,
      headers,
      body: json ? JSON.stringify(json) : undefined,
    });
    for (const cookie of response.headers.getSetCookie()) {
      const pair = cookie.split(';')[0];
      const separator = pair.indexOf('=');
      const [name, value] = [pair.slice(0, separator), pair.slice(separator + 1)];
      if (value) {
        this.cookies.set(name, value);
      } else {
        this.cookies.delete(name);
      }
    }
    return response;
  }

  async signIn() {
    assert.equal((await this.request('GET', '/api/v1/auth/session')).status, 200);
    const response = await this.request('POST', '/api/v1/auth/login', { json: { email: OWNER, password } });
    assert.equal(response.status, 204, 'sign-in');
  }
}

async function apiUrl() {
  const address = compose(['port', 'api', '8080']).stdout.trim();
  const url = `http://${address}`;
  for (let attempt = 0; attempt < 120; attempt += 1) {
    try {
      if ((await globalThis.fetch(`${url}/health/ready`)).ok) {
        return url;
      }
    } catch {
      // The API is still starting.
    }
    await delay(1000);
  }
  throw new Error('The API did not become ready within two minutes.');
}

let stepNumber = 0;
function passed(text) {
  stepNumber += 1;
  console.log(`${String(stepNumber).padStart(2)}. ok  ${text}`);
}

async function rehearse() {
  mkdirSync(backups, { mode: 0o700 });
  const dbUser = 'rehearsal';
  const source = {
    ASPNETCORE_ENVIRONMENT: 'Development',
    DB_HOST: 'db',
    DB_NAME: 'procureaccess_source',
    DB_USER: dbUser,
    DB_PASSWORD: randomBytes(24).toString('hex'),
    DB_PORT_INNER: '5432',
    DATA_PROTECTION_CERTIFICATE: keyRingCertificate(),
    BACKUP_DIR: backups,
    BACKUP_RETENTION_DAYS: String(RETENTION_DAYS),
    CONCURRENCY_ETAG_EPOCH: '1',
    REHEARSAL_SEED_PASSWORD: password,
    REHEARSAL_TEST_RELEASE_PATH: '/app/catalog/test-releases',
    REHEARSAL_TEST_RELEASE_VERSION: 'T.1',
  };
  writeEnv(source);

  compose(['up', '-d', '--build', '--wait', 'db']);
  compose(['up', '-d', '--build', '--no-deps', 'api']);
  const sourceSession = new Session(await apiUrl());
  await sourceSession.signIn();
  passed('source stack started (key ring encrypted with a throwaway certificate), signed in');

  const projectName = 'Probe-Vergabe vor dem Backup';
  const created = await sourceSession.request('POST', `/api/v1/organizations/${ORGANIZATION}/projects`, { json: { name: projectName } });
  assert.equal(created.status, 201);
  const projectId = (await created.json()).id;
  const tagAtBackup = created.headers.get('etag');
  const version = /^"1-(\d+)"$/.exec(tagAtBackup)?.[1];
  assert.ok(version, `tag of epoch 1, got ${tagAtBackup}`);
  passed(`project created, ETag ${tagAtBackup}`);

  const now = Date.now();
  const expired = `procureaccess-${stampOf(new Date(now - (RETENTION_DAYS + 1) * DAY_MS))}.dump`;
  const kept = `procureaccess-${stampOf(new Date(now - (RETENTION_DAYS - 1) * DAY_MS))}.dump`;
  writeFileSync(join(backups, expired), 'old');
  writeFileSync(join(backups, kept), 'recent');
  const countsAtBackup = tableCounts(source.DB_NAME, dbUser);
  const backupRun = backupTool(['/opt/backup/backup.sh']);
  const dumpName = /Backup written: \/backups\/(\S+)/.exec(backupRun.stdout)?.[1];
  assert.ok(dumpName, 'the backup names its dump');
  const files = readdirSync(backups);
  assert.ok(files.includes(dumpName) && files.includes(kept) && !files.includes(expired), `retention: ${files.join(', ')}`);
  assert.equal(statSync(join(backups, dumpName)).mode & 0o777, 0o600);
  rmSync(join(backups, kept));
  passed(`backup on the running stack: ${countsAtBackup.length} tables, ${statSync(join(backups, dumpName)).size} bytes, mode 600; retention removed the ${RETENTION_DAYS + 1}-day dump and kept the ${RETENTION_DAYS - 1}-day one`);

  const afterBackup = await sourceSession.request('PATCH', `/api/v1/projects/${projectId}`, { json: { name: 'Nach dem Backup geändert' }, ifMatch: tagAtBackup });
  assert.equal(afterBackup.status, 200);
  const tagAfterBackup = afterBackup.headers.get('etag');
  passed(`project changed after the backup, ETag ${tagAfterBackup} (this change must be lost)`);

  compose(['stop', 'api']);
  compose(['rm', '-sf', 'db']);
  compose(['up', '-d', '--wait', 'db']);
  const databases = compose(['exec', '-T', 'db', 'psql', '-U', dbUser, '-d', 'postgres', '-At', '-c', `SELECT count(*) FROM pg_database WHERE datname = '${RESTORED_DATABASE}'`]);
  assert.equal(databases.stdout.trim(), '0');
  passed('database server replaced by an empty one');

  const restored = backupTool(['/opt/backup/restore.sh', `/backups/${dumpName}`, RESTORED_DATABASE]);
  assert.match(restored.stdout, /Restored/);
  assert.deepEqual(tableCounts(RESTORED_DATABASE, dbUser), countsAtBackup);
  passed(`restored into the new database ${RESTORED_DATABASE}: all ${countsAtBackup.length} tables present, row counts equal (log table not counted)`);

  const second = backupTool(['/opt/backup/restore.sh', `/backups/${dumpName}`, RESTORED_DATABASE], { allowFailure: true });
  assert.equal(second.status, 1);
  assert.match(second.stderr, /already exists/);
  passed('a second restore into the same database is refused');

  writeEnv({
    ...source,
    DB_NAME: RESTORED_DATABASE,
    CONCURRENCY_ETAG_EPOCH: '2',
    REHEARSAL_SEED_PASSWORD: '',
    REHEARSAL_TEST_RELEASE_PATH: '',
    REHEARSAL_TEST_RELEASE_VERSION: '',
  });
  compose(['up', '-d', '--no-deps', '--no-build', 'api']);
  const baseUrl = await apiUrl();
  passed('API started on the restored database, ETag epoch 2, seeders off');

  sourceSession.baseUrl = baseUrl;
  const probe = await (await sourceSession.request('GET', '/api/v1/auth/session')).json();
  assert.ok(probe.user, 'the session from before the backup is still valid');
  passed('session cookie from before the backup still accepted (restored key ring read with the certificate)');

  const session = new Session(baseUrl);
  await session.signIn();
  passed('signed in with the password from the dump');

  const opened = await session.request('GET', `/api/v1/projects/${projectId}`);
  assert.equal(opened.status, 200);
  assert.equal((await opened.json()).name, projectName);
  const restoredTag = opened.headers.get('etag');
  assert.equal(restoredTag, `"2-${version}"`);
  passed(`project opened with its state at the backup, ETag ${restoredTag}`);

  const catalog = await session.request('GET', '/api/v1/catalog/filter-options');
  assert.equal(catalog.status, 200);
  assert.equal((await catalog.json()).release, 'T.1');
  passed('catalog release T.1 served from the restored database');

  for (const staleTag of [tagAtBackup, tagAfterBackup]) {
    const refused = await session.request('PATCH', `/api/v1/projects/${projectId}`, { json: { name: 'Mit altem ETag' }, ifMatch: staleTag });
    assert.equal(refused.status, 409, `stale tag ${staleTag}`);
  }
  passed(`tags from before the restore refused with 409 (${tagAtBackup}, ${tagAfterBackup})`);

  const changed = await session.request('PATCH', `/api/v1/projects/${projectId}`, { json: { name: 'Nach dem Restore geändert' }, ifMatch: restoredTag });
  assert.equal(changed.status, 200);
  passed(`change with the new tag accepted, ETag ${changed.headers.get('etag')}`);
}

try {
  await rehearse();
  console.log('Restore rehearsal passed.');
} catch (error) {
  console.error(`Restore rehearsal FAILED: ${error instanceof Error ? error.message : String(error)}`);
  const logs = compose(['logs', '--no-color', '--tail', '40', 'api'], { allowFailure: true });
  console.error(logs.stdout);
  process.exitCode = 1;
} finally {
  compose(['down', '-v', '--remove-orphans', '--rmi', 'local'], { allowFailure: true });
  rmSync(work, { recursive: true, force: true });
}
