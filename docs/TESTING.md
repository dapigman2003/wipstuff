## 0.0.175 / Step 42.0 controlled InitPools

0.0.174 compile failure: no runtime conclusion. Codemagic rejected four identifier families in the new Step-42 source. 0.0.175 corrects only those identifiers to the existing `Step35ExecutionLoadContext` counters and shared exception formatter; the Step-42 device contract is unchanged.

Physical prerequisite: same-process Step 41.0 4/4 with rendering frozen. Gate B must write `Step42-InitPools-StaticMap-<RunId>.txt` before Gate C. Gate C invokes exact audited `NGame.InitPools()` once; after Gate C is armed never retry in-process. Success requires state 2, rendering stopped, and resolver/host/private/initializer/rejected/native deltas all zero. GameStartup/migrations/cloud/platform/Steam/main-menu/deferred remain forbidden.

# Testing — Step 42.0

Active candidate: `0.0.175 (175)`, IPA `StS2-Launcher-Step-42.ipa`, workflow `ios-canonical`.

Canonical validation must preserve physical Step-36/37/38/39 closures and the physical Step-40 4/4 render-pulse closure. Step 41.0 requires same-process Step-40 4/4 with rendering stopped, maps exact `NGame.GameStartup` plus its `AsyncStateMachineAttribute` and compiler-generated `MoveNext` body, then maps the transitive same-sts2 startup closure with deferred/rejecting Cecil. No GameStartup invocation or render restart is permitted.

Host regressions cover four-gate ordering/ordinals. Static validation must assert the Step41 UI exposes no StartRendering/StopRendering/GameStartup invocation call site, full direct IL/state-machine mapping exists, closure classification exists, durable static-map/checkpoint reports are wired, and physical 0.0.171 Step-40 success provenance is sealed. Codemagic remains the first actual C# compiler/AOT/link/package authority.

Physical sequence: fresh process → Step 15 A-C → Step 35 MODEL-BOOTSTRAP 4/4 → Step 36.0.5 4/4 → Step 37.0.1 4/4 → **skip Step 38** → Step 39.0 4/4 → same-process Step 40.1 4/4 → same-process Step 41.0 A-D once.

Expected Step41 success evidence: Gate A frozen Step40 authority; Gate B exact GameStartup token + async state-machine type + MoveNext token with zero external resolution; Gate C static map written with closure count and boundary-category/path evidence plus zero unresolved/external resolution; Gate D frozen no-invocation confinement; `RUN_STEP41_4OF4`; normal report return with rendering inactive.
