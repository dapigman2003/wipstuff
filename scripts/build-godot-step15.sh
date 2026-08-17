#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: Godot iOS static-host build requires macOS/Xcode." >&2
  exit 2
fi

GODOT_TAG="4.5.1-stable"
CACHE_ROOT="${HOME}/.cache/sts2launcher/godot-step15"
CACHE_LIB="$CACHE_ROOT/libgodot-step15.a"
CACHE_FINGERPRINT="$CACHE_ROOT/fingerprint.txt"
SOURCE_DIR="$ROOT/artifacts/godot-step15-source"
NATIVE_DIR="$ROOT/src/StS2Launcher.Step05.iOS/NativeBuild"
OUT_LIB="$NATIVE_DIR/libgodot-step15.a"
LOG_DIR="$ROOT/artifacts/logs"
mkdir -p "$CACHE_ROOT" "$NATIVE_DIR" "$LOG_DIR"

FINGERPRINT="$(python3 - "$GODOT_TAG" <<'PYFINGERPRINT'
from pathlib import Path
import hashlib
import sys

tag = sys.argv[1]
h = hashlib.sha256()
for marker in (
    f"godot-tag={tag}",
    "godot-main-symbol-patch=v1",
    "godot-embedded-view-controller-service-patch=v1",
    "scons-options=platform=ios,target=template_release,arch=arm64,metal=yes,vulkan=no,opengl3=yes,lto=none",
):
    h.update(marker.encode("utf-8"))
    h.update(b"\0")

root = Path("native/step15/godot_module/sts2_ios_host")
for path in sorted(p for p in root.rglob("*") if p.is_file()):
    h.update(path.as_posix().encode("utf-8"))
    h.update(b"\0")
    h.update(path.read_bytes())
    h.update(b"\0")
print(h.hexdigest())
PYFINGERPRINT
)"

echo "Step 15 Godot source fingerprint: $FINGERPRINT" | tee "$LOG_DIR/step15-godot-build.log"

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

  # Write nm output once instead of piping a very large archive through
  # `grep -q` under `set -o pipefail`. An early grep exit can SIGPIPE nm and
  # make a successful match look like a failed pipeline.
  local symbols_file
  symbols_file="$(mktemp "${TMPDIR:-/tmp}/sts2-step15-nm.XXXXXX")"
  if ! nm -gU "$lib" >"$symbols_file" 2>/dev/null; then
    echo "Godot archive validation: nm could not inspect exported symbols." >&2
    rm -f "$symbols_file"
    return 1
  fi

  if ! grep -F '_sts2_step15_get_engine_version' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing Step 15 engine-version bridge export." >&2
    rm -f "$symbols_file"
    return 1
  fi
  if ! grep -F '_sts2_step15_start' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing Step 15 start bridge export." >&2
    rm -f "$symbols_file"
    return 1
  fi

  # platform/ios/main_ios.mm is Objective-C++, and upstream declares
  # apple_embedded_main without extern "C". Its Mach-O symbol is therefore
  # C++-mangled (for example it contains `apple_embedded_main` rather than
  # being the unmangled C symbol `_apple_embedded_main`). The project-owned
  # Objective-C++ bridge uses the same C++ declaration, so validate the
  # defined symbol by its stable function-name fragment.
  if ! grep -F 'apple_embedded_main' "$symbols_file" >/dev/null; then
    echo "Godot archive validation: missing C++ apple_embedded_main definition." >&2
    rm -f "$symbols_file"
    return 1
  fi

  if grep -E '[[:space:]]_main$' "$symbols_file" >/dev/null; then
    echo "ERROR: Godot archive still exports its UIApplicationMain entry symbol; host patch did not isolate main()." >&2
    rm -f "$symbols_file"
    return 1
  fi

  rm -f "$symbols_file"
  return 0
}

if [[ -f "$CACHE_LIB" && -f "$CACHE_FINGERPRINT" ]] && \
   [[ "$(cat "$CACHE_FINGERPRINT")" == "$FINGERPRINT" ]] && \
   validate_archive "$CACHE_LIB"; then
  echo "Using validated cached Godot $GODOT_TAG Step 15 archive." | tee -a "$LOG_DIR/step15-godot-build.log"
  cp "$CACHE_LIB" "$OUT_LIB"
  exit 0
fi

rm -rf "$SOURCE_DIR"
echo "Cloning pinned Godot $GODOT_TAG source..." | tee -a "$LOG_DIR/step15-godot-build.log"
git clone --depth 1 --branch "$GODOT_TAG" https://github.com/godotengine/godot.git "$SOURCE_DIR" \
  2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"

ACTUAL_TAG="$(git -C "$SOURCE_DIR" describe --tags --exact-match 2>/dev/null || true)"
if [[ "$ACTUAL_TAG" != "$GODOT_TAG" ]]; then
  echo "ERROR: cloned Godot source is not exactly tag $GODOT_TAG (got '$ACTUAL_TAG')." >&2
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

SCONS_VENV="$CACHE_ROOT/scons-venv"
if [[ ! -x "$SCONS_VENV/bin/scons" ]]; then
  rm -rf "$SCONS_VENV"
  python3 -m venv "$SCONS_VENV"
  "$SCONS_VENV/bin/python" -m pip install --disable-pip-version-check 'scons==4.8.1' \
    2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"
fi

SDK_ROOT="$(xcrun --sdk iphoneos --show-sdk-path)"
# These are app-link frameworks for the embedded Godot archive. Upstream Godot
# also adds AudioUnit.framework/Headers to its compile include path, but on the
# current iPhoneOS SDK that is a header surface rather than a standalone
# framework we should pass to ld. AudioUnit C APIs are linked via AudioToolbox.
for framework in \
  Accelerate AudioToolbox AVFoundation CFNetwork CoreAudio CoreFoundation CoreGraphics \
  CoreHaptics CoreLocation CoreMedia CoreMotion CoreText CoreVideo Foundation GameController ImageIO \
  MediaPlayer Metal MetalFX MetalKit OpenGLES QuartzCore Security SystemConfiguration UIKit UniformTypeIdentifiers \
  VideoToolbox WebKit; do
  if [[ ! -d "$SDK_ROOT/System/Library/Frameworks/${framework}.framework" ]]; then
    echo "ERROR: Step 15 requested iPhoneOS framework is absent: $framework" >&2
    exit 4
  fi
done
if [[ -d "$SDK_ROOT/System/Library/Frameworks/DiskArbitration.framework" ]]; then
  echo "ERROR: DiskArbitration.framework unexpectedly exists in iPhoneOS SDK." >&2
  exit 4
fi

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
    -j"$JOBS"
) 2>&1 | tee -a "$LOG_DIR/step15-godot-build.log"

BUILT_LIB="$(find "$SOURCE_DIR/bin" -maxdepth 1 -type f -name 'libgodot*.a' | sort | tail -1)"
if [[ -z "$BUILT_LIB" ]]; then
  echo "ERROR: Godot SCons build completed without a combined libgodot*.a archive." >&2
  exit 5
fi

if ! validate_archive "$BUILT_LIB"; then
  echo "ERROR: built Godot Step 15 archive failed symbol/architecture validation: $BUILT_LIB" >&2
  exit 6
fi

cp "$BUILT_LIB" "$OUT_LIB"
cp "$BUILT_LIB" "$CACHE_LIB"
printf '%s\n' "$FINGERPRINT" > "$CACHE_FINGERPRINT"

{
  echo "STEP 15 GODOT STATIC BUILD: PASS"
  echo "Godot tag: $GODOT_TAG"
  echo "Archive: $OUT_LIB"
  echo "Archive bytes: $(stat -f%z "$OUT_LIB")"
  echo "Architecture: $(lipo -info "$OUT_LIB")"
  echo "main() collision patch: PASS"
  echo "apple_embedded_main retained: PASS"
  echo "Step 15 bridge exports retained: PASS"
  echo "Metal: enabled"
  echo "Vulkan/MoltenVK: disabled for this foundation build"
  echo "OpenGLES fallback objects: compiled for upstream iOS archive compatibility; Step 15 runtime is forced to Metal"
} | tee -a "$LOG_DIR/step15-godot-build.log"
