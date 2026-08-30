# Documentation — Step 35.0.7 / 0.0.130

Physical 0.0.129 proved the deferred-open/configure/write correction and advanced the diagnostic clone through Gate A, Gate B and into Gate C. It then returned a managed `MissingMethodException` for the bridge's synthetic `System.Action<string>.Invoke(string)` MemberRef before any `INMETHOD_*` marker.

0.0.130 changes only that generic delegate MemberRef encoding. The bridge models open `Action<T>` explicitly, constructs the field as `Action<string>`, encodes `Invoke(!0)`, and requires rejecting-resolver post-write verification of that exact generic-variable signature. The strict resolver, exact-source protection, timeout, later-boundary prohibitions and diagnostic-only evidence semantics remain unchanged.

See `CURRENT-STATUS.md` for the active frontier and `history/INDEX.md` for the physical records and Step 35.0.7 design record.
