# Step 23.4.3 — Cecil Core-Library Scope Construction Fix

## Trigger

The Step 23.4.2 Codemagic run passed static validation and Core compilation but stopped at **153/155 host tests**. Both failing tests were the synthetic module-initializer cases. Their written assemblies still contained:

`mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089`

even though Step 23.4.2 cleared the synthetic module's `AssemblyReferences` collection before write.

## Root cause

Cecil's `CommonTypeSystem.GetCoreLibraryReference()` chooses an existing recognized core-library AssemblyRef (`mscorlib`, `System.Runtime`, `System.Private.CoreLib`, or `netstandard`) when one exists. If none exists, it creates legacy `mscorlib` for a synthetic module.

Step 23.4.2 created the `<Module>..cctor` return type with `MainModule.TypeSystem.Void` **before** adding the intended AssemblyRefs. That embedded the legacy mscorlib scope into the `System.Void` TypeReference. Clearing `AssemblyReferences` afterward removed the collection entry but not the TypeReference's scope, so Cecil recreated the AssemblyRef during serialization.

## Correction

Initializer-bearing synthetic fixtures now:

1. obtain the real host `System.Runtime` identity;
2. declare `System.Runtime` in the synthetic AssemblyRef set;
3. add the full declared AssemblyRef set before accessing `TypeSystem.Void`;
4. assert `TypeSystem.Void.Scope` is exactly the predeclared `System.Runtime` reference;
5. write and reopen with `ReadingMode.Immediate`;
6. assert the persisted AssemblyRef set exactly equals the declared set;
7. reject any `mscorlib` reference;
8. assert the initializer return type is primitive `MetadataType.Void` and remains scoped to `System.Runtime`.

The synthetic binding-plan builder recognizes the resulting `System.Runtime` edge as a normal host-framework binding.

## Production boundary

No production Step 23 load/resolver rule changed. There is still no `mscorlib` → `System.Private.CoreLib` alias and no weakened binding-plan metadata coverage.
