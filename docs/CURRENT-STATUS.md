# Current Status — Step 22.4.2 Canonical Foundation Regression Correction

## Physically closed boundary

**Steps 01–22 are closed on a physical iPhone.** The authoritative runtime/framework-binding closure remains Step 22.2:

- Step 22 A–D: 4/4;
- 22/22 required host-binding roots qualified;
- explicit binding blockers: 0;
- runtime closure ready for first real CLR load: YES;
- OfflineReady regression: PASS;
- Foundation 5/5 regression: PASS.

The wider 44-name diagnostic still contains 18 transitive-only desktop/workspace implementation names that are not independent private-runtime requirements.

## Canonical-foundation acceptance history

Step 22.4 established the canonical source/document/history architecture and passed Codemagic static validation 122/122. Codemagic then stopped compiling one additive report-writer unit test because the installed MSTest 4.x API uses `Assert.ThrowsExactlyAsync` rather than the removed `ThrowsExceptionAsync` API.

Step 22.4.1 corrected that host-test API mismatch. Codemagic built and the resulting physical iPhone regression run was healthy except for **Step 19 Gate A**. Every other test run by the user passed.

The Step 19 failure was traced to a stale historical assertion, not a launcher/runtime regression. Step 19 was originally physically proven before Step 20 enabled the Mono interpreter, when the iPhone reported:

- `RuntimeFeature.IsDynamicCodeSupported = false`
- `RuntimeFeature.IsDynamicCodeCompiled = false`

Step 20 intentionally established the canonical `MtouchInterpreter=-all` runtime. Later physical diagnostics report:

- `RuntimeFeature.IsDynamicCodeSupported = true`
- `RuntimeFeature.IsDynamicCodeCompiled = false`

The old Step 19 regression incorrectly required both values to stay false forever, even though Step 20 deliberately changed the first value.

## Active candidate — Step 22.4.2

Step 22.4.2 corrects the **current regression contract** while preserving the historical Step 19 documentation.

- Version: **0.0.64 (64)**
- Codemagic workflow: **`ios-step-22-4-2`**
- Live iOS project: **`src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`**
- Real StS2 CLR load/execution: **still intentionally not attempted**

Current Step 19 Gate A now requires:

1. `Compile()` returns and executes to `42`;
2. `Compile(preferInterpretation: false)` returns and executes to `42`;
3. `Compile(preferInterpretation: true)` returns and executes to `42`;
4. on iOS, `RuntimeFeature.IsDynamicCodeCompiled == false`;
5. `RuntimeFeature.IsDynamicCodeSupported` is recorded diagnostically and may be either `false` (historical pre-Step-20 mode) or `true` (canonical interpreter-enabled Step-20+ mode).

A new pure `ExpressionRuntimeCompatibilityPolicy` makes this distinction explicit and unit-testable. Static validation rejects reintroduction of the obsolete `IsDynamicCodeSupported == false` current-runtime assertion.

No Steam/install/Godot/runtime-binding behavior changed. The only physically proven Step 22.2 Core behavior file intentionally modified is `ExpressionInterpreterCompatibility.cs`, and that delta is separately hash-pinned; the other 96 baseline Core behavior files remain byte-for-byte protected.

## Acceptance required before Step 23

Codemagic must pass static validation, host unit tests, Godot/native build/preflight, iOS publish, and IPA verification.

On device:

1. confirm `STEP 22.4.2 — CANONICAL FOUNDATION`, version `0.0.64`;
2. run Step 19 A–D and require 4/4; on the canonical runtime expect `IsDynamicCodeSupported=true` and `IsDynamicCodeCompiled=false` unless the runtime implementation changes while still satisfying the non-JIT contract;
3. run Step 22 A–D and require 4/4, explicit binding blockers 0, runtime closure ready YES;
4. run `Verify Offline-Ready Install (Local Only)` and require PASS;
5. run Foundation 5/5 and require PASS;
6. confirm the corresponding `.txt` reports are created in Files.

Only after this completely green canonical-foundation acceptance should Step 23 begin.
