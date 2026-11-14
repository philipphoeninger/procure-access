// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type APIRequestContext } from '@playwright/test';

/**
 * Reads the mails the E2E stack sent from Mailpit's API (docker-compose.e2e.yml publishes it on
 * 127.0.0.1:8025; `E2E_MAILPIT_URL` overrides it). Every mail of the stack goes there, none leaves
 * the run (S5-11).
 */
const MAILPIT_URL = process.env['E2E_MAILPIT_URL'] ?? 'http://localhost:8025';

/** A mail as Mailpit stores it: the parts a test checks. */
export interface Mail {
  subject: string;
  from: string;
  to: string[];
  text: string;
  html: string;
}

interface SearchAnswer {
  messages: { ID: string }[];
}

interface MessageAnswer {
  Subject: string;
  From: { Address: string };
  To: { Address: string }[];
  Text: string;
  HTML: string;
}

/** The mails to an address, newest first. */
async function mailsTo(request: APIRequestContext, address: string): Promise<SearchAnswer['messages']> {
  const response = await request.get(`${MAILPIT_URL}/api/v1/search`, { params: { query: `to:"${address}"` } });
  expect(response.ok(), `Mailpit answers ${MAILPIT_URL}`).toBe(true);
  return ((await response.json()) as SearchAnswer).messages;
}

/**
 * Waits until `count` mails to the address arrived (within a few seconds: the API sends most mails
 * synchronously, the password reset from a background service) and returns the newest.
 */
export async function waitForMail(request: APIRequestContext, address: string, count = 1): Promise<Mail> {
  let messages: SearchAnswer['messages'] = [];
  await expect
    .poll(async () => (messages = await mailsTo(request, address)).length, { message: `${count} mail(s) to ${address}` })
    .toBeGreaterThanOrEqual(count);
  const response = await request.get(`${MAILPIT_URL}/api/v1/message/${messages[0].ID}`);
  expect(response.ok()).toBe(true);
  const message = (await response.json()) as MessageAnswer;
  return {
    subject: message.Subject,
    from: message.From.Address,
    to: message.To.map((recipient) => recipient.Address),
    text: message.Text,
    html: message.HTML,
  };
}

/** Every `href` of the HTML part. */
export function linksOf(html: string): string[] {
  return [...html.matchAll(/<a\s[^>]*href="([^"]*)"/gi)].map((match) => match[1].replaceAll('&amp;', '&'));
}
