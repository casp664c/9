# Kaninbanker: Portrait Mayhem

Android-first Unity arcade game built for a **phone-only + Unity Cloud Build workflow**.

## Current direction

The game is now intentionally **portrait-only (9:16 phone-first)** instead of a landscape prototype. Both the Unity build settings and the runtime force portrait orientation, and the HUD is laid out against `Screen.safeArea` so notches and system gesture areas do not eat important controls.

## Expanded game systems

- 15-hole portrait arena arranged as a tall 3x5 playfield
- Runtime-generated 3D arena framing, fences, totems and theme colours
- Four arena themes:
  - Mega Eng
  - Neon Nat
  - Sukkerland
  - Lava Pit
- Four game modes:
  - Classic: 45 seconds
  - Turbo: 30 seconds
  - Marathon: 90 seconds
  - Boss Rush: 60 seconds
- Rabbit variants:
  - normal
  - fast
  - gold
  - armored multi-hit rabbit
  - bomb trap
  - oversized multi-hit boss
- Dynamic difficulty ramp
- Combo multiplier and per-round best combo
- Mission target in every round
- Four one-tap powerups:
  - Slow-Mo
  - Double Score
  - Shield
  - Rabbit Frenzy
- Persistent progression:
  - coins
  - XP
  - levels
  - ranks
  - trophies / achievements
  - total rounds, hits and best combo
- Persistent high score per game mode
- Results screen with reward summary
- Portrait lobby and mode picker
- Android vibration on stronger hits
- Pooled hit/miss particle bursts and camera shake

## Audio

The repository remains self-contained: `KaninbankerAudio.cs` synthesizes the soundtrack and sound effects at runtime with `AudioClip.Create`.

Current procedural sound palette includes:

- heavy bonk / hit
- combo stinger
- miss sound
- rabbit pop
- round start
- game over
- UI click
- powerup arpeggio
- bomb impact
- boss impact
- reward fanfare
- denser looping arcade soundtrack

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

`Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs` automatically:

- locks Android to portrait
- disables landscape autorotation
- configures package id `com.casp664c.kaninbanker`
- selects ARM64 + IL2CPP
- creates `Assets/Kaninbanker/Scenes/Main.unity` when needed
- adds the scene to Build Settings
- keeps external audio mixing enabled

`Assets/Kaninbanker/Editor/KaninbankerPreflightValidator.cs` then performs a second pre-build check and intentionally fails early with a readable error if portrait orientation, the build scene, core source files, or required Unity modules are missing. This is designed to prevent long cloud-build cycles from failing late for avoidable configuration mistakes.

If a custom build method is needed, use:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

## Important validation note

A source commit is not considered device-ready until Unity Build Automation has compiled it successfully. After a major gameplay rewrite, start a **new build** from Build History rather than replaying an old revision, then install the resulting APK on a portrait Android phone and verify touch targets, safe-area spacing, frame pacing and audio on-device.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, release keystores or passwords to this repository.
