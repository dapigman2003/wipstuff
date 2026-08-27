# Step 27.0.6 — Bounded iOS prefix-descriptor registration

Candidate: `0.0.90 (90)`

## Physical evidence that motivated this candidate

Step 27.0.5 / `0.0.89 (89)` added a synchronously flushed crash checkpoint. On physical iPhone, the last durable checkpoint was:

- Phase: `PROGRESS`
- Gate: `S — PrefixRegistration`
- Detail: `S1 — entering exact PatchProcessor.AddPrefix(MethodInfo) reflection invocation.`

The process terminated before the S2 checkpoint. Therefore the hard crash is localized inside the exact `PatchProcessor.AddPrefix(MethodInfo)` reflection invocation. Gate T (`PatchProcessor.Patch()`) was not reached.

The earlier Step 27.0 / `0.0.84` managed failure already showed that `AddPrefix(MethodInfo)` constructs `HarmonyMethod(MethodInfo)`, whose `ImportMethod` path touches `HarmonyLib.AccessTools`. Later candidates explicitly admitted and initialized AccessTools, but 0.0.89 still hard-terminated inside the convenience wrapper.

## Candidate change

The patch objective is unchanged: one real Harmony patch against a launcher-owned deterministic method, followed by exact unpatch and restoration proof. StS2 remains untouched.

Gate O continues to require the exact six-instruction `PatchProcessor.AddPrefix(MethodInfo)` reference implementation and now additionally admits the exact parameterless `HarmonyMethod()` constructor. The default constructor must be exactly the bounded `priority=-1 -> object::.ctor -> ret` shape.

Gate S no longer invokes `AddPrefix(MethodInfo)` or `HarmonyMethod(MethodInfo)`. For the launcher prefix only, it performs the equivalent descriptor setup explicitly:

1. prove the launcher prefix has zero Harmony annotations;
2. invoke exact public `HarmonyMethod()`;
3. require `method == null` and `priority == -1`;
4. set only `HarmonyMethod.method` to the exact launcher-owned prefix `MethodInfo`;
5. set only `PatchProcessor.prefix` to that descriptor;
6. re-audit hashes, context membership, resolver/native counts, and probe counters.

This is equivalent to the measured `AddPrefix(MethodInfo)` result for this deliberately annotation-free prefix while avoiding the crashing `HarmonyMethod(MethodInfo) -> ImportMethod` annotation-import path.

Crash checkpoints S1–S5 bracket every operation. Gate T remains the first `PatchProcessor.Patch()` invocation and is otherwise unchanged.

## Non-goals / still forbidden

No `Harmony.Patch`, `PatchAll`, patch discovery, postfix/transpiler/finalizer, StS2 reflection/patching/invocation, Godot startup, game entry point, or native game-library loading. No broad Activator construction and no mutation of prepared/live managed bytes.
