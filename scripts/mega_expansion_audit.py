#!/usr/bin/env python3
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
MEGA_HUB = ROOT / "Assets/Kaninbanker/Scripts/KaninbankerMegaHub.cs"
BOSS_PHASES = ROOT / "Assets/Kaninbanker/Scripts/KaninbankerBossPhases2D.cs"
VALIDATOR = ROOT / "Assets/Kaninbanker/Editor/KaninbankerMegaExpansionValidator.cs"


def fail(message: str) -> None:
    print("ERROR:", message)
    raise SystemExit(1)


for path in (MEGA_HUB, BOSS_PHASES, VALIDATOR):
    if not path.is_file():
        fail(f"missing mega expansion file: {path.relative_to(ROOT)}")

mega = MEGA_HUB.read_text(encoding="utf-8")
boss = BOSS_PHASES.read_text(encoding="utf-8")
validator = VALIDATOR.read_text(encoding="utf-8")

for marker in [
    "QuestClaimKey",
    "CoinUpgradeKey",
    "XpUpgradeKey",
    "QuestUpgradeKey",
    "VaultUpgradeKey",
    "CodexNames",
    "Screen.safeArea",
    "RuntimeInitializeOnLoadMethod",
]:
    if marker not in mega:
        fail(f"Mega Hub marker missing: {marker}")

for marker in [
    "RabbitKind2D.Boss",
    "Kaninbanker2DArt.Circle",
    "SpriteRenderer",
    "ReducedFxKey",
    "MaxHealth",
    "RuntimeInitializeOnLoadMethod",
]:
    if marker not in boss:
        fail(f"Boss phase marker missing: {marker}")

for marker in ["KaninbankerMegaHub.cs", "KaninbankerBossPhases2D.cs", "BuildFailedException"]:
    if marker not in validator:
        fail(f"Mega expansion validator marker missing: {marker}")

for path, text in ((MEGA_HUB, mega), (BOSS_PHASES, boss)):
    for forbidden in [
        "GameObject.CreatePrimitive(",
        "Physics.Raycast(",
        "MeshRenderer",
        "MeshFilter",
        "SkinnedMeshRenderer",
        "AddComponent<Rigidbody>",
        "LightType.Directional",
        "LightType.Point",
        "LightType.Spot",
        "orthographic = false",
    ]:
        if forbidden in text:
            fail(f"forbidden 3D marker in {path.relative_to(ROOT)}: {forbidden}")

print("PASS: Mega Hub + daily quests + upgrades + codex + TRUE-2D boss phases are source-audited")
sys.exit(0)
