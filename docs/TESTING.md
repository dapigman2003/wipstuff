# Testing

Active candidate: `0.0.159 (159)`, IPA `StS2-Launcher-Step-36.ipa`, TRX `step36.trx`, workflow `ios-canonical`.

Run the canonical validation/test/build pipeline. The stable workflow/cache lineage is intentionally unchanged. Static validation must prove the corrected Step-36.0.5 metadata strategy, release identity, protected historical boundaries, and absence of proprietary game payloads. Host tests and native-link preflight must pass before iOS publish/IPA verification.

Physical sequence: fresh process -> Step 15 A-C -> Step 35 MODEL-BOOTSTRAP 4/4 -> Step 36.0.5 A-D once. Do not retry Step 35 after Gate B or Step 36 after Gate C in the same process.
