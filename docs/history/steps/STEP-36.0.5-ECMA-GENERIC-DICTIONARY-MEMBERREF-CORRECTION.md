# Step 36.0.5 — ECMA generic Dictionary MemberRef correction

Physical 0.0.158 closed the Step-35 MODEL-BOOTSTRAP authority and reached the unchanged `ExecuteEssential` call. Step-36 Gates A/B passed, the exact receipt-backed PCK mounted, and `res://localization/eng` remained visible. Gate C failed normally inside `ModelDb.Init` with `MissingMethodException: Method not found: bool System.Collections.Generic.Dictionary`2.ContainsKey(MegaCrit.Sts2.Core.Models.ModelId)`.

The dependency-aware bootstrap analysis itself completed and reported the expected authority graph: 1,624 canonical model types, 56 direct static-cctor ModelDb dependency edges, 47 backward edges, 41 pre-injected dependency targets, and the physical `BowlbugsNormal` index 658 -> `BowlbugEgg` index 846 edge. Resolver/context telemetry remained confined with zero initializer-bearing requests, rejected managed requests, or native-load attempts.

The defect is the 0.0.158 synthesized generic MemberRef signature. ECMA-335 members on a constructed generic declaring type still encode the declaring type's generic parameters (`VAR(0)` / `VAR(1)`) in the member signature. 0.0.158 instead encoded concrete `ModelId` / `AbstractModel` parameter types, which the iOS runtime binder did not resolve to the open `Dictionary<TKey,TValue>` member.

0.0.159 / Step 36.0.5 mirrors the already-physical Step-35 correction used for `Action<string>` and managed `Dictionary<string,string>`: it models open `Dictionary<TKey,TValue>`, constructs the declaring type as `Dictionary<ModelId,AbstractModel>`, and emits `ContainsKey`, `get_Item`, `Remove`, and `Add` with `VAR(0)`/`VAR(1)` member signatures. The written derivative is reopened and each member's declaring host, arity, return type, and generic-parameter positions are verified before CLR admission.

No other runtime boundary changes. The 41-model pre-injection graph, same-instance canonical-order restoration, exact prepared GodotSharp bridge, resource-pack handoff, strict resolver confinement, one-shot unchanged `ExecuteEssential`, and all later-startup prohibitions remain unchanged.
