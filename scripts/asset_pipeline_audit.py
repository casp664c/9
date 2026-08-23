#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]

FILES = {
    "catalog": ROOT / "Assets/Kaninbanker/Scripts/Kaninbanker2DAssetCatalog.cs",
    "presentation": ROOT / "Assets/Kaninbanker/Scripts/KaninbankerImported2DPresentation.cs",
    "audio": ROOT / "Assets/Kaninbanker/Scripts/KaninbankerAudio.cs",
    "feedback": ROOT / "Assets/Kaninbanker/Scripts/KaninbankerFeedback.cs",
    "hammer": ROOT / "Assets/Kaninbanker/Scripts/KaninbankerHammer2D.cs",
    "builder": ROOT / "Assets/Kaninbanker/Editor/Kaninbanker2DAssetCatalogBuilder.cs",
    "importer": ROOT / "Assets/Kaninbanker/Editor/Kaninbanker2DAssetImporter.cs",
    "postprocessor": ROOT / "Assets/Kaninbanker/Editor/Kaninbanker2DAssetPostprocessor.cs",
    "bootstrap": ROOT / "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs",
    "preflight": ROOT / "Assets/Kaninbanker/Editor/KaninbankerPreflightValidator.cs",
}

PACKAGES = {
    "com.unity.2d.animation": "10.2.2",
    "com.unity.2d.aseprite": "1.1.10",
    "com.unity.2d.pixel-perfect": "5.0.3",
    "com.unity.2d.psdimporter": "9.1.1",
    "com.unity.2d.spriteshape": "10.0.7",
    "com.unity.2d.tilemap.extras": "4.1.0",
    "com.unity.addressables": "2.7.6",
}


def fail(message: str) -> None:
    print("ERROR:", message)
    raise SystemExit(1)


def require(text: str, markers, label: str) -> None:
    for marker in markers:
        if marker not in text:
            fail(f"{label} missing marker: {marker}")


def main() -> int:
    for label, path in FILES.items():
        if not path.is_file():
            fail(f"asset pipeline file missing ({label}): {path.relative_to(ROOT)}")

    manifest_path = ROOT / "Packages/manifest.json"
    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    except Exception as exc:
        fail(f"manifest JSON invalid: {exc}")
    deps = manifest.get("dependencies", {})
    for package, version in PACKAGES.items():
        if deps.get(package) != version:
            fail(f"Unity package mismatch: {package} expected {version}, got {deps.get(package)}")

    catalog = FILES["catalog"].read_text(encoding="utf-8")
    require(catalog, [
        "normalRabbits", "fastRabbits", "goldRabbits", "armoredRabbits",
        "bombRabbits", "bossRabbits", "holes", "backgrounds", "effects",
        "ui", "hammers", "hit", "combo", "miss", "rabbitPop", "music"
    ], "runtime catalog")

    builder = FILES["builder"].read_text(encoding="utf-8")
    require(builder, [
        'AssetDatabase.FindAssets("t:Sprite"',
        'AssetDatabase.FindAssets("t:AudioClip"',
        "catalog.normalRabbits", "catalog.bossRabbits", "catalog.backgrounds",
        "catalog.effects", "catalog.hammers", "catalog.music"
    ], "editor catalog builder")

    bootstrap = FILES["bootstrap"].read_text(encoding="utf-8")
    require(bootstrap, ["Kaninbanker2DAssetCatalogBuilder.BuildCatalog();"], "cloud bootstrap")

    presentation = FILES["presentation"].read_text(encoding="utf-8")
    require(presentation, [
        "Kaninbanker2DAssetCatalog.Load()", "PickBackground", "PickHole",
        "normalRabbits", "fastRabbits", "goldRabbits", "armoredRabbits",
        "bombRabbits", "bossRabbits"
    ], "imported art presentation")

    audio = FILES["audio"].read_text(encoding="utf-8")
    require(audio, [
        "ApplyImportedAudioOverrides", "PickHit", "PickCombo", "PickMiss",
        "PickRabbitPop", "PickRoundStart", "PickGameOver", "PickPower",
        "PickBomb", "PickBoss", "PickReward", "PickMusic"
    ], "audio integration")

    feedback = FILES["feedback"].read_text(encoding="utf-8")
    require(feedback, ["catalog.PickEffect"], "FX integration")

    hammer = FILES["hammer"].read_text(encoding="utf-8")
    require(hammer, ["catalog.PickHammer"], "hammer integration")

    importer = FILES["importer"].read_text(encoding="utf-8")
    require(importer, ["TextureImporterType.Sprite", "AudioCompressionFormat.Vorbis"], "managed asset importer")

    postprocessor = FILES["postprocessor"].read_text(encoding="utf-8")
    require(postprocessor, ["TextureImporterType.Sprite", "TextureImporterFormat.ASTC_6x6"], "Art2D postprocessor")

    preflight = FILES["preflight"].read_text(encoding="utf-8")
    require(preflight, ["ValidateAssetCatalogPipeline", "IMPORTED ASSET CATALOG"], "Unity preflight")

    print("ASSET PIPELINE AUDIT PASS — official Unity 6 2D packages + automatic sprite/audio catalog are wired")
    return 0


if __name__ == "__main__":
    sys.exit(main())
