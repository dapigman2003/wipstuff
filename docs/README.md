# Documentation — Step 35.0.4 / 0.0.127

Start with `CURRENT-STATUS.md`. Steps 32–34 remain closed positive. Step 35 remains open. Physical 0.0.126 validated same-run durable telemetry and reproduced the synchronous `ExecuteVeryEarly` hard-kill frontier. 0.0.127 advances diagnostics only by executing a separately instrumented clone of the reverified exact transformed source.

The diagnostic clone adds durable entry markers to the pre-first-await call chain; the original transformed image, resolver authority, timeout, and later-boundary prohibitions remain preserved. See `history/INDEX.md` for the 0.0.126 physical report and the Step 35.0.4 design record.
