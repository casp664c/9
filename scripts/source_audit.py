#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]

REQUIRED = [
    "Assets/Kaninbanker/Scripts/KaninbankerGame.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerAudio.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerFeedback.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerProfile.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerMusicPanel.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerMayhemPass.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerEventCircuit.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerCareerBook.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerWorldTour.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerSettingsPanel.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerTutorial.cs",
    "Assets/Kaninbanker/Scripts/KaninbankerPerformanceGovernor.cs",
    "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs",
    "Assets/Kaninbanker/Editor/KaninbankerPreflightValidator.cs",
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
]

REQUIRED_MODULES = [
    "com.unity.modules.androidjni",
    "com.unity.modules.audio",
    "com.unity.modules.imgui",
    "com.unity.modules.particlesystem",
    "com.unity.modules.physics",
]


def fail(message: str) -> None:
    print(f"ERROR: {message}")
    raise SystemExit(1)


def check_files() -> None:
    missing = [p for p in REQUIRED if not (ROOT / p).is_file()]
    if missing:
        fail("Missing required files: " + ", ".join(missing))
    print(f"PASS files: {len(REQUIRED)} required files present")


def check_manifest() -> None:
    path = ROOT / "Packages/manifest.json"
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        fail(f"Packages/manifest.json is invalid JSON: {exc}")

    deps = data.get("dependencies")
    if not isinstance(deps, dict):
        fail("manifest dependencies must be an object")

    for module in REQUIRED_MODULES:
        if module not in deps:
            fail(f"required Unity module missing: {module}")

    if "com.unity.modules.input" in deps:
        fail("forbidden invalid package returned: com.unity.modules.input")

    print("PASS manifest: required built-in Unity modules present")


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


def check_portrait() -> None:
    bootstrap = (ROOT / "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs").read_text(encoding="utf-8")
    game = (ROOT / "Assets/Kaninbanker/Scripts/KaninbankerGame.cs").read_text(encoding="utf-8")
    required_snippets = [
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
            fail(f"portrait invariant missing: {snippet}")
    print("PASS portrait: editor and runtime landscape paths disabled")


def check_version() -> None:
    bootstrap = (ROOT / "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs").read_text(encoding="utf-8")
    if 'PlayerSettings.bundleVersion = "0.8.0"' not in bootstrap:
        fail("expected Android bundleVersion 0.8.0 not found")
    if "PlayerSettings.Android.bundleVersionCode = 8" not in bootstrap:
        fail("expected Android versionCode 8 not found")
    print("PASS version: 0.8.0 / versionCode 8")


def main() -> int:
    print("Kaninbanker source audit")
    check_files()
    check_manifest()
    check_csharp_structure()
    check_portrait()
    check_version()
    print("SOURCE AUDIT PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
