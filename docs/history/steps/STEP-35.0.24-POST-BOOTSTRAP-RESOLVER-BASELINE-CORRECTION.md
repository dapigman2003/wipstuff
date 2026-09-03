# Step 35.0.24 — Post-bootstrap resolver baseline correction

Release: 0.0.147 (147)
Status: diagnostic candidate; exact Step 35 remains OPEN.

## Trigger

Physical 0.0.146 completed the entire generated Godot managed-plugin bridge experiment: the 37-pointer `ManagedCallbacks` table was created, game scripts were registered, the complete callback struct was adopted by source-built Godot, reverse binding became ready, and `GD_OnCoreApiAssemblyLoaded` returned. Gate C then failed normally before target binding because its resolver precheck still compared the current state with the older snapshot captured immediately after the 225-pointer NativeFuncs handoff.

The bridge itself legitimately caused exactly eight additional managed framework requests and eight exact host-framework loads, with zero added private loads and zero initializer-bearing/rejected/native activity. Therefore the 0.0.146 stop is a resolver-contract defect, not a new game/Godot compatibility frontier.

## Correction

0.0.147 preserves the 0.0.146 bootstrap behavior unchanged.

After `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`, the core now:
1. requires the generated reverse bridge to have been prepared;
2. requires initializer-bearing/rejected/native resolver activity to remain zero;
3. compares the post-handoff managed resolver delta against the exact eight-request closure physically measured in 0.0.146;
4. requires the corresponding host-load delta to contain exactly those eight requests and no extra private load;
5. records `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS` and seals the resulting resolver counters;
6. requires Gate C to observe no further resolver/private/native changes from that sealed post-bootstrap baseline before target type binding.

Any changed/additional request still fails closed. The old pre-bootstrap snapshot is retained as handoff evidence but is no longer incorrectly used as the Gate-C baseline after an intentional managed-plugin bootstrap.

## Regression guard

The host suite adds a negative test proving the new baseline-seal API rejects use before Gate A/preflight and emits exactly one durable `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_FAIL` checkpoint. Static validation requires the new seal method, exact eight-request closure, PASS/RETURNED markers, post-bootstrap Gate-C wording, and the physical 0.0.146 provenance record.

## Non-authority / prohibitions

- No second CLR / hostfxr / CoreCLR instance is started.
- No game native executable is loaded.
- No individual reverse callback pointer is fabricated or substituted.
- `GDMono::runtime_initialized` remains unclaimed/false.
- Initializer-bearing 0Harmony remains forbidden.
- ExecuteEssential, ExecuteDeferred, game entry point, arbitrary resolver fallback, and broader game startup remain forbidden.
- A diagnostic 4/4 still cannot close exact Step 35.

## Expected high-value physical sequence

Prior 0.0.146 bridge markers through:

`CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`
→ `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS` (`addedManaged=8; addedHost=8; addedPrivate=0`)
→ `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_RETURNED`
→ `C_ENTRY`
→ `C_RESOLVER_PRECHECK_PASS — post-bootstrap resolver baseline is intact ...`
→ natural Gate-C binding/invocation markers.

If the new baseline seal fails, preserve its resolver-state detail; do not weaken the expected closure. If it passes, any later natural frontier is downstream of both physically proven managed->native and native->managed Godot bootstrap state.
