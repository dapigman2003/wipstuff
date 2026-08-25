# Step 28.0.1 — Callback Progress Compile Fix

Candidate: `0.0.110 (110)`

## Trigger

Codemagic Step 28.0 / `0.0.109 (109)` passed canonical static validation **845/845**, acquired the pinned official Harmony host fixture, and built every external managed fixture, including the separately built Step-28 ahead-of-load fixture. Compilation of `StS2Launcher.Core` then stopped before MSTest with:

```text
AheadOfLoadManagedTransformation.cs(88,23): error CS0246:
The type or namespace name 'CallbackProgress<>' could not be found
```

The raw Codemagic output is preserved at `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`. 0.0.109 produced no host-test verdict, iOS publish, IPA, or physical-device evidence.

## Correction

`AheadOfLoadManagedTransformation` already constructed a callback-backed `IProgress<SteamOfflineInstallProgress>` at Gate A so established OfflineReady progress can be translated into `AheadOfLoadManagedTransformationProgress`. The class omitted the small helper declaration used by other proven compatibility/runtime boundaries.

0.0.110 adds the same local pattern:

```csharp
private sealed class CallbackProgress<T> : IProgress<T>
{
    private readonly Action<T> _callback;
    public CallbackProgress(Action<T> callback) => _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    public void Report(T value) => _callback(value);
}
```

The static validator now pins the helper declaration, non-null callback storage, and `Report(T)` forwarding.

## Protected behavior

The Step-28 experiment is unchanged. In particular:

- source fixture and post-publish packaging remain unchanged;
- Gates A–E remain exactly the same ordered capability boundary;
- `Adjustment() 1 -> 1000` remains the sole Step-28 semantic rewrite;
- Gate D acceptance remains `1000 / 1041 / 1041`;
- Gate E still re-proves OfflineReady/isolation;
- trusted Step-12 bytes remain immutable;
- resolver behavior remains explicit/fail-closed;
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain protected;
- runtime Harmony/MonoMod detours remain retired;
- no real StS2 member transformation/invocation is introduced.

`MASTER-PLAN.md` is unchanged because this correction does not alter architecture, methodology, roadmap, or end-state assumptions.

## Authority

Local static validation is the source-structure authority and should report **850/850 PASS**. The local environment used to recreate this candidate has no .NET SDK, so it cannot establish a host-test verdict. Codemagic is the next compile/full-host-test/iOS-publish/IPA authority; physical iPhone remains final Step-28 runtime authority.
