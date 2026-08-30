# Documentation — Step 35.0.9 / 0.0.132

Read `CURRENT-STATUS.md` first for the current authority/evidence boundary and `TESTING.md` before producing or interpreting a physical build.

Physical 0.0.131 proves execution enters `NullPlatformUtilStrategy..ctor` but does not reach `GodotFileIo..ctor`. 0.0.132 therefore localizes only inside that constructor by adding ordered pre/post markers around its existing non-base call-like instructions and by extending the same-run static map with the constructor IL/CALLSITE ordinals.

Do not revive previously closed-negative runtime patching, do not restore Immediate Cecil open before writer configuration, do not use source MethodDef tokens as post-write locators, and do not broaden Godot/native/resolver authority to make a diagnostic build pass.

Key documents:

- `CURRENT-STATUS.md` — authoritative current frontier and active candidate;
- `ARCHITECTURE.md` — durable architecture and historical boundary decisions;
- `REGRESSION-CONTRACTS.md` — implementation constraints that must survive revisions;
- `TESTING.md` — host/static and physical-device procedure;
- `REPORTS.md` — run-correlated evidence interpretation;
- `RELEASE-CHECKLIST.md` — packaging/release identity checks;
- `history/` — immutable step designs and physical-result records.
