# Step 16.1 — Physical Gate D + Codemagic verifier hotfix

Physical Step 16 results proved Gates A–C. Gate D failed because the public macOS depot legitimately contains receipt-backed `sts2.dll` copies under both `data_sts2_macos_arm64` and `data_sts2_macos_x86_64`. Step 16.1 deterministically selects the unique macOS arm64 copy as the primary game assembly for iPhone/AOT analysis, while still reading and post-hash-verifying every managed candidate read-only.

Codemagic also produced the IPA successfully but the final verifier falsely failed because it searched the managed PE file for an ASCII string literal. .NET managed string literals are not required to appear as plain ASCII bytes. Step 16.1 instead compares the bundled fixture byte-for-byte with the exact project-owned fixture DLL built earlier in the same CI run.

No real game assembly is written or executed.
