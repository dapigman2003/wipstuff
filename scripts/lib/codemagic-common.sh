#!/usr/bin/env bash
set -euo pipefail

STS2_TIMING_REPORT="${STS2_TIMING_REPORT:-artifacts/reports/phase-timings.txt}"
DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"
DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
DOTNET_SDK_CACHE="${DOTNET_SDK_CACHE:-$HOME/.cache/sts2launcher/dotnet-sdk-$DOTNET_SDK_VERSION}"
NUGET_PACKAGES="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
export DOTNET_ROOT NUGET_PACKAGES
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1 NUGET_XMLDOC_MODE=skip

sts2_timing_init() {
  mkdir -p "$(dirname "$STS2_TIMING_REPORT")"
  : > "$STS2_TIMING_REPORT"
  printf 'StS2 Launcher Codemagic phase timings\nUTC start: %s\n' "$(date -u '+%Y-%m-%dT%H:%M:%SZ')" >> "$STS2_TIMING_REPORT"
}

sts2_run_timed() {
  local label="$1"; shift
  local start end elapsed status
  start="$(date +%s)"
  echo "=== $label ==="
  set +e
  "$@"
  status=$?
  set -e
  end="$(date +%s)"
  elapsed=$((end - start))
  printf '%-34s %5ss status=%s\n' "$label" "$elapsed" "$status" | tee -a "$STS2_TIMING_REPORT"
  return "$status"
}

sts2_ensure_dotnet() {
  mkdir -p "$HOME/.cache/sts2launcher" "$NUGET_PACKAGES"
  if [[ -x "$DOTNET_ROOT/dotnet" && "$($DOTNET_ROOT/dotnet --version 2>/dev/null || true)" == "$DOTNET_SDK_VERSION" ]]; then
    echo "Using existing pinned .NET SDK from $DOTNET_ROOT"
    return 0
  fi

  if [[ -x "$DOTNET_SDK_CACHE/dotnet" && "$($DOTNET_SDK_CACHE/dotnet --version 2>/dev/null || true)" == "$DOTNET_SDK_VERSION" ]]; then
    echo "Restoring pinned .NET SDK $DOTNET_SDK_VERSION from Codemagic cache..."
    rm -rf "$DOTNET_ROOT"
    mkdir -p "$DOTNET_ROOT"
    cp -R "$DOTNET_SDK_CACHE/." "$DOTNET_ROOT/"
  else
    echo "Pinned .NET SDK cache miss; downloading $DOTNET_SDK_VERSION once..."
    rm -rf "$DOTNET_ROOT"
    mkdir -p "$DOTNET_ROOT"
    local installer
    installer="$(mktemp)"
    curl -fsSL --retry 4 --retry-delay 3 --retry-all-errors https://dot.net/v1/dotnet-install.sh -o "$installer"
    bash "$installer" --version "$DOTNET_SDK_VERSION" --install-dir "$DOTNET_ROOT" --no-path
    rm -f "$installer"
    rm -rf "$DOTNET_SDK_CACHE"
    mkdir -p "$DOTNET_SDK_CACHE"
    cp -R "$DOTNET_ROOT/." "$DOTNET_SDK_CACHE/"
  fi

  [[ "$($DOTNET_ROOT/dotnet --version)" == "$DOTNET_SDK_VERSION" ]] || {
    echo "ERROR: unexpected .NET SDK $($DOTNET_ROOT/dotnet --version 2>/dev/null || echo missing)" >&2
    exit 3
  }
}

sts2_report_cache_sizes() {
  {
    echo "=== Cache / tool sizes ==="
    du -sh "$DOTNET_SDK_CACHE" 2>/dev/null || true
    du -sh "$DOTNET_ROOT" 2>/dev/null || true
    du -sh "$NUGET_PACKAGES" 2>/dev/null || true
    du -sh "$HOME/.cache/sts2launcher/godot-step15" 2>/dev/null || true
    du -sh "$HOME/.cache/sts2launcher/harmony-fat-2.4.2" 2>/dev/null || true
  } | tee artifacts/reports/cache-sizes.txt
}

sts2_write_environment_report() {
  local mode="$1"
  {
    echo "StS2 Launcher — Step 32 $mode build environment"
    date -u
    uname -a
    xcodebuild -version
    xcrun --sdk iphoneos --show-sdk-version
    sw_vers
    python3 --version
    echo "Commit: ${CM_COMMIT:-unknown}"
    echo "Branch: ${CM_BRANCH:-unknown}"
  } | tee artifacts/reports/build-environment.txt
}
