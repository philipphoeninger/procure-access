// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// No imports: login-navigation.ts and the guard read this while app.config.ts may still be loading.

/** Control characters and whitespace: browsers drop tab and newline, so `/\t/host` would become `//host`. */
const CONTROL_OR_SPACE = /[\u0000- \u007F-\u009F\s]/;

/**
 * The return target after the sign-in (S6-03, TD-65), or `null` when it is not a path of this app.
 *
 * Only a same-origin relative path passes: it starts with one `/`, has no `//` or backslash anywhere
 * (`/\host` is `//host` to a browser), no control character or whitespace, does not open the `login`
 * outlet again (a sign-in loop), and resolves against `origin` to `origin` itself. Absolute URLs,
 * protocol-relative `//host`, `javascript:` and anything that is not a string are refused, so the
 * value can never send the user to another site (open redirect).
 *
 * @param value the target as the navigation state carries it; never trusted
 * @param origin the app's origin, e.g. `location.origin`
 */
export function safeReturnUrl(value: unknown, origin: string): string | null {
  if (typeof value !== 'string' || !value.startsWith('/')) {
    return null;
  }
  if (value.includes('//') || value.includes('\\') || CONTROL_OR_SPACE.test(value) || value.includes('(login:')) {
    return null;
  }
  try {
    return new URL(value, origin).origin === origin ? value : null;
  } catch {
    return null;
  }
}
