#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/reports artifacts/test-results artifacts/logs artifacts/host-step20-fixtures artifacts/host-step27-fixtures
REPORT="artifacts/reports/host-unit-tests.txt"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

TEST_PROJECT="tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj"
DYNAMIC_PROJECT="fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj"
DEPENDENCY_PROJECT="fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj"
ROOT_PROJECT="fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj"
FIXTURE_DIR="artifacts/host-step20-fixtures"
STEP27_FIXTURE_DIR="artifacts/host-step27-fixtures"
STEP27_HARMONY_RELEASE_URL="https://github.com/pardeike/Harmony/releases/download/v2.4.2.0/Harmony-Fat.2.4.2.0.zip"
STEP27_HARMONY_ARCHIVE="$STEP27_FIXTURE_DIR/Harmony-Fat.2.4.2.0.zip"
STEP27_HARMONY_ARCHIVE_SUFFIX="/netstandard2.0/0Harmony.dll"
STEP27_HARMONY_MEMBER_LIST="$STEP27_FIXTURE_DIR/archive-members.txt"
STEP27_HARMONY_MATCH_LIST="$STEP27_FIXTURE_DIR/netstandard2.0-0Harmony-members.txt"
STEP27_HARMONY_FIXTURE="$STEP27_FIXTURE_DIR/0Harmony.dll"

command -v dotnet >/dev/null 2>&1 || { echo "ERROR: dotnet is required to run host tests."; exit 2; }
command -v curl >/dev/null 2>&1 || { echo "ERROR: curl is required to acquire the quarantined Step-27 Harmony fixture."; exit 2; }
command -v unzip >/dev/null 2>&1 || { echo "ERROR: unzip is required to inspect the quarantined Step-27 Harmony fixture."; exit 2; }

echo "StS2 Launcher — Step 27 canonical host regression tests"
echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
echo ".NET: $(dotnet --version)"

echo "Acquiring exact official Harmony-Fat 2.4.2 host regression fixture..."
rm -rf "$STEP27_FIXTURE_DIR"
mkdir -p "$STEP27_FIXTURE_DIR"
curl --fail --location --silent --show-error \
  --retry 3 --retry-delay 2 --retry-all-errors \
  --proto '=https' --tlsv1.2 \
  "$STEP27_HARMONY_RELEASE_URL" \
  --output "$STEP27_HARMONY_ARCHIVE"
unzip -Z1 "$STEP27_HARMONY_ARCHIVE" > "$STEP27_HARMONY_MEMBER_LIST"
# Official Harmony-Fat ZIPs wrap framework directories under a release root such as
# Harmony-Fat.2.4.2.0/. Match the exact framework/DLL suffix while retaining the
# archive's original member name for extraction. Backslashes are normalized only
# for comparison so this remains deterministic if a ZIP producer changes separators.
awk -v suffix="$STEP27_HARMONY_ARCHIVE_SUFFIX" '
  { original=$0; normalized=$0; gsub(/\\/, "/", normalized); if (length(normalized) >= length(suffix) && substr(normalized, length(normalized)-length(suffix)+1) == suffix) print original }
' "$STEP27_HARMONY_MEMBER_LIST" > "$STEP27_HARMONY_MATCH_LIST"
STEP27_HARMONY_MEMBER_COUNT="$(awk 'END { print NR + 0 }' "$STEP27_HARMONY_MATCH_LIST")"
if [[ "$STEP27_HARMONY_MEMBER_COUNT" != "1" ]]; then
  echo "ERROR: official Harmony-Fat 2.4.2 archive must contain exactly one member ending in $STEP27_HARMONY_ARCHIVE_SUFFIX; found $STEP27_HARMONY_MEMBER_COUNT."
  echo "Discovered 0Harmony.dll archive members:"
  awk '{ normalized=$0; gsub(/\\/, "/", normalized); if (normalized ~ /(^|\/)0Harmony\.dll$/) print "  " $0 }' "$STEP27_HARMONY_MEMBER_LIST" || true
  exit 3
fi
STEP27_HARMONY_ARCHIVE_MEMBER="$(cat "$STEP27_HARMONY_MATCH_LIST")"
echo "Harmony archive member: $STEP27_HARMONY_ARCHIVE_MEMBER"
unzip -p "$STEP27_HARMONY_ARCHIVE" "$STEP27_HARMONY_ARCHIVE_MEMBER" > "$STEP27_HARMONY_FIXTURE"
[[ -s "$STEP27_HARMONY_FIXTURE" ]] || { echo "ERROR: extracted Step-27 Harmony fixture is empty."; exit 3; }
echo "Harmony release URL: $STEP27_HARMONY_RELEASE_URL"
echo "Harmony archive SHA-256: $(shasum -a 256 "$STEP27_HARMONY_ARCHIVE" | awk '{print $1}')"
echo "Harmony fixture SHA-256: $(shasum -a 256 "$STEP27_HARMONY_FIXTURE" | awk '{print $1}')"
export STS2_STEP27_REAL_HARMONY_FIXTURE="$ROOT/$STEP27_HARMONY_FIXTURE"

echo "Building external managed fixtures once for host tests and later IPA packaging..."
dotnet build "$DYNAMIC_PROJECT" -c Release --nologo
dotnet build "$DEPENDENCY_PROJECT" -c Release --nologo
dotnet build "$ROOT_PROJECT" -c Release --nologo
rm -rf "$FIXTURE_DIR"
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

dotnet test "$TEST_PROJECT" \
  -c Release \
  --nologo \
  --results-directory artifacts/test-results \
  --logger "trx;LogFileName=step27.trx" \
  --logger "console;verbosity=normal"

echo "HOST UNIT TESTS: PASS"
echo "TRX: artifacts/test-results/step27.trx"
echo "Text report: $REPORT"
