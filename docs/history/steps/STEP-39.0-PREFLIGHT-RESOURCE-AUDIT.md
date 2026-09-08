# Step 39.0 — targeted preflight resource audit

No proprietary resource bytes are stored in this repository. This record preserves only hashes, sizes, and derived structural findings from the user-owned PCK.

Exact PCK resources:

- `res://src/gdscript/audio_manager_proxy.gd.remap` — 59 bytes — SHA-256 `1c923c319eaf9fd5b4c72a4a85364214b915f11738f991d99ec1c269e2825e13` — MD5 `f80dcac3ec4374287c72025b58bf5c57`.
- `res://src/gdscript/audio_manager_proxy.gdc` — 2,799 bytes — SHA-256 `4fc30d10a67bd68d1ba57bc4bf214218fb8b6375e2df60341b86f960f094f1bb` — MD5 `314fabe03fbc80155e8f494885691205`.
- `res://scenes/ui/reaction_wheel.tscn` — 12,328 bytes — SHA-256 `0927e9debca56fe6726b682debe2037928be34f1d363b04ea6f18d97430028d5` — MD5 `786dd2f3278c4462bb1bd46e36c89dff`.
- `res://scenes/ui/multiplayer_timeout_overlay.tscn` — 3,157 bytes — SHA-256 `f32ce3ff57698802566ff36ee3c11e84f586bdfa72e5e13a467a148117bb6dee` — MD5 `3b145fa2a00fcb88d903e3c350cfcd5b`.

The `.gd.remap` points exactly to the `.gdc`. The tokenized GDScript uses Godot tokenizer-buffer version 101, reports a 9,828-byte decompressed token buffer, and has 69 decoded identifiers. Those identifiers describe an FMOD command proxy (`FmodServer`, `FmodEvent`, play/stop/volume/parameter operations) and contain no `_enter_tree`, `_ready`, `_process`, or equivalent automatic lifecycle callback identifier. Therefore merely inserting its parent scene does not statically imply an automatic FMOD invocation from this script.

The reaction-wheel and timeout-overlay scenes contain ordinary Godot UI/resources plus already-present sts2 C# scripts. Their exact text contains no FMOD, Spine, Sentry, Steam, or `.gdextension` declaration. Runtime Step-39 Gate B additionally audits the instantiated managed lifecycle surface before any `AddChild`.
