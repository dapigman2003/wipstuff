# Testing — Steps 43–50 direct main-menu startup ladder / 0.0.179

Active candidate: `0.0.179 (179)`, IPA `StS2-Launcher-Steps-43-50.ipa`, workflow `ios-canonical`.

The ladder does not auto-run rungs. On device run **43 → 44 → 45 → 46 → 47 → 48 → 49 → 50** only after the immediately preceding rung shows 4/4. Stop on the first failure. Each rung has its own run-correlated crash checkpoint, last checkpoint, final report, and distinct static map. Steps 43–49 require rendering frozen. Step 50 alone may pulse rendering and must synchronously stop again before its Gate-C result is evaluated.

Step 43: same-process Step42 4/4; exact concrete `NullPlatformUtilStrategy`; read-only platform probes only; no external Steam/native path; state/context/native confinement.

Step 44: Step43 4/4; map and invoke only two `HasLegacyData()` probes. Any legacy-data result stops before migration/archive mutation.

Step 45: Step44 4/4/no legacy data; exact `_mockInstance`/`_instance` singleton authority; no lazy `get_Instance()`/`ConstructDefault()` fallback; exact `InitProfileId(null) -> InitProgressData() -> InitPrefsData()` one-shot sequence; at most one exactly paired framework-shaped persisted-plan host binding; private/initializer/rejected/native deltas zero; post-action baseline frozen; `SettingsSave`, `PrefsSave`, and `Progress` non-null. Physical 0.0.178 closes this rung via Step46 Gate-A acceptance.

Step 46.1: Step45 4/4; locate exact `NGame.LaunchMainMenu(bool)` wrapper, async state machine, fields, full `MoveNext` IL; map immediate `call/callvirt/newobj/jmp` edges; record but do not traverse `ldftn/ldvirtftn/ldtoken` deferred frontiers; recursively expand compiler state machines only from immediately reached wrappers; zero unresolved same-sts2 and zero external Cecil resolution; durable map before Gate D; original `LaunchMainMenu` never invoked or authorized; rendering frozen.

Step 47: Step46 4/4 durable map; exact receipt-backed main-menu/background byte/hash authority; deterministic private Spine-neutral background derivative; no trusted-install mutation; write resource map before Godot loading; arm/disable the one-shot resource-cache boundary before loading; load derivative with cache-ignore + exact background `TakeOverPath`; load original `main_menu.tscn` with cache-reuse; require instantiable PackedScenes; no scene instantiation/rendering; frozen post-load baseline.

Step 48: Step47 4/4; exact `PackedScene.Instantiate(GenEditState)` binding; exact NMainMenu type; one-shot off-tree instantiate with `GenEditState.Disabled`; require private selected load-context ownership and `IsInsideTree=false`; enumerate actual instantiated hierarchy/callback roots; immediate-frontier audit must be admissible; durable actual hierarchy/lifecycle map before Step49; never retry after arm.

Step 49: Step48 4/4; exact real `NGame.RootSceneContainer`; exact AddChild binding and child-count map written before mutation; one-shot `AddChild(NMainMenu)` while renderer frozen; require child-count +1, exact parent, `IsInsideTree=true`, retained NGame/state and frozen confinement; never retry after arm.

Step 50: Step49 4/4; enumerate actual in-tree menu frame/input roots and immediate frontiers; durable map before rendering; one-shot `StartRendering`; request 100 ms; `StopRendering` at first managed continuation before post-stop telemetry/file I/O; no abandonment timeout; Gate C/D require rendering stopped again and retained menu/NGame/state/context/native authority. Exception cleanup also refreezes.

Codemagic remains compile/unit-test/iOS-AOT/native-link/IPA authority. Physical iPhone remains runtime authority. A later rung cannot close or be attempted if an earlier rung failed in that process.
