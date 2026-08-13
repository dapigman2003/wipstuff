#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

IOS_PROJECT="src/StS2Launcher.Step03.iOS/StS2Launcher.Step03.iOS.csproj"
CORE_PROJECT="src/StS2Launcher.Core/StS2Launcher.Core.csproj"

[[ -f "$IOS_PROJECT" ]] || {
  echo "ERROR: missing Step 03 iOS project." >&2
  exit 2
}

[[ -f "$CORE_PROJECT" ]] || {
  echo "ERROR: missing StS2Launcher.Core project." >&2
  exit 2
}

python3 - <<'PY'
from pathlib import Path
import plistlib
import xml.etree.ElementTree as ET

ios_project = Path("src/StS2Launcher.Step03.iOS/StS2Launcher.Step03.iOS.csproj")
ios_root = ET.parse(ios_project).getroot()

# Step 03 adds one and only one project reference: Core.
project_refs = [
    e.attrib.get("Include", "")
    for e in ios_root.iter("ProjectReference")
]
expected = ["../StS2Launcher.Core/StS2Launcher.Core.csproj"]
if project_refs != expected:
    raise SystemExit(
        f"ERROR: Step 03 expected exactly one Core ProjectReference; got {project_refs}")

for forbidden in ("PackageReference", "NativeReference"):
    if any(True for _ in ios_root.iter(forbidden)):
        raise SystemExit(f"ERROR: Step 03 iOS project must not contain {forbidden}.")

core_project = Path("src/StS2Launcher.Core/StS2Launcher.Core.csproj")
core_root = ET.parse(core_project).getroot()

for forbidden in ("PackageReference", "ProjectReference", "NativeReference"):
    if any(True for _ in core_root.iter(forbidden)):
        raise SystemExit(f"ERROR: Core project must not contain {forbidden}.")

tfm = None
for e in core_root.iter("TargetFramework"):
    tfm = (e.text or "").strip()
if tfm != "net9.0":
    raise SystemExit(f"ERROR: Core must target net9.0, got {tfm!r}")

plist_path = Path("src/StS2Launcher.Step03.iOS/Info.plist")
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
    raise SystemExit("ERROR: SceneDelegate is not explicitly registered.")

scene_source = Path(
    "src/StS2Launcher.Step03.iOS/SceneDelegate.cs").read_text()
if "class SceneDelegate : UIWindowSceneDelegate" not in scene_source:
    raise SystemExit(
        "ERROR: SceneDelegate must derive from UIWindowSceneDelegate.")
if "public override UIWindow? Window" not in scene_source:
    raise SystemExit(
        "ERROR: SceneDelegate must override UIWindowSceneDelegate.Window.")

root_view = Path(
    "src/StS2Launcher.Step03.iOS/RootViewController.cs").read_text()

for required in (
    "STEP 03 — CORE STATE MACHINE",
    "CORE LINK: PASS",
    "Run Core Self-Test",
    "CoreSelfTest.Run()",
    "LauncherController",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: missing Step 03 UI/Core marker: {required}")

controller = Path(
    "src/StS2Launcher.Core/LauncherController.cs").read_text()
self_test = Path(
    "src/StS2Launcher.Core/CoreSelfTest.cs").read_text()

for state in (
    "SignedOut",
    "Authenticating",
    "CheckingOwnership",
    "ReadyToInstall",
    "Downloading",
    "ReadyToPlay",
    "Error",
):
    if state not in controller:
        raise SystemExit(f"ERROR: missing Core state: {state}")

if "CORE SELF-TEST PASS" not in self_test or "const int total = 12;" not in self_test:
    raise SystemExit("ERROR: Step 03 Core self-test contract is missing.")

# Core must remain platform-neutral.
core_text = "\n".join(
    p.read_text(errors="ignore")
    for p in Path("src/StS2Launcher.Core").glob("*.cs")
)
for forbidden in ("UIKit", "Foundation", "Security.", "ObjCRuntime"):
    if forbidden in core_text:
        raise SystemExit(
            f"ERROR: Core contains iOS platform dependency: {forbidden}")

print("Step 03 project, scene, Core boundary and self-test validation passed.")
PY

# Still no later subsystems.
if grep -RniE 'Godot|SteamKit2|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 03 contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 03 repository validation passed."
