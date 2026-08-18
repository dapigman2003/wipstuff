# Current project status

**Steps 01–18 are complete and closed on a physical iPhone.** Step 18.4 / runtime `0.0.51 (51)` passed `REAL ASSEMBLY REWRITE WORKSPACE PASS — 4/4`, followed by `Verify Offline-Ready Install (Local Only)` and `Run Foundation 5/5 Regression`. That physically closes the safe real-assembly rewrite-workspace boundary.

The protected foundation now includes Steam authentication/content acquisition, atomic managed installation, offline verification, compatibility inventory, the source-built Godot 4.5.1 iOS Metal/touch/lifecycle host, Mono.Cecil 0.11.6 AOT read/write/IL editing, real IL/native/dependency analysis, and a closed SHA-1-verified real-game rewrite workspace with explicit assembly/metadata resolver control.

**Current source candidate:** Step 19.1 — strong-name identity-safe Expression Interpreter Compatibility.

- App version: `0.0.53 (53)`
- Codemagic workflow: `ios-step-19-1`
- Mono.Cecil runtime pin: `0.11.6`
- Godot host pin: `4.5.1-stable`
- Test model: one tightly related subsystem, ordered Gates A–D, stop at first failure

Step 19 is the first behaviorally meaningful compatibility preparation. It does not guess that expression compilation is present: Gate B scans the freshly verified ARM64/shared managed payload and requires real direct `System.Linq.Expressions` `Compile` call sites before any rewrite is allowed.

Step 19 gates:

A. prove a captured `System.Linq.Expressions` expression can execute through `Compile(preferInterpretation: true)` in the actual launcher process, then create a fresh receipt-backed ARM64/shared Step 19 source workspace;
B. discover and classify real direct `LambdaExpression.Compile` / `Expression<TDelegate>.Compile` call sites, classifying strong-name identity/signature state and rejecting only malformed or structurally unsafe candidates;
C. rewrite only structurally-safe parameterless/literal-false sites; for modified strong-name identities preserve public key/token/full identity and clear only the stale `StrongNameSigned` bit to explicitly prefer interpretation, then reopen with the proven closed-workspace resolver and validate metadata/instruction invariants;
D. audit source, prepared output, and the original managed install so only the selected launcher-private prepared assemblies may differ.

The parameterless rewrite is intentionally conservative around IL prefixes, branch/EH entry points, and crossing short branches. Literal `false` constants are mutated in place while preserving their original instruction encoding/size. Dynamic `Compile(bool)` arguments are left untouched rather than having runtime semantics guessed.

## Step 19 physical evidence and Step 19.1 correction

The first Step 19 / `0.0.52 (52)` physical run passed Gate A. The iPhone executed the captured expression through `Compile(preferInterpretation: true)` and returned `42`; it also reported both dynamic-code feature flags as `False`. Gate B then found **8 structurally-safe parameterless real `Compile()` sites**, **0 literal-false sites**, and **2 structurally unsafe parameterless sites**. All 8 safe sites carried strong-name identity/signature metadata, so the original unsigned-only policy deliberately stopped before rewriting.

Step 19.1 / `0.0.53 (53)` keeps the same evidence-backed expression target and replaces only that over-broad exclusion. Selected prepared copies preserve name/version/culture/public key/public-key token/full assembly identity exactly; if the source has `StrongNameSigned`, only that stale signature flag is cleared in the modified prepared output. No private key is used, dependent `AssemblyRef` identities are not rewritten, and malformed signed-without-public-key modules are rejected.

**Still out of scope:** game assembly loading/execution, Harmony/MonoMod runtime detours, broad Reflection.Emit replacement, native FMOD/Spine integration, first-frame/main-menu execution, Cloud, and Workshop.
