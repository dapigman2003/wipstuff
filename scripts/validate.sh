#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/reports
REPORT="artifacts/reports/static-validation.txt"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

echo "StS2 Launcher — canonical static validation entry point"
echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"

echo "Checking active shell syntax..."
while IFS= read -r -d '' script; do
  bash -n "$script"
  echo "PASS: shell syntax — ${script#./}"
done < <(find scripts -type f -name '*.sh' -print0 | sort -z)

python3 -m py_compile tools/validate_current.py
echo "PASS: Python validator compiles"
python3 tools/validate_current.py
