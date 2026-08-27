# Step 05.16 final device test

Step 05.15 already proved the key Step 05 boundary: SteamKit2 can complete an unauthenticated CM WebSocket connection on the physical iPhone and reach `STEAM CONNECTION PASS — 3/3`.

Step 05.16 is a cleanup/regression build. It should not expose the old diagnostic sequence. Instead, install the Codemagic IPA, launch it, keep the app in the foreground, and tap **Run Steps 01–05 Device Verification** once.

Expected result:

```text
FOUNDATION PASS — 5/5
App/UI startup: PASS
Lifecycle active: PASS
CORE SELF-TEST PASS — 12/12
CREDENTIAL STORE PASS — 7/7
STEAM CONNECTION PASS — 3/3
CMWebSocket factory used: YES
ConnectedCallback: YES
DisconnectedCallback: YES
```

Also confirm:

- app installs and launches;
- app stays open;
- SteamKit assembly reports the 3.4.0 line;
- the CurrentEndPoint is populated after connection;
- no login/authentication UI or account data is requested.

If this passes, Step 05 is finalized. The next major step is Step 06 authentication only.
