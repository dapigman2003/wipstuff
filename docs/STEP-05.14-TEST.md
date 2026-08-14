# Step 05.14 device test

Step 05.13 proved that the repeated Reflection.Emit first-chance exception came from our own no-op protobuf AOT diagnostic before SteamKit setup. Step 05.14 removes that diagnostic and captures SteamKit's own internal `DebugLog` around the unchanged WebSocket connection attempt.

Build the unsigned IPA with the normal Codemagic workflow, install it on the same physical iPhone, and run **Run Step 05.14 SteamKit DebugLog Test** once with the app in the foreground.

Send a screenshot containing the completed SteamKit section, especially:

```text
STEAMKIT ASSEMBLY: ...
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: ...
DisconnectedCallback: ...
IsConnected ever: ...
CurrentEndPoint: ...
Outgoing ClientHello: YES/NO
SteamKit post-connect exception logged: YES/NO
Connection-setup exception logged: YES/NO
SteamKit DebugLog:
...
First-chance supplemental exceptions:
...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
```

The most important new evidence is the `SteamKit DebugLog` section. This step performs no authentication and records no raw Steam message payloads.
