# Browser verification

Every story that touches the UI must be exercised in a real browser before it is
Done. These specs are that gate, kept in the repository rather than re-derived
each time.

## What they are, and are not

They are **verification against a running stack**: the real React app, the real
API, real Postgres. Nothing is mocked, because the defects this gate exists to
catch — a missing React `key`, a 404 rendered as a generic error, a mutation
that never invalidates its cache — only appear when the whole thing runs.

They are **not** a CI suite. They assume seeded data and a running environment.

## Running them

```bash
docker start postgres-local
dotnet run --project src/Host/RegOS.Api      # http://localhost:5225
cd web/regos-web && npm run dev              # http://localhost:5173

cd tests/Browser
npm install        # downloads no browsers: it drives your installed Chrome
npm test
```

## Conventions these specs follow

Each was learned from a check that reported the wrong answer:

1. **Wait for the observable business outcome, not an implementation detail.**
   Waiting on a row count passed while the previous result was still on screen,
   and the assertion read stale data. Wait for the content you expect.

2. **Fail on unexpected errors, not on every console message.** A test that
   deliberately requests a 404 will see the browser log it. Filter that one
   message narrowly — never disable the check.

3. **Verify the consumer of invalidated state.** Checking that a detail page
   shows an edit proves little; that view holds fresh state anyway. Check the
   *list* too, which only refreshes if the cache was genuinely invalidated.
