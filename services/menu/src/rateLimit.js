import { rateLimit } from 'express-rate-limit';

const DEFAULT_RATE_LIMIT = 600;

/**
 * How many requests a client may make to the menu in a minute: RATE_LIMIT, or 600 if it is not set (a blank value, as Docker
 * Compose passes for an unset variable, is not set). The default is far above what the sample needs, so that only a runaway
 * client is ever refused. A value that is not a whole number above 0 stops the service starting.
 *
 * @param {string | undefined} value
 */
export function parseRateLimit(value) {
  const text = (value ?? '').trim();

  if (text === '') {
    return DEFAULT_RATE_LIMIT;
  }

  if (!/^[1-9]\d*$/.test(text) || !Number.isSafeInteger(Number(text))) {
    throw new Error(`RATE_LIMIT must be a whole number above 0, but was '${text}'.`);
  }

  return Number(text);
}

/**
 * Middleware that refuses a client's requests (with the error passed to `onLimit`) once it has made too many in a minute.
 * It is registered before the authentication of a route, so that requests with bad credentials count too.
 *
 * @param {() => Error} onLimit Makes the error that the application's error handler reports.
 */
export function createRateLimiter(onLimit) {
  return rateLimit({
    windowMs: 60_000,
    limit: parseRateLimit(process.env.RATE_LIMIT),
    standardHeaders: true,
    legacyHeaders: false,
    handler: (_req, _res, next) => next(onLimit()),
  });
}
