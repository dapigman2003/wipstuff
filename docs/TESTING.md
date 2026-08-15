# Testing strategy — Step 10

The project uses host unit tests, repository/build validation, Codemagic iOS compilation/AOT/linking, and physical-iPhone verification because no single layer proves every boundary.

## 1. Host unit tests

Run:

```text
bash scripts/run-unit-tests.sh
```

Coverage retains the foundation/auth/session/ownership/discovery/single-file contracts and adds deterministic Step 10 policy tests:

- target App ID remains exactly `2868840`;
- the direct/public/macOS-first depot selection policy remains stable;
- byte-based progress reports deterministic percentages and file counts;
- Step 10 result telemetry cannot expose raw downloaded bytes or token/key/request-code values.

The source validator additionally enforces manifest path safety, exact chunk coverage, SHA-1 verification, staging cleanup, and atomic directory commit markers.

Host tests do not prove live Steam CDN behavior, a large depot download, native iOS AOT/linking, or real iOS filesystem/network behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step10.sh
```

The validator protects Steps 01–09, requires one selected direct-public depot queue, progress/cancel plumbing, staging-only partial writes, per-file SHA-1 verification, and final directory rename. It rejects markers for resume, update/install/repair, multi-depot app install, Godot, Cloud, or Workshop behavior.

Codemagic then runs host tests, the isolated SteamKit iOS compatibility patch, .NET iOS AOT/native linking, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The device must prove:

- the saved Steam session still authenticates with matching identity;
- Step 07 ownership is re-proven;
- Step 08 PICS metadata discovery still succeeds;
- one direct public depot is selected;
- depot key, manifest request code, and CDN access succeed;
- the manifest becomes a complete file/chunk/byte queue;
- every queued regular file reaches its expected size and Steam SHA-1;
- progress updates during the operation;
- no partial final directory is visible during the queue;
- the final manifest directory appears only after the complete verified staging tree is renamed;
- cancellation leaves no final commit and removes current staging data;
- secret/key/token values remain undisplayed;
- Foundation 5/5 still passes.

See `STEP-10-TEST.md`.
