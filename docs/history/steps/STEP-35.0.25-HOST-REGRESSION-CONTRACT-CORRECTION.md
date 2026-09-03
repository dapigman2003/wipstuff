# Step 35.0.25 — Host regression contract correction

Release: 0.0.148 (148)

## Trigger

Codemagic for 0.0.147 passed 881/881 static checks and executed 213 host tests, with 212 passing and one failing. The sole failure was `GodotManagedPluginResolverBaselineRejectsMissingPreflightWithDurableFailure`: production threw `Step 35 Gate A must pass before Gate B.` while the test required the literal substring `preflight`.

## Correction

0.0.148 changes only that negative host-test message assertion to pin the actual production exception contract. The 0.0.147 managed-plugin bridge bootstrap, exact eight-request/eight-host-load/zero-private-load post-bootstrap resolver seal, Gate-C resolver freeze, native bridge, and game diagnostic behavior remain unchanged.

## Safety/authority

No game-native executable is loaded. No second CLR is started. No callback pointer is fabricated. No initializer-bearing dependency policy is weakened. Exact Step-35 closure remains open; this candidate is still diagnostic derivative work.
