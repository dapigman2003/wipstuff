#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import os
import plistlib
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FAILURES: list[str] = []
CHECKS: list[str] = []


def ok(message: str) -> None:
    CHECKS.append(message)
    print(f"PASS: {message}")


def fail(message: str) -> None:
    FAILURES.append(message)
    print(f"FAIL: {message}")


def require(condition: bool, message: str) -> None:
    ok(message) if condition else fail(message)


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path: str) -> str:
    return (ROOT / path).read_text()


print("StS2 Launcher — Step 22.3 Foundation Consolidation static validation")
print(f"Root: {ROOT}")

# Parse project/plist before textual assertions.
project_path = ROOT / "src/StS2Launcher.Step05.iOS/StS2Launcher.Step05.iOS.csproj"
plist_path = ROOT / "src/StS2Launcher.Step05.iOS/Info.plist"
try:
    project_root = ET.parse(project_path).getroot()
    ok("iOS csproj parses as XML")
except Exception as ex:
    fail(f"iOS csproj XML parse failed: {ex}")
    project_root = None
try:
    with plist_path.open("rb") as stream:
        plist = plistlib.load(stream)
    ok("Info.plist parses")
except Exception as ex:
    fail(f"Info.plist parse failed: {ex}")
    plist = {}

project_text = project_path.read_text()
require("<ApplicationVersion>61</ApplicationVersion>" in project_text, "build version is 61")
require("<ApplicationDisplayVersion>0.0.61</ApplicationDisplayVersion>" in project_text, "display version is 0.0.61")
require(plist.get("CFBundleVersion") == "61", "Info.plist build version is 61")
require(plist.get("CFBundleShortVersionString") == "0.0.61", "Info.plist display version is 0.0.61")
require(plist.get("UIFileSharingEnabled") is True, "iOS Files sharing remains enabled")
require(plist.get("LSSupportsOpeningDocumentsInPlace") is True, "open-in-place Documents access remains enabled")
require("<TrimMode>full</TrimMode>" in project_text, "full trimming policy retained")
require("<MtouchInterpreter>-all</MtouchInterpreter>" in project_text, "Step 20 interpreter policy retained")
require("'$(UseInterpreter)' == 'true'" in project_text, "build guard still rejects broad UseInterpreter=true")
require("'$(PublishAot)' == 'true'" in project_text, "build guard still rejects NativeAOT")

# The physically proven Step 22.2 Core implementation is protected byte-for-byte.
manifest = ROOT / "tools/validation/protected-step22.2-core.sha256"
expected: dict[str, str] = {}
for line in manifest.read_text().splitlines():
    if not line.strip():
        continue
    digest, name = line.split("  ", 1)
    expected[name] = digest
current_by_name = {p.name: p for p in (ROOT / "src/StS2Launcher.Core").rglob("*.cs")}
missing = sorted(set(expected) - set(current_by_name))
changed = sorted(name for name, digest in expected.items() if name in current_by_name and sha256(current_by_name[name]) != digest)
require(not missing, f"all {len(expected)} physically proven Step 22.2 Core source files are retained")
if missing:
    print("  missing: " + ", ".join(missing))
require(not changed, "physically proven Step 22.2 Core behavior files are byte-for-byte unchanged")
if changed:
    print("  changed: " + ", ".join(changed))
require((ROOT / "src/StS2Launcher.Core/Diagnostics/DeviceTestReportWriter.cs").is_file(), "new report writer is isolated as additive Core infrastructure")

platform_manifest = ROOT / "tools/validation/protected-step22.2-platform-native.sha256"
platform_changed = []
platform_missing = []
for line in platform_manifest.read_text().splitlines():
    if not line.strip():
        continue
    digest, relative = line.split("  ", 1)
    path = ROOT / relative
    if not path.is_file():
        platform_missing.append(relative)
    elif sha256(path) != digest:
        platform_changed.append(relative)
require(not platform_missing, "physically proven iOS platform/native Step 22.2 files are retained")
require(not platform_changed, "physically proven iOS platform/native behavior is byte-for-byte unchanged")
if platform_missing:
    print("  missing platform/native: " + ", ".join(platform_missing))
if platform_changed:
    print("  changed platform/native: " + ", ".join(platform_changed))

# Exact direct root frontier: 3 legacy serialization roots + 22 Step 22 roots.
all_roots = re.findall(r'<TrimmerRootAssembly Include="([^"]+)"\s*/>', project_text)
step22_roots = [
    "netstandard", "System.Data.Common", "System.Diagnostics.Contracts", "System.Diagnostics.StackTrace",
    "System.Diagnostics.TraceSource", "System.Diagnostics.Tracing", "System.IO.FileSystem.DriveInfo",
    "System.IO.MemoryMappedFiles", "System.Net.Ping", "System.Net.Quic", "System.Numerics.Vectors",
    "System.Reflection.Metadata", "System.Runtime.CompilerServices.Unsafe", "System.Runtime.Loader",
    "System.Runtime.Serialization.Json", "System.Runtime.Serialization.Primitives", "System.Runtime.Serialization.Xml",
    "System.Threading.Tasks.Parallel", "System.Threading.ThreadPool", "System.Xml.XDocument",
    "System.Xml.XmlSerializer", "System.Xml.XPath",
]
require(set(step22_roots).issubset(set(all_roots)), "all 22 physically proven Step 22 direct host roots remain rooted")
require(len([x for x in all_roots if x in step22_roots]) == 22, "Step 22 direct root set contains exactly 22 unique roots")
require({"SteamKit2", "protobuf-net", "protobuf-net.Core"}.issubset(set(all_roots)), "Step 05 reflection roots remain protected")

# File structure consolidation.
core_dirs = {p.name for p in (ROOT / "src/StS2Launcher.Core").iterdir() if p.is_dir()}
require({"Foundation", "Steam", "Compatibility", "Godot", "Runtime", "Diagnostics"}.issubset(core_dirs), "Core source is organized by subsystem")
require(list((ROOT / "src/StS2Launcher.Core").glob("*.cs")) == [], "Core root contains no loose C# implementation files")
ui_files = sorted((ROOT / "src/StS2Launcher.Step05.iOS/UI").glob("RootViewController*.cs"))
require(len(ui_files) >= 8, "RootViewController is split into focused partial files")
large_ui = [(p.name, p.stat().st_size) for p in ui_files if p.stat().st_size > 50_000]
require(not large_ui, "no RootViewController partial exceeds 50 KB")
if large_ui:
    print("  oversized: " + ", ".join(f"{n}={s}" for n, s in large_ui))
require("public sealed partial class RootViewController" in read("src/StS2Launcher.Step05.iOS/UI/RootViewController.cs"), "RootViewController uses sealed partial structure")

# Device report coverage and safety.
reports_source = read("src/StS2Launcher.Step05.iOS/UI/RootViewController.Reports.cs")
writer_source = read("src/StS2Launcher.Core/Diagnostics/DeviceTestReportWriter.cs")
require("File.Move(temporary, destination, overwrite: true)" in writer_source, "device reports use atomic temporary-file replacement")
require("Path.GetFileName(trimmed)" in writer_source and ".txt" in writer_source, "device report file names are constrained to local .txt files")
required_report_files = {
    "Foundation-5of5.txt",
    "Step12-ManagedInstall.txt",
    "Step13-OfflineReady.txt",
    "Step14-CompatibilityInventory.txt",
    "Step15-GodotFoundation.txt",
    "Step16-ManagedPreparation.txt",
    "Step17-CompatibilityCallSites.txt",
    "Step18-RealAssemblyRewrite.txt",
    "Step19-ExpressionInterpreter.txt",
    "Step20-DynamicManagedExecution.txt",
    "Step21-RuntimeFrameworkBinding.txt",
    "Step22-HostBindingFrontier.txt",
    "TestSetup-Repair.txt",
    "TestSetup-Update.txt",
    "TestSetup-DownloadCacheClear.txt",
    "TestSetup-FreshDownload.txt",
}
ui_text = "\n".join(p.read_text() for p in ui_files)
missing_reports = sorted(name for name in required_report_files if name not in ui_text)
require(not missing_reports, "all current on-device verification/test paths have deterministic text-report outputs")
if missing_reports:
    print("  missing reports: " + ", ".join(missing_reports))
require("Steam password" in writer_source and "refresh tokens" in writer_source, "report schema explicitly documents secret exclusions")
require("_passwordField" not in reports_source and "_usernameField" not in reports_source, "shared report writer never reads credential UI fields")

# Unit-test consolidation helper.
require((ROOT / "tests/StS2Launcher.Core.Tests/TestSupport/TempTestDirectory.cs").is_file(), "unit tests use shared temporary-directory helper")
all_test_text = "\n".join(p.read_text() for p in (ROOT / "tests/StS2Launcher.Core.Tests").rglob("*.cs"))
require("private sealed class TemporaryDirectory" not in all_test_text, "duplicated per-test TemporaryDirectory helpers are removed")
require((ROOT / "tests/StS2Launcher.Core.Tests/Runtime/DeviceTestReportWriterTests.cs").is_file(), "device report writer has host unit tests")

# Active tooling must be small and every test/validation produces plain-text output.
active_scripts = {p.name for p in (ROOT / "scripts").glob("*.sh")}
expected_active = {"build-godot-step15.sh", "preflight-godot-link-step15.sh", "build-ios.sh", "test.sh", "validate.sh", "verify-ipa.sh", "codemagic.sh"}
require(active_scripts == expected_active, "active scripts are consolidated to seven current entry points")
require((ROOT / "history/scripts/steps").is_dir(), "historical step scripts are separated from active tooling")
require(len(list((ROOT / "history/scripts/steps").glob("*.sh"))) >= 60, "historical step scripts are retained for archaeology")
require((ROOT / "history/docs/steps").is_dir(), "historical step documentation is separated from current docs")
for script_name, report_marker in [
    ("test.sh", "artifacts/reports/host-unit-tests.txt"),
    ("validate.sh", "artifacts/reports/static-validation.txt"),
    ("verify-ipa.sh", "artifacts/reports/ipa-verification.txt"),
    ("preflight-godot-link-step15.sh", "artifacts/reports/godot-native-preflight.txt"),
    ("codemagic.sh", "artifacts/reports/build-summary.txt"),
]:
    path = ROOT / "scripts" / script_name
    require(path.is_file(), f"active {script_name} exists")
    if path.is_file():
        require(report_marker in path.read_text(), f"{script_name} emits a shareable text report")

# Codemagic is reduced to one current workflow.
codemagic = read("codemagic.yaml")
require("ios-step-22-3:" in codemagic, "Codemagic exposes Step 22.3 workflow")
workflow_count = len(re.findall(r'^  ios-[^:]+:', codemagic, re.M))
require(workflow_count == 1, "Codemagic contains one active launcher workflow")
require("scripts/codemagic.sh" in codemagic, "Codemagic calls the consolidated build entry point")

# No game/proprietary payload or obvious secrets in source archive.
forbidden_names = re.compile(r'(^|/)(sts2\.dll|SlayTheSpire2\.app)(/|$)|fmod|spine_godot', re.I)
forbidden_files = [str(p.relative_to(ROOT)) for p in ROOT.rglob("*") if p.is_file() and forbidden_names.search(str(p.relative_to(ROOT)))]
require(not forbidden_files, "source tree contains no StS2/proprietary runtime payload")
if forbidden_files:
    print("  forbidden files: " + ", ".join(forbidden_files[:20]))
secret_patterns = [re.compile(x, re.I) for x in [r'password\s*=\s*"[^"\n]+"', r'refresh[_ -]?token\s*=\s*"[^"\n]+"', r'codesignkey\s*=\s*"[^"\n]+"']]
secret_hits=[]
for p in ROOT.rglob("*"):
    if not p.is_file() or p.suffix.lower() in {".png", ".jpg", ".zip", ".a", ".dll"}:
        continue
    try: t=p.read_text(errors="ignore")
    except Exception: continue
    if any(rx.search(t) for rx in secret_patterns): secret_hits.append(str(p.relative_to(ROOT)))
require(not secret_hits, "source tree contains no obvious embedded credential/signing secrets")
if secret_hits:
    print("  secret-pattern hits: " + ", ".join(secret_hits))

# Fixture isolation: external IL fixtures remain post-publish data, never iOS project inputs.
require("StS2Launcher.Step20.DynamicFixture" not in project_text and "StS2Launcher.Step20.DependencyFixture" not in project_text and "StS2Launcher.Step20.RootFixture" not in project_text, "Step 20 dynamic fixtures remain absent from iOS build inputs")

# Documentation baseline.
for doc in ["README.md", "docs/CURRENT-STATUS.md", "docs/TESTING.md", "docs/ARCHITECTURE.md", "docs/REPORTS.md", "docs/RELEASE-CHECKLIST.md", "docs/STEP-22.3-FOUNDATION-CONSOLIDATION.md"]:
    require((ROOT / doc).is_file(), f"current documentation exists: {doc}")

print()
if FAILURES:
    print(f"VALIDATION FAILED: {len(FAILURES)} failure(s), {len(CHECKS)} passes")
    for item in FAILURES:
        print(f"  - {item}")
    sys.exit(1)
print(f"VALIDATION PASS: {len(CHECKS)} checks")
