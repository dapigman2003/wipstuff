# Step 45.0 — 0.0.176 SaveManager singleton-field correction

Physical 0.0.176 reached Step 45 after same-process Step-44 4/4/no-legacy-data authority. Step 45 Gate A passed with rendering frozen, NGame state 2, unchanged selected compatibility SHA-256 `89e7ef79d74457fa4e96078c2363d1c0f6342e8b96a2e6e13bd308024bfbf60a`, and zero resolver/host/private/initializer/rejected/native deltas.

Gate B then failed before any Step-45 mutation because the 0.0.176 source assumed that `SaveManager.Instance` used an auto-property field named `<Instance>k__BackingField`. The physical selected image has no such field, so the rung stopped with `MissingFieldException` before the static map was accepted and before Gate C was armed. Rendering remained stopped and the run ended normally.

Reference-image ECMA-335 metadata inspection confirms exact `MegaCrit.Sts2.Core.Saves.SaveManager` static fields `_mockInstance` and `_instance`. Exact `get_Instance()` IL loads `_mockInstance` twice, loads `_instance` twice, stores `_instance` once, and calls exact zero-argument `ConstructDefault()` once only as the null fallback.

0.0.177 therefore keeps the Steps 43–47 ladder and all runtime boundaries unchanged, but corrects Step 45 singleton authority as follows:

- Gate B requires exact static `SaveManager _mockInstance` and `_instance` fields.
- Gate B verifies the serialized `get_Instance()` field/call shape before accepting the local-save audit.
- Gate C/D never invoke `get_Instance()` or `ConstructDefault()`.
- Runtime access requires `_mockInstance == null` and existing production `_instance != null`; otherwise Step 45 fails closed before local-save calls.
- `InitProfileId(null) -> InitProgressData() -> InitPrefsData()` remains the only Step-45 mutation boundary.
- Steps 46–47 are unchanged and remain locked until corrected Step 45 closes 4/4.

This is a metadata-authority correction, not a broadening of startup scope.
