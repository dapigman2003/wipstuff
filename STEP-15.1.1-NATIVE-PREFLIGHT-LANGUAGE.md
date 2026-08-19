# Step 15.1.1 — standalone native-preflight language-mode hotfix

Runtime version remains **0.0.43 (43)**. No launcher/Godot runtime code changed.

The Step 15.1 Codemagic run successfully built Godot 4.5.1 but stopped in the new standalone native-link preflight before linking. Xcode 26.5 compiled the generated `preflight.mm` probe as Objective-C, so C++ constructs (`extern "C"`, `auto`, `nullptr`) were rejected.

Step 15.1.1 removes language-mode inference from this gate:

- probe source is `preflight.cc`;
- compile is an explicit first stage using `clang++ -std=c++17 -x c++ -c`;
- link is a separate second stage using the compiled object;
- the link still mirrors normal archive selection, `ReferenceNativeSymbol` roots, `-ObjC -lz`, the iOS deployment target, and the exact framework list from the app project;
- source validation now refuses the old `.mm` probe/inferred-language pattern.

The physical Step 15 A–D gate procedure is unchanged.
