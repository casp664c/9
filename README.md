# Kaninbanker: TRUE 2D Portrait Mayhem

Android-first Unity arcade game built for a **phone-only + Unity Build Automation workflow**.

## Current production direction — 0.10.0

Kaninbanker is now a **TRUE 2D, portrait-only 9:16 game**. This is not a perspective camera pretending to be 2D.

Hard invariants:

- Android portrait only, TikTok/Reels-style tall-phone composition.
- Unity Editor Default Behavior Mode is forced to `Mode2D` before every cloud build.
- The generated Main scene always attaches `KaninbankerGame2D`.
- The gameplay camera is orthographic.
- World visuals use `SpriteRenderer` on the XY plane.
- Rabbits and holes use flat 2D sprite composition.
- Touch targeting uses `CircleCollider2D` + `Physics2D.OverlapPoint`.
- Feedback and atmosphere use flat sprites instead of meshes, lights or ParticleSystem.
- Legacy `KaninbankerGame.cs` has been deleted.
- Unity 3D Physics and ParticleSystem modules are removed from `Packages/manifest.json`.
- CI and Unity preflight reject 3D runtime APIs across the entire Kaninbanker script folder.

Android package: `com.casp664c.kaninbanker`

Current version: `0.10.0` / Android versionCode `10`

## Core gameplay

- Tall 3x5 arena with 15 rabbit holes
- Classic, Turbo, Marathon and Boss Rush
- Normal, fast, gold, armored, bomb and boss rabbits
- Dynamic difficulty ramp
- Combo multiplier
- Per-round missions
- Persistent high scores per mode
- Large touch targets designed for one-handed portrait play
- 2D sprite burst feedback plus orthographic camera shake

## Powerups

- Slow-Mo
- Double Score
- Shield
- Rabbit Frenzy

## Arenas

- Mega Eng
- Neon Nat
- Sukkerland
- Lava Pit

The current cloud-safe prototype builds all arena/rabbit graphics from runtime-generated **2D sprites**, so it can compile without imported production art. Imported sprite sheets and Sprite Atlases can replace the procedural art later without changing the gameplay architecture.

## Long-term systems

- `KaninbankerProfile.cs`: coins, XP, levels, ranks, trophies and lifetime stats
- `KaninbankerMayhemPass.cs`: 50-level pass, daily missions, tokens and login streaks
- `KaninbankerEventCircuit.cs`: rotating offline event circuit, event points, chests and medals
- `KaninbankerWorldTour.cs`: 60-stage campaign across 12 worlds
- `KaninbankerCareerBook.cs`: milestones, collection unlocks and records
- `KaninbankerTutorial.cs`: first-run portrait tutorial

All gameplay-facing systems resolve `KaninbankerGame2D`; no legacy 3D game controller is part of the app.

## Audio and music

`KaninbankerAudio.cs` synthesizes game music and SFX at runtime using non-spatial AudioSources (`spatialBlend = 0`).

`KaninbankerExternalMusic.cs` and `KaninbankerMusicPanel.cs` open the official YouTube Music experience as a companion flow. The game does not download, extract or proxy YouTube audio.

## TRUE-2D performance policy

`KaninbankerPerformanceGovernor.cs` targets selectable 30/60 FPS without any 3D lighting/shadow/LOD controls. Its quality tiers only change sprite-friendly sampling settings such as antialiasing and anisotropic filtering.

`KaninbankerSettingsPanel.cs` provides:

- 30 FPS
- 60 FPS
- Reduced FX
- sound controls

The goal is stable frame pacing on Android while preserving readable 2D sprites.

## 2D art pipeline

Production art must follow a sprite-first pipeline:

1. lock one approved rabbit/hammer/FX seed frame;
2. create full animation strips rather than generating frames independently;
3. preserve transparent backgrounds and stable silhouettes;
4. normalize every strip to consistent frame size and bottom-center anchors;
5. pack approved production sprites into Sprite Atlases where appropriate;
6. inspect the animation in-engine on a 9:16 Android device before shipping.

See `Docs/TRUE_2D_MIGRATION.md` and `Docs/2D_ART_DIRECTION.md`.

## Cloud Build reliability

Recommended Unity Build Automation configuration:

- Branch: `build/kaninbanker-unity-live`
- Platform: Android
- Unity version: `6000.0.60f1`
- Project subfolder: empty
- Test artifact: APK
- Store artifact later: AAB + release signing

`KaninbankerCloudBootstrap.cs` performs deterministic cloud setup. Before each build it:

- sets Unity Default Behavior Mode to 2D;
- locks Android to portrait;
- configures package/version/ARM64/IL2CPP;
- regenerates `Assets/Kaninbanker/Scenes/Main.unity` from scratch;
- attaches only `KaninbankerGame2D` to the generated scene root;
- replaces Build Settings with that scene.

This prevents a cached 3D scene from an older Unity Cloud workspace from being reused.

`KaninbankerPreflightValidator.cs` then refuses the build if the project contains legacy 3D gameplay files/modules or forbidden 3D runtime APIs.

## Automated source audit

GitHub Actions runs `scripts/source_audit.py` on pushes and pull requests to `build/kaninbanker-unity-live`.

The audit checks:

- required TRUE-2D files
- Physics2D present
- legacy Physics/ParticleSystem modules absent
- no old `KaninbankerGame.cs`
- balanced C# structure
- Unity 2D mode + portrait lock
- orthographic/SpriteRenderer/Collider2D/Physics2D markers
- every runtime C# file for forbidden 3D APIs
- version `0.10.0` / versionCode `10`
- documentation consistency

## Validation rule

A commit is not device-ready until a **new Unity Build Automation run** compiles it successfully.

After a large rewrite:

1. Start a new Unity build; do not replay an old build revision.
2. Confirm the log checked out the newest commit.
3. Require `PREFLIGHT PASS: FULL APP TRUE 2D ONLY`.
4. Require Android build `SUCCESS`.
5. Install the resulting APK on a real portrait Android phone.
6. Verify flat front-facing 2D visuals, safe area, touch targets, frame pacing, audio, tutorial, World Tour and a complete round.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, release keystores or passwords.
