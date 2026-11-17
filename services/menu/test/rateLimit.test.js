import { after, before, test } from 'node:test';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { SignJWT } from 'jose';
import { parseRateLimit } from '../src/rateLimit.js';

const port = 18082;
const base = `http://localhost:${port}`;
const signingKey = 'unit-test-signing-key-0123456789-abcdef';
const limit = 3;
let child;

const bearer = async () => {
  const jwt = await new SignJWT({ name: 'grot' })
    .setProtectedHeader({ alg: 'HS256' })
    .setSubject('42')
    .setIssuer('grots-web')
    .setAudience('grots-pizzeria')
    .setIssuedAt()
    .setExpirationTime('5m')
    .sign(new TextEncoder().encode(signingKey));
  return { authorization: `Bearer ${jwt}` };
};

before(async () => {
  child = spawn(process.execPath, ['src/server.js'], {
    env: { ...process.env, PORT: String(port), JWT_SIGNING_KEY: signingKey, RATE_LIMIT: String(limit) },
    stdio: 'ignore',
  });
  for (let i = 0; i < 50; i++) {
    try {
      if ((await fetch(`${base}/healthz`)).ok) return;
    } catch {
      await new Promise((r) => setTimeout(r, 100));
    }
  }
  throw new Error('Server did not start.');
});

after(() => child.kill());

test('refuses a client that makes too many requests, but not the health check or the documentation', async () => {
  const headers = await bearer();

  for (let i = 0; i < limit; i++) {
    assert.equal((await fetch(`${base}/menu`, { headers })).status, 200);
  }

  const refused = await fetch(`${base}/menu`, { headers });
  assert.equal(refused.status, 429);
  assert.ok(refused.headers.has('retry-after'));
  assert.deepEqual(await refused.json(), {
    title: 'Too Many Requests',
    status: 429,
    detail: 'Too many requests have been made. Try again later.',
  });

  // The limit applies to every menu endpoint, and to requests that are not authenticated
  assert.equal((await fetch(`${base}/menu/margherita`)).status, 429);
  assert.equal((await fetch(`${base}/menu/price`, { method: 'POST' })).status, 429);

  for (const path of ['/healthz', '/openapi.json', '/']) {
    assert.equal((await fetch(`${base}${path}`)).status, 200, path);
  }
});

test('RATE_LIMIT is a whole number above 0, or not set', () => {
  assert.equal(parseRateLimit(undefined), 600);
  assert.equal(parseRateLimit(''), 600);
  assert.equal(parseRateLimit(' 25 '), 25);

  for (const value of ['0', '-1', '1.5', 'many', '1e3']) {
    assert.throws(() => parseRateLimit(value), /RATE_LIMIT must be a whole number above 0/, value);
  }
});
