#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
source scripts/lib/current-release.sh

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: Godot iOS static-host build requires macOS/Xcode." >&2
  exit 2
fi

GODOT_TAG="4.5.1-stable"
# The first successful Codemagic source build proved this is the commit behind
# the pinned 4.5.1-stable tag. Verify both tag and immutable commit so a moved
# remote tag cannot silently change our native engine input.
GODOT_COMMIT="f62fdbde15035c5576dad93e586201f4d41ef0cb"
SCONS_VERSION="4.8.1"
CACHE_ROOT="${HOME}/.cache/sts2launcher/godot-step15"
CACHE_LIB="$CACHE_ROOT/libgodot-step15.a"
CACHE_FINGERPRINT="$CACHE_ROOT/fingerprint.txt"
SOURCE_DIR="$ROOT/artifacts/godot-step15-source"
NATIVE_DIR="$ROOT/src/StS2Launcher.iOS/NativeBuild"
OUT_LIB="$NATIVE_DIR/libgodot-step15.a"
LOG_DIR="$ROOT/artifacts/logs"
PROJECT="$ROOT/$STS2_IOS_PROJECT"
mkdir -p "$CACHE_ROOT" "$NATIVE_DIR" "$LOG_DIR"

# Older Step 15 candidates created a Python venv under CACHE_ROOT. Codemagic
# explicitly does not support caching symlinks; keep this cache directory
# symlink-free and limited to the archive + fingerprint from now on.
rm -rf "$CACHE_ROOT/scons-venv"
find "$CACHE_ROOT" -mindepth 1 -maxdepth 1 \
  ! -name 'libgodot-step15.a' \
  ! -name 'fingerprint.txt' \
  -exec rm -rf {} + 2>/dev/null || true

XCODE_ID="$(xcodebuild -version | tr '\n' ' ' | sed 's/[[:space:]]*$//')"
SDK_VERSION="$(xcrun --sdk iphoneos --show-sdk-version)"
SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"

# Read the one authoritative framework list from the .NET NativeReference.
STEP15_FRAMEWORKS="$(python3 - "$PROJECT" <<'PY'
from pathlib import Path
import re, sys
text = Path(sys.argv[1]).read_text()
m = re.search(r'<NativeReference Include="NativeBuild/libgodot-step15\.a">(.*?)</NativeReference>', text, re.S)
if not m:
    raise SystemExit('ERROR: Step 15 Godot NativeReference block missing.')
fm = re.search(r'<Frameworks>([^<]+)</Frameworks>', m.group(1))
if not fm:
    raise SystemExit('ERROR: Step 15 Godot Frameworks metadata missing.')
print(' '.join(fm.group(1).split()))
PY
)"
STEP15_ROOT_SYMBOLS="$(python3 - "$PROJECT" <<'PY'
from pathlib import Path
import re, sys
text = Path(sys.argv[1]).read_text()
symbols = re.findall(r'<ReferenceNativeSymbol Include="([^"]+)" SymbolType="Function"\s*/>', text)
if not symbols:
    raise SystemExit('ERROR: Step 15 ReferenceNativeSymbol function roots missing.')
print(' '.join(symbols))
PY
)"

FINGERPRINT="$(python3 - "$GODOT_TAG" "$GODOT_COMMIT" "$SCONS_VERSION" "$XCODE_ID" "$SDK_VERSION" <<'PYFINGERPRINT'
from pathlib import Path
import hashlib
import sys

tag, commit, scons, xcode, sdk = sys.argv[1:]
h = hashlib.sha256()
for marker in (
    f"godot-tag={tag}",
    f"godot-commit={commit}",
    f"scons={scons}",
    f"xcode={xcode}",
    f"iphoneos-sdk={sdk}",
    "godot-main-symbol-patch=v1",
    "godot-embedded-view-controller-service-patch=v1",
    "godot-empty-ios-plugin-glue=v1",
    "scons-options=platform=ios,target=template_release,arch=arm64,metal=yes,vulkan=no,opengl3=yes,lto=none,module_mono_enabled=yes",
):
    h.update(marker.encode("utf-8"))
    h.update(b"\0")

roots = [
    Path("native/step15/godot_module/sts2_ios_host"),
    # The script itself contains the two guarded upstream source patches. Hash it so
    # any future patch change automatically invalidates an older cached archive.
    Path("scripts/build-godot.sh"),
]
paths = []
for root in roots:
    if root.is_file():
        paths.append(root)
    else:
        paths.extend(p for p in root.rglob("*") if p.is_file())
for path in sorted(paths):
    h.update(path.as_posix().encode("utf-8"))
    h.update(b"\0")
    h.update(path.read_bytes())
    h.update(b"\0")
print(h.hexdigest())
PYFINGERPRINT
)"

echo "Step 15 Godot source fingerprint: $FINGERPRINT" | tee "$LOG_DIR/step15-godot-build.log"
echo "Pinned Godot: $GODOT_TAG @ $GODOT_COMMIT" | tee -a "$LOG_DIR/step15-godot-build.log"
echo "Toolchain fingerprint: $XCODE_ID; iPhoneOS SDK $SDK_VERSION" | tee -a "$LOG_DIR/step15-godot-build.log"

validate_archive() {
  local lib="$1"
  if [[ ! -f "$lib" ]]; then
    echo "Godot archive validation: archive missing: $lib" >&2
    return 1
  fi

  local arch_info
  if ! arch_info="$(lipo -info "$lib" 2>/dev/null)"; then
    echo "Godot archive validation: lipo could not inspect archive: $lib" >&2
    return 1
  fi
  if [[ "$arch_info" != *arm64* ]]; then
    echo "Godot archive validation: arm64 architecture missing: $arch_info" >&2
    return 1
  fi

  local symbols_file
  symbols_file="$(mktemp "${TMPDIR:-/tmp}/sts2-step15-nm.XXXXXX")"
  if ! nm -gU "$lib" >"$symbols_file" 2>/dev/null; then
    echo "Godot archive validation: nm could not inspect exported symbols." >&2
    rm -f "$symbols_file"
    return 1
  fi

  for root_symbol in $STEP15_ROOT_SYMBOLS; do
    symbol="_${root_symbol}"
    if ! grep -E "[[:space:]]T[[:space:]]${symbol}$" "$symbols_file" >/dev/null; then
      echo "Godot archive validation: missing defined Step 15 bridge export: $symbol" >&2
      rm -f "$symbols_file"
      return 1
    fi
  done

  if ! grep -E '[[:space:]]T[[:space:]].*apple_embedded_main' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing defined C++ apple_embedded_main symbol." >&2
    rm -f "$symbols_file"
    return 1
  fi
  if ! grep -E '[[:space:]]T[[:space:]].*godot_apple_embedded_plugins_initialize' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing defined Step 15 no-plugin initialize glue symbol." >&2
    rm -f "$symbols_file"
    return 1
  fi
  if ! grep -E '[[:space:]]T[[:space:]].*godot_apple_embedded_plugins_deinitialize' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing defined Step 15 no-plugin deinitialize glue symbol." >&2
    rm -f "$symbols_file"
    return 1
  fi
  if grep -E '[[:space:]]T[[:space:]]_main$' "$symbols_file" >/dev/null; then
    echo "ERROR: Godot archive still exports its UIApplicationMain entry symbol; host patch did not isolate main()." >&2
    rm -f "$symbols_file"
    return 1
  fi

  rm -f "$symbols_file"
  return 0
}

# Make framework mistakes fail before SCons or dotnet publish. This list comes
# directly from the csproj, so there is no second hand-maintained list to drift.
for framework in $STEP15_FRAMEWORKS; do
  if [[ ! -d "$SDK_ROOT/System/Library/Frameworks/${framework}.framework" ]]; then
    echo "ERROR: Step 15 requested iPhoneOS framework is absent: $framework" >&2
    exit 4
  fi
done
if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "ERROR: DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." >&2
  exit 4
fi

if [[ -f "$CACHE_LIB" && -f "$CACHE_FINGERPRINT" ]] && \
   [[ "$(cat "$CACHE_FINGERPRINT")" == "$FINGERPRINT" ]] && \
   validate_archive "$CACHE_LIB"; then
  echo "Using validated cached Godot $GODOT_TAG Step 15 archive." | tee -a "$LOG_DIR/step15-godot-build.log"
  cp "$CACHE_LIB" "$OUT_LIB"
  if bash scripts/preflight-godot-link.sh "$OUT_LIB"; then
    echo "Cached Godot archive passed standalone native-link preflight." | tee -a "$LOG_DIR/step15-godot-build.log"
    rm -rf "$SOURCE_DIR" "$ROOT/artifacts/step15-scons-venv"
    exit 0
  fi
  echo "Cached Godot archive failed native-link preflight; rebuilding once from pinned source." | tee -a "$LOG_DIR/step15-godot-build.log"
  rm -f "$CACHE_LIB" "$CACHE_FINGERPRINT" "$OUT_LIB"
fi

rm -rf "$SOURCE_DIR"
echo "Cloning pinned Godot $GODOT_TAG source..." | tee -a "$LOG_DIR/step15-godot-build.log"
CLONED=0
for attempt in 1 2 3; do
  rm -rf "$SOURCE_DIR"
  set +e
  git -c advice.detachedHead=false clone --depth 1 --branch "$GODOT_TAG" \
    https://github.com/godotengine/godot.git "$SOURCE_DIR" \
    2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"
  status=${PIPESTATUS[0]}
  set -e
  if [[ "$status" == "0" ]]; then
    CLONED=1
    break
  fi
  echo "Godot clone attempt $attempt/3 failed (exit $status)." | tee -a "$LOG_DIR/step15-godot-build.log"
  sleep $((attempt * 5))
done
if [[ "$CLONED" != "1" ]]; then
  echo "ERROR: unable to clone pinned Godot after 3 attempts." >&2
  exit 3
fi

ACTUAL_TAG="$(git -C "$SOURCE_DIR" describe --tags --exact-match 2>/dev/null || true)"
ACTUAL_COMMIT="$(git -C "$SOURCE_DIR" rev-parse HEAD 2>/dev/null || true)"
if [[ "$ACTUAL_TAG" != "$GODOT_TAG" ]]; then
  echo "ERROR: cloned Godot source is not exactly tag $GODOT_TAG (got '$ACTUAL_TAG')." >&2
  exit 3
fi
if [[ "$ACTUAL_COMMIT" != "$GODOT_COMMIT" ]]; then
  echo "ERROR: Godot tag $GODOT_TAG resolved to unexpected commit $ACTUAL_COMMIT; expected $GODOT_COMMIT." >&2
  exit 3
fi

python3 - "$SOURCE_DIR/platform/ios/main_ios.mm" <<'PY'
from pathlib import Path
import sys
path = Path(sys.argv[1])
text = path.read_text()
old = 'int main(int argc, char *argv[]) {'
new = 'int sts2_godot_template_main_disabled(int argc, char *argv[]) {'
if text.count(old) != 1:
    raise SystemExit(f'ERROR: expected exactly one Godot iOS main() definition, found {text.count(old)}')
if text.count('int apple_embedded_main(int argc, char **argv) {') != 1:
    raise SystemExit('ERROR: pinned Godot apple_embedded_main signature changed unexpectedly.')
text = text.replace(old, new, 1)
path.write_text(text)
PY

python3 - "$SOURCE_DIR/drivers/apple_embedded/app_delegate_service.h" "$SOURCE_DIR/drivers/apple_embedded/app_delegate_service.mm" <<'PY'
from pathlib import Path
import sys
header = Path(sys.argv[1])
impl = Path(sys.argv[2])
header_text = header.read_text()
header_old = '@property(strong, class, readonly, nonatomic) GDTViewController *viewController;\n\n@end'
header_new = '@property(strong, class, readonly, nonatomic) GDTViewController *viewController;\n+ (void)sts2_setEmbeddedViewController:(GDTViewController *)viewController;\n\n@end'
if header_text.count(header_old) != 1:
    raise SystemExit(f'ERROR: pinned Godot app_delegate_service.h anchor changed unexpectedly (found {header_text.count(header_old)}).')
header.write_text(header_text.replace(header_old, header_new, 1))
impl_text = impl.read_text()
impl_old = '+ (GDTViewController *)viewController {\n\treturn mainViewController;\n}\n'
impl_new = '+ (GDTViewController *)viewController {\n\treturn mainViewController;\n}\n+ (void)sts2_setEmbeddedViewController:(GDTViewController *)viewController {\n\tmainViewController = viewController;\n}\n'
if impl_text.count(impl_old) != 1:
    raise SystemExit(f'ERROR: pinned Godot app_delegate_service.mm getter anchor changed unexpectedly (found {impl_text.count(impl_old)}).')
impl.write_text(impl_text.replace(impl_old, impl_new, 1))
PY

rm -rf "$SOURCE_DIR/modules/sts2_ios_host"
cp -R native/step15/godot_module/sts2_ios_host "$SOURCE_DIR/modules/sts2_ios_host"

# Keep the Python venv outside Codemagic's cached directory; venvs contain
# symlinks and Codemagic does not support caching symlinks.
SCONS_VENV="$ROOT/artifacts/step15-scons-venv"
rm -rf "$SCONS_VENV"
python3 -m venv "$SCONS_VENV"
"$SCONS_VENV/bin/python" -m pip install \
  --disable-pip-version-check --retries 5 --timeout 30 "scons==$SCONS_VERSION" \
  2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"

JOBS="$(sysctl -n hw.logicalcpu 2>/dev/null || echo 8)"
echo "Building Godot $GODOT_TAG iOS arm64 static archive with native Metal (jobs=$JOBS)..." | tee -a "$LOG_DIR/step15-godot-build.log"
(
  cd "$SOURCE_DIR"
  "$SCONS_VENV/bin/scons" \
    platform=ios \
    target=template_release \
    arch=arm64 \
    metal=yes \
    vulkan=no \
    opengl3=yes \
    lto=none \
    module_mono_enabled=yes \
    -j"$JOBS"
) 2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"

# Godot 4.5.1-stable deterministically emits this combined archive for the
# pinned SCons options. Never pick an arbitrary libgodot*.a if upstream emits
# additional archives in the future.
BUILT_LIB="$SOURCE_DIR/bin/libgodot.ios.template_release.arm64.a"
if [[ ! -f "$BUILT_LIB" ]]; then
  echo "ERROR: expected Godot combined archive missing: $BUILT_LIB" >&2
  find "$SOURCE_DIR/bin" -maxdepth 1 -type f -name 'libgodot*.a' -print >&2 || true
  exit 5
fi

if ! validate_archive "$BUILT_LIB"; then
  echo "ERROR: built Godot Step 15 archive failed symbol/architecture validation: $BUILT_LIB" >&2
  exit 6
fi

cp "$BUILT_LIB" "$OUT_LIB"
# Run the Apple linker independently before caching. This catches the exact
# class of native-link failures that consumed the Step 15.0.2–15.0.4 cycles.
bash scripts/preflight-godot-link.sh "$OUT_LIB"

cp "$BUILT_LIB" "$CACHE_LIB"
printf '%s\n' "$FINGERPRINT" > "$CACHE_FINGERPRINT"

# The source checkout and Python venv are no longer needed once the validated
# archive has been copied into NativeBuild + the fingerprinted Codemagic cache.
# Free that disk before the substantially larger .NET iOS AOT/link stage.
rm -rf "$SOURCE_DIR" "$SCONS_VENV"

{
  echo "STEP 15 GODOT STATIC BUILD: PASS"
  echo "Godot tag: $GODOT_TAG"
  echo "Godot commit: $GODOT_COMMIT"
  echo "Archive: $OUT_LIB"
  echo "Archive bytes: $(stat -f%z "$OUT_LIB")"
  echo "Architecture: $(lipo -info "$OUT_LIB")"
  echo "Standalone native-link preflight: PASS"
  echo "main() collision patch: PASS"
  echo "apple_embedded_main retained: PASS"
  echo "Step 15 bridge exports retained: PASS"
  echo "Godot native C#/.NET module: enabled for Step-35.0.19 runtime interop callback-table exposure (managed Godot assemblies are not built here)"
  echo "Metal: enabled"
  echo "Vulkan/MoltenVK: disabled for this foundation build"
  echo "OpenGLES fallback objects: compiled for upstream iOS archive compatibility; Step 15 runtime is forced to Metal"
} | tee -a "$LOG_DIR/step15-godot-build.log"
