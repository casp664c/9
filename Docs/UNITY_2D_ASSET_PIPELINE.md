# Kaninbanker — Unity 6 TRUE 2D asset pipeline

This project is Android-first, portrait-only and TRUE 2D. Imported assets should enrich the game without reintroducing meshes, 3D physics, perspective cameras or 3D lighting.

## Official Unity 6 packages pinned in `Packages/manifest.json`

- `com.unity.2d.animation` 10.2.2
- `com.unity.2d.aseprite` 1.1.10
- `com.unity.2d.pixel-perfect` 5.0.3
- `com.unity.2d.psdimporter` 9.1.1
- `com.unity.2d.spriteshape` 10.0.7
- `com.unity.2d.tilemap.extras` 4.1.0
- `com.unity.addressables` 2.7.6
- built-in Android JNI, Audio, IMGUI, JSON serialization and Physics2D modules

The legacy 3D Physics and ParticleSystem modules are intentionally absent.

## How imported art becomes usable

`Kaninbanker2DAssetCatalogBuilder` scans every usable `Sprite` and `AudioClip` under `Assets/` during Unity cloud bootstrap. It does not copy Asset Store source files. It creates one generated runtime catalog in `Assets/Resources/Kaninbanker/Generated` that references the imported assets in place.

`KaninbankerImported2DPresentation` then prefers imported sprites for:

- normal rabbits
- fast rabbits
- gold rabbits
- armored rabbits
- bomb rabbits
- bosses
- rabbit holes
- backgrounds

`KaninbankerHammer2D` prefers imported hammer/mallet art.

`KaninbankerFeedback` prefers imported 2D hit/impact/explosion/confetti sprites.

`KaninbankerAudio` prefers imported clips for hit, combo, miss, rabbit spawn, round start, game over, UI, power, bomb, boss, reward and music.

If a category has no matching imported asset, the safe procedural TRUE-2D fallback remains active. Special rabbit categories do not silently fall back to a normal imported rabbit, because bombs/bosses must remain visually readable.

## Filename/path classification

The builder uses case-insensitive path/name keywords. Useful names include:

- rabbits: `rabbit`, `bunny`, `hare`, `kanin`
- fast: `fast`, `speed`, `runner`, `sprinter`, `hurtig`
- gold: `gold`, `golden`, `guld`, `jackpot`
- armored: `armor`, `armour`, `armored`, `panser`
- bomb: `bomb`, `bombe`, `explosive`, `mine`
- boss: `boss`, `emperor`, `kejser`, `titan`, `baron`
- holes: `hole`, `burrow`, `molehill`, `hul`
- backgrounds: `background`, `backdrop`, `arena`, `world`, `environment`, `bg_`
- FX: `effect`, `fx`, `spark`, `burst`, `impact`, `explosion`, `confetti`
- hammer: `hammer`, `mallet`, `club`, `bonk`
- UI: `ui`, `icon`, `button`, `badge`, `medal`, `coin`, `panel`, `hud`
- music: `music`, `theme`, `loop`, `soundtrack`, `bgm`
- SFX: `hit`, `bonk`, `whack`, `smash`, `combo`, `miss`, `spawn`, `pop`, `reward`, `power`, `boss`, `bomb`, `click`

## Managed import folders

For assets intentionally added for Kaninbanker, use either:

- `Assets/Kaninbanker/Art2D/` — mobile sprite import policy with Android ASTC 6x6
- `Assets/Kaninbanker/Imported2D/` — automatic Sprite import plus music/SFX compression/loading defaults

Publisher assets elsewhere under `Assets/` keep their publisher import settings, but any existing Sprites/AudioClips can still be indexed by the catalog.

## Asset Store licensing rule

Do not scrape, bulk-copy, or redistribute raw Unity Asset Store source assets. Only use assets the project owner has a valid license to use. Asset Store content may be incorporated into the game according to its applicable license, but the repository must not become a downloadable asset collection.

## Validation

Every push runs:

1. `scripts/source_audit.py`
2. `scripts/asset_pipeline_audit.py`

Unity Cloud Build preflight additionally requires the official 2D toolset, the generated catalog pipeline and a clean TRUE-2D runtime before Android build continues.
