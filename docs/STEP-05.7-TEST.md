# Step 05.7 device test

Build the unsigned IPA with the normal Codemagic workflow and install it on the same physical iPhone used for Steps 05.5–05.6.

Run **Run SteamKit iOS WebSocket Fix Test** once with the app kept in the foreground.

Record the displayed CM Network result and SteamKit WebSocket result. The target result is native CM networking 4/4 and SteamKit WebSocket 3/3. If SteamKit still fails, copy the HTTP factory calls, CurrentEndPoint, callback states, elapsed time, and first-chance exception text exactly.

This step performs no authentication.
