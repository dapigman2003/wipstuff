# Step 29 — Real StS2 Compatibility Target Audit

## Why this boundary exists

Physical Step 28.0.2 / `0.0.111 (111)` closed the transform-before-load mechanism at **5/5 PASS**. The project is therefore allowed to move from a launcher-owned semantic fixture toward one real receipt-backed StS2 compatibility transformation.

However, the repository does not preserve the old physical Step-17 report containing the exact concrete `sts2.dll` source → target call-site samples. Step 17's scanner code remains, but selecting a semantic patch now from broad categories or memory would violate the project's evidence rule.

Step 29.0 therefore regenerates the exact current primary ARM64 evidence and selects **at most one audit candidate**. It deliberately performs zero real-game writes. The later transformation candidate must be designed from Step 29's exact method/token/IL/target/body fingerprint.

## Ordered gates

### Gate A — SourceAdmissionAndOfflineReady

- require a fresh process with no `sts2` assembly already CLR-resident;
- re-prove the exact local `OfflineReady` tree;
- read the Step-12 receipt and require exactly one `data_sts2_macos_arm64/sts2.dll`;
- verify its receipt SHA-1, compute a diagnostic SHA-256, and open it only through Mono.Cecil `ReadingMode.Deferred`;
- bind an explicitly rejecting Cecil assembly/metadata resolver and require **zero dependency-resolution requests**;
- record exact assembly identity, MVID and runtime metadata version;
- do not CLR-load or invoke StS2.

### Gate B — ExactRiskCallSiteAudit

Scan only concrete managed IL in the primary receipt-backed ARM64 `sts2.dll`. Record exact source method, metadata token, IL offset/opcode, target scope/member, and a canonical SHA-256 fingerprint of the complete source method body.

The Step-29 candidate categories are intentionally bounded to post-Step-28 compatibility surfaces:

1. Harmony runtime patch APIs;
2. MonoMod runtime-detour/dynamic-method APIs;
3. `System.Reflection.Emit`;
4. `RuntimeHelpers.PrepareMethod`;
5. dynamic assembly loading;
6. selected platform/native managed APIs (`Process`, Registry, Windows principal, `NativeLibrary`, DllImport resolver, native function-pointer conversion);
7. indirect `calli`.

`Expression.Compile` is counted but excluded from Step-29 candidacy because Step 19 already physically established the host's compatible interpreter behavior. Godot/GodotSharp, Steamworks, FMOD, Spine, Harmony and MonoMod subsystem call counts are reported as context, but a subsystem label alone is not a rewrite authorization.

Cecil dependency resolution remains forbidden. A concrete IL call site proves code exists, not runtime reachability.

### Gate C — DeterministicCandidateSelection

Order candidate sites by the predeclared priority above, then by source method, IL offset and target. Select at most one exact site.

If no direct primary candidate exists, Gate C still completes with `NO DIRECT PRIMARY TARGET` and explicitly refuses to invent a rewrite. That result would direct the next iteration to broaden evidence deliberately rather than silently changing scope.

If one candidate is selected, the report must include:

- priority/category;
- source type/method;
- method metadata token;
- IL offset/opcode;
- target assembly scope/member;
- source method-body SHA-256 fingerprint.

The selected site is **audit evidence only**. Step 29 performs no Cecil write.

### Gate D — FinalIsolationAudit

- re-hash the primary source and require exact SHA-1/SHA-256/length stability;
- re-prove `OfflineReady` after the audit;
- require no `sts2` CLR identity was introduced;
- require zero Cecil dependency-resolution requests;
- report zero Cecil writes, zero Harmony/MonoMod runtime patching, zero Godot/game startup and zero native game loading.

## Acceptance

Step 29.0 closes at **A–D / 4/4 PASS**. A pass establishes that the repository now has a current, exact, receipt-backed real-StS2 target-selection report suitable for designing the first semantic transformation candidate.

It does **not** establish that the selected site is runtime-reachable, that a particular semantic replacement is safe, or that the game starts.

## Next-step rule

After the physical Step-29 report is supplied, inspect the selected method's exact surrounding IL and semantics. The next candidate may then predeclare **one** deterministic launcher-private Cecil transformation and verify it before CLR admission. Real game-member invocation, Godot startup and native game loading remain separately gated.

## Local candidate validation

Canonical static validation for the recreated `0.0.112 (112)` source candidate is **894/894 PASS**. The local environment does not contain the .NET SDK, so `scripts/test.sh` records `ERROR: dotnet is required to run host tests.`; this is an environment limitation, not a host-test failure. Codemagic is the next compile/full-host/iOS-publish/IPA authority before any physical Step-29 run.
