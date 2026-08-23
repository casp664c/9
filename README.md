# Kaninbanker: Portrait Mayhem

Android-first Unity arcade game built for a **phone-only + Unity Cloud Build workflow**.

## Current direction

The game is intentionally **portrait-only (9:16 phone-first)** instead of a landscape prototype. Both the Unity build settings and the runtime force portrait orientation, and the HUD is laid out against `Screen.safeArea` so notches and system gesture areas do not eat important controls.

## Expanded game systems

- 15-hole portrait arena arranged as a tall 3x5 playfield
- Runtime-generated 3D arena framing, fences, totems and theme colours
- Four arena themes: Mega Eng, Neon Nat, Sukkerland and Lava Pit
- Four game modes: Classic, Turbo, Marathon and Boss Rush
- Rabbit variants: normal, fast, gold, armored multi-hit, bomb trap and oversized multi-hit boss
- Dynamic difficulty ramp
- Combo multiplier and per-round best combo
- Mission target in every round
- Four one-tap powerups: Slow-Mo, Double Score, Shield and Rabbit Frenzy
- Persistent progression: coins, XP, levels, ranks, trophies, total rounds, total hits and best combo
- Persistent high score per game mode
- Results screen with reward summary
- Portrait lobby and mode picker
- Android vibration on stronger hits
- Pooled hit/miss particle bursts and camera shake

## Mayhem Pass meta-game

`Assets/Kaninbanker/Scripts/KaninbankerMayhemPass.cs` adds a second long-term progression layer without needing a server:

- 50-level Mayhem Pass
- persistent Mayhem XP and tokens
- daily login rewards
- login streaks
- three rotating daily progression goals based on play, accumulated score and best run
- automatic mission rewards
- season rank labels
- portrait-only pass panel that is hidden during active gameplay so it never covers the tap arena

The meta-game is deliberately stored locally with `PlayerPrefs` for the cloud-build prototype. It can later be migrated to a backend without rewriting the core round loop.

## Adaptive Android performance

`Assets/Kaninbanker/Scripts/KaninbankerPerformanceGovernor.cs` samples sustained frame rate and gradually changes expensive rendering settings instead of letting a busy phone fall into a long low-FPS/thermal spiral. The high tier keeps shadows and antialiasing; medium reduces them; low disables expensive shadows and lowers light/LOD cost. The game still targets 60 FPS.

This lets the project grow in content and visual density while remaining usable across a much wider range of Android phones.

## Audio

`KaninbankerAudio.cs` synthesizes the soundtrack and sound effects at runtime with `AudioClip.Create`, keeping the repository self-contained.

Current procedural sound palette includes heavy bonk/hit, combo stinger, miss, rabbit pop, round start, game over, UI click, powerup arpeggio, bomb impact, boss impact, reward fanfare and a denser looping arcade soundtrack.

The internal soundtrack and game SFX use separate `AudioSource` objects. This allows the YouTube Music companion mode to disable only the internal soundtrack while keeping game SFX active.

## YouTube Music companion mode

`KaninbankerExternalMusic.cs` and `KaninbankerMusicPanel.cs` open the official YouTube Music experience rather than downloading or extracting YouTube audio. The player can start a track there and return to Kaninbanker. Unity is configured with `PlayerSettings.muteOtherAudioSources = false` so compatible external audio can continue alongside game effects when Android/YouTube Music permits it.

See `Docs/YOUTUBE_MUSIC.md` for details.

## Build reliability

Unity Cloud Build remains the validation environment.

Recommended configuration:

- Branch: `build/kaninbanker-unity-live`
- Platform: Android
- Unity version: `6000.0.60f1`
- Project subfolder: leave empty
- Device testing: APK
- Google Play release later: AAB + release signing

`Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs` automatically locks Android to portrait, disables landscape autorotation, configures package id `com.casp664c.kaninbanker`, selects ARM64 + IL2CPP, creates `Assets/Kaninbanker/Scenes/Main.unity` when needed, adds the scene to Build Settings and keeps external audio mixing enabled.

`Assets/Kaninbanker/Editor/KaninbankerPreflightValidator.cs` performs a second pre-build check and intentionally fails early with a readable error if portrait orientation, the build scene, core source files, or required Unity modules are missing.

If a custom build method is needed, use:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

## Scaling plan

The project should become large through real systems and content, not artificial APK padding. Next expansion layers are designed to be added independently: more rabbit archetypes, multi-stage bosses, seasonal arenas, cosmetic collections, event modifiers, quests, achievements, additional procedural music sets, richer particles, optional Addressables-backed downloadable content and backend/cloud saves when the core device build is stable.

## Important validation note

A source commit is not considered device-ready until Unity Build Automation has compiled it successfully. After a major gameplay rewrite, start a **new build** from Build History rather than replaying an old revision, then install the resulting APK on a portrait Android phone and verify touch targets, safe-area spacing, frame pacing and audio on-device.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, release keystores or passwords to this repository.
