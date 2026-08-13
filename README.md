# StS2 Launcher iOS — Step 05.4

This step isolates the remaining Steam connection failure by testing SteamKit2 3.3.1 with explicit CM transports on iOS.

Step 05.3 proved:

- unsigned IPA build succeeds;
- install/launch/lifecycle succeed;
- Core regression test is 12/12;
- SteamKit2 loads and `SteamClient` constructs after the iOS `Process.StartTime` compatibility patch;
- the default Steam connection path receives no `ConnectedCallback` and terminates with an early `DisconnectedCallback`.

Step 05.4 keeps every proven build fix and changes only the connection configuration. One button runs two independent clients in order:

1. `ProtocolTypes.WebSocket` only;
2. `ProtocolTypes.Tcp` only.

Each client must construct, receive `ConnectedCallback`, explicitly disconnect, and receive `DisconnectedCallback` for a 3/3 result. The screen also reports whether an early `DisconnectedCallback` was user initiated.

No authentication, Steam Guard, ownership, depot, Cecil-at-runtime, Godot, or game behavior is added.

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.4.ipa
```

Expected device header:

```text
STEP 05.4 — STEAM TRANSPORT ISOLATION
Version 0.0.10
```
