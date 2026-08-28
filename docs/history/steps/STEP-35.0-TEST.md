# Step 35.0 test contract — 0.0.123

Canonical CI authority: static validator -> complete host suite -> iOS publish -> IPA verification. Physical authority: fresh-process iPhone A–D report.

Host/static tests must prove:

- four ordered gates and first-failure stopping;
- exact source `ExecuteVeryEarly` token `0x06007D02`, full static parameterless `Task` signature, `<ExecuteVeryEarly>d__7` and source MoveNext token `0x0600BC71`;
- Gate-A re-runs Step-32 A–D and compares source/transformed semantic fingerprints without Cecil dependency resolution;
- MoveNext directly calls none of `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit` and directly references no Harmony method;
- strict private ALC loads an initializer-free prepared private dependency but rejects an initializer-bearing one;
- Gate C uses exact reflection, one `MethodInfo.Invoke`, exact `Task`, and `WaitAsync(TimeSpan.FromSeconds(60), cancellationToken)`;
- unplanned managed/native resolution remains fail-closed;
- release/UI/report names are exactly Step 35 / 0.0.123.

On device, preserve `Step35-TransformedRealStS2VeryEarlyInitialization.txt` whether PASS or FAIL. A Gate-C timeout, Task fault, target exception, initializer-bearing request, unplanned resolver request, or native attempt is a valid evidence-producing FAIL; do not broaden authority in-place.
