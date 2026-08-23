# Kaninbanker Art2D

This is the authoring/source-art area for Kaninbanker production 2D assets.

All gameplay art remains sprite-based. Do not add FBX/OBJ/GLB meshes or 3D materials here.

## Core rules

- portrait 9:16 target
- transparent PNG/PSB/Aseprite for characters and FX
- front-facing rabbit art
- consistent pixels-per-unit convention
- bottom-center anchors for character animation frames
- no baked perspective floor or isometric camera angle
- no 3D model renders presented as game assets unless flattened and approved as a normal 2D sprite
- keep source sheets organized by character/world/FX family

## Production layout

Suggested source structure:

- `Rabbits/Normal/`
- `Rabbits/Fast/`
- `Rabbits/Gold/`
- `Rabbits/Armored/`
- `Rabbits/Bomb/`
- `Rabbits/Boss/`
- `Hammers/`
- `FX/`
- `Worlds/`
- `UI/`
- `Collectibles/`

## Runtime core-sprite path

Always-needed approved sprites that should replace procedural fallback art can be placed under:

`Assets/Kaninbanker/Resources/KaninbankerArt2D/`

Rabbit idle sprite convention:

- `Rabbits/Normal/Idle.png`
- `Rabbits/Fast/Idle.png`
- `Rabbits/Gold/Idle.png`
- `Rabbits/Armored/Idle.png`
- `Rabbits/Bomb/Idle.png`
- `Rabbits/Boss/Idle.png`

Hammer convention:

- `Hammers/Starter/Head.png`
- `Hammers/Starter/Handle.png`

The runtime falls back to code-generated sprites if these resources are not present, so Cloud Build stays self-contained while production art is built.

## Large/optional content

Optional worlds, seasonal skins, boss packs and cosmetic collections should move to Addressables rather than bloating the always-loaded core Resources set.
