# Testing — Steps 53–57 single-player submenu continuation / 0.0.185

Active candidate: `0.0.185 (185)`, IPA `StS2-Launcher-Steps-53-57.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, exact hashes, fail-stop sequencing, one-shot guards, report surfaces, release identity, provenance, cache configuration and payload/security policy. Codemagic remains compile/AOT/link/package authority. Physical iPhone reports remain runtime authority.

## Physical sequence

0.0.185 is the focused correction after physical 0.0.184 safely stopped at Step 53 Gate B on a return-type assumption. The corrected contract is zero args + exact `NSingleplayerSubmenu` return; Step 54 additionally requires the returned object to be the same retained submenu instance.

Start from a **fresh process** and press **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52**. It must end 4/4 with rendering frozen. The runner does not execute Step 15 Gate D or Step 38 and remains capped at the physically closed frontier.

Then run manually, stopping immediately on the first failure:

1. Step 53 — exact `OpenSingleplayerSubmenu` map only.
2. Step 54 — one-shot frozen exact `OpenSingleplayerSubmenu` invocation.
3. Step 55 — actual submenu callback audit + bounded 750 ms render/refreeze.
4. Step 56 — exact `OpenCharacterSelect` frontier/resource-literal map only.
5. Step 57 — read-only exact PCK character-select resource preflight.

Never retry Step 54 or Step 55 in-process after their one-shot boundary is armed. Steps 53, 54, 56 and 57 must keep rendering frozen. Step 55 must call `StopRendering()` on the first managed continuation before post-stop telemetry/file I/O and end frozen.

Step 56/57 must never call `OpenCharacterSelect`, `ResourceLoader`, `PackedScene.Instantiate`, or character-select admission. Risk findings in Step 57 are evidence only and authorize nothing.

## Codemagic performance telemetry

The 0.0.183 cache experiment is retained unchanged. `artifacts/reports/cache-state.txt` and `build-summary.txt` continue to report AOT sentinel presence plus LLVM `opt`/`llc` counts. CI performance changes are not required to run Steps 53–57.
