# Release Checklist — Step 33.0 Verified Transformed Real-StS2 CLR Admission

## Candidate identity

- step/candidate: **Step 33.0**
- version: `0.0.121 (121)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-33.ipa`
- expected device report: `Documents/StS2Launcher/Reports/Step33-TransformedRealStS2AssemblyAdmission.txt`

## Required before device testing

- [ ] release identity is exactly `0.0.121 (121)`;
- [ ] canonical static validation passes;
- [ ] complete active host suite passes;
- [ ] iOS publish succeeds with `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy`;
- [ ] IPA verification succeeds;
- [ ] source archive contains no proprietary `sts2.dll` or game payload;
- [ ] physical Step-32 0.0.120 4/4 closure report is preserved in history;
- [ ] exact closed transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef` is pinned in Step-33 code/docs;
- [ ] no Step-33 code invokes a game member, starts Godot/game logic, or enables native/private dependency loading.

## Physical run

Use a fresh app process. Do not run Step 23/24/other real-game CLR-load buttons first and do not start Godot.

Gate A must re-run Step 32 A–D, require the exact closed transformed hash/identity/MVID/semantic fingerprint and zero PrepareMethod references, then requalify the zero-blocker runtime plan with no StS2 CLR admission.

Gate B must LoadFromStream only the exact transformed primary and stop after identity/MVID/context verification. The receipt-backed/prepared original must not be a CLR load input.

Gate C must prove transformed `sts2` is the only private Step-33 context assembly, with zero private dependency requests, zero unplanned managed requests, and zero native requests.

Gate D must re-prove OfflineReady, original/transformed/plan hashes, and unique transformed-context residency. Game entry point/member invocation and Godot/game startup must remain NO.

## Failure discipline

Do not broaden authority to make the run advance. A transformed hash/semantic mismatch, private dependency request, unplanned managed request, native request, context-membership drift, or source/isolation failure is new evidence and must fail closed.

A Step-33 4/4 PASS authorizes only design of a later separately gated controlled transformed-site execution boundary.
