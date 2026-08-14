# Step 05.15 device test

Step 05.14 proved the fatal post-connect exception is `Arg_GetMethNotFnd` inside protobuf-net reflection while serializing SteamKit's initial `ClientHello` header. Step 05.15 keeps `TrimMode=full` but roots `SteamKit2`, `protobuf-net`, and `protobuf-net.Core` so reflection-only members survive trimming.

Build the unsigned IPA with the normal Codemagic workflow, install it on the same physical iPhone, and run **Run Step 05.15 Protobuf Trim Test** once with the app in the foreground.

Send a screenshot containing the completed SteamKit section, especially:

```text
STEAMKIT ASSEMBLY: ...
TRIM ROOTS: SteamKit2 • protobuf-net • protobuf-net.Core
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: ...
DisconnectedCallback: ...
IsConnected ever: ...
CurrentEndPoint: ...
Outgoing ClientHello: YES/NO
SteamKit post-connect exception logged: YES/NO
SteamKit DebugLog:
...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
```

If Codemagic fails before producing the IPA, send the first linker/AOT error and the `step05-15-framework-filter.log` / focused failure excerpt. This step performs no authentication and records no raw Steam message payloads.
