# Step 05.12 device test

Step 05.11 showed `RuntimeTypeModel.Default.AutoCompile: False -> False`, so disabling protobuf runtime compilation did not actually change the device configuration and did not remove `PlatformNotSupported_ReflectionEmit`. `Outgoing ClientHello` remained `NO`, while the exact SteamKit-selected CM WebSocket replay still passed.

Step 05.12 changes the SteamKit2 package only: **3.3.1 -> 3.4.0**. The existing iOS compatibility fixes and diagnostics are retained.

Build the unsigned IPA with the normal Codemagic workflow, install it on the same physical iPhone, and run **Run Step 05.12 SteamKit 3.4.0 Comparison** once with the app in the foreground.

Send a screenshot containing the completed SteamKit section, especially:

```text
STEAMKIT ASSEMBLY: ...
STEAMKIT WEBSOCKET ... x/3
ConnectedCallback: ...
DisconnectedCallback: ...
IsConnected ever: ...
CurrentEndPoint: ...
Outgoing ClientHello: YES/NO
Protobuf AOT mode: ...
First-chance SteamKit/runtime exceptions: ...
EXACT STEAMKIT ENDPOINT REPLAY PASS/FAIL
```

If Codemagic fails before producing the IPA, send the SteamKit compatibility-patch section of the build log as well. This step performs no authentication.
