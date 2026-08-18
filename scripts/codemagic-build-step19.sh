#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
mkdir -p artifacts/logs artifacts/test-results

DOTNET_SDK_VERSION="${DOTNET_SDK_VERSION:-9.0.314}"
DOTNET_WORKLOAD_SET="${DOTNET_WORKLOAD_SET:-9.0.314.3}"
DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"

export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
export NUGET_XMLDOC_MODE=skip

if [[ "$(uname -s)" != "Darwin" ]]; then
  echo "ERROR: Codemagic iOS build must run on a macOS worker." >&2
  exit 2
fi

{
  echo "=== StS2 Launcher Step 19 Expression Interpreter Compatibility environment ==="
  date -u
  uname -a
  xcodebuild -version
  xcrun --sdk iphoneos --show-sdk-version
  sw_vers
  python3 --version
  git --version
} | tee artifacts/logs/step19-environment.log

NEED_DOTNET=1
if [[ -x "$DOTNET_ROOT/dotnet" ]]; then
  INSTALLED="$("$DOTNET_ROOT/dotnet" --version 2>/dev/null || true)"
  [[ "$INSTALLED" == "$DOTNET_SDK_VERSION" ]] && NEED_DOTNET=0
fi

if [[ "$NEED_DOTNET" == "1" ]]; then
  echo "Installing .NET SDK $DOTNET_SDK_VERSION..."
  rm -rf "$DOTNET_ROOT"
  mkdir -p "$DOTNET_ROOT"
  INSTALLER="$(mktemp)"
  curl -fsSL --retry 4 --retry-delay 3 --retry-all-errors https://dot.net/v1/dotnet-install.sh -o "$INSTALLER"
  bash "$INSTALLER" --version "$DOTNET_SDK_VERSION" --install-dir "$DOTNET_ROOT" --no-path
  rm -f "$INSTALLER"
fi

[[ "$(dotnet --version)" == "$DOTNET_SDK_VERSION" ]] || {
  echo "ERROR: unexpected .NET SDK version: $(dotnet --version)" >&2
  exit 3
}

bash scripts/validate-step19.sh | tee artifacts/logs/step19-validation.log
bash scripts/run-unit-tests-step19.sh

WORKLOAD_CWD="$(mktemp -d)"
trap 'rm -rf "$WORKLOAD_CWD"' EXIT
(
  cd "$WORKLOAD_CWD"
  WORKLOAD_OK=0
  for attempt in 1 2; do
    if "$DOTNET_ROOT/dotnet" workload install ios --version "$DOTNET_WORKLOAD_SET"; then
      WORKLOAD_OK=1
      break
    fi
    echo "iOS workload install attempt $attempt/2 failed; retrying after a short delay." >&2
    sleep $((attempt * 5))
  done
  [[ "$WORKLOAD_OK" == "1" ]] || { echo "ERROR: iOS workload install failed after 2 attempts." >&2; exit 4; }
  "$DOTNET_ROOT/dotnet" workload --info
) | tee artifacts/logs/step19-workload.log
rm -rf "$WORKLOAD_CWD"
trap - EXIT

bash scripts/build-step19.sh 2>&1 | tee artifacts/logs/step19-wrapper.log
bash scripts/verify-step19-ipa.sh artifacts/StS2-Launcher-Step-19.ipa \
  2>&1 | tee artifacts/logs/step19-ipa-verification.log

{
  echo "StS2 Launcher iOS — Step 19 Expression Interpreter Compatibility"
  echo "UTC: $(date -u '+%Y-%m-%dT%H:%M:%SZ')"
  echo "Commit: ${CM_COMMIT:-unknown}"
  echo "Branch: ${CM_BRANCH:-unknown}"
  echo "Xcode: $(xcodebuild -version | tr '\n' ' ')"
  echo ".NET SDK: $(dotnet --version)"
  echo "iOS workload set requested: $DOTNET_WORKLOAD_SET"
  echo "Host unit tests: PASS (required before publish)"
  echo "SteamKit: 3.4.0"
  echo "Mono.Cecil: 0.11.6 runtime metadata/IL analysis dependency"
  echo "Foundation + Steps 01-18 physically closed regression boundaries: retained"
  echo "Godot source pin: 4.5.1-stable @ f62fdbde15035c5576dad93e586201f4d41ef0cb"
  echo "Step 16 regression: managed-preparation 4/4 retained"
  echo "Step 17 regression: compatibility call-site analysis 4/4 source boundary retained"
  echo "Step 18 regression: real assembly rewrite workspace 4/4 + OfflineReady + Foundation closure retained"
  echo "Step 19 gate A: physical System.Linq.Expressions interpreter probe + fresh receipt-backed arm64/shared workspace clone"
  echo "Step 19 gate B: real direct LambdaExpression/Expression<TDelegate>.Compile target discovery with structural safety classification"
  echo "Step 19 gate C: safe unsigned Compile()/literal Compile(false) sites rewritten to preferInterpretation=true; explicit workspace-resolver reopen/validation"
  echo "Step 19 gate D: source/prepared/live SHA-1 + structural isolation audit; only selected prepared assemblies may differ"
  echo "Game-data policy: no StS2 game files/assemblies or proprietary FMOD/Spine binaries in IPA"
  echo "Still absent: game execution, Harmony/MonoMod detours, Reflection.Emit replacement, FMOD/Spine runtime integration, Cloud, Workshop"
  echo "IPA: artifacts/StS2-Launcher-Step-19.ipa"
  if command -v shasum >/dev/null 2>&1; then
    echo "IPA SHA-256: $(shasum -a 256 artifacts/StS2-Launcher-Step-19.ipa | awk '{print $1}')"
  fi
} > artifacts/step19-build-summary.txt

cat artifacts/step19-build-summary.txt
