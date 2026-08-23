# Kaninbanker: TRUE 2D Portrait Mayhem — Mega Expansion Plan

## Non-negotiables

- Android-first and portrait-only.
- Primary gameplay viewport is tall-phone 9:16, TikTok/Reels style.
- The entire runtime remains TRUE 2D: orthographic camera, SpriteRenderer, Collider2D and Physics2D.
- No gameplay meshes, 3D primitives, perspective camera, 3D Physics, 3D colliders, 3D lights, shadows/LOD systems or model pipeline.
- Center and lower-middle playfield stay clear enough to hit rabbits reliably.
- `Screen.safeArea` is respected for camera cutouts and system gesture regions.
- A larger game must add real systems and content, not meaningless APK padding.
- New systems stay modular so Unity Cloud Build errors can be isolated quickly.
- Every major batch must pass source audit + Unity preflight + Android Cloud Build before being treated as device-ready.

## Current playable pillars

### Core arcade
- 15-hole 3x5 portrait arena.
- Normal, fast, gold, armored, bomb and boss rabbits.
- Combo scoring, dynamic difficulty and missions.
- Classic, Turbo, Marathon and Boss Rush.
- Slow-Mo, Double Score, Shield and Frenzy powers.

### TRUE-2D world presentation
- Flat front-facing 9:16 arena framing.
- Meadow, Night, Candy and Volcano palettes.
- Sprite-based holes, rabbits, arena decorations and atmosphere.
- SpriteRenderer impact bursts and orthographic camera shake.
- Procedural 2D sprites keep the cloud prototype self-contained until production art is approved.

### Long-term progression
- Coins, XP, levels, ranks and trophies.
- Persistent high scores per mode.
- Mayhem Pass with 50 levels.
- Daily missions and login streak.
- Rotating three-day Event Circuit with event points, chests and medals.
- Career Book with lifetime milestones, collection unlocks and record pages.
- 60-stage World Tour across 12 worlds.

### Device layer
- Hard portrait lock in Editor build settings and runtime.
- 30/60 FPS user preference.
- Reduced FX comfort option.
- TRUE-2D performance governor with sampling/frame-pacing controls only.
- Cloud preflight validator for orientation, scenes, scripts and required Unity modules.
- Repository audit scans every Kaninbanker runtime C# file for forbidden 3D APIs.

## Expansion tracks

### Track A — Rabbit roster
Target: 30+ gameplay-distinct 2D rabbit archetypes.

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
- ghost
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

Each archetype must change a player decision, not merely recolor a sprite.

### Track B — Boss system
Target: multi-phase bosses with readable portrait telegraphs and 2D animation states.

Boss structure:
- 2D intro sting
- health bar
- phase thresholds
- animated sprite telegraph
- safe/unsafe targets
- enraged phase
- defeat sprite burst
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

### Track C — World Tour
Target: 12 chapters with multiple stages each and unique 2D visual identities.

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

Each chapter should introduce a mechanic, rabbit family, sprite palette, FX set and boss.

### Track D — Meta game
- 100+ achievements
- 100-level seasonal pass after validation
- daily and weekly mission pools
- event shop
- cosmetic 2D hammer collection
- rabbit collection book
- arena badges
- player titles
- prestige ranks
- career statistics
- limited-time event medals

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
- Chain Lightning rendered as 2D sprite/line FX
- Rabbit Radar
- Score Storm

Power rules:
- visible cooldown/state
- large thumb target
- no accidental overlap with rabbit taps
- strong but short 2D feedback
- Reduced FX variant

### Track F — 2D presentation
- larger sprite impact library
- boss phase sprite FX
- reward confetti sprites and medal bursts
- layered 2D arena ambience
- rare-rabbit spawn stingers
- combo milestone screen effects
- accessibility-safe flash intensity
- reduced-motion/FX mode
- production sprite sheets replacing procedural shapes once visual direction is approved

### Track G — Audio
- layered soundtrack states: lobby, normal, danger, frenzy, boss, results
- rabbit family stingers
- boss vocal/synth motifs
- power activation sounds
- UI reward sounds
- event music themes
- audio variation to avoid repetitive bonk fatigue
- YouTube Music remains a companion flow, not extracted/downloaded YouTube audio

### Track H — 2D content scale and delivery
Once imported art/audio becomes large:
- organize production sprites by characters, worlds, UI, FX and cosmetics
- use Sprite Atlases where appropriate to reduce sprite texture churn/draw overhead
- move optional content to Addressables only after the current baseline is stable
- keep the initial client quick to install
- load/release optional content deliberately
- profile texture memory before increasing sprite resolution
- compress production textures appropriately for Android
- keep raw source PSD/AI files out of runtime builds

## 2D asset pipeline

All production character/FX animation follows this sequence:

1. approve one in-game seed frame;
2. generate/draw the full animation strip in one pass;
3. transparent background only;
4. preserve the same silhouette, facing direction, palette and proportions;
5. normalize all frames to a shared size and bottom-center anchor;
6. preview the strip before import;
7. import as Sprite (2D and UI);
8. validate readability at real Android game scale;
9. pack approved sprites into Sprite Atlases where useful.

No 3D models should be introduced as a shortcut for character art.

## Architecture target

Keep boundaries explicit:

- `KaninbankerGame2D`: current round and core tap gameplay
- `KaninbankerProfile`: permanent player progression
- `KaninbankerMayhemPass`: daily/pass progression
- `KaninbankerEventCircuit`: rotating events
- `KaninbankerCareerBook`: lifetime goals/collection/records
- `KaninbankerWorldTour`: campaign state
- `KaninbankerAudio`: runtime audio layer
- `KaninbankerFeedback`: pooled 2D sprite impact FX
- `KaninbankerAtmosphere`: flat 2D arena ambience
- `KaninbankerScreenJuice`: screen-space 2D reward feedback
- `KaninbankerSettingsPanel`: device/comfort preferences
- `KaninbankerPerformanceGovernor`: 2D frame pacing/sampling management
- Editor bootstrap: deterministic TRUE-2D cloud build setup
- Preflight validator: fail fast on any 3D regression

Permanent progression state must never depend on render objects or sprite filenames.

## Mobile performance budget

The game can look explosive without actually exploding the phone.

Priorities:
- stable frame pacing over maximum visual density
- pooled repeated sprite effects
- capped simultaneous translucent sprites
- controlled overdraw
- texture sizes based on actual phone display usage
- compressed production textures
- Sprite Atlas use where it reduces state/texture churn
- optional content loaded on demand later
- validate thermal behavior during longer Marathon sessions

Default target: 60 FPS on capable phones, selectable 30 FPS comfort/battery mode, with lightweight adaptive sampling fallback.

## UI rule

Persistent gameplay HUD should show only what matters immediately:
- score
- timer
- combo
- mission progress
- four powers
- boss health when relevant

Events, career, pass, music and settings stay collapsed outside active gameplay. The playfield must read as a 2D arcade game first, not a dashboard.

## Validation loop

For every large batch:
1. commit to `build/kaninbanker-unity-live`;
2. require GitHub source audit PASS;
3. start a fresh Unity Build Automation run, never Replay an old revision;
4. confirm the log uses the current commit;
5. require `PREFLIGHT PASS: FULL APP TRUE 2D ONLY`;
6. require Android build SUCCESS;
7. install APK on a real portrait Android phone;
8. visually confirm no perspective/3D presentation returned;
9. test safe area, touch targets, FPS, audio, menus and a full round;
10. fix the first concrete failure before adding another risky dependency.

## Next production batches

1. multi-phase 2D boss state machine
2. larger World Tour stage mechanics
3. shop/equipment layer tied to coin economy
4. additional mechanically distinct rabbit archetypes
5. layered audio state transitions
6. production sprite-sheet + Sprite Atlas manifest
7. Addressables plan for optional art/audio after baseline validation
8. real-device performance/playtest pass
9. replace procedural sprite shapes with approved production 2D art
