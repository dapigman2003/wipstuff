# StS2 Launcher iOS — Step 19 Expression Interpreter Compatibility

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–18 are complete and closed on a physical iPhone.** Step 18.4 / `0.0.51 (51)` passed all four Real Assembly Rewrite Workspace gates, then also passed the reserved OfflineReady and Foundation 5/5 closure regressions. The protected baseline can therefore clone, write, rewrite, reopen, and audit the real receipt-backed ARM64 StS2 managed payload without modifying the trusted installation.

This archive is **Step 19 / `0.0.52 (52)`**. Step 19 is the first behaviorally meaningful managed compatibility preparation. It targets one narrow AOT-sensitive API shape: direct `System.Linq.Expressions` `Compile()` calls that can explicitly request interpretation instead of runtime code generation.

## Step 19 subsystem — Expression Interpreter Compatibility

Ordered gates:

- **Gate A — InterpreterCapabilityAndWorkspaceClone:** run a launcher-owned captured-expression probe using `Compile(preferInterpretation: true)` in the actual iOS/AOT process; re-prove OfflineReady; clone and receipt-SHA-1-verify a fresh macOS ARM64 + architecture-neutral managed workspace while excluding x86_64 duplicates.
- **Gate B — RealCompileTargetDiscovery:** scan every managed module in that verified workspace for real direct `LambdaExpression.Compile` / `Expression<TDelegate>.Compile` calls; classify parameterless, literal-false, already-true, dynamic-bool, structurally unsafe, and strong-named sites. Gate B fails rather than broadening the rewrite if no safe unsigned real target exists.
- **Gate C — PreferInterpretationRewrite:** rewrite only the selected safe unsigned sites: parameterless `Compile()` becomes `Compile(true)`, and immediate literal `Compile(false)` becomes `Compile(true)` without changing the original constant instruction width. Reopen each generated assembly with the explicit verified-workspace Cecil resolver and prove structural invariants.
- **Gate D — IsolationAudit:** re-hash every Step 19 source copy and corresponding live Step 12 install file against the receipt; prove non-target prepared files are byte-identical; prove only selected prepared assemblies changed; reopen and structurally revalidate every rewritten output.

Step 19 deliberately skips strong-named assemblies, dynamic/non-literal `Compile(bool)` arguments, prefix/branch/EH-sensitive parameterless insertion points, and short-branch crossings whose displacement could be changed by inserting a byte of IL.

## Build

Use Codemagic workflow:

```text
ios-step-19
```

Expected app:

```text
0.0.52 (52)
STEP 19 — EXPRESSION INTERPRETER COMPATIBILITY
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-19.ipa
```

See `docs/STEP-19-DESIGN.md` and `docs/STEP-19-TEST.md`.

## Scope boundary

Step 19 does **not** load or execute StS2 assemblies, implement Harmony/MonoMod detours, replace Reflection.Emit generally, integrate FMOD/Spine, launch the game, add Cloud, or add Workshop. All game-file writes remain under launcher-private `Step19-ExpressionInterpreterCompatibility`; the receipt-backed live installation remains read-only.
