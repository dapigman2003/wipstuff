# Step 35.0.7 — Generic Delegate MemberRef Correction

Candidate: `0.0.130 (130)`.

## Evidence

Physical 0.0.129 advanced the diagnostic derivative through Gate A and Gate B and reached the armed Gate-C invocation. It then returned a managed `MissingMethodException` for `void System.Action`1.Invoke(string)` before any `INMETHOD_*` marker. This is a diagnostic bridge defect, not evidence of a changed exact-game frontier.

## Root cause

The bridge field was correctly constructed as `System.Action<string>`, but the synthetic Cecil `MethodReference` for `Invoke` used a concrete `System.String` parameter. A member reference on a constructed generic declaring type must retain the declaring type's generic variable in the method signature. The correct shape is `System.Action<string>::Invoke(!0)`, where `!0` is the type generic parameter supplied as `string` by the constructed declaring type.

## Correction

`CreateInstrumentedDiagnosticClone` must:

1. create open `System.Action`1` in the existing `System.Runtime` metadata scope;
2. add exactly one Cecil type generic parameter owned by that open type;
3. construct the callback field type as `Action<string>`;
4. encode `Invoke` with the open type generic parameter as its sole method parameter;
5. preserve the existing stack-neutral bridge body (`ldsfld`, null check, `ldarg.0`, `callvirt`, `ret`);
6. after serialization, reopen with the rejecting resolver and require exactly one bridge `callvirt` whose declaring type is `Action<string>` and whose sole parameter is a type generic parameter at position 0;
7. refuse the physically disproven concrete `Invoke(string)` encoding.

All Step-35.0.6 deferred-open bounded-writer protections remain unchanged. The exact closed Step-32 transformed source remains immutable and separately re-verified. Gate B/C continue to execute only the diagnostic derivative. A 4/4 result remains localization evidence only and cannot close exact Step 35.
