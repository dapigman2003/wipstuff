#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/reports artifacts/test-results artifacts/logs artifacts/host-step20-fixtures artifacts/host-step28-ahead-of-load-fixture
REPORT="artifacts/reports/host-unit-tests.txt"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

TEST_PROJECT="tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj"
DYNAMIC_PROJECT="fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj"
DEPENDENCY_PROJECT="fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj"
ROOT_PROJECT="fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj"
STEP28_AHEAD_OF_LOAD_PROJECT="fixtures/StS2Launcher.Step28.AheadOfLoadFixture/StS2Launcher.Step28.AheadOfLoadFixture.csproj"
FIXTURE_DIR="artifacts/host-step20-fixtures"
STEP28_AHEAD_OF_LOAD_DIR="artifacts/host-step28-ahead-of-load-fixture"

command -v dotnet >/dev/null 2>&1 || { echo "ERROR: dotnet is required to run host tests."; exit 2; }

echo "StS2 Launcher — Step 34 canonical host regression tests"
echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
echo ".NET: $(dotnet --version)"


echo "Building active external managed fixtures once for host tests and later IPA packaging..."
dotnet build "$DYNAMIC_PROJECT" -c Release --nologo
dotnet build "$DEPENDENCY_PROJECT" -c Release --nologo
dotnet build "$ROOT_PROJECT" -c Release --nologo
dotnet build "$STEP28_AHEAD_OF_LOAD_PROJECT" -c Release --nologo
rm -rf "$FIXTURE_DIR" "$STEP28_AHEAD_OF_LOAD_DIR"
mkdir -p "$FIXTURE_DIR"
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

mkdir -p "$STEP28_AHEAD_OF_LOAD_DIR"
cp fixtures/StS2Launcher.Step28.AheadOfLoadFixture/bin/Release/net9.0/StS2Launcher.Step28.AheadOfLoadFixture.dll "$STEP28_AHEAD_OF_LOAD_DIR/"
(
  cd "$STEP28_AHEAD_OF_LOAD_DIR"
  shasum -a 256 StS2Launcher.Step28.AheadOfLoadFixture.dll > step28-ahead-of-load-fixture.sha256
)
export STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT="$ROOT/$STEP28_AHEAD_OF_LOAD_DIR"

dotnet test "$TEST_PROJECT" \
  -c Release \
  --nologo \
  --results-directory artifacts/test-results \
  --logger "trx;LogFileName=step34.trx" \
  --logger "console;verbosity=normal"

echo "HOST UNIT TESTS: PASS"
echo "TRX: artifacts/test-results/step34.trx"
echo "Text report: $REPORT"
