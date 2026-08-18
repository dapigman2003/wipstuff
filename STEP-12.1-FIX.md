# Step 12.1 — iOS/AOT managed-receipt JSON compatibility fix

## Physical-device failure that opened this substep

Step 12 version `0.0.33 (33)` reached the managed-install staging path on the physical iPhone for AppID `2868840`, depot `2868842`, manifest `8653035385353091849`.

Observed proof before failure:

```text
State before: NotInstalled
Action taken: Install
Planned files: 428
Planned bytes: 2323747842
Verified source files/bytes: 428 / 2323747842
Replaced files/bytes: 428 / 2323747842
Previous install preserved until commit: YES
Atomic commit completed: NO
Staging absent after result: YES
Backup absent after result: YES
```

Fatal boundary:

```text
NotSupportedException: ConstructorContainsNullParameterNames,
StS2Launcher.Core.SteamManagedInstallReceipt
```

This localizes the failure to receipt JSON metadata construction after the complete staging copy/hash loop and before the directory-swap commit.

## Narrow fix

`SteamManagedInstallReceipt` and `SteamManagedInstallFile` now have a compile-time `System.Text.Json` source-generation context:

```text
SteamManagedInstallJsonContext
```

All receipt reads and writes use its generated `JsonTypeInfo<SteamManagedInstallReceipt>`.

The old generic/options-based receipt serializer calls are rejected by `scripts/validate-step12.sh` so this reflection path cannot silently return.

A host unit test round-trips the exact receipt contract through the generated metadata.

## Unchanged boundaries

- `TrimMode=full` remains enabled.
- SteamKit2 remains `3.4.0`.
- The proven SteamKit/protobuf trim roots remain.
- The narrow DiskArbitration framework filter remains.
- The Process.StartTime SteamKit patch remains build-only.
- Step 11 remains the acquisition engine.
- Receipt schema/content remains non-secret and unchanged.
- Install/update/repair staging, SHA-1 verification, previous-install preservation, rollback, and atomic directory swap are unchanged.
- No ownership/download/Godot/Cloud/Workshop scope was added.

## Completion gate

Build workflow `ios-step-12-1`, install `0.0.34 (34)`, then rerun the existing `docs/STEP-12-TEST.md` Gates A–D. Step 12 is not closed until all gates pass on the physical iPhone.
