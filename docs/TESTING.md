# Testing

Active candidate: `0.0.164 (164)`, IPA `StS2-Launcher-Step-38.ipa`, workflow `ios-canonical`.

Canonical validation must preserve the physical Step-36 and Step-37 closures; preserve the exact ModelDb compatibility transform; require the additional serialized `GameStartupWrapper` body to be only `call Task.get_CompletedTask; ret`; require unchanged `_EnterTree` to retain one direct wrapper call; prove Step38 core contains no SceneTree `AddChild` or direct later-startup invocation; preserve resolver/native fail-closed policy; and keep proprietary game/native payloads out of the archive.

Host tests cover Step38 gate ordering and pinned lifecycle names. Codemagic remains the first actual C# compiler for the new iOS/core source in this environment.

Physical sequence: fresh process -> Step 15 A-C -> Step 35 MODEL-BOOTSTRAP 4/4 -> Step 36.0.5 4/4 -> Step 37.0.1 4/4 -> Step 38.1 A-D once. Use a fresh process after any Gate-C-started failure.
