# Step 22.4.1 — MSTest v4 Codemagic Fix

## Trigger

The first Codemagic run of Step 22.4 passed canonical static validation 122/122 and built all external fixtures, then stopped while compiling the host test project.

The first real error was:

`DeviceTestReportWriterTests.cs(58,22): CS0117: 'Assert' does not contain a definition for 'ThrowsExceptionAsync'`

The test project uses `MSTest.TestFramework` 4.3.2. MSTest v4 removed the legacy `ThrowsExceptionAsync` API in favor of `ThrowsExactlyAsync` / `ThrowsAsync`; `DataTestMethod` is also obsolete in favor of `TestMethod` with `DataRow`.

## Fix

Only the additive Step 22.3/22.4 report-writer test was changed:

- `Assert.ThrowsExceptionAsync<ArgumentException>` -> `Assert.ThrowsExactlyAsync<ArgumentException>`;
- `[DataTestMethod]` -> `[TestMethod]` while retaining the same four `[DataRow]` cases.

The production report writer, the 97 physically protected Step 22.2 Core behavior files, the iOS runtime/framework binding implementation, native bridge, interpreter policy, and 22 host roots are unchanged.

## Prevention

Canonical static validation now rejects the removed `ThrowsExceptionAsync` API and obsolete `DataTestMethod` in the report-writer test and requires the MSTest v4-compatible `ThrowsExactlyAsync` assertion.

## Authority

Codemagic remains the compile/test authority for this correction. Physical Step 22 acceptance is still required after a successful IPA build because Step 22.4/22.4.1 is a foundation candidate, not a new compatibility subsystem.
