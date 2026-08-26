#!/usr/bin/env bash
set -euo pipefail
# Compatibility entry point. Use codemagic-fast.sh first; this wrapper runs the device candidate path.
# Device summary: artifacts/reports/build-summary.txt
exec bash "$(cd "$(dirname "$0")" && pwd)/codemagic-device.sh" "$@"
