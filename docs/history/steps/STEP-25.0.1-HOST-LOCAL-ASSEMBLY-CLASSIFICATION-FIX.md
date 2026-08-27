# Step 25.0.1 — Host Local-Assembly Classification Fix

## Evidence from Step 25.0 / 0.0.80

Codemagic successfully compiled the Step 25 Core/test projects and executed the complete host suite. Static validation was 416/416 PASS. Host tests were 177/180 PASS. No IPA/device result is accepted from this candidate.

The three failures were:

- `SyntheticStep24ReplayThenExactHarmonyConstructionPasses`;
- `GateCReportsThrowingModuleInitializerAndDoesNotAdvance`;
- `GateAConditionallyAcceptsExactPhysicalMonoModLoggerFingerprintOnlyWhenInert`.

The first two failed in Gate A because the synthetic Harmony fixture intentionally uses a unique assembly identity, while `ReadHarmonyConstructorMetadata` classified same-assembly constructor calls by comparing the call scope to the production constant `0Harmony`. The fixture-local `HarmonyLib.Harmony::set_Id(System.String)` was therefore incorrectly reported as an unexpected external execution edge.

The third failure was test-only wording drift: production correctly identifies the seven MonoMod logger findings as the physically measured **Step 24.0.4** fingerprint, while the Step 25 test incorrectly expected `Step 25.0.4`.

## Step 25.0.1 correction

Version: **0.0.81 (81)**.

The constructor metadata audit now determines same-assembly calls from `module.Assembly.Name.Name`, i.e. the assembly actually being audited. On the real Step 25 target this remains exactly `0Harmony`; the production allow/deny boundary is therefore unchanged. Synthetic fixtures may retain unique assembly identities for collectible-context isolation without being misclassified.

The stale fingerprint assertion is corrected to `Step 24.0.4`. No Harmony API, resolver, native policy, game/Godot boundary, type-initialization rule, or construction rule is broadened.

Step 25 remains unproven until Codemagic is fully green and a physical A–I run completes according to the current acceptance criteria.
