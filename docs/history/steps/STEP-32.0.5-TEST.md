# Step 32.0.5 Test — 0.0.120

## Host/CI acceptance

1. Canonical static validation must pass.
2. The full active host suite must compile and pass.
3. The Step-32 host fixture must still prove private-only 6+4 rewrite, exact audited System.Runtime/Sentry requirement enforcement, unaudited-scope fail-closed behavior, and branch-target refusal.
4. A new regression must prove transformed-method lookup uses exact declaring type + full signature rather than a historical source MethodDef token.
5. iOS publish and IPA verification must pass with release identity `0.0.120 (120)` and unchanged `MtouchInterpreter=-all`, `MtouchLink=None`, `TrimMode=copy` runtime policy.

## Physical acceptance

Run Step 32 once in a fresh app process and preserve `Documents/StS2Launcher/Reports/Step32-RealStS2PrepareMethodRewrite.txt`.

Required Gate A evidence remains the exact physical Step-31 source identity, token `0x06007D05`, 10/10 sites, OfflineReady 428/428, zero read-time resolver requests, zero CLR load, and no trusted-install mutation.

Required Gate B evidence remains 6/6 + 4/4 replacements, exact transformed semantic fingerprint plan, the same three audited constant-metadata requirements, only exact System.Runtime/Sentry write-time resolution, zero external dependency bytes opened, and a private transformed output.

Gate C must now either:

- PASS the full independent semantic/constant-metadata reopen verification and report the transformed MethodDef token plus the old-source-token occupant; or
- fail on a specific semantic/metadata invariant after stable-identity lookup, which becomes the next evidence boundary.

Gate D must re-prove trusted/source/transformed hashes, OfflineReady, no real-StS2 CLR admission, and isolation.

No 0.0.120 result authorizes Step 33 unless A–D physically close **4/4**.
