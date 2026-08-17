#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

LIB="${1:-src/StS2Launcher.Step05.iOS/NativeBuild/libgodot-step15.a}"
if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: Step 15 native-link preflight requires macOS/Xcode." >&2
  exit 2
fi
if [[ ! -f "$LIB" ]]; then
  echo "ERROR: Step 15 native-link preflight archive missing: $LIB" >&2
  exit 2
fi

PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
DEPLOYMENT_TARGET="$(python3 - "$PROJECT" <<'PY'
from pathlib import Path
import re, sys
text = Path(sys.argv[1]).read_text()
m = re.search(r'<SupportedOSPlatformVersion>([^<]+)</SupportedOSPlatformVersion>', text)
if not m:
    raise SystemExit('ERROR: SupportedOSPlatformVersion missing from iOS project.')
print(m.group(1).strip())
PY
)"

# Read the NativeReference settings from the app project itself so the preflight
# cannot silently drift away from the frameworks/link flags used by dotnet iOS.
eval "$(python3 - "$PROJECT" <<'PY'
from pathlib import Path
import re, shlex, sys
text = Path(sys.argv[1]).read_text()
fm = re.search(r'<NativeReference Include="NativeBuild/libgodot-step15\.a">(.*?)</NativeReference>', text, re.S)
if not fm:
    raise SystemExit('ERROR: Step 15 Godot NativeReference block missing.')
block = fm.group(1)
def one(name):
    m = re.search(fr'<{name}>([^<]*)</{name}>', block)
    if not m:
        raise SystemExit(f'ERROR: NativeReference {name} missing.')
    return m.group(1).strip()
frameworks = one('Frameworks')
flags = one('LinkerFlags')
if one('ForceLoad').lower() != 'false':
    raise SystemExit('ERROR: Step 15 preflight requires ForceLoad=false.')
if one('SmartLink').lower() != 'false':
    raise SystemExit('ERROR: Step 15 preflight requires SmartLink=false.')
symbols = re.findall(r'<ReferenceNativeSymbol Include="([^"]+)" SymbolType="Function"\s*/>', text)
if not symbols:
    raise SystemExit('ERROR: Step 15 ReferenceNativeSymbol function roots missing.')
print('STEP15_FRAMEWORKS=' + shlex.quote(frameworks))
print('STEP15_LINKER_FLAGS=' + shlex.quote(flags))
print('STEP15_ROOT_SYMBOLS=' + shlex.quote(' '.join(symbols)))
PY
)"

for framework in $STEP15_FRAMEWORKS; do
  if [[ ! -d "$SDK_ROOT/System/Library/Frameworks/${framework}.framework" ]]; then
    echo "ERROR: Step 15 project requests missing iPhoneOS framework: $framework" >&2
    exit 3
  fi
done

TMP="$(mktemp -d "${TMPDIR:-/tmp}/sts2-step15-link-preflight.XXXXXX")"
trap 'rm -rf "$TMP"' EXIT
SRC="$TMP/preflight.cc"
OBJ="$TMP/preflight.o"
OUT="$TMP/preflight"
LOG="artifacts/logs/step15-native-link-preflight.log"
mkdir -p artifacts/logs

cat > "$SRC" <<'SRC'
extern "C" {
const char *sts2_step15_get_engine_version(void);
int sts2_step15_start(void *, void *, const char *);
int sts2_step15_requires_process_restart(void);
int sts2_step15_is_metal_layer_ready(void);
int sts2_step15_touch_marker_ready(void);
}

// Make the test executable carry real unresolved references into the archive.
// Explicit function-pointer types plus volatile dispatch prevent the compiler
// from erasing the roots while keeping the probe valid C++17.
int main(int argc, char **argv) {
    using VersionFn = const char *(*)(void);
    using StartFn = int (*)(void *, void *, const char *);
    using IntFn = int (*)(void);

    volatile VersionFn version_fn = &sts2_step15_get_engine_version;
    volatile StartFn start_fn = &sts2_step15_start;
    volatile IntFn restart_fn = &sts2_step15_requires_process_restart;
    volatile IntFn metal_fn = &sts2_step15_is_metal_layer_ready;
    volatile IntFn touch_fn = &sts2_step15_touch_marker_ready;

    if (argc == -1) {
        return start_fn(nullptr, nullptr, argv ? argv[0] : nullptr);
    }
    return version_fn() == nullptr || restart_fn() < 0 || metal_fn() < 0 || touch_fn() < 0;
}
SRC

FRAMEWORK_ARGS=()
for framework in $STEP15_FRAMEWORKS; do
  FRAMEWORK_ARGS+=( -framework "$framework" )
done

# Link with the same normal archive selection and explicit app-link flags used
# by the NativeReference. This catches missing frameworks, missing plugin glue,
# duplicate archive members and undefined Godot dependencies before dotnet
# spends time compiling/AOTing the full launcher.
LINK_FLAG_ARGS=()
for flag in $STEP15_LINKER_FLAGS; do
  LINK_FLAG_ARGS+=( "$flag" )
done
# Mirror .NET's ReferenceNativeSymbol Function behavior in this standalone clang
# preflight. Mach-O C function symbols receive the conventional leading underscore.
for symbol in $STEP15_ROOT_SYMBOLS; do
  LINK_FLAG_ARGS+=( "-Wl,-u,_${symbol}" )
done

{
  echo "=== Step 15 standalone native-link preflight ==="
  echo "Archive: $LIB"
  echo "SDK: $SDK_ROOT"
  echo "Deployment target: $DEPLOYMENT_TARGET"
  echo "Frameworks: $STEP15_FRAMEWORKS"
  echo "Linker flags: $STEP15_LINKER_FLAGS"
  echo "ReferenceNativeSymbol roots: $STEP15_ROOT_SYMBOLS"
} | tee "$LOG"

# Compile the probe in an explicit C++ language mode first. Do not rely on
# xcrun/driver-name/file-extension inference: Codemagic's Xcode 26.5 worker
# compiled the earlier .mm probe as Objective-C, which rejected extern "C",
# auto and nullptr before the linker was reached.
xcrun --sdk iphoneos clang++ \
  -arch arm64 \
  -isysroot "$SDK_ROOT" \
  -miphoneos-version-min="$DEPLOYMENT_TARGET" \
  -O0 \
  -std=c++17 \
  -x c++ \
  -c "$SRC" \
  -o "$OBJ" \
  2>&1 | tee -a "$LOG"

# Link the already-compiled C++ object against the same normal Godot archive
# selection, ReferenceNativeSymbol roots, app linker flags and frameworks.
xcrun --sdk iphoneos clang++ \
  -arch arm64 \
  -isysroot "$SDK_ROOT" \
  -miphoneos-version-min="$DEPLOYMENT_TARGET" \
  "$OBJ" \
  "$LIB" \
  "${LINK_FLAG_ARGS[@]}" \
  "${FRAMEWORK_ARGS[@]}" \
  -o "$OUT" \
  2>&1 | tee -a "$LOG"

file "$OUT" | tee -a "$LOG"
lipo -info "$OUT" | tee -a "$LOG"

echo "STEP 15 STANDALONE NATIVE LINK PREFLIGHT: PASS" | tee -a "$LOG"
