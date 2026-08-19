#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

TEST_PROJECT="tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj"
DYNAMIC_PROJECT="fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj"
DEPENDENCY_PROJECT="fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj"
ROOT_PROJECT="fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj"
RESULTS_DIR="artifacts/test-results"
FIXTURE_DIR="artifacts/host-step20-fixtures"
mkdir -p "$RESULTS_DIR" artifacts/logs
rm -rf "$FIXTURE_DIR"
mkdir -p "$FIXTURE_DIR"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet is required to run unit tests." >&2
  exit 2
fi

echo "Building project-owned Step 20 external managed fixtures for host regression tests..."
dotnet build "$DYNAMIC_PROJECT" -c Release --nologo 2>&1 | tee artifacts/logs/step20-dynamic-fixture-host-build.log
dotnet build "$DEPENDENCY_PROJECT" -c Release --nologo 2>&1 | tee artifacts/logs/step20-dependency-fixture-host-build.log
dotnet build "$ROOT_PROJECT" -c Release --nologo 2>&1 | tee artifacts/logs/step20-root-fixture-host-build.log

cp fixtures/StS2Launcher.Step20.DynamicFixture/bin/Release/net9.0/StS2Launcher.Step20.DynamicFixture.dll "$FIXTURE_DIR/"
cp fixtures/StS2Launcher.Step20.DependencyFixture/bin/Release/net9.0/StS2Launcher.Step20.DependencyFixture.dll "$FIXTURE_DIR/"
cp fixtures/StS2Launcher.Step20.RootFixture/bin/Release/net9.0/StS2Launcher.Step20.RootFixture.dll "$FIXTURE_DIR/"
(
  cd "$FIXTURE_DIR"
  shasum -a 256 \
    StS2Launcher.Step20.DynamicFixture.dll \
    StS2Launcher.Step20.DependencyFixture.dll \
    StS2Launcher.Step20.RootFixture.dll > step20-fixtures.sha256
)

export STS2_STEP20_FIXTURE_ROOT="$ROOT/$FIXTURE_DIR"
echo "=== Step 20 host unit tests (Steps 01-19 regressions + dynamic managed execution foundation) ==="
dotnet test "$TEST_PROJECT" \
  -c Release \
  --nologo \
  --results-directory "$RESULTS_DIR" \
  --logger "trx;LogFileName=step20.trx" \
  2>&1 | tee artifacts/logs/step20-unit-tests.log
