# Step 15.0.2 — AudioUnit native-link hotfix

The second Step 15 Codemagic attempt progressed through the pinned Godot 4.5.1-stable source build and the corrected Step 15.0.1 archive validator, then failed during the .NET/iOS final native link:

```text
ld: framework 'AudioUnit' not found
```

The failure came from the project-owned `NativeReference` `<Frameworks>` list. Godot 4.5.1's iOS build adds the AudioUnit framework **headers** to its compile include path, but Step 15 had additionally requested `-framework AudioUnit` for the final app. Under the pinned Xcode 26.5 iPhoneOS SDK that standalone linker request fails.

Step 15.0.2 therefore:

- removes only `AudioUnit` from the `NativeReference` app-link framework list;
- retains `AudioToolbox`;
- retains the upstream Godot compile behavior;
- adds static validation that `AudioUnit` cannot silently return as a standalone Step 15 link item.

There is **no launcher runtime change**. App version remains `0.0.42 (42)`, workflow remains `ios-step-15`, and the physical Gate A–D procedure is unchanged.
