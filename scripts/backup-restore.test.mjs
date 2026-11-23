// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of scripts/backup/*.sh (S6-29, R-ADM-04): the scripts run in a real shell against fake
// PostgreSQL tools, which record their calls. The real tools are exercised by the restore rehearsal
// (scripts/rehearse-restore.mjs), which needs Docker and does not run in CI.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { chmodSync, existsSync, mkdirSync, mkdtempSync, readFileSync, readdirSync, rmSync, statSync, utimesSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

const scripts = join(dirname(fileURLToPath(import.meta.url)), 'backup');
const DAY_MS = 86_400_000;

/** `20261123T020000Z` for a date, the stamp in a dump's file name. */
function stampOf(date) {
  return date.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z$/, 'Z');
}

/**
 * A folder with fake tools and an empty backup folder. Each tool appends its arguments to calls.log.
 * `pg_dump` writes the file named by --file; a tool named in `failing` exits 1, and `pg_dump` then
 * leaves half a file behind, as a full disk would.
 */
function sandbox(t, { failing = [], databaseExists = false } = {}) {
  const root = mkdtempSync(join(tmpdir(), 'pa-backup-test-'));
  t.after(() => rmSync(root, { recursive: true, force: true }));
  const bin = join(root, 'bin');
  const backups = join(root, 'backups');
  const log = join(root, 'calls.log');
  mkdirSync(bin);
  mkdirSync(backups);

  const tool = (name, body) => {
    const file = join(bin, name);
    const fail = failing.includes(name) ? 'exit 1' : ':';
    writeFileSync(file, `#!/bin/sh\necho "${name} $*" >> "${log}"\n${body(fail)}\n`);
    chmodSync(file, 0o755);
  };
  tool('pg_dump', (fail) => `for a in "$@"; do case $a in --file=*) printf dump > "\${a#--file=}";; esac; done\n${fail}`);
  tool('pg_restore', (fail) => fail);
  tool('psql', (fail) => `${fail}\n${databaseExists ? 'echo 1' : ':'}`);
  tool('createdb', (fail) => fail);
  tool('dropdb', (fail) => fail);

  const run = (script, args = [], env = {}) =>
    spawnSync('sh', [join(scripts, script), ...args], {
      encoding: 'utf8',
      env: {
        PATH: `${bin}:${process.env.PATH}`,
        PGDATABASE: 'live',
        BACKUP_PATH: backups,
        BACKUP_RETENTION_DAYS: '14',
        ...env,
      },
    });
  const calls = () => (existsSync(log) ? readFileSync(log, 'utf8').trim().split('\n') : []);
  return { backups, run, calls, root };
}

test('backup: writes one readable dump of the whole database, for its owner only', (t) => {
  const { backups, run, calls } = sandbox(t);
  const result = run('backup.sh');
  assert.equal(result.status, 0, result.stderr);
  const files = readdirSync(backups);
  assert.equal(files.length, 1);
  assert.match(files[0], /^procureaccess-\d{8}T\d{6}Z\.dump$/);
  assert.equal(statSync(join(backups, files[0])).mode & 0o777, 0o600);
  const dump = calls().find((call) => call.startsWith('pg_dump'));
  assert.match(dump, /--format=custom/);
  assert.doesNotMatch(dump, /--schema|--table|--exclude/, 'every schema is dumped');
  assert.ok(calls().some((call) => call.startsWith('pg_restore --list')));
});

test('backup: removes only dumps past the retention, and keeps foreign files', (t) => {
  const { backups, run } = sandbox(t);
  const now = Date.now();
  const expired = `procureaccess-${stampOf(new Date(now - 15 * DAY_MS))}.dump`;
  const kept = `procureaccess-${stampOf(new Date(now - 13 * DAY_MS))}.dump`;
  const foreign = ['procureaccess-manual.dump', 'notes.txt'];
  for (const name of [expired, kept, ...foreign]) {
    writeFileSync(join(backups, name), 'x');
  }

  const result = run('backup.sh');

  assert.equal(result.status, 0, result.stderr);
  const files = readdirSync(backups);
  assert.ok(!files.includes(expired), 'older than 14 days');
  assert.ok(files.includes(kept), 'younger than 14 days');
  for (const name of foreign) {
    assert.ok(files.includes(name), `${name} is not a dump of this script`);
  }
});

for (const failing of ['pg_dump', 'pg_restore']) {
  test(`backup: when ${failing} fails, no new dump appears and no old dump is removed`, (t) => {
    const { backups, run } = sandbox(t, { failing: [failing] });
    const old = `procureaccess-${stampOf(new Date(Date.now() - 30 * DAY_MS))}.dump`;
    writeFileSync(join(backups, old), 'x');

    const result = run('backup.sh');

    assert.notEqual(result.status, 0);
    assert.deepEqual(readdirSync(backups), [old]);
  });
}

for (const days of ['', '0', '014', '-1', 'abc', '10000']) {
  test(`backup: refuses the retention "${days}" before it dumps anything`, (t) => {
    const { run, calls } = sandbox(t);
    const result = run('backup.sh', [], { BACKUP_RETENTION_DAYS: days });
    assert.equal(result.status, 2);
    assert.deepEqual(calls(), []);
  });
}

test('restore: creates the new database and restores into it', (t) => {
  const { backups, run, calls } = sandbox(t);
  const dump = join(backups, 'd.dump');
  writeFileSync(dump, 'x');

  const result = run('restore.sh', [dump, 'procureaccess_restored']);

  assert.equal(result.status, 0, result.stderr);
  assert.ok(calls().some((call) => call.startsWith('createdb') && call.endsWith('procureaccess_restored')));
  const restore = calls().find((call) => call.includes('--dbname=procureaccess_restored'));
  assert.match(restore, /--single-transaction/);
  assert.ok(!calls().some((call) => call.startsWith('dropdb')));
});

test('restore: refuses a database that exists and changes nothing', (t) => {
  const { backups, run, calls } = sandbox(t, { databaseExists: true });
  const dump = join(backups, 'd.dump');
  writeFileSync(dump, 'x');

  const result = run('restore.sh', [dump, 'live']);

  assert.equal(result.status, 1);
  assert.match(result.stderr, /already exists/);
  for (const tool of ['createdb', 'dropdb', 'pg_restore --dbname']) {
    assert.ok(!calls().some((call) => call.startsWith(tool)), `${tool} must not run`);
  }
});

for (const name of ['', 'Live', '1db', 'a-b', "x'; DROP DATABASE live; --", 'a'.repeat(64)]) {
  test(`restore: refuses the database name "${name.slice(0, 30)}"`, (t) => {
    const { backups, run, calls } = sandbox(t);
    const dump = join(backups, 'd.dump');
    writeFileSync(dump, 'x');
    const result = run('restore.sh', [dump, name]);
    assert.equal(result.status, 2);
    assert.deepEqual(calls(), []);
  });
}

test('restore: a failed restore removes only the database it created', (t) => {
  const { backups, run, calls, root } = sandbox(t);
  const dump = join(backups, 'd.dump');
  writeFileSync(dump, 'x');
  // pg_restore reads the dump (--list) and fails on the restore itself.
  const fake = join(root, 'bin', 'pg_restore');
  writeFileSync(fake, `${readFileSync(fake, 'utf8')}\ncase "$1" in --dbname=*) exit 1;; esac\n`);

  const result = run('restore.sh', [dump, 'procureaccess_restored']);

  assert.notEqual(result.status, 0);
  const drops = calls().filter((call) => call.startsWith('dropdb'));
  assert.equal(drops.length, 1);
  assert.ok(drops[0].endsWith('procureaccess_restored'));
});

test('restore: when createdb fails, it drops nothing', (t) => {
  const { backups, run, calls } = sandbox(t, { failing: ['createdb'] });
  const dump = join(backups, 'd.dump');
  writeFileSync(dump, 'x');

  const result = run('restore.sh', [dump, 'procureaccess_restored']);

  assert.notEqual(result.status, 0);
  assert.ok(!calls().some((call) => call.startsWith('dropdb')));
});

test('restore: refuses a missing dump', (t) => {
  const { backups, run, calls } = sandbox(t);
  const result = run('restore.sh', [join(backups, 'missing.dump'), 'procureaccess_restored']);
  assert.equal(result.status, 2);
  assert.deepEqual(calls(), []);
});

for (const [time, now, expected] of [
  ['02:00', Date.UTC(2026, 10, 23, 1, 0, 0) / 1000, 3600],
  ['02:00', Date.UTC(2026, 10, 23, 3, 0, 0) / 1000, 23 * 3600],
  ['02:00', Date.UTC(2026, 10, 23, 2, 0, 0) / 1000, 86400],
  ['00:00', Date.UTC(2026, 10, 23, 23, 59, 0) / 1000, 60],
  ['08:09', Date.UTC(2026, 10, 23, 8, 0, 0) / 1000, 540],
]) {
  test(`schedule: from ${new Date(now * 1000).toISOString()} to the next ${time} is ${expected} s`, (t) => {
    const { run } = sandbox(t);
    const result = run('schedule.sh', ['seconds-until', String(now)], { BACKUP_TIME_UTC: time });
    assert.equal(result.status, 0, result.stderr);
    assert.equal(result.stdout.trim(), String(expected));
  });
}

/** Runs the schedule for a moment, then stops it as `docker stop` does. */
function runScheduleBriefly({ backups, root, calls }) {
  const result = spawnSync('sh', [join(scripts, 'schedule.sh')], {
    encoding: 'utf8',
    timeout: 800,
    killSignal: 'SIGTERM',
    env: {
      PATH: `${join(root, 'bin')}:${process.env.PATH}`,
      PGDATABASE: 'live',
      BACKUP_PATH: backups,
      BACKUP_RETENTION_DAYS: '14',
    },
  });
  return { result, dumps: calls().filter((call) => call.startsWith('pg_dump')).length };
}

test('schedule: catches up at the start when no dump is younger than a day, and stops on SIGTERM', (t) => {
  const box = sandbox(t);
  const old = join(box.backups, `procureaccess-${stampOf(new Date(Date.now() - 2 * DAY_MS))}.dump`);
  writeFileSync(old, 'x');
  utimesSync(old, new Date(Date.now() - 2 * DAY_MS), new Date(Date.now() - 2 * DAY_MS));

  const { result, dumps } = runScheduleBriefly(box);

  assert.equal(dumps, 1);
  assert.equal(result.status, 143);
});

test('schedule: a restart with a dump younger than a day does not dump again', (t) => {
  const box = sandbox(t);
  writeFileSync(join(box.backups, `procureaccess-${stampOf(new Date())}.dump`), 'x');

  const { result, dumps } = runScheduleBriefly(box);

  assert.equal(dumps, 0);
  assert.equal(result.status, 143);
});

for (const time of ['24:00', '2:00', '02:60', 'nachts']) {
  test(`schedule: refuses the time "${time}"`, (t) => {
    const { run } = sandbox(t);
    const result = run('schedule.sh', ['seconds-until', '0'], { BACKUP_TIME_UTC: time });
    assert.equal(result.status, 2);
  });
}
