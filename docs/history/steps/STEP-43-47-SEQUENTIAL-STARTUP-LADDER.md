# Steps 43–47 — sequential startup ladder

Candidate: 0.0.176 (176)

## Motivation

Physical 0.0.175 closed Step 42 at 4/4. The remaining startup frontier is no longer a single unknown feasibility boundary; it is a sequence of separately understandable platform/save/menu boundaries. Rebuilding/reinstalling for every clean rung adds iteration latency without adding safety.

0.0.176 therefore changes the packaging unit from one experimental rung per IPA to several independently gated rungs per IPA. The safety unit remains one rung at a time. Every later rung requires exact same-process prior-rung 4/4 authority and is locked after any failure. Mutation rungs remain one-shot.

## Ladder

### Step 43 — Null platform authority

Re-prove that `PlatformUtil` dispatches to exact `MegaCrit.Sts2.Core.Platform.Null.NullPlatformUtilStrategy`, map its read-only platform surface with rejecting-resolver Cecil, then invoke only read getters. Exact `SteamInitializer.get_Initialized()` observation is tolerated as a non-native branch discriminator; external Steamworks is not.

### Step 44 — legacy migration guard

Map and invoke only account/profile `HasLegacyData()` probes. If either is true, stop before mutation. A later dedicated candidate can snapshot and migrate from evidence. If both are false, no migration work is needed for this sandbox.

### Step 45 — local SaveManager initialization

Map and one-shot invoke `InitProfileId(null)`, `InitProgressData()`, and `InitPrefsData()` in original GameStartup order. Cloud sync and migrations remain uninvoked. Require non-null settings/prefs/progress authority and zero resolver/native context drift.

### Step 46 — LaunchMainMenu async map

Step 41 only mapped the async wrapper reference reached from GameStartup. Step 46 explicitly resolves `LaunchMainMenu(bool)`'s own `AsyncStateMachineAttribute`, compiler state-machine fields, exact `MoveNext` IL, and transitive same-sts2 closure. It then recursively expands every reached same-sts2 async/iterator compiler state machine into its `MoveNext` so nested wrappers cannot hide an unsafe edge. The full expanded map must be admissible and durably written before any launch attempt.

### Step 47 — controlled live LaunchMainMenu

Only if Step 46 closes 4/4/admissible: bind exact runtime token, start rendering once, invoke `LaunchMainMenu(skipIntro=true)` once, await its un-cancelable Task to completion with no abandonment timeout, and require `RootSceneContainer` child growth plus state/context/native confinement. A hard hang remains at the durable pre-invocation checkpoint until the process is killed/relaunched; normal managed failures refreeze rendering. Leave rendering active only on 4/4 success so the real scene can be observed.

## Global exclusions

The ladder never invokes `GameStartup()` as a whole, `DoCloudSync`, migration/archive mutation, `InitializePlatform`, external Steamworks/native Steam APIs, `ExecuteDeferred`, `LoadDeferredStartupAssets`, FMOD/Spine/native game extensions, `_ExitTree`, RemoveChild/Free, or trusted-install mutation. Step 47 is the only rung allowed to restart rendering.
