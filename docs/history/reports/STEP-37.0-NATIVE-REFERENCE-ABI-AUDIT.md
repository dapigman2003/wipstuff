# Native reference ABI audit — FMOD + Spine (analysis-only)

This report records metadata/symbol observations from user-provided, legally owned macOS game binaries. The binaries themselves are **not** included in this source archive and are never eligible for iOS loading.

## Exact reference files

| File | Bytes | SHA-256 | ARM64 platform |
|---|---:|---|---|
| libfmod.dylib | 2,574,448 | f16fde5ee511a2c3b235a03c734214ddd53e76459a7d5a12455baddecf0de5a0 | macOS, min 11.0 |
| libfmodstudio.dylib | 2,325,856 | fbe838ac5c98eed83f225ae0ad4b17edeffe7aa4216027dc7359d509b53749fe | macOS, min 11.0 |
| libGodotFmod.macos.template_release | 1,776,736 | 5899d23565175d7da1340a2512c226c0641c582e40051f7eebc2da90dda73271 | macOS, min 14.0 |
| libspine_godot.macos.template_release | 2,746,976 | dde5c7682eb29f3c69e4191f6361a1f0731292188b2603bee02adf726abde0d8 | macOS, min 15.0 |

All four are universal Mach-O files containing x86_64 and arm64 slices.

## FMOD Godot bridge surface relevant to Step 37

The arm64 bridge links `@rpath/libfmod.dylib` and `@rpath/libfmodstudio.dylib` and exposes Godot-facing classes including:

- `FmodBankLoader`
- `FmodListener2D`
- `FmodListener3D`
- `FmodEventEmitter2D`
- `FmodEventEmitter3D`
- `FmodServer`
- `FmodBank`, `FmodEvent`, `FmodEventDescription`, `FmodBus`, `FmodVCA`

The exact `bank_paths` property string is present. The sealed `res://scenes/game.tscn` uses only `FmodBankLoader`, `bank_paths`, and `FmodListener2D` directly, so Step 37 neutralizes exactly those three scene-level constructs and does not emulate the rest of FMOD.

## Spine bridge surface relevant to later main-menu work

The macOS bridge exposes classes/resources including:

- `SpineSprite`
- `SpineSkeletonDataResource`
- `SpineSkeletonFileResource`
- `SpineAtlasResource`
- `SpineAnimationState`
- `SpineTrackEntry`
- `SpineBoneNode`
- `SpineSlotNode`

The sealed `game.tscn` contains no direct Spine node/resource declaration. Spine first becomes relevant in the audited main-menu background resources and is therefore intentionally outside Step 37.

## Policy consequence

These macOS binaries are reference-only. Step 37 must never copy, bundle, sign, dlopen, or otherwise load them on iOS. Compatibility is expressed only through a copied text-scene derivative in launcher storage, while the receipt-backed Step-12 install remains immutable.
