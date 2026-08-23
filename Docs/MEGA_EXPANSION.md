# Kaninbanker: Portrait Mayhem — Mega Expansion Plan

## Non-negotiables

- Android-first and portrait-only.
- The primary gameplay viewport is tall-phone 9:16, TikTok/Reels style.
- Center and lower-middle playfield stay clear enough to hit rabbits reliably.
- `Screen.safeArea` is respected for camera cutouts and system gesture regions.
- A larger game must add real systems and content, not meaningless APK padding.
- New systems are modular so Unity Cloud Build errors can be isolated quickly.
- Every major batch must pass Cloud Build before being treated as device-ready.

## Current playable pillars

### Core arcade
- 15-hole 3x5 portrait arena.
- Normal, fast, gold, armored, bomb and boss rabbits.
- Combo scoring, dynamic difficulty and missions.
- Classic, Turbo, Marathon and Boss Rush.
- Slow-Mo, Double Score, Shield and Frenzy powers.

### World presentation
- Tall 3D arena framing.
- Meadow, Night, Candy and Volcano palettes.
- Fences, arena totems, lighting, particles and camera impact.
- Procedural rabbits and procedural audio keep the cloud prototype self-contained.

### Long-term progression
- Coins, XP, levels, ranks and trophies.
- Persistent high scores per mode.
- Mayhem Pass with 50 levels.
- Daily missions and login streak.
- Rotating three-day Event Circuit with event points, chests and medals.
- Career Book with lifetime milestones, collection unlocks and record pages.

### Device layer
- Hard portrait lock in Editor build settings and at runtime.
- 30/60 FPS user preference.
- Reduced FX comfort option.
- Adaptive quality governor for shadows, lights, antialiasing and LOD.
- Cloud preflight validator for orientation, scenes, scripts and required Unity modules.

## Expansion tracks

### Track A — Rabbit roster
Target: 30+ gameplay-distinct rabbit archetypes.

Planned families:
- normal
- sprinter
- gold
- armored
- bomb
- boss
- healer
- thief
- decoy
- invisible/ghost
- splitter
- shielded
- combo breaker
- time stealer
- time giver
- multiplier rabbit
- giant rabbit
- mini swarm
- lava rabbit
- ice rabbit
- neon rabbit
- candy rabbit
- crown rabbit
- cursed rabbit
- jackpot rabbit
- mimic rabbit
- teleport rabbit
- berserker
- emperor boss
- seasonal event bosses

Each new archetype must change a player decision, not just recolor the model.

### Track B — Boss system
Target: multi-phase bosses with readable portrait telegraphs.

Boss structure:
- intro sting
- health bar
- phase thresholds
- attack telegraph
- safe/unsafe targets
- enraged phase
- defeat burst
- unique reward table

Planned bosses:
- Mega Kanin
- Bombebaronen
- Guldtyven
- Lava-Kejseren
- Neon-Spøgelset
- Pansret Titan
- Sukker-Monsteret
- Kanin-Kejseren

### Track C — World tour
Target: 12 chapters with multiple stages each.

Example chapters:
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

A chapter should introduce a mechanic, rabbit family and boss.

### Track D — Meta game
- 100+ achievements.
- 100-level seasonal pass after prototype validation.
- Daily and weekly mission pools.
- Event shop.
- Cosmetic hammer collection.
- Rabbit collection book.
- Arena badges.
- Player titles.
- Prestige ranks.
- Career statistics.
- Limited-time event medals.

### Track E — Power system
Current powers: Slow-Mo, x2, Shield, Frenzy.

Planned powers:
- Freeze Field
- Mega Hammer
- Auto Bonk
- Magnet
- Golden Touch
- Extra Time
- Combo Lock
- Bomb Defuser
- Boss Breaker
- Chain Lightning
- Rabbit Radar
- Score Storm

Power rules:
- visible cooldown/state
- large thumb target
- no accidental overlap with rabbit taps
- strong but short feedback

### Track F — Presentation
- Larger impact particle library.
- Boss phase FX.
- Reward confetti and medal bursts.
- Arena ambience.
- Rare-rabbit spawn stingers.
- Combo milestone effects.
- Screen-space flash kept accessibility-safe.
- Optional reduced-motion/FX mode.
- Imported production art can replace procedural shapes after gameplay stabilizes.

### Track G — Audio
- Layered soundtrack states: lobby, normal, danger, frenzy, boss, results.
- Rabbit family stingers.
- Boss vocal/synth motifs.
- Power activation sounds.
- UI reward sounds.
- Event music themes.
- Audio variation to avoid repetitive bonk fatigue.
- YouTube Music remains a companion flow, not extracted/downloaded YouTube audio.

### Track H — Content scale and delivery
Once imported art/audio becomes large:
- Move optional environments and cosmetics to Addressables.
- Use stable asset keys grouped by characters, environments, UI, audio and FX.
- Keep the initial client small enough to install quickly.
- Load/release optional content deliberately.
- Profile memory before increasing texture/model resolution.
- Prefer optimized production assets over raw source files.

## Architecture target

Keep boundaries explicit:

- `KaninbankerGame`: current round and core tap gameplay.
- `KaninbankerProfile`: permanent player progression.
- `KaninbankerMayhemPass`: daily/pass progression.
- `KaninbankerEventCircuit`: rotating events.
- `KaninbankerCareerBook`: lifetime goals/collection/records.
- `KaninbankerAudio`: runtime audio layer.
- `KaninbankerFeedback`: pooled impact FX.
- `KaninbankerSettingsPanel`: device and comfort preferences.
- `KaninbankerPerformanceGovernor`: automatic quality management.
- Editor bootstrap: deterministic cloud build setup.
- Preflight validator: fail fast before expensive Android build work.

Do not move permanent progression state into render objects. Do not make asset filenames the public gameplay API when the project grows into an Addressables catalog.

## Mobile performance budget

The game can look explosive without actually exploding the phone.

Priorities:
- stable frame pacing over maximum visual density
- pooled repeated effects
- capped simultaneous particles
- reasonable shadow distance
- minimal unnecessary transparent overdraw
- texture sizes based on actual screen usage
- compressed production textures
- load optional content on demand
- validate thermal behavior during longer Marathon sessions

Default target: 60 FPS on capable phones, selectable 30 FPS comfort/battery mode, with adaptive quality fallback.

## UI rule

Persistent gameplay HUD should show only what matters immediately:
- score
- timer
- combo
- mission progress
- four powers
- boss health when relevant

Events, career, pass, music and settings stay collapsed outside active gameplay. The playfield must read as a game first, not a dashboard.

## Validation loop

For every large batch:
1. Commit to `build/kaninbanker-unity-live`.
2. Start a fresh Unity Build Automation run; do not replay an old revision.
3. Confirm the log uses the current commit.
4. Require preflight PASS.
5. Require Android build SUCCESS.
6. Install APK on a real portrait Android phone.
7. Test safe area, touch targets, FPS, audio, menus and a full round.
8. Fix the first concrete failure before adding another risky dependency.

## Next production batches after the current build passes

1. Multi-phase boss state machine.
2. World Tour chapter/stage progression.
3. Shop/equipment layer tied to the existing coin economy.
4. More rabbit archetypes with actual mechanical differences.
5. Layered audio state transitions.
6. Production asset manifest and Addressables migration plan.
7. Device playtest/performance pass.
8. Imported art pipeline after visual direction is approved.
