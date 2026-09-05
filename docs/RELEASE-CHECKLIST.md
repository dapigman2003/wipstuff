# Release checklist — Step 35.0.31 / Step 36.0.1 / 0.0.155

Release identity: display/build `0.0.155 (155)`, IPA `StS2-Launcher-Step-36.ipa`, workflow `ios-canonical`.

- [ ] static validation passes;
- [ ] host regression suite passes with Step-36 tests;
- [ ] Step-15 native-link preflight passes;
- [ ] iOS compile/link succeeds;
- [ ] IPA verification succeeds;
- [ ] no `sts2.dll`, `GodotSharp.dll`, PCK, game executable/app bundle, IPA, credentials or other proprietary runtime payload are shipped in the source archive;
- [ ] Step-35 Gate-D outer-worker fix retains exact bridge/resolver/core behavior;
- [ ] Step-36 source token is exactly `0x06007D03`, exact full signature is static parameterless `System.Void`, and source/transformed semantic equality is enforced before invocation;
- [ ] Step-36 requires same-process exact Step-35 closure and state 1 before invocation;
- [ ] Gate B locates the exact PCK only through the verified Step-12 receipt, mounts it through exact prepared GodotSharp with `replaceFiles=false`/offset 0, and proves `res://localization/eng` before invocation;
- [ ] Step-36 requires state 2 after successful `ExecuteEssential` return;
- [ ] `ExecuteDeferred`, `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain forbidden.
