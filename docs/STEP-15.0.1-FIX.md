# Step 15.0.1 — Godot archive validation hotfix

Step 15's first Codemagic attempt successfully source-built Godot 4.5.1-stable for iOS arm64, then stopped in the project-owned post-build static-archive validator before the .NET/iOS app build.

Two validator assumptions were corrected:

1. `apple_embedded_main(int, char **)` is defined in upstream `platform/ios/main_ios.mm`, which is Objective-C++. It does not use `extern "C"`, so the Mach-O symbol is C++-mangled. The bridge is also Objective-C++ and intentionally calls the same C++ symbol. Validation now checks the defined symbol by the stable `apple_embedded_main` name fragment instead of incorrectly requiring an unmangled `_apple_embedded_main`.
2. The old validator piped the very large `nm` output through `grep -q` while the script runs with `set -o pipefail`. If `grep -q` exits after an early match, `nm` can receive SIGPIPE and make a valid match look like a failed pipeline. The validator now writes `nm -gU` output once to a temporary file and performs all checks against that file.

The validator now prints the exact failed archive condition instead of only a generic validation error.

There is **no launcher runtime change** in 15.0.1. App version remains `0.0.42 (42)`, workflow remains `ios-step-15`, and the physical Gate A–D procedure is unchanged.
