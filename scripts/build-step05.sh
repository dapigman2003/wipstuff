#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: iOS device builds require macOS/Xcode." >&2
  exit 2
fi

bash scripts/validate-step05.sh

rm -rf artifacts/publish artifacts/Payload
mkdir -p artifacts/publish artifacts/logs

APP="$ROOT/artifacts/publish/StS2Launcher.Step05.iOS.app"
IPA="$ROOT/artifacts/StS2-Launcher-Step-05.2.ipa"
PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
PUBLISH_LOG="artifacts/logs/step05-2-publish.log"
FRAMEWORK_LOG="artifacts/logs/step05-2-framework-filter.log"
GENERATED_FRAMEWORKS_LOG="artifacts/logs/step05-2-generated-linker-frameworks.txt"
SYMBOL_LOG="artifacts/logs/step05-2-native-symbols.log"

capture_linker_diagnostics() {
  : > "$FRAMEWORK_LOG"
  : > "$GENERATED_FRAMEWORKS_LOG"
  : > "$SYMBOL_LOG"

  {
    echo "=== Step 05.2 in-memory framework filter ==="
    grep 'STEP05.2 LINKER FRAMEWORKS' "$PUBLISH_LOG" || true
  } | tee "$FRAMEWORK_LOG" >/dev/null

  {
    echo "=== Generated _LinkerFrameworks.items files ==="
    FOUND_ITEMS=0
    while IFS= read -r file; do
      FOUND_ITEMS=1
      echo "--- $file"
      cat "$file" 2>/dev/null || strings "$file" 2>/dev/null || true
      echo
    done < <(find \
      "$ROOT/src/StS2Launcher.Step05.iOS/obj" \
      "$ROOT/src/StS2Launcher.Core/obj" \
      -type f -name '_LinkerFrameworks.items' -print 2>/dev/null || true)
    if [[ "$FOUND_ITEMS" == "0" ]]; then
      echo "(none found)"
    fi
  } > "$GENERATED_FRAMEWORKS_LOG"

  {
    echo "=== SteamKit2 AOT/native unresolved-symbol scan ==="
    FOUND_OBJECT=0
    while IFS= read -r file; do
      FOUND_OBJECT=1
      echo "--- $file"
      if command -v xcrun >/dev/null 2>&1; then
        # Keep the complete undefined-symbol list in the artifact. If a real
        # DiskArbitration/IOKit call survives, it will be visible here.
        xcrun nm -u "$file" 2>/dev/null || true
      else
        nm -u "$file" 2>/dev/null || true
      fi
      echo
    done < <(find "$ROOT/src/StS2Launcher.Step05.iOS/obj" \
      -type f \( -name 'SteamKit2.dll.o' -o -name 'SteamKit2.dll.llvm.o' \
                   -o -name '*SteamKit2*.o' \) -print 2>/dev/null || true)
    if [[ "$FOUND_OBJECT" == "0" ]]; then
      echo "(no SteamKit2 AOT object found)"
    fi
  } > "$SYMBOL_LOG"
}

echo "=== Step 05.2 native-framework preflight ===" \
  | tee artifacts/logs/step05-2-native-preflight.log

SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
echo "iPhoneOS SDK: $SDK_ROOT" \
  | tee -a artifacts/logs/step05-2-native-preflight.log

if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "ERROR: DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-2-native-preflight.log
  exit 3
else
  echo "Confirmed: DiskArbitration.framework is NOT in the active iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-2-native-preflight.log
fi

cat <<'TXT' | tee -a artifacts/logs/step05-2-native-preflight.log
Step 05.2 policy:
  - keep TrimMode=full;
  - let .NET iOS generate its normal linker framework set;
  - remove only DiskArbitration after _LoadLinkerOutput;
  - keep every other generated framework untouched;
  - if a live DA* symbol survives, allow clang/ld to report it directly.
TXT

echo
echo "Publishing Step 05.2 with the narrow iOS framework filter..."

set +e
dotnet publish "$PROJECT" \
  -c Release \
  -f net9.0-ios \
  -r ios-arm64 \
  -p:BuildIpa=false \
  -p:EnableCodeSigning=false \
  -p:CodesignKey="" \
  -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" \
  -bl:artifacts/logs/step05-2-dotnet-ios.binlog \
  2>&1 | tee "$PUBLISH_LOG"
PUBLISH_STATUS=${PIPESTATUS[0]}
set -e

capture_linker_diagnostics

if [[ "$PUBLISH_STATUS" != "0" ]]; then
  {
    echo
    echo "=== Step 05.2 publish failed: focused native-link scan ==="
    echo "dotnet publish exit code: $PUBLISH_STATUS"
    echo
    echo "Framework filter messages:"
    cat "$FRAMEWORK_LOG" || true
    echo
    echo "First relevant fatal/native-link lines:"
    grep -E -m 80 \
      '(^|: )(error|fatal error)|undefined symbol|Undefined symbols|framework .+ not found|DiskArbitration|DASession|DADisk|IOService|IORegistry|IOKit' \
      "$PUBLISH_LOG" || true
    echo
    echo "Literal DiskArbitration occurrences in intermediates:"
  } | tee artifacts/logs/step05-2-failure-scan.log

  SCAN_ROOTS=(
    "$ROOT/src/StS2Launcher.Step05.iOS/obj"
    "$ROOT/src/StS2Launcher.Core/obj"
    "$HOME/.nuget/packages/steamkit2/3.3.1"
  )

  MATCHES=0
  for scan_root in "${SCAN_ROOTS[@]}"; do
    [[ -d "$scan_root" ]] || continue

    while IFS= read -r file; do
      if grep -a -q 'DiskArbitration' "$file" 2>/dev/null; then
        echo "CONTAINS DiskArbitration: $file" \
          | tee -a artifacts/logs/step05-2-failure-scan.log
        MATCHES=$((MATCHES + 1))
      fi
    done < <(
      find "$scan_root" -type f \
        \( -name '*.dll' -o -name '*.xml' -o -name '*.json' -o -name '*.rsp' \
           -o -name '*.response' -o -name '*.txt' -o -name '*.items' \
           -o -name '*.a' -o -name '*.o' -o -name '*.cs' \) \
        -print 2>/dev/null
    )
  done

  if [[ "$MATCHES" == "0" ]]; then
    echo "No literal DiskArbitration string found by the focused scan." \
      | tee -a artifacts/logs/step05-2-failure-scan.log
  fi

  exit "$PUBLISH_STATUS"
fi

# A successful native link is only accepted if our target demonstrably ran and
# its AFTER set no longer contains DiskArbitration.
BEFORE_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS BEFORE:' "$PUBLISH_LOG" | tail -1 || true)"
AFTER_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS AFTER:' "$PUBLISH_LOG" | tail -1 || true)"

if [[ -z "$BEFORE_LINE" || -z "$AFTER_LINE" ]]; then
  echo "ERROR: publish succeeded but Step 05.2 framework-filter telemetry was not emitted." >&2
  exit 5
fi

if [[ "$BEFORE_LINE" != *"DiskArbitration"* ]]; then
  echo "ERROR: Step 05.2 expected to observe DiskArbitration before filtering, but did not." >&2
  echo "$BEFORE_LINE" >&2
  exit 5
fi

if [[ "$AFTER_LINE" == *"DiskArbitration"* ]]; then
  echo "ERROR: DiskArbitration survived the Step 05.2 in-memory framework filter." >&2
  echo "$AFTER_LINE" >&2
  exit 5
fi

if [[ ! -d "$APP" ]]; then
  echo "ERROR: publish completed but the expected .app was not created:" >&2
  echo "  $APP" >&2
  find "$ROOT/src/StS2Launcher.Step05.iOS" -type d -name '*.app' -print >&2 || true
  exit 4
fi

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/

(
  cd artifacts
  rm -f StS2-Launcher-Step-05.2.ipa
  /usr/bin/zip -qry StS2-Launcher-Step-05.2.ipa Payload
)

echo "Created $IPA"
