#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

PROJECT="src/StS2Launcher.Step02.iOS/StS2Launcher.Step02.iOS.csproj"

[[ -f "$PROJECT" ]] || {
  echo "ERROR: missing Step 02 project." >&2
  exit 2
}

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

project = Path("src/StS2Launcher.Step02.iOS/StS2Launcher.Step02.iOS.csproj")
root = ET.parse(project).getroot()

for forbidden in ("PackageReference", "ProjectReference", "NativeReference"):
    if any(True for _ in root.iter(forbidden)):
        raise SystemExit(f"ERROR: Step 02 must not contain {forbidden}.")

plist_path = Path("src/StS2Launcher.Step02.iOS/Info.plist")
with plist_path.open("rb") as f:
    plist = plistlib.load(f)

if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")

manifest = plist.get("UIApplicationSceneManifest", {})
configs = manifest.get("UISceneConfigurations", {})
app_configs = configs.get("UIWindowSceneSessionRoleApplication", [])
if not app_configs:
    raise SystemExit("ERROR: no UIWindow application scene configuration.")

config = app_configs[0]
if config.get("UISceneClassName") != "UIWindowScene":
    raise SystemExit("ERROR: scene class is not UIWindowScene.")
if config.get("UISceneDelegateClassName") != "SceneDelegate":
    raise SystemExit("ERROR: SceneDelegate is not explicitly registered in Info.plist.")


scene_source = Path("src/StS2Launcher.Step02.iOS/SceneDelegate.cs").read_text()
if "class SceneDelegate : UIWindowSceneDelegate" not in scene_source:
    raise SystemExit(
        "ERROR: SceneDelegate must derive from UIWindowSceneDelegate for a UIWindowScene.")
if "public override UIWindow? Window" not in scene_source:
    raise SystemExit(
        "ERROR: SceneDelegate must override UIWindowSceneDelegate.Window.")


root_view = Path("src/StS2Launcher.Step02.iOS/RootViewController.cs").read_text()
state_model = Path("src/StS2Launcher.Step02.iOS/LauncherDemoState.cs").read_text()

required_states = [
    "SignedOut",
    "Authenticating",
    "CheckingOwnership",
    "ReadyToInstall",
    "Downloading",
    "ReadyToPlay",
    "Error",
]
for state in required_states:
    if state not in state_model:
        raise SystemExit(f"ERROR: missing Step 02 demo state: {state}")

if "STEP 02 — LAUNCHER UI SHELL" not in root_view:
    raise SystemExit("ERROR: Step 02 UI identifier missing.")

print("Step 02 project/plist validation passed.")
PY

# The actual app source must remain isolated from later subsystems.
if grep -RniE 'Godot|SteamKit2|Mono\.Cecil|Harmony|NativeGodotHost' \
  src/StS2Launcher.Step02.iOS --exclude='*.csproj'; then
  echo "ERROR: Step 02 contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 02 repository validation passed."
