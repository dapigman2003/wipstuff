# Testing — Steps 43–47 startup ladder / 0.0.176

Active candidate: `0.0.176 (176)`, IPA `StS2-Launcher-Steps-43-47.ipa`, workflow `ios-canonical`.

Physical prerequisite authority: Step 42.0 / 0.0.175 is closed 4/4. Exact `NGame.InitPools()` token `0x06001BFA`, 3 direct IL instructions, 22-method transitive same-sts2 closure, zero classified/unresolved/external Cecil boundaries, one exact invocation returned, state 2 preserved, renderer frozen, and resolver/host/private/initializer/rejected/native deltas all zero.

The new packaging strategy does not auto-run rungs. The device operator runs Step 43, then 44, then 45, then 46, then 47 only after the immediately preceding rung shows 4/4. Stop on the first failure. Each rung has a separate crash checkpoint, last checkpoint, final report, and (Steps 43–46) static map.

Step 43 requirements: same-process Step42 4/4; frozen rendering; complete `PlatformUtil` + concrete `NullPlatformUtilStrategy` static map written before the read probe; runtime `PrimaryPlatform` dispatch must return exact concrete Null strategy; read-only local player/branch/language/window-mode probes only; state/context/native deltas zero.

Step 44 requirements: Step43 4/4; map only the two `HasLegacyData()` methods; no migration/archive method invocation. If either probe returns true, this is an expected fail-closed stop requiring a dedicated migration/backup iteration. If both false, frozen confinement closes 4/4.

Step 45 requirements: Step44 4/4/no legacy data; map `InitProfileId(Nullable<int>)`, `InitProgressData()`, `InitPrefsData()` and write the map before Gate C; one-shot invoke `InitProfileId(null) -> InitProgressData -> InitPrefsData`; never retry after arm; require SaveManager `SettingsSave`, `PrefsSave`, `Progress` non-null plus state/context/native confinement.

Step 46 requirements: Step45 4/4; map exact `NGame.LaunchMainMenu(bool)` wrapper, `AsyncStateMachineAttribute`, compiler state-machine fields, full `MoveNext` IL, then its transitive same-sts2 closure. Only proven Null-platform read helpers and inert `SentryService` wrappers may be classified. `OneTimeInitialization`, `InitializePlatform`, `LoadDeferredStartupAssets`, external Steamworks, FMOD, Spine, external Sentry and native-extension edges fail before invocation. The complete map must be durably written and marked before Gate D; LaunchMainMenu remains uninvoked and rendering frozen.

Step 47 requirements: Step46 4/4/admissible/durable; exact runtime `LaunchMainMenu(bool)` token equals Step46 token and returns `Task`; UI starts rendering exactly once immediately before Gate C; invoke `LaunchMainMenu(skipIntro=true)` exactly once; await the un-cancelable Task to completion with **no abandonment timeout**; state/context/native deltas zero; Gate D requires `RootSceneContainer` child count to increase. A hard hang is localized by the durable pre-invocation/start-render checkpoint and requires process relaunch. On any normal managed failure return, rendering must be refrozen at the first managed opportunity. On 4/4 success, rendering intentionally remains active for visual observation.

Codemagic remains compile/unit-test/iOS-AOT/native-link/IPA authority. Physical iPhone remains runtime authority. A later rung cannot close or be attempted if an earlier rung failed in that process.
