# Step 13.0.1 — host-test UInt64 Sum compile hotfix

The initial Step 13 Codemagic run stopped at the mandatory host-unit-test compile gate before the iOS build.

## Failure

`SteamOfflineInstallTests.cs` used:

```csharp
files.Values.Sum(bytes => (ulong)bytes.Length)
```

On the pinned .NET SDK this produced `CS0121` because `Enumerable.Sum` has no `UInt64` selector overload and overload resolution was ambiguous between the floating/decimal selector forms.

## Fix

The test now computes the same expected byte count with a compile-safe UInt64 accumulator:

```csharp
files.Values.Aggregate(0UL, (total, bytes) => total + (ulong)bytes.Length)
```

`validate-step13.sh` now rejects the ambiguous expression and requires the compile-safe form.

No runtime launcher code changed. The app remains Step 13 / `0.0.40 (40)`, and the Codemagic workflow remains `ios-step-13`. The physical Step 13 gate is unchanged.
