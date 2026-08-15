# Testing strategy — Step 09

The project uses host unit tests, repository/build validation, Codemagic iOS compilation/AOT/linking, and physical-iPhone verification because no single layer proves every boundary.

## 1. Host unit tests

Run:

```text
bash scripts/run-unit-tests.sh
```

Coverage retains the foundation/auth/session/ownership/discovery contracts and adds deterministic Step 09 policy tests:

- target App ID remains exactly `2868840`;
- the controlled file cap is exactly 2 MiB;
- a direct public macOS depot is preferred;
- shared/proxied depots are not selected for this proof;
- a visible `public` manifest is required;
- traversal/rooted manifest paths are rejected;
- Step 09 result telemetry cannot expose raw downloaded bytes or token/key/request-code values.

Host tests do not prove live Steam CDN behavior, native iOS AOT/linking, or real iOS storage/network behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step09.sh
```

The validator protects Steps 01–08, requires the single-file Steam content-access API path, checks the 2 MiB/path/hash/atomic-write guards, and rejects markers for a full downloader, resume, update/install/repair, Godot, Cloud, or Workshop.

Codemagic then runs host tests, the isolated SteamKit iOS compatibility patch, .NET iOS AOT/native linking, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The device must prove:

- the saved Steam session still authenticates with matching identity;
- Step 07 ownership is re-proven;
- Step 08 PICS metadata discovery still succeeds;
- exactly one direct public depot is selected;
- one depot key and one manifest request code are obtained;
- one manifest is fetched;
- one safe regular file at most 2 MiB is selected;
- all selected-file chunks arrive at expected uncompressed sizes;
- assembled file SHA-1 matches the manifest;
- exactly one final verified file is written;
- secret/key/token values remain undisplayed;
- Foundation 5/5 still passes.

See `STEP-09-TEST.md`.
