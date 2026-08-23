# Kaninbanker

Android-first Unity game built for a **phone-only + cloud-build workflow**.

## Current prototype

- 3x3 rabbit holes
- Procedurally generated 3D rabbits with eyes, ears, nose and pop/wobble animation
- Tap/click to hit the visible rabbit
- 30-second rounds
- Score, high score, combo multiplier, hit/miss stats and accuracy
- Difficulty ramps up during each round
- Android vibration on successful hits
- Pooled hit/miss particle bursts
- Camera shake on strong hits/combos
- Runtime-created camera, light, ground and targets
- Procedural sound system with no external audio files required
  - bonk/hit sound
  - combo stinger
  - miss sound
  - rabbit pop sound
  - round-start sound
  - game-over sound
  - UI click sound
  - looping lightweight chiptune-style background music
  - persistent sound on/off toggle
- Headless/editor bootstrap that creates the build scene automatically
- Android package id: `com.casp664c.kaninbanker`

## Unity Cloud / Build Automation

This repository is designed so Unity Build Automation can be the first real Unity Editor that opens and compiles it.

Recommended build configuration:

- Branch: `build/kaninbanker-unity-cloud` while testing, then `main` after merge
- Platform: Android
- Unity version: `6000.0.60f1`
- Project subfolder: leave empty
- Build output for device testing: APK
- Release output for Google Play: AAB + release signing

The editor bootstrap at `Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs` configures Android player settings and creates `Assets/Kaninbanker/Scenes/Main.unity` plus Build Settings when Unity imports the project.

If a custom build method is needed, use:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

## Audio implementation

`Assets/Kaninbanker/Scripts/KaninbankerAudio.cs` synthesizes the prototype music and effects once at runtime with `AudioClip.Create`. This keeps the first cloud-build prototype self-contained and avoids licensing/import problems while still producing real sound on-device. Imported WAV/OGG assets can replace individual clips later without changing the gameplay rules.

## Visual feedback

`Assets/Kaninbanker/Scripts/KaninbankerFeedback.cs` owns a small reusable Particle System pool and camera shake. Effects are presentation-only and do not own scoring or round state.

## Important validation note

The source has been created and reviewed through GitHub/Unity Essentials, but it has **not yet been compiled by a real Unity Editor**. The first Unity Build Automation run is therefore the first compilation/build validation step. Build logs must be used to fix any environment/version-specific errors before calling the APK ready.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, keystores or passwords to this repository.
