# Step 34.0 test contract — 0.0.122

## Host/static requirements

- Four ordered Step-34 gates; no advancement after the first failure.
- Exact closed Step-32 transformed SHA-256, MVID, semantic fingerprint and transformed MethodDef token `0x0600AFEA` are hard-pinned.
- Gate A re-runs Step-32 A–D and the Step-23 prepared-plan preflight before any CLR admission.
- The execution context admits the transformed primary with zero admission-time resolver activity.
- The execution resolver can load a hash-pinned initializer-free prepared private dependency.
- The same resolver refuses an initializer-bearing private dependency and records the refusal separately.
- Native resolution remains throwing/fail-closed.
- Gate C uses exact type/signature/token reflection and exactly one `MethodInfo.Invoke` call.
- The receipt-backed/prepared original primary is never a CLR input.
- No arbitrary `AssemblyDependencyResolver`, default probing or disk fallback is introduced.
- Copy/no-link plus `MtouchInterpreter=-all` runtime policy is unchanged.

## Physical evidence required

The device report must show:

- Gate A PASS with exact transformed hash/identity/MVID/semantic fingerprint/token and zero PrepareMethod references.
- Gate B PASS with exact transformed-only primary admission and zero resolver/native activity at admission.
- Gate C PASS only if exact transformed `PrewarmJit()` returns normally once; report resolver/private-load counts and zero initializer-bearing/unplanned/native requests.
- Gate D PASS with OfflineReady, source/transformed/plan/dependency hash reproof and exact context residency.

If Gate C fails, preserve the full report. The exception type/message/inner-exception chain plus resolver state becomes the next evidence boundary; do not broaden Step 34 in-place.
