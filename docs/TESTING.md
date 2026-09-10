# Testing — Steps 43–52 physically closed stabilization / 0.0.183

Active candidate: `0.0.183 (183)`, IPA `StS2-Launcher-Steps-43-52.ipa`, workflow `ios-canonical`.

Static/container validation proves source wiring, exact hashes, fail-stop sequencing, one-shot guards, report surfaces, release identity, provenance, cache configuration and payload/security policy. Codemagic remains compile/AOT/link/package authority. Physical iPhone reports remain runtime authority.

## Preferred physical reproof

Start from a **fresh process** and press **Run Physically Closed Path — Step 15 A–C → 35–37 → SKIP 38 → 39–52**. The runner invokes the existing implementations in exactly that order and checks the exact per-step pass/closure predicate after every return. It stops immediately at the first non-closed return. Never retry an armed runner or failed one-shot rung in-process; force-quit/relaunch.

The runner intentionally does **not** execute Step 15 Gate D and does **not** execute Step 38. It preserves every normal per-step report/checkpoint and adds `PhysicallyClosedPath-ToStep52.txt` as convenience state. A successful run must end with rendering frozen after Step 52.

Manual per-step controls remain available for diagnosis. When diagnosing manually, use the same proven order and stop-on-first-failure discipline.

## Step 50 timing contract

Step 50 still requests 100 ms. On the first managed continuation it must synchronously call `StopRendering()` before post-stop telemetry/file I/O. 0.0.183 changes only Step 50's evidence classification ceiling from 2000 ms to **4000 ms**, covering the supplied cold-frame observation of ~2526 ms. Historical Step 40 remains 100/2000 ms. Step 52 remains 1500/6000 ms.

## Codemagic performance check

The first successful 0.0.183 build may still be long because previous workflow caches never contained the newly declared `AOTCompileInputs.cache` / `.uptodate` files. Inspect `artifacts/reports/cache-state.txt` and `build-summary.txt` for:

- AOT input cache/sentinel present before build;
- AOT assemblies reported up-to-date;
- LLVM `opt` execution count;
- LLVM `llc` execution count;
- missing-sentinel diagnostic count;
- iOS publish/package seconds.

The **following warm build** is the decisive cache-speed comparison. Do not change the pinned SDK/workload/Xcode based on CI timing alone.

Step 53 and later gameplay/Steam/cloud/native-extension boundaries remain unopened.
