# Unity Project Context — Kaninbanker TRUE 2D

<!-- unity-onboarding:generated:start -->

## Project summary

Kaninbanker is an Android-first **TRUE 2D portrait Unity game** designed for a phone-only development workflow. Unity Build Automation is the authoritative importer/compiler/build environment.

Current production version: **0.10.0 / Android versionCode 10**.

## Non-negotiable architecture

- Unity engine, not React Native.
- Android is the primary production platform.
- Portrait-only 9:16 composition.
- Unity `EditorBehaviorMode.Mode2D` is forced before cloud build.
- Generated Main scene root is `KaninbankerGame2D`.
- Gameplay camera is orthographic.
- Runtime world presentation uses `SpriteRenderer` only.
- Gameplay collisions/hit tests use `Collider2D` / `Physics2D`.
- No legacy `KaninbankerGame.cs` file.
- No runtime `GameObject.CreatePrimitive`, `Physics.Raycast`, MeshRenderer/MeshFilter, 3D colliders, Rigidbody, 3D LightType, shadow-quality or LOD quality controls.
- `com.unity.modules.physics2d` is required.
- `com.unity.modules.physics` and `com.unity.modules.particlesystem` are forbidden.

## Confirmed environment

- Unity project root: repository root
- Unity version marker: `6000.0.60f1`
- Primary platform: Android
- Application id: `com.casp664c.kaninbanker`
- Android architecture: ARM64
- Scripting backend: IL2CPP
- Minimum API: 26
- Package manifest: minimal Unity built-in modules only
- Input: legacy `UnityEngine.Input` touch/mouse compatibility
- Networking: none required for core game
- Persistence: PlayerPrefs-based local prototype progression

## Directory structure

- `Assets/Kaninbanker/Scripts/` — TRUE-2D runtime gameplay, progression, UI, audio and sprite FX
- `Assets/Kaninbanker/Editor/` — deterministic cloud bootstrap, Android builder and preflight validator
- `ProjectSettings/` — Unity version marker
- `Packages/` — minimal TRUE-2D module manifest
- `Docs/` — build, 2D architecture, art and integration rules
- `scripts/source_audit.py` — repository-level 2D regression gate

## Scene and startup

`Assets/Kaninbanker/Scenes/Main.unity` is generated every build. The generated scene is deliberately overwritten instead of reused, preventing an old cached 3D scene from surviving in Unity Cloud Build.

The generated root attaches only:

`KaninbankerGame2D`

Runtime support systems install themselves through Unity runtime initialization where appropriate.

## Runtime architecture

- `KaninbankerGame2D` — authoritative round state, scoring, combos, 15-hole layout, rabbit spawning, touch hit testing and orthographic sprite scene setup
- `KaninbankerAudio` — non-spatial procedural soundtrack/SFX
- `KaninbankerFeedback` — SpriteRenderer hit bursts + orthographic camera shake
- `KaninbankerAtmosphere` — flat 2D decorative sprites
- `KaninbankerScreenJuice` — safe-area screen-space reward feedback
- `KaninbankerProfile` — persistent progression
- `KaninbankerMayhemPass` — pass/daily/login progression
- `KaninbankerEventCircuit` — rotating events
- `KaninbankerCareerBook` — milestones/collection/records
- `KaninbankerWorldTour` — 60-stage campaign
- `KaninbankerSettingsPanel` — FPS/reduced-FX/audio preferences
- `KaninbankerPerformanceGovernor` — 30/60 FPS and sprite-friendly sampling quality only; no shadows/lights/LOD
- `KaninbankerTutorial` — first-run portrait tutorial
- `KaninbankerExternalMusic` + `KaninbankerMusicPanel` — official YouTube Music companion handoff

## Current gameplay/presentation

- TRUE 2D 3x5 arena with 15 rabbit holes
- front-facing flat sprite composition
- normal, fast, gold, armored, bomb and boss rabbits
- Classic, Turbo, Marathon and Boss Rush
- combos, missions, coins, XP and high scores
- Slow-Mo, x2, Shield and Frenzy powers
- touch/mouse whacking through `Physics2D.OverlapPoint`
- flat 2D atmosphere, sprite impact bursts and orthographic shake
- 60-stage World Tour
- Mayhem Pass, events and career systems
- portrait safe-area UI

## 2D art direction

The prototype currently creates simple sprites procedurally for cloud-build independence. Production art must remain sprite-based.

Preferred pipeline:

- one approved seed frame per character/look
- whole animation strip generation
- transparent backgrounds
- consistent silhouette and proportions
- normalized fixed frame size
- shared bottom-center anchors
- Sprite Atlas packing for production content where appropriate
- 9:16 in-engine preview before approval

Do not introduce meshes/models simply because an art tool can generate them.

## Build behavior

`KaninbankerCloudBootstrap`:

- forces Unity Default Behavior Mode to 2D
- locks Android portrait
- applies `0.10.0` / versionCode 10
- configures ARM64 + IL2CPP + API 26
- regenerates Main.unity every build
- attaches `KaninbankerGame2D`
- sets Build Settings deterministically

`KaninbankerPreflightValidator` rejects:

- non-portrait configuration
- non-2D editor mode
- missing scene/source/module requirements
- old `KaninbankerGame.cs`
- 3D Physics/ParticleSystem modules
- forbidden 3D APIs anywhere in runtime scripts
- wrong app version

GitHub Actions additionally runs `scripts/source_audit.py` for repository-level regression checking.

## YouTube Music integration rule

The project does not extract, download, proxy or secretly background-play YouTube audio. It opens the official YouTube Music experience externally and lets Android/YouTube Music own playback.

## Testing and validation

Source-level audits are useful but not equivalent to Unity compilation. A revision is device-ready only after:

1. fresh Unity Build Automation run on `build/kaninbanker-unity-live`;
2. current commit confirmed in log;
3. TRUE-2D preflight PASS;
4. Android build SUCCESS;
5. APK installed on a real portrait Android device;
6. visual confirmation that gameplay is flat/front-facing 2D rather than perspective 3D.

## Important constraints

- User should not need to own or connect a PC.
- Keep Unity as the implementation engine.
- Do not introduce React Native migration into this Unity game.
- Keep build setup deterministic and code-driven.
- Never commit secrets, tokens, passwords or release keystores.
- Every new gameplay visual must default to sprites/2D APIs.

Last analyzed branch: `build/kaninbanker-unity-live`
Last analyzed date: 2026-08-23

<!-- unity-onboarding:generated:end -->