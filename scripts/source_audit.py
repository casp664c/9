#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
RUNTIME = ROOT / "Assets/Kaninbanker/Scripts"
TRUE_2D_GAME = "Assets/Kaninbanker/Scripts/KaninbankerGame2D.cs"
LEGACY_3D_GAME = "Assets/Kaninbanker/Scripts/KaninbankerGame.cs"
BOOTSTRAP = "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs"
PREFLIGHT = "Assets/Kaninbanker/Editor/KaninbankerPreflightValidator.cs"
ASSET_POSTPROCESSOR = "Assets/Kaninbanker/Editor/Kaninbanker2DAssetPostprocessor.cs"

REQUIRED = [
    TRUE_2D_GAME,
    "Assets/Kaninbanker/Scripts/KaninbankerAudio.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerFeedback.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerHammer2D.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerAtmosphere.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerScreenJuice.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerProfile.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerMusicPanel.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerMayhemPass.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerEventCircuit.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerCareerBook.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerWorldTour.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerSettingsPanel.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerTutorial.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerPerformanceGovernor.cs",
    BOOTSTRAP,
    PREFLIGHT,
    ASSET_POSTPROCESSOR,
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "Docs/TRUE_2D_MIGRATION.md",
    "Docs/2D_ART_DIRECTION.md",
    "Docs/AI/UnityProjectContext.md",
]

REQUIRED_PACKAGES = {
    "com.unity.2d.animation": "10.2.2",
    "com.unity.2d.aseprite": "1.1.10",
    "com.unity.2d.pixel-perfect": "5.0.3",
    "com.unity.2d.psdimporter": "9.1.1",
    "com.unity.2d.spriteshape": "10.0.7",
    "com.unity.2d.tilemap.extras": "4.1.0",
    "com.unity.addressables": "2.7.6",
    "com.unity.modules.androidjni": "1.0.0",
    "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.imgui": "1.0.0",
    "com.unity.modules.physics2d": "1.0.0",
}

FORBIDDEN_MODULES = [
    "com.unity.modules.physics",
    "com.unity.modules.particlesystem",
]

FORBIDDEN_RUNTIME_MARKERS = [
    "GameObject.CreatePrimitive(",
    "Physics.Raycast(",
    "Physics.RaycastAll(",
    "Physics.SphereCast(",
    "Physics.Overlap",
    "MeshRenderer",
    "MeshFilter",
    "SkinnedMeshRenderer",
    "new Mesh(",
    "AddComponent<BoxCollider>",
    "AddComponent<SphereCollider>",
    "AddComponent<CapsuleCollider>",
    "AddComponent<MeshCollider>",
    "AddComponent<Rigidbody>",
    "LightType.Directional",
    "LightType.Point",
    "LightType.Spot",
    "ShadowQuality.",
    "QualitySettings.shadowDistance",
    "QualitySettings.pixelLightCount",
    "QualitySettings.lodBias",
]


def fail(message: str) -> None:
    print(f"ERROR: {message}")
    raise SystemExit(1)


def check_files() -> None:
    missing = [p for p in REQUIRED if not (ROOT / p).is_file()]
    if missing:
        fail("Missing required files: " + ", ".join(missing))
    if (ROOT / LEGACY_3D_GAME).exists():
        fail("legacy 3D runtime has returned: " + LEGACY_3D_GAME)
    print(f"PASS files: {len(REQUIRED)} TRUE-2D production/tool files present; legacy 3D runtime absent")


def check_manifest() -> None:
    path = ROOT / "Packages/manifest.json"
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        fail(f"Packages/manifest.json is invalid JSON: {exc}")

    deps = data.get("dependencies")
    if not isinstance(deps, dict):
        fail("manifest dependencies must be an object")

    for package, expected_version in REQUIRED_PACKAGES.items():
        actual = deps.get(package)
        if actual != expected_version:
            fail(f"required Unity 2D package/version mismatch: {package} expected {expected_version}, got {actual}")

    for module in FORBIDDEN_MODULES:
        if module in deps:
            fail(f"legacy 3D-oriented Unity module returned: {module}")

    if "com.unity.modules.input" in deps:
        fail("forbidden invalid package returned: com.unity.modules.input")

    print(f"PASS manifest: {len(REQUIRED_PACKAGES)} pinned Unity 6 2D/runtime packages present; legacy Physics/ParticleSystem absent")


def strip_strings_and_comments(text: str) -> str:
    out = []
    i = 0
    state = "code"
    while i < len(text):
        c = text[i]
        n = text[i + 1] if i + 1 < len(text) else ""
        if state == "code":
            if c == '"':
                state = "string"
                out.append(" ")
            elif c == "'":
                state = "char"
                out.append(" ")
            elif c == "/" and n == "/":
                state = "line_comment"
                out.extend("  ")
                i += 1
            elif c == "/" and n == "*":
                state = "block_comment"
                out.extend("  ")
                i += 1
            else:
                out.append(c)
        elif state == "string":
            if c == "\\":
                out.append(" ")
                if i + 1 < len(text):
                    out.append(" ")
                    i += 1
            elif c == '"':
                state = "code"
                out.append(" ")
            else:
                out.append("\n" if c == "\n" else " ")
        elif state == "char":
            if c == "\\":
                out.append(" ")
                if i + 1 < len(text):
                    out.append(" ")
                    i += 1
            elif c == "'":
                state = "code"
                out.append(" ")
            else:
                out.append("\n" if c == "\n" else " ")
        elif state == "line_comment":
            if c == "\n":
                state = "code"
                out.append("\n")
            else:
                out.append(" ")
        elif state == "block_comment":
            if c == "*" and n == "/":
                state = "code"
                out.extend("  ")
                i += 1
            else:
                out.append("\n" if c == "\n" else " ")
        i += 1

    if state in {"string", "char", "block_comment"}:
        fail(f"unterminated C# lexical region: {state}")
    return "".join(out)


def check_balanced(path: pathlib.Path) -> None:
    text = strip_strings_and_comments(path.read_text(encoding="utf-8"))
    pairs = {"}": "{", ")": "(", "]": "["}
    opens = set(pairs.values())
    stack = []
    line = 1
    for c in text:
        if c == "\n":
            line += 1
            continue
        if c in opens:
            stack.append((c, line))
        elif c in pairs:
            if not stack or stack[-1][0] != pairs[c]:
                fail(f"{path.relative_to(ROOT)} has unmatched {c!r} near line {line}")
            stack.pop()
    if stack:
        c, open_line = stack[-1]
        fail(f"{path.relative_to(ROOT)} has unclosed {c!r} from line {open_line}")


def check_csharp_structure() -> None:
    files = sorted((ROOT / "Assets/Kaninbanker").rglob("*.cs"))
    if not files:
        fail("no Kaninbanker C# files found")
    for path in files:
        check_balanced(path)
    print(f"PASS C# structure: balanced delimiters in {len(files)} files")


def check_portrait_and_editor_2d() -> None:
    bootstrap = (ROOT / BOOTSTRAP).read_text(encoding="utf-8")
    game = (ROOT / TRUE_2D_GAME).read_text(encoding="utf-8")
    required_snippets = [
        "EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D",
        "PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait",
        "PlayerSettings.allowedAutorotateToLandscapeLeft = false",
        "PlayerSettings.allowedAutorotateToLandscapeRight = false",
        "Screen.orientation = ScreenOrientation.Portrait",
        "Screen.autorotateToLandscapeLeft = false",
        "Screen.autorotateToLandscapeRight = false",
    ]
    combined = bootstrap + "\n" + game
    for snippet in required_snippets:
        if snippet not in combined:
            fail(f"TRUE-2D/portrait invariant missing: {snippet}")
    print("PASS mode: Unity default behavior=2D and Android portrait is locked")


def check_true_2d_runtime() -> None:
    game = (ROOT / TRUE_2D_GAME).read_text(encoding="utf-8")
    required = [
        "gameplayCamera.orthographic = true",
        "SpriteRenderer",
        "CircleCollider2D",
        "Physics2D.OverlapPoint",
    ]
    for marker in required:
        if marker not in game:
            fail(f"TRUE-2D runtime marker missing: {marker}")

    runtime_files = sorted(RUNTIME.rglob("*.cs"))
    for path in runtime_files:
        code = strip_strings_and_comments(path.read_text(encoding="utf-8"))
        for marker in FORBIDDEN_RUNTIME_MARKERS:
            if marker in code:
                fail(f"3D-only runtime API found in {path.relative_to(ROOT)}: {marker}")

    bootstrap = (ROOT / BOOTSTRAP).read_text(encoding="utf-8")
    if "root.AddComponent<global::Kaninbanker.KaninbankerGame2D>()" not in bootstrap:
        fail("generated Main scene is not rooted in KaninbankerGame2D")
    if "RegenerateTrue2DScene();" not in bootstrap:
        fail("cloud bootstrap no longer forcibly regenerates the TRUE-2D Main scene")

    post = (ROOT / ASSET_POSTPROCESSOR).read_text(encoding="utf-8")
    for marker in ["TextureImporterType.Sprite", "mipmapEnabled = false", "TextureImporterFormat.ASTC_6x6"]:
        if marker not in post:
            fail(f"2D asset import policy marker missing: {marker}")

    print(f"PASS TRUE 2D: {len(runtime_files)} runtime C# files scanned; sprite import policy active; no forbidden 3D APIs")


def check_version() -> None:
    bootstrap = (ROOT / BOOTSTRAP).read_text(encoding="utf-8")
    if 'PlayerSettings.bundleVersion = "0.10.0"' not in bootstrap:
        fail("expected Android bundleVersion 0.10.0 not found")
    if "PlayerSettings.Android.bundleVersionCode = 10" not in bootstrap:
        fail("expected Android versionCode 10 not found")
    print("PASS version: 0.10.0 / versionCode 10")


def check_docs() -> None:
    readme = (ROOT / "README.md").read_text(encoding="utf-8")
    context = (ROOT / "Docs/AI/UnityProjectContext.md").read_text(encoding="utf-8")
    for marker in ["0.10.0", "TRUE 2D", "KaninbankerGame2D"]:
        if marker not in readme:
            fail(f"README missing current 2D marker: {marker}")
        if marker not in context:
            fail(f"UnityProjectContext missing current 2D marker: {marker}")
    print("PASS docs: README and AI Unity context describe the current TRUE-2D build")


def main() -> int:
    print("Kaninbanker FULL TRUE-2D + UNITY 2D TOOLSET source audit")
    check_files()
    check_manifest()
    check_csharp_structure()
    check_portrait_and_editor_2d()
    check_true_2d_runtime()
    check_version()
    check_docs()
    print("SOURCE AUDIT PASS — FULL APP TRUE 2D + OFFICIAL UNITY 2D TOOLSET")
    return 0


if __name__ == "__main__":
    sys.exit(main())