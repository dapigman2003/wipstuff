#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

IOS_PROJECT="src/StS2Launcher.Step04.iOS/StS2Launcher.Step04.iOS.csproj"
CORE_PROJECT="src/StS2Launcher.Core/StS2Launcher.Core.csproj"

[[ -f "$IOS_PROJECT" ]] || {
  echo "ERROR: missing Step 04 iOS project." >&2
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

ios_project = Path("src/StS2Launcher.Step04.iOS/StS2Launcher.Step04.iOS.csproj")
ios_root = ET.parse(ios_project).getroot()

project_refs = [
    e.attrib.get("Include", "")
    for e in ios_root.iter("ProjectReference")
]
expected = ["../StS2Launcher.Core/StS2Launcher.Core.csproj"]
if project_refs != expected:
    raise SystemExit(
        f"ERROR: expected exactly one Core reference; got {project_refs}")

for forbidden in ("PackageReference", "NativeReference"):
    if any(True for _ in ios_root.iter(forbidden)):
        raise SystemExit(f"ERROR: Step 04 must not contain {forbidden}.")

core_root = ET.parse(
    "src/StS2Launcher.Core/StS2Launcher.Core.csproj").getroot()
for forbidden in ("PackageReference", "ProjectReference", "NativeReference"):
    if any(True for _ in core_root.iter(forbidden)):
        raise SystemExit(f"ERROR: Core must not contain {forbidden}.")

plist_path = Path("src/StS2Launcher.Step04.iOS/Info.plist")
with plist_path.open("rb") as f:
    plist = plistlib.load(f)

if plist.get("CFBundleIdentifier") != "com.community.sts2launcher":
    raise SystemExit("ERROR: unexpected bundle ID.")

scene = Path("src/StS2Launcher.Step04.iOS/SceneDelegate.cs").read_text()
if "class SceneDelegate : UIWindowSceneDelegate" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate regression.")
if "public override UIWindow? Window" not in scene:
    raise SystemExit("ERROR: UIWindowSceneDelegate.Window regression.")

contract = Path("src/StS2Launcher.Core/ICredentialStore.cs").read_text()
for required in ("void Set", "string? Get", "bool Delete"):
    if required not in contract:
        raise SystemExit(
            f"ERROR: ICredentialStore missing contract member: {required}")

keychain = Path(
    "src/StS2Launcher.Step04.iOS/Platform/KeychainCredentialStore.cs").read_text()
for required in (
    "SecKeyChain.QueryAsData",
    "SecKeyChain.Add",
    "SecKeyChain.Remove",
    "SecKind.GenericPassword",
    "AfterFirstUnlockThisDeviceOnly",
):
    if required not in keychain:
        raise SystemExit(f"ERROR: Keychain adapter missing: {required}")

probe = Path(
    "src/StS2Launcher.Step04.iOS/Platform/KeychainProbe.cs").read_text()
for required in (
    "STEP04-ALPHA",
    "STEP04-BETA",
    "KEYCHAIN ROUND-TRIP PASS",
    "const int total = 6;",
):
    if required not in probe:
        raise SystemExit(f"ERROR: Keychain probe contract missing: {required}")

root_view = Path(
    "src/StS2Launcher.Step04.iOS/RootViewController.cs").read_text()
for required in (
    "STEP 04 — KEYCHAIN PROBE",
    "Run Keychain Round-Trip",
    "Delete Test Secret",
    "PERSISTENCE: PASS — STEP04-BETA found",
    "CORE LINK: PASS",
):
    if required not in root_view:
        raise SystemExit(f"ERROR: Step 04 UI marker missing: {required}")

# Core remains platform-neutral.
core_text = "\n".join(
    p.read_text(errors="ignore")
    for p in Path("src/StS2Launcher.Core").glob("*.cs")
)
for forbidden in ("UIKit", "Foundation", "Security.", "ObjCRuntime"):
    if forbidden in core_text:
        raise SystemExit(
            f"ERROR: Core contains iOS platform dependency: {forbidden}")

print("Step 04 Keychain/Core boundary validation passed.")
PY

# Real Steam/Godot/etc. are still forbidden.
if grep -RniE 'Godot|SteamKit2|Mono\.Cecil|Harmony|NativeGodotHost' \
  src --include='*.cs' --include='*.csproj'; then
  echo "ERROR: Step 04 contains a forbidden later-stage subsystem." >&2
  exit 3
fi

for script in scripts/*.sh; do
  bash -n "$script"
done

echo "Step 04 repository validation passed."
