#!/usr/bin/env python3
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
IMPORTER = ROOT / "Assets/Kaninbanker/Editor/KaninbankerOfficialUnitySamples.cs"
VALIDATOR = ROOT / "Assets/Kaninbanker/Editor/KaninbankerOfficialUnitySamplesValidator.cs"
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


for path in [IMPORTER, VALIDATOR, BOOTSTRAP, MANIFEST, DOCS]:
    if not path.is_file():
        fail(f"missing official Unity sample pipeline file: {path.relative_to(ROOT)}")

importer = IMPORTER.read_text(encoding="utf-8")
bootstrap = BOOTSTRAP.read_text(encoding="utf-8")
manifest = MANIFEST.read_text(encoding="utf-8")

for marker in [
    "UnityEditor.PackageManager.UI",
    "Sample.FindByPackage",
    "Sample.ImportOptions.OverridePreviousImports",
    "sample.isImported",
    "sample.interactiveImport",
]:
    if marker not in importer:
        fail(f"official Unity Package Manager Sample API marker missing: {marker}")

for package in REQUIRED_PACKAGES:
    if package not in importer:
        fail(f"official Unity sample source package missing from importer: {package}")
    if package not in manifest:
        fail(f"official Unity sample source package missing from manifest: {package}")

if "KaninbankerOfficialUnitySamples.ImportAllNonInteractiveSamples();" not in bootstrap:
    fail("Cloud Bootstrap no longer imports official Unity samples before catalog build")

sample_call = bootstrap.index("KaninbankerOfficialUnitySamples.ImportAllNonInteractiveSamples();")
catalog_call = bootstrap.index("Kaninbanker2DAssetCatalogBuilder.BuildCatalog();")
if sample_call > catalog_call:
    fail("official Unity samples must import before the asset catalog is generated")

print("PASS: official Unity 2D package samples are wired into cloud build and asset catalog generation")
sys.exit(0)
