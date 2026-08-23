# Imported2D

Place project-owned or properly licensed Kaninbanker production assets here when they should receive automatic mobile import defaults.

Recommended subfolder/name patterns:

- `Rabbits/rabbit_normal_*`
- `Rabbits/rabbit_fast_*`
- `Rabbits/rabbit_gold_*`
- `Rabbits/rabbit_armored_*`
- `Rabbits/rabbit_bomb_*`
- `Bosses/boss_*`
- `World/background_*`
- `Holes/hole_*`
- `FX/fx_*`, `impact_*`, `explosion_*`, `confetti_*`
- `UI/ui_*`, `icon_*`, `badge_*`, `medal_*`
- `Hammer/hammer_*` or `mallet_*`
- `Audio/music_*`, `hit_*`, `combo_*`, `miss_*`, `rabbit_pop_*`, `power_*`, `bomb_*`, `boss_*`, `reward_*`

Textures placed under this folder are automatically treated as 2D Sprites with mipmaps disabled. Music is configured for streaming; short SFX use Decompress On Load. All usable Sprites and AudioClips are indexed into the generated Kaninbanker 2D Asset Catalog during Unity cloud bootstrap.

Do not place unlicensed Asset Store source files in this repository.
