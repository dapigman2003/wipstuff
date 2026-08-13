#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

IOS_PROJECT="src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
CORE_PROJECT="src/StS2Launcher.Core/StS2Launcher.Core.csproj"

[[ -f "$IOS_PROJECT" ]] || {
  echo "ERROR: missing Step 05 iOS project." >&2
  exit 2
}
[[ -f "$CORE_PROJECT" ]] || {
  echo "ERROR: missing Core project." >&2
  exit 2
}

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

ios_root = ET.parse(
    "src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj").getroot()

refs = [e.attrib.get("Include", "") for e in ios_root.iter("ProjectReference")]
if refs != ["../StS2Launcher.Core/StS2Launcher.Core.csproj"]:
    raise SystemExit(
        f"ERROR: iOS project expected only Core ProjectReference; got {refs}")

if any(True for _ in ios_root.iter("PackageReference")):
    raise SystemExit(
        "ERROR: SteamKit package belongs in Core, not directly in the iOS UI project.")
if any(True for _ in ios_root.iter("NativeReference")):
    raise SystemExit("ERROR: Step 05.1 must not contain NativeReference.")

trim_mode = None
for e in ios_root.iter("TrimMode"):
    trim_mode = (e.text or "").strip()
if trim_mode != "full":
    raise SystemExit(
        f"ERROR: Step 05.1 requires TrimMode=full; got {trim_mode!r}.")

core_root = ET.parse(
    "src/StS2Launcher.Core/StS2Launcher.Core.csproj").getroot()

packages = {
    e.attrib.get("Include"): e.attrib.get("Version")
    for e in core_root.iter("PackageReference")
}

if packages != {"SteamKit2": "3.3.1"}:
    raise SystemExit(
        f"ERROR: Step 05 expects exactly SteamKit2 3.3.1; got {packages}")

plist_path = Path("src/StS2Launcher.Step05.iOS/Info.plist")
with plist_path.open("rb") as f:
    plist = plistlib.load(f)

if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")

scene = Path("src/StS2Launcher.Step05.iOS/SceneDelegate.cs").read_text()
if "class SceneDelegate : UIWindowSceneDelegate" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate regression.")
if "public override UIWindow? Window" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate.Window regression.")

probe = Path("src/StS2Launcher.Core/SteamConnectionProbe.cs").read_text()
for required in (
    "new SteamClient()",
    "new CallbackManager(steamClient)",
    "ConnectedCallback",
    "DisconnectedCallback",
    "steamClient.Connect()",
    "steamClient.Disconnect()",
    "manager.RunWaitCallbacks",
    "STEAM CONNECTION PASS — 3/3",
):
    if required not in probe:
        raise SystemExit(f"ERROR: Steam probe missing: {required}")

# Authentication is intentionally forbidden in Step 05.
for forbidden in (
    "BeginAuthSession",
    "AuthSessionDetails",
    "SteamUser.LogOn",
    "RefreshToken",
    "Password =",
    "TwoFactor",
    "SteamGuard",
):
    if forbidden in probe:
        raise SystemExit(
            f"ERROR: Step 05.1 connection probe contains authentication behavior: {forbidden}")

root_view = Path(
    "src/StS2Launcher.Step05.iOS/RootViewController.cs").read_text()

for required in (
    "STEP 05.1 — STEAM LINK FIX",
    "NO LOGIN • NO PASSWORD • NO STEAM GUARD • NO TOKEN",
    "Run Steam Connection Probe",
    "STEAMKIT ASSEMBLY: PASS",
    "TimeSpan.FromSeconds(20)",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: Step 05.1 UI marker missing: {required}")

# Core may now depend on SteamKit2 but remains free of iOS platform APIs.
core_text = "\n".join(
    p.read_text(errors="ignore")
    for p in Path("src/StS2Launcher.Core").glob("*.cs")
)
for forbidden in ("UIKit", "Foundation", "Security.", "ObjCRuntime"):
    if forbidden in core_text:
        raise SystemExit(
            f"ERROR: Core contains iOS platform dependency: {forbidden}")

print("Step 05.1 SteamKit/full-trim/Core/UI boundary validation passed.")
PY

# Still forbidden: Godot, Cecil, Harmony/native runtime host.
if grep -RniE 'Godot|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 05.1 contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 05.1 repository validation passed."
