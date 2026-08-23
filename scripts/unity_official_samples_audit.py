#!/usr/bin/env python3
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
IMPORTER = ROOT / "Assets/Kaninbanker/Editor/KaninbankerOfficialUnitySamples.cs"
VALIDATOR = ROOT / "Assets/Kaninbanker/Editor/KaninbankerOfficialUnitySamplesValidator.cs"
SOURCE_REGISTRY = ROOT / "Assets/Kaninbanker/Editor/KaninbankerUnityOfficialSourceRegistry.cs"
BOOTSTRAP = ROOT / "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs"
MANIFEST = ROOT / "Packages/manifest.json"
DOCS = ROOT / "Docs/OFFICIAL_UNITY_ASSET_SOURCES.md"

REQUIRED_PACKAGES = [
    "com.unity.2d.animation",
    "com.unity.2d.aseprite",
    "com.unity.2d.pixel-perfect",
    "com.unity.2d.psdimporter",
    "com.unity.2d.sprite",
    "com.unity.2d.spriteshape",
    "com.unity.2d.tilemap",
    "com.unity.2d.tilemap.extras",
]


def fail(message: str) -> None:
    print("ERROR:", message)
    raise SystemExit(1)


for path in [IMPORTER, VALIDATOR, SOURCE_REGISTRY, BOOTSTRAP, MANIFEST, DOCS]:
    if not path.is_file():
        fail(f"missing official Unity content pipeline file: {path.relative_to(ROOT)}")

importer = IMPORTER.read_text(encoding="utf-8")
validator = VALIDATOR.read_text(encoding="utf-8")
registry = SOURCE_REGISTRY.read_text(encoding="utf-8")
bootstrap = BOOTSTRAP.read_text(encoding="utf-8")
manifest = MANIFEST.read_text(encoding="utf-8")

for marker in [
    "UnityEditor.PackageManager",
    "PackageInfo.GetAllRegisteredPackages()",
    "package.name.StartsWith(\"com.unity.\"",
    "packageName.StartsWith(\"com.unity.2d.\"",
    "Sample.FindByPackage",
    "Sample.ImportOptions.OverridePreviousImports",
    "sample.isImported",
    "sample.interactiveImport",
]:
    if marker not in importer:
        fail(f"dynamic official Unity discovery/import marker missing: {marker}")

for package in REQUIRED_PACKAGES:
    if package not in manifest:
        fail(f"official Unity 2D source package missing from manifest: {package}")

for marker in [
    "PackageInfo.GetAllRegisteredPackages()",
    "com.unity.2d.",
]:
    if marker not in validator:
        fail(f"Unity build validator does not enforce dynamic official package discovery: {marker}")

for marker in [
    "https://unity.com/features/2d",
    "https://unity.com/campaign/unity-6-resources",
    "https://assetstore.unity.com/publishers/1",
    "ACCOUNT/LICENSE REQUIRED",
]:
    if marker not in registry:
        fail(f"official Unity web source registry marker missing: {marker}")

if "KaninbankerUnityOfficialSourceRegistry.ReportOfficialSources();" not in bootstrap:
    fail("Cloud Bootstrap no longer reports the official Unity web source registry")
if "KaninbankerOfficialUnitySamples.ImportAllNonInteractiveSamples();" not in bootstrap:
    fail("Cloud Bootstrap no longer imports official Unity samples before catalog build")

source_call = bootstrap.index("KaninbankerUnityOfficialSourceRegistry.ReportOfficialSources();")
sample_call = bootstrap.index("KaninbankerOfficialUnitySamples.ImportAllNonInteractiveSamples();")
catalog_call = bootstrap.index("Kaninbanker2DAssetCatalogBuilder.BuildCatalog();")
if source_call > sample_call:
    fail("official Unity source registry must run before sample import")
if sample_call > catalog_call:
    fail("official Unity samples must import before the asset catalog is generated")

print("PASS: Unity-owned web sources are registered; all installed Unity packages are discovered; TRUE-2D-safe com.unity.2d.* samples auto-import before catalog generation")
sys.exit(0)
