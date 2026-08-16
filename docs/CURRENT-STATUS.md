# Current status

**Steps 01–11: complete on physical iPhone.**

**Current source boundary: Step 12.1 — AOT receipt-serialization compatibility hotfix for the existing Step 12 install/update/repair manager.**

App version: `0.0.34 (34)`.
Codemagic workflow: `ios-step-12-1`.

The first Step 12 (`0.0.33`) physical-iPhone install attempt reached the full one-depot source/staging path for depot `2868842` and processed all `428` planned files / `2323747842` bytes, but failed before commit with:

```text
NotSupportedException: ConstructorContainsNullParameterNames,
StS2Launcher.Core.SteamManagedInstallReceipt
```

The prior managed install remained absent/preserved correctly, atomic commit did not run, and staging/backup cleanup completed.

Step 12.1 makes one narrow compatibility change: `.sts2launcher-install.json` serialization/deserialization now uses a compile-time `System.Text.Json` source-generation context (`JsonSerializerContext`) instead of the runtime reflection/options path that depends on positional-record constructor parameter metadata surviving full iOS trimming.

All Step 12 install/update/repair semantics remain unchanged. Step 12 is still open until the original physical-device Gates A–D pass.

Later boundaries remain excluded: multi-depot composition, compatibility inventory, Godot/runtime execution, Cloud, Workshop.
