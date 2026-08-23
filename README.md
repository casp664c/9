# Kaninbanker: Portrait Mayhem

Android-first Unity arcade game built for a **phone-only + Unity Cloud Build workflow**.

## Current build direction — 0.8.0

The game is intentionally **portrait-only** for tall Android phones instead of the original landscape prototype. Both the Unity build settings and runtime force portrait orientation. HUD and meta panels use `Screen.safeArea` so camera cutouts and system gesture areas do not cover important controls.

Android package: `com.casp664c.kaninbanker`

Current prototype version: `0.8.0` / Android versionCode `8`

## Core gameplay

- Tall 3x5 arena with 15 rabbit holes
- Classic, Turbo, Marathon and Boss Rush
- Normal, fast, gold, armored multi-hit, bomb and boss rabbits
- Dynamic difficulty ramp
- Combo multiplier
- Per-round missions
- Persistent high scores per mode
- Android vibration on stronger hits
- Large pooled particle feedback and camera impact

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

The runtime builds the arena, lighting, fences, totems, holes and procedural rabbit geometry without requiring imported art for the cloud prototype.

## Permanent progression

`KaninbankerProfile.cs` stores coins, XP, levels, ranks, trophies, total hits, total rounds and best combo.

## Mayhem Pass

`KaninbankerMayhemPass.cs` adds a 50-level pass, Mayhem XP, tokens, daily missions, login rewards and login streak progression.

## Mega Event Circuit

`KaninbankerEventCircuit.cs` adds an offline live-ops style event that rotates every three UTC days.

- eight rotating event themes
- event points
- event rounds
- best event score
- milestone reward chests
- medals
- lifetime event points
- event streak

## World Tour

`KaninbankerWorldTour.cs` adds a **60-stage campaign**:

- 12 worlds
- 5 stages per world
- 1–3 stars per stage
- escalating score targets
- persistent stage unlocks
- persistent total stars

Worlds:

1. Grøn Eng
2. Mørk Skov
3. Neon City
4. Sukkerland
5. Lava Pit
6. Frosthuler
7. Robotfabrik
8. Piratøen
9. Spøgelsesbyen
10. Rummet
11. Kaosdimensionen
12. Kejserens Arena

## Career Book and collection

`KaninbankerCareerBook.cs` adds long-term milestones, hit/round/combo/level/event goals, Mayhem Pass goals, streak goals, 24 collection unlock slots and career records.

## Tutorial

`KaninbankerTutorial.cs` provides a first-run portrait tutorial explaining tapping/combos, rabbit types, bombs, bosses, powerups and progression. It can be reopened outside active gameplay.

## Audio

`KaninbankerAudio.cs` synthesizes the prototype soundtrack and sound effects at runtime with `AudioClip.Create`.

Current palette includes heavy bonk/hit, combo stinger, miss, rabbit pop, round start, game over, UI click, powerup arpeggio, bomb impact, boss impact, reward fanfare and looping arcade music.

## YouTube Music companion mode

`KaninbankerExternalMusic.cs` and `KaninbankerMusicPanel.cs` open the official YouTube Music experience rather than downloading or extracting YouTube audio. Internal music can be disabled while game SFX remain active.

See `Docs/YOUTUBE_MUSIC.md`.

## Device and comfort settings

`KaninbankerSettingsPanel.cs` adds:

- 30 FPS mode
- 60 FPS mode
- reduced visual effects
- sound toggle

`KaninbankerPerformanceGovernor.cs` watches sustained frame rate and dynamically adjusts shadows, shadow distance, pixel lights, antialiasing, LOD bias and anisotropic filtering.

`KaninbankerFeedback.cs` uses a larger pooled FX system while respecting Reduced FX mode.

## Cloud Build reliability

Recommended Unity Build Automation configuration:

- Branch: `build/kaninbanker-unity-live`
- Platform: Android
- Unity version: `6000.0.60f1`
- Project subfolder: empty
- Device testing: APK
- Release later: AAB + release signing

`KaninbankerCloudBootstrap.cs` automatically locks Android to portrait, disables landscape autorotation, configures package ID, applies version `0.8.0` / versionCode `8`, selects ARM64 + IL2CPP, creates `Assets/Kaninbanker/Scenes/Main.unity` if needed, adds the scene to Build Settings and keeps external audio mixing enabled.

`KaninbankerPreflightValidator.cs` intentionally fails early with readable errors if portrait orientation, the generated build scene, required source systems or Unity modules are missing.

Custom build method if required:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

## Permanent expansion plan

See `Docs/MEGA_EXPANSION.md` for the roadmap covering 30+ mechanically different rabbit types, multi-phase bosses, larger World Tour mechanics, shop/equipment, expanded powers, 100+ achievements, seasonal content, production art, Addressables/content streaming, performance budgets and device validation.

## Validation rule

A source commit is not device-ready until Unity Build Automation compiles it successfully.

After a large rewrite:

1. Start a **new** Unity build rather than replaying an old revision.
2. Confirm Unity checked out the current commit.
3. Require the preflight PASS message.
4. Require Android build SUCCESS.
5. Install the resulting APK on a real portrait Android phone.
6. Test safe area, touch targets, frame pacing, audio, tutorial, World Tour and a complete round.

## Security

Never commit GitHub Personal Access Tokens, Unity credentials, release keystores or passwords to this repository.
