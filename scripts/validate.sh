#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/reports
set +e
python3 tools/validate_current.py 2>&1 | tee artifacts/reports/static-validation.txt
status=${PIPESTATUS[0]}
set -e
exit "$status"
