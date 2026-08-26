#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/reports artifacts/test-results artifacts/logs artifacts/host-step20-fixtures artifacts/host-step27-fixtures artifacts/host-step28-ahead-of-load-fixture
REPORT="artifacts/reports/host-unit-tests.txt"
: > "$REPORT"
exec > >(tee -a "$REPORT") 2>&1

TEST_PROJECT="tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj"
DYNAMIC_PROJECT="fixtures/StS2Launcher.Step20.DynamicFixture/StS2Launcher.Step20.DynamicFixture.csproj"
DEPENDENCY_PROJECT="fixtures/StS2Launcher.Step20.DependencyFixture/StS2Launcher.Step20.DependencyFixture.csproj"
ROOT_PROJECT="fixtures/StS2Launcher.Step20.RootFixture/StS2Launcher.Step20.RootFixture.csproj"
STEP27_INTERPRETED_PROJECT="fixtures/StS2Launcher.Step27.InterpretedPatchFixture/StS2Launcher.Step27.InterpretedPatchFixture.csproj"
STEP28_AHEAD_OF_LOAD_PROJECT="fixtures/StS2Launcher.Step28.AheadOfLoadFixture/StS2Launcher.Step28.AheadOfLoadFixture.csproj"
FIXTURE_DIR="artifacts/host-step20-fixtures"
STEP27_INTERPRETED_DIR="artifacts/host-step27-interpreted-fixture"
STEP28_AHEAD_OF_LOAD_DIR="artifacts/host-step28-ahead-of-load-fixture"
STEP27_FIXTURE_DIR="artifacts/host-step27-fixtures"
STEP27_HARMONY_RELEASE_URL="https://github.com/pardeike/Harmony/releases/download/v2.4.2.0/Harmony-Fat.2.4.2.0.zip"
STEP27_HARMONY_ARCHIVE="$STEP27_FIXTURE_DIR/Harmony-Fat.2.4.2.0.zip"
STEP27_HARMONY_CACHE_DIR="${STS2_HARMONY_CACHE_DIR:-$HOME/.cache/sts2launcher/harmony-fat-2.4.2}"
STEP27_HARMONY_CACHE_ARCHIVE="$STEP27_HARMONY_CACHE_DIR/Harmony-Fat.2.4.2.0.zip"
STEP27_HARMONY_ARCHIVE_RELATIVE="net9.0/0Harmony.dll"
STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED="a5fc5f9d9640b927d786a0527faa18bf7aa776788235140c59e9b73de87a7774"
STEP27_HARMONY_FIXTURE_SHA256_EXPECTED="a849b726e1f9248d71aabbed8114deaf79beb7acc25e8344ff92a27ad8ac87ab"
STEP27_HARMONY_MEMBER_LIST="$STEP27_FIXTURE_DIR/archive-members.txt"
STEP27_HARMONY_MATCH_LIST="$STEP27_FIXTURE_DIR/net9.0-0Harmony-members.txt"
STEP27_HARMONY_FIXTURE="$STEP27_FIXTURE_DIR/0Harmony.dll"

command -v dotnet >/dev/null 2>&1 || { echo "ERROR: dotnet is required to run host tests."; exit 2; }
command -v curl >/dev/null 2>&1 || { echo "ERROR: curl is required to acquire the quarantined Step-27 Harmony fixture."; exit 2; }
command -v unzip >/dev/null 2>&1 || { echo "ERROR: unzip is required to inspect the quarantined Step-27 Harmony fixture."; exit 2; }

echo "StS2 Launcher — Step 32 canonical host regression tests"
echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
echo ".NET: $(dotnet --version)"

echo "Acquiring exact official Harmony-Fat 2.4.2 host regression fixture..."
rm -rf "$STEP27_FIXTURE_DIR"
mkdir -p "$STEP27_FIXTURE_DIR" "$STEP27_HARMONY_CACHE_DIR"
cache_hash="$(shasum -a 256 "$STEP27_HARMONY_CACHE_ARCHIVE" 2>/dev/null | awk '{print $1}' || true)"
if [[ "$cache_hash" != "$STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED" ]]; then
  echo "Harmony regression cache miss; downloading pinned archive..."
  temp_archive="$STEP27_HARMONY_CACHE_ARCHIVE.tmp.$$"
  rm -f "$temp_archive"
  curl --fail --location --silent --show-error \
    --retry 3 --retry-delay 2 --retry-all-errors \
    --proto '=https' --tlsv1.2 \
    "$STEP27_HARMONY_RELEASE_URL" \
    --output "$temp_archive"
  temp_hash="$(shasum -a 256 "$temp_archive" | awk '{print $1}')"
  [[ "$temp_hash" == "$STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED" ]] || {
    echo "ERROR: downloaded Harmony archive hash drift: expected $STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED, observed $temp_hash." >&2
    rm -f "$temp_archive"
    exit 3
  }
  mv "$temp_archive" "$STEP27_HARMONY_CACHE_ARCHIVE"
else
  echo "Harmony regression cache hit."
fi
cp "$STEP27_HARMONY_CACHE_ARCHIVE" "$STEP27_HARMONY_ARCHIVE"
STEP27_HARMONY_ARCHIVE_SHA256_ACTUAL="$(shasum -a 256 "$STEP27_HARMONY_ARCHIVE" | awk '{print $1}')"
if [[ "$STEP27_HARMONY_ARCHIVE_SHA256_ACTUAL" != "$STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED" ]]; then
  echo "ERROR: official Harmony-Fat 2.4.2 archive hash drift: expected $STEP27_HARMONY_ARCHIVE_SHA256_EXPECTED, observed $STEP27_HARMONY_ARCHIVE_SHA256_ACTUAL."
  exit 3
fi
unzip -Z1 "$STEP27_HARMONY_ARCHIVE" > "$STEP27_HARMONY_MEMBER_LIST"
# Harmony-Fat 2.4.2 does not publish a netstandard2.0 implementation in the release ZIP.
# Use its official merged net9.0 implementation as the host-only structural surrogate:
# the production normalizer remains pinned to the on-device exact 0Harmony 2.4.2 metadata
# fingerprint, while this fixture exercises the same upstream HarmonySharedState source,
# merged MonoMod surface, Cecil deferred-reader path, and EditorBrowsable metadata.
# Match by exact framework/DLL suffix while retaining the archive member verbatim.
awk -v target="$STEP27_HARMONY_ARCHIVE_RELATIVE" '
  { original=$0; normalized=$0; gsub(/\\/, "/", normalized); wrapped="/" target; if (normalized == target || (length(normalized) > length(wrapped) && substr(normalized, length(normalized)-length(wrapped)+1) == wrapped)) print original }
' "$STEP27_HARMONY_MEMBER_LIST" > "$STEP27_HARMONY_MATCH_LIST"
STEP27_HARMONY_MEMBER_COUNT="$(awk 'END { print NR + 0 }' "$STEP27_HARMONY_MATCH_LIST")"
if [[ "$STEP27_HARMONY_MEMBER_COUNT" != "1" ]]; then
  echo "ERROR: official Harmony-Fat 2.4.2 archive must contain exactly one net9.0 structural-surrogate member matching $STEP27_HARMONY_ARCHIVE_RELATIVE at archive root or under one release wrapper; found $STEP27_HARMONY_MEMBER_COUNT."
  echo "Discovered 0Harmony.dll archive members:"
  awk '{ normalized=$0; gsub(/\\/, "/", normalized); if (normalized ~ /(^|\/)0Harmony\.dll$/) print "  " $0 }' "$STEP27_HARMONY_MEMBER_LIST" || true
  exit 3
fi
STEP27_HARMONY_ARCHIVE_MEMBER="$(cat "$STEP27_HARMONY_MATCH_LIST")"
echo "Harmony archive member: $STEP27_HARMONY_ARCHIVE_MEMBER"
unzip -p "$STEP27_HARMONY_ARCHIVE" "$STEP27_HARMONY_ARCHIVE_MEMBER" > "$STEP27_HARMONY_FIXTURE"
[[ -s "$STEP27_HARMONY_FIXTURE" ]] || { echo "ERROR: extracted Step-27 Harmony fixture is empty."; exit 3; }
STEP27_HARMONY_FIXTURE_SHA256_ACTUAL="$(shasum -a 256 "$STEP27_HARMONY_FIXTURE" | awk '{print $1}')"
if [[ "$STEP27_HARMONY_FIXTURE_SHA256_ACTUAL" != "$STEP27_HARMONY_FIXTURE_SHA256_EXPECTED" ]]; then
  echo "ERROR: official Harmony-Fat 2.4.2 net9.0 fixture hash drift: expected $STEP27_HARMONY_FIXTURE_SHA256_EXPECTED, observed $STEP27_HARMONY_FIXTURE_SHA256_ACTUAL."
  exit 3
fi
echo "Harmony release URL: $STEP27_HARMONY_RELEASE_URL"
echo "Harmony archive SHA-256: $STEP27_HARMONY_ARCHIVE_SHA256_ACTUAL (PIN MATCH)"
echo "Harmony fixture SHA-256: $STEP27_HARMONY_FIXTURE_SHA256_ACTUAL (PIN MATCH)"
export STS2_STEP27_REAL_HARMONY_FIXTURE="$ROOT/$STEP27_HARMONY_FIXTURE"
export STS2_STEP27_REAL_HARMONY_ARCHIVE_MEMBER="$STEP27_HARMONY_ARCHIVE_MEMBER"

echo "Building external managed fixtures once for host tests and later IPA packaging..."
dotnet build "$DYNAMIC_PROJECT" -c Release --nologo
dotnet build "$DEPENDENCY_PROJECT" -c Release --nologo
dotnet build "$ROOT_PROJECT" -c Release --nologo
dotnet build "$STEP27_INTERPRETED_PROJECT" -c Release --nologo
dotnet build "$STEP28_AHEAD_OF_LOAD_PROJECT" -c Release --nologo
rm -rf "$FIXTURE_DIR" "$STEP27_INTERPRETED_DIR" "$STEP28_AHEAD_OF_LOAD_DIR"
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
mkdir -p "$STEP27_INTERPRETED_DIR"
cp fixtures/StS2Launcher.Step27.InterpretedPatchFixture/bin/Release/net9.0/StS2Launcher.Step27.InterpretedPatchFixture.dll "$STEP27_INTERPRETED_DIR/"
(
  cd "$STEP27_INTERPRETED_DIR"
  shasum -a 256 StS2Launcher.Step27.InterpretedPatchFixture.dll > step27-interpreted-patch-fixture.sha256
)
export STS2_STEP27_INTERPRETED_PATCH_FIXTURE="$ROOT/$STEP27_INTERPRETED_DIR/StS2Launcher.Step27.InterpretedPatchFixture.dll"

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
  --logger "trx;LogFileName=step32.trx" \
  --logger "console;verbosity=normal"

echo "HOST UNIT TESTS: PASS"
echo "TRX: artifacts/test-results/step32.trx"
echo "Text report: $REPORT"
