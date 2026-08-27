# Step 32.0.3 — Maintenance Acceptance

Version: `0.0.118 (118)`

This candidate validates removal of retired runtime-Harmony experiment surface. It is **not** a Sentry resolver correction and is not expected to produce new Step-32 physical closure evidence.

## Required CI evidence

- Static validator: PASS with the canonical seven active scripts and no dependency on the inert historical archive.
- Host tests: PASS without downloading Harmony-Fat 2.4.2 and without compiling the retired Step-25/26/27 suites or Step-27 interpreted fixture.
- iOS publish: PASS with `MtouchInterpreter=-all`, `MtouchLink=None`, and `TrimMode=copy` unchanged.
- IPA verification: PASS with no retired Step-27 fixture payload and with release identity `0.0.118 (118)`.
- Record Codemagic duration and resulting IPA size for comparison with the previous active surface when available.

## Regression requirements

- Step 24 controlled initialization remains active.
- Step 28 ahead-of-load transformation remains active and tested.
- Step 32 implementation/test sources remain unchanged from 0.0.117 except release-level validation/hash manifests that describe this maintenance candidate.
- The known physical `Sentry, Version=5.0.0.0` constant-metadata scope remains documented as unresolved.
- No source, build, test, or runtime path reads or executes material from the inert historical archive.

## Physical device

No physical Step-32 rerun is required merely to accept this maintenance trim. If 0.0.118 is run on-device, the known Sentry Gate-B fail-closed result is expected because the resolver behavior is intentionally unchanged.
