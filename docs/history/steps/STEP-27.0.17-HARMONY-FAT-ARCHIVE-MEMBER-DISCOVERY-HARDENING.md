# Step 27.0.17 — Harmony-Fat archive member discovery hardening

Candidate: `0.0.101 (101)`

## Evidence from 0.0.100

Codemagic 0.0.100 passed the static validator (738 checks) and successfully downloaded the exact official `Harmony-Fat.2.4.2.0.zip`. The canonical host-test script then exited before any `dotnet build` or MSTest execution because it required the archive member to equal `netstandard2.0/0Harmony.dll` and found zero matches. The download itself therefore worked; the remaining error was the script's assumption that the framework directory lived at ZIP root.

Harmony fat distributions are packaged under a release-root directory (for example `Harmony-Fat.2.4.2.0/net48/0Harmony.dll`), while Harmony's ReleaseFat build output includes a `netstandard2.0/0Harmony.dll` implementation. The correct admission rule is therefore the framework/DLL suffix, not an invented archive-root path.

## Correction

`scripts/test.sh` still downloads the same exact tagged HTTPS release URL. It now writes the archive member list once and requires exactly one original member whose slash-normalized path ends in `/netstandard2.0/0Harmony.dll`. The original member string is retained for `unzip -p`; comparison-only normalization also tolerates ZIPs that encode backslashes. On zero or multiple matches, the script prints every discovered `0Harmony.dll` member before failing, making future packaging drift immediately visible.

The extracted DLL remains quarantined at `artifacts/host-step27-fixtures/0Harmony.dll`, both archive and DLL SHA-256 values are recorded, and `STS2_STEP27_REAL_HARMONY_FIXTURE` remains the only handoff into the real-Harmony regression.

## Runtime scope

`src/StS2Launcher.Core/Runtime/ControlledHarmonyPatchExecution.cs` is unchanged from 0.0.100. Deferred Cecil normalization, the exact 11-instruction HarmonySharedState initializer, Gate S, Gate T, the detour stop rule, and all StS2 prohibitions are unchanged. This candidate exists only to let the real-binary CI proof execute.

The master plan remains unchanged.
