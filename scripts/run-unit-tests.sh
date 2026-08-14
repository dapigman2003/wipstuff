#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

TEST_PROJECT="tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj"
RESULTS_DIR="artifacts/test-results"
mkdir -p "$RESULTS_DIR" artifacts/logs

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet is required to run unit tests." >&2
  exit 2
fi

echo "=== Step 06.3 host unit tests ==="
dotnet test "$TEST_PROJECT" \
  -c Release \
  --nologo \
  --results-directory "$RESULTS_DIR" \
  --logger "trx;LogFileName=step06.3.trx" \
  2>&1 | tee artifacts/logs/step06.3-unit-tests.log
