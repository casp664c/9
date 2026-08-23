# Kaninbanker

Android-first Unity game built for a **phone-only + cloud-build workflow**.

## Current prototype

- 3x3 rabbit holes
- Procedurally generated 3D rabbits
- Tap/click to hit the visible rabbit
- 30-second rounds
- Score counter and restart button
- Runtime-created camera, light, ground and targets
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

The editor bootstrap at `Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs` configures basic Android player settings and creates `Assets/Kaninbanker/Scenes/Main.unity` plus Build Settings when Unity imports the project.

## Important validation note

The source has been created and reviewed through GitHub/Unity Essentials, but it has **not yet been compiled by a real Unity Editor**. The first Unity Build Automation run is therefore the first compilation/build validation step. Build logs must be used to fix any environment/version-specific errors before calling the APK ready.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, keystores or passwords to this repository.
