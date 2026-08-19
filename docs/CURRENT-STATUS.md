# Current Status — Step 22.4.1 Canonical Foundation Candidate

## Physically closed boundary

**Steps 01–22 are closed on a physical iPhone.** The authoritative runtime/framework-binding closure remains Step 22.2:

- Step 22 A–D: 4/4;
- 22/22 required host-binding roots qualified;
- explicit binding blockers: 0;
- runtime closure ready for first real CLR load: YES;
- OfflineReady regression: PASS;
- Foundation 5/5 regression: PASS.

The wider 44-name diagnostic still contains 18 transitive-only desktop/workspace implementation names that are not independent private-runtime requirements.

## Foundation consolidation history

Step 22.3 never reached C# compilation because its first static validator incorrectly treated historical material as a build dependency.

Step 22.4 fixed the canonical source/document/history architecture and **passed Codemagic static validation 122/122**. Codemagic then built the external fixtures and reached the host test-project compilation. The first real compile error was limited to the additive report-writer unit test: MSTest 4.3.2 no longer exposes `Assert.ThrowsExceptionAsync`, and `DataTestMethod` is obsolete.

No Core/iOS/runtime compatibility regression was observed before that stop.

## Active candidate — Step 22.4.1

Step 22.4.1 is the same behavior-neutral canonical foundation with only the MSTest v4 test-source correction.

- Version: **0.0.63 (63)**
- Codemagic workflow: **`ios-step-22-4-1`**
- Live iOS project: **`src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`**
- Real StS2 CLR load/execution: **still intentionally not attempted**

Changes from 22.4:

- `Assert.ThrowsExactlyAsync<ArgumentException>` replaces removed `ThrowsExceptionAsync` in the report-writer test;
- `[TestMethod]` replaces obsolete `[DataTestMethod]` while retaining the same `DataRow` cases;
- canonical validation now enforces those MSTest v4-compatible forms;
- release/version identifiers are bumped so the Codemagic/device result is unambiguous.

Production compatibility behavior is unchanged.

## Acceptance required before Step 23

Codemagic must pass static validation, host unit tests, Godot/native build/preflight, iOS publish, and IPA verification.

On device:

1. confirm `STEP 22.4.1 — CANONICAL FOUNDATION`, version `0.0.63`;
2. run Step 22 A–D and require 4/4, explicit binding blockers 0, runtime closure ready YES;
3. run `Verify Offline-Ready Install (Local Only)` and require PASS;
4. run Foundation 5/5 and require PASS;
5. confirm the expected `.txt` reports are created in Files.

Only after that acceptance should Step 23 begin.
