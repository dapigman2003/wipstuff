# StS2 Launcher iOS — Step 05.2

Step 05.1 proved that `TrimMode=full` is useful but does not eliminate the iOS native-link failure:

```text
ld: framework 'DiskArbitration' not found
```

The failed build's MSBuild/binlog artifacts showed that .NET iOS generated `DiskArbitration` inside its `_LinkerFrameworks` item set and passed it explicitly to the final `clang++` command.

Step 05.2 keeps the Steam runtime probe unchanged and makes one narrowly scoped link-boundary change:

1. keep `TrimMode=full`;
2. let the .NET iOS linker produce its normal framework list;
3. after `_LoadLinkerOutput`, remove only `DiskArbitration` from `_LinkerFrameworks`;
4. do so before `_ComputeLinkNativeExecutableInputs`;
5. leave every other framework untouched.

If SteamKit contains a genuinely live call to a DiskArbitration symbol, the build should now progress far enough for `clang`/`ld` to report that concrete undefined symbol. If no live symbol remains, the native link can proceed without trying to load a macOS-only framework from the iPhoneOS SDK.

## Scope remains unchanged

The on-device test still does only this:

1. construct `SteamClient`;
2. construct `CallbackManager`;
3. call `Connect()`;
4. receive `ConnectedCallback`;
5. call `Disconnect()`;
6. receive `DisconnectedCallback`.

Still excluded:

- Steam login/authentication;
- passwords/tokens/Steam Guard;
- ownership checks;
- depot downloads;
- Mono.Cecil;
- Godot;
- game files.

## Device marker

```text
STEP 05.2 — IOS FRAMEWORK FILTER
Version 0.0.8
```

## Build artifact

```text
artifacts/StS2-Launcher-Step-05.2.ipa
```

## New diagnostics

```text
artifacts/logs/step05-2-framework-filter.log
artifacts/logs/step05-2-generated-linker-frameworks.txt
artifacts/logs/step05-2-native-symbols.log
artifacts/logs/step05-2-failure-scan.log
artifacts/logs/step05-2-publish.log
artifacts/logs/step05-2-dotnet-ios.binlog
```

The key successful-build telemetry should show `DiskArbitration` in `BEFORE` and absent from `AFTER`.

See `docs/STEP-05.2-TEST.md` for the result format.
