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
IPA="$ROOT/artifacts/StS2-Launcher-Step-05.15.ipa"
PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
PATCHER="tools/StS2Launcher.SteamKitIosPatcher/StS2Launcher.SteamKitIosPatcher.csproj"
PUBLISH_LOG="artifacts/logs/step05-15-publish.log"
PATCH_LOG="artifacts/logs/step05-15-steamkit-patch.log"
FRAMEWORK_LOG="artifacts/logs/step05-15-framework-filter.log"
GENERATED_FRAMEWORKS_LOG="artifacts/logs/step05-15-generated-linker-frameworks.txt"
SYMBOL_LOG="artifacts/logs/step05-15-native-symbols.log"

# Never mutate the global/cached NuGet package installation. Step 05.15 retains
# the version-aware SteamKit iOS constructor compatibility patch, so restore into
# a disposable repository-local package root and compile against that exact copy.
export NUGET_PACKAGES="$ROOT/.nuget/packages"
rm -rf "$NUGET_PACKAGES/steamkit2/3.4.0"
mkdir -p "$NUGET_PACKAGES"

capture_linker_diagnostics() {
  : > "$FRAMEWORK_LOG"
  : > "$GENERATED_FRAMEWORKS_LOG"
  : > "$SYMBOL_LOG"

  {
    echo "=== Step 05.15 retained in-memory framework filter ==="
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

echo "=== Step 05.15 native-framework preflight ===" \
  | tee artifacts/logs/step05-15-native-preflight.log

SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
echo "iPhoneOS SDK: $SDK_ROOT" \
  | tee -a artifacts/logs/step05-15-native-preflight.log

if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "ERROR: DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-15-native-preflight.log
  exit 3
else
  echo "Confirmed: DiskArbitration.framework is NOT in the active iPhoneOS SDK." \
    | tee -a artifacts/logs/step05-15-native-preflight.log
fi

cat <<'TXT' | tee -a artifacts/logs/step05-15-native-preflight.log
Step 05.15 policy:
  - retain TrimMode=full;
  - retain the proven DiskArbitration generated-framework filter;
  - retain SteamKit2 3.4.0;
  - if 3.4.0 still contains exactly one Process.StartTime call, replace it with DateTime.UtcNow before AOT;
  - if 3.4.0 already removed that call, verify it is absent and leave the assembly untouched;
  - root SteamKit2, protobuf-net, and protobuf-net.Core so reflection-only protobuf accessors survive iOS full trimming;
  - probe CM directory HTTPS/DNS/raw TCP/raw WebSocket below SteamKit;
  - retain the Step 05.7 CMWebSocket SocketsHttpHandler factory;
  - isolate SocketsHttpHandler HTTPS and ClientWebSocket custom-invoker behavior below SteamKit;
  - enable and capture SteamKit DebugLog only around the unchanged WebSocket-only connection probe;
  - replay SteamKit's exact selected CurrentEndPoint through the proven custom-invoker path;
  - do not add authentication or account behavior.
TXT

echo
echo "Restoring Step 05.15 into isolated NuGet package root..."
dotnet restore "$PROJECT" \
  2>&1 | tee artifacts/logs/step05-15-restore.log

STEAMKIT_DLL="$NUGET_PACKAGES/steamkit2/3.4.0/lib/net8.0/SteamKit2.dll"
if [[ ! -f "$STEAMKIT_DLL" ]]; then
  echo "ERROR: restored SteamKit2 3.4.0 assembly not found: $STEAMKIT_DLL" >&2
  exit 6
fi

{
  echo "=== Step 05.15 SteamKit iOS compatibility patch ==="
  echo "Input: $STEAMKIT_DLL"
  if command -v shasum >/dev/null 2>&1; then
    echo "Before SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')"
  fi
} | tee "$PATCH_LOG"

set +e
dotnet run --project "$PATCHER" -c Release -- "$STEAMKIT_DLL" \
  2>&1 | tee -a "$PATCH_LOG"
PATCH_STATUS=${PIPESTATUS[0]}
set -e

if [[ "$PATCH_STATUS" != "0" ]]; then
  echo "ERROR: SteamKit iOS compatibility patch failed with exit code $PATCH_STATUS." \
    | tee -a "$PATCH_LOG" >&2
  exit "$PATCH_STATUS"
fi

if command -v shasum >/dev/null 2>&1; then
  echo "After SHA-256: $(shasum -a 256 "$STEAMKIT_DLL" | awk '{print $1}')" \
    | tee -a "$PATCH_LOG"
fi

for required in \
  'STEP05.15 STEAMKIT IOS PATCH: PASS' \
  'Assembly: SteamKit2 3.4.0' \
  'Process.StartTime status:'; do
  if ! grep -Fq "$required" "$PATCH_LOG"; then
    echo "ERROR: SteamKit patch telemetry missing: $required" >&2
    exit 7
  fi
done

if ! grep -Eq '^Replacement count: [01]$' "$PATCH_LOG"; then
  echo "ERROR: SteamKit patch telemetry must report replacement count 0 or 1." >&2
  exit 7
fi

echo
echo "Publishing Step 05.15 against SteamKit2 3.4.0 with protobuf/SteamKit trimmer roots..."

set +e
dotnet publish "$PROJECT" \
  --no-restore \
  -c Release \
  -f net9.0-ios \
  -r ios-arm64 \
  -p:BuildIpa=false \
  -p:EnableCodeSigning=false \
  -p:CodesignKey="" \
  -p:CodesignProvision="" \
  -p:AppBundleDir="$APP" \
  -bl:artifacts/logs/step05-15-dotnet-ios.binlog \
  2>&1 | tee "$PUBLISH_LOG"
PUBLISH_STATUS=${PIPESTATUS[0]}
set -e

capture_linker_diagnostics

if [[ "$PUBLISH_STATUS" != "0" ]]; then
  {
    echo
    echo "=== Step 05.15 publish failed: focused scan ==="
    echo "dotnet publish exit code: $PUBLISH_STATUS"
    echo
    echo "SteamKit patch telemetry:"
    grep -E 'STEP05\.7|Assembly:|Patched method:|Replacement|Strong-name' "$PATCH_LOG" || true
    echo
    echo "Framework filter messages:"
    cat "$FRAMEWORK_LOG" || true
    echo
    echo "First relevant fatal/native-link lines:"
    grep -E -m 100 \
      '(^|: )(error|fatal error)|undefined symbol|Undefined symbols|framework .+ not found|DiskArbitration|DASession|DADisk|IOService|IORegistry|IOKit|PlatformNotSupported' \
      "$PUBLISH_LOG" || true
  } | tee artifacts/logs/step05-15-failure-scan.log

  exit "$PUBLISH_STATUS"
fi

# A successful native link is accepted only if the retained Step 05.2 filter
# demonstrably ran exactly as before.
BEFORE_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS BEFORE:' "$PUBLISH_LOG" | tail -1 || true)"
AFTER_LINE="$(grep 'STEP05.2 LINKER FRAMEWORKS AFTER:' "$PUBLISH_LOG" | tail -1 || true)"

if [[ -z "$BEFORE_LINE" || -z "$AFTER_LINE" ]]; then
  echo "ERROR: publish succeeded but retained framework-filter telemetry was not emitted." >&2
  exit 8
fi

if [[ "$BEFORE_LINE" != *"DiskArbitration"* ]]; then
  echo "ERROR: expected DiskArbitration before filtering, but did not observe it." >&2
  echo "$BEFORE_LINE" >&2
  exit 8
fi

if [[ "$AFTER_LINE" == *"DiskArbitration"* ]]; then
  echo "ERROR: DiskArbitration survived the retained framework filter." >&2
  echo "$AFTER_LINE" >&2
  exit 8
fi

if [[ ! -d "$APP" ]]; then
  echo "ERROR: publish completed but expected .app was not created: $APP" >&2
  find "$ROOT/src/StS2Launcher.Step05.iOS" -type d -name '*.app' -print >&2 || true
  exit 9
fi

mkdir -p artifacts/Payload
cp -R "$APP" artifacts/Payload/

(
  cd artifacts
  rm -f StS2-Launcher-Step-05.15.ipa
  /usr/bin/zip -qry StS2-Launcher-Step-05.15.ipa Payload
)

echo "Created $IPA"
