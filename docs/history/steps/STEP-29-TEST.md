# Step 29 — Physical iPhone Test

Build Codemagic workflow:

```text
ios-step-29
```

Expected app:

```text
STEP 29.0 — REAL STS2 COMPATIBILITY TARGET AUDIT
Version 0.0.112 (112)
```

## Before testing

- keep the known-good receipt-backed Step-12 install;
- force-quit and relaunch before Step 29;
- do not run a real-StS2 CLR-load boundary in the same process first;
- Step 29 itself is local/read-only and does not contact Steam.

## Run

Tap:

```text
Run Step 29 A–D — Admit Real sts2 Metadata → Audit Exact IL → Select One Candidate → Re-Prove Isolation
```

Stop at the first failed gate.

Target:

```text
REAL STS2 COMPATIBILITY TARGET AUDIT PASS — 4/4
```

Preserve/share:

```text
Documents/StS2Launcher/Reports/Step29-RealStS2CompatibilityTargetAudit.txt
```

The most important Gate-C fields are the selected candidate category, source method, method token, IL offset/opcode, target scope/member and source method-body SHA-256. If Gate C reports `NO DIRECT PRIMARY TARGET`, preserve that result unchanged; do not choose a rewrite manually from a broad subsystem count.

Required isolation lines include:

```text
Cecil dependency resolution requests: 0
Trusted Step 12 managed install unchanged: YES
sts2 assembly/type/member CLR load or invocation by Step 29: NO
Cecil writes performed by Step 29: 0
```

A 4/4 pass authorizes target-specific design work only. It does not authorize game startup or a broad compatibility patch set.
