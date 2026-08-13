# Step 05.4 — Steam Transport Isolation

Install the generated `artifacts/StS2-Launcher-Step-05.4.ipa`, launch it, and run **WebSocket + TCP Probes** once.

Report the complete result blocks shown for both transports, especially:

```text
WEBSOCKET PASS/FAIL — x/3
ConnectedCallback: YES/NO
DisconnectedCallback: YES/NO
Disconnected.UserInitiated: True/False/N/A
Elapsed: ...
Detail: ...

TCP PASS/FAIL — x/3
ConnectedCallback: YES/NO
DisconnectedCallback: YES/NO
Disconnected.UserInitiated: True/False/N/A
Elapsed: ...
Detail: ...
```

Also report Core 12/12 PASS/FAIL. If the build fails, upload the complete Codemagic artifacts ZIP instead.
