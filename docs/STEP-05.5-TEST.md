# Step 05.6 device report

Build/install/launch must still pass before interpreting the network result.

Run **CM Boundary Diagnostics** once with the app in the foreground and network access enabled.

Report the complete on-screen output for:

- `CM NETWORK` score;
- Directory HTTPS PASS/FAIL and HTTP status;
- CM endpoint count;
- DNS PASS/FAIL and detail;
- Raw TCP PASS/FAIL, selected endpoint, and detail;
- Raw WebSocket PASS/FAIL, selected endpoint, and detail;
- SteamKit WebSocket x/3, callbacks, UserInitiated, and detail;
- SteamKit TCP x/3, callbacks, UserInitiated, and detail;
- SteamKit assembly version;
- Core self-test result.

If the Codemagic build fails, upload the complete `artifacts` ZIP instead of transcribing the failure.
