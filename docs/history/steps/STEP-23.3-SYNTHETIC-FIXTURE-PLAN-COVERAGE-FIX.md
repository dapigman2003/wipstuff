# Step 23.3 — Synthetic Fixture Binding-Plan Coverage Fix

The Step 23.2 Codemagic run reached **153/154** host tests. Unique synthetic identities solved the prior cross-test contamination. The sole remaining failure was `GateARejectsModuleInitializerBeforeAnyRealClrLoad`.

Cecil adds an implicit `mscorlib, Version=4.0.0.0` `AssemblyRef` when the synthetic `<Module>..cctor` uses `TypeSystem.Void`. The fixture plan had described only `Game.Dependency` and `System.Linq`, so Gate A correctly failed its exact AssemblyRef-plan coverage check before reaching the intended module-initializer refusal.

Step 23.3 derives synthetic plan edges from the assemblies' actual post-write Cecil `AssemblyReferences`: `Game.Dependency` binds to the prepared private dependency, `System.Linq` binds to the host framework, and the module-initializer-only `mscorlib` reference maps to the host core library. Unexpected synthetic references fail fixture construction.

Production `FirstRealGameAssemblyLoad` is unchanged. The strict Gate A metadata-coverage invariant is preserved.
