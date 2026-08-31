# Step 35.0.10 — Command-Line / Godot boundary localization

Candidate: 0.0.133 (133). Status: diagnostic design/candidate; Step 35 remains OPEN.

## Motivation

Physical 0.0.132 emitted the NullPlatform PRE marker for `CommandLineHelper.TryGetValue` and hard-terminated before POST. Its same-run exact-source map also proved the dynamic NP ordinal was +1 because the synthetic entry-marker bridge call was counted before exclusion. Static inspection of the matching managed app files shows `CommandLineHelper` type initialization calls `Godot.OS.GetCmdlineArgs()` before the thin dictionary lookup body can execute.

## Change

Preserve all exact-source, writer-only resolver, runtime resolver, fresh-process, no-native, no-Godot-startup, one-invocation, timeout and later-boundary restrictions. On the separately verified diagnostic clone only:

1. ignore diagnostic bridge Emit calls before CALLSITE ordinal accounting;
2. keep the NullPlatform direct base constructor unwrapped while retaining its original ordinal;
3. add `INMETHOD_027` to `CommandLineHelper.TryGetValue`;
4. retain automatic cctor entry telemetry for `CommandLineHelper..cctor`;
5. add ordered `INMETHOD_CLxxx_PRE/POST` around eligible original cctor call/callvirt/newobj instructions;
6. require the cctor plan to include `Godot.OS.GetCmdlineArgs`, failing Gate A otherwise;
7. add `INMETHOD_CLTVxxx_PRE/POST` around eligible original call-like instructions in TryGetValue;
8. extend the exact-source static map with cctor and TryGetValue sections;
9. allow unrelated branch-target callsites in the new sweeps to remain unwrapped while still consuming their exact-source ordinals; required Godot coverage may not be skipped;
10. add host regressions reproducing production entry-marker-before-sweep ordering and branch-target ordinal preservation.

## Expected physical value

One device run can distinguish failure before cctor entry, inside a specific cctor outgoing call, specifically at `Godot.OS.GetCmdlineArgs`, after type initialization but before/inside the actual TryGetValue body, or later. This minimizes diagnostic iterations without authorizing Godot initialization or a behavioral workaround.

A 4/4 diagnostic result does not close Step 35.
