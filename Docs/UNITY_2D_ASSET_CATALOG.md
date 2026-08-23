# Kaninbanker — Unity 6 TRUE-2D Asset Catalog

This catalog defines the approved asset/tooling surface for the Kaninbanker Android game.

The rule is **use as much useful 2D capability as possible, but never blindly import every Asset Store item**. Asset Store packages can be paid, account-licensed, redundant, old, 3D-oriented or incompatible with the current Unity version. Every imported asset must have a clear gameplay/presentation job and a valid license.

## Installed official Unity 6.0 packages

Pinned in `Packages/manifest.json` for Unity `6000.0.60f1`:

| Package | Version | Kaninbanker use |
| --- | --- | --- |
| `com.unity.2d.animation` | `10.2.2` | sprite skeletal/character animation tooling |
| `com.unity.2d.aseprite` | `1.1.10` | import `.aseprite` source art |
| `com.unity.2d.pixel-perfect` | `5.0.3` | optional crisp pixel-art camera workflows |
| `com.unity.2d.psdimporter` | `9.1.1` | layered PSB/Photoshop character import |
| `com.unity.2d.spriteshape` | `10.0.7` | spline-based 2D world shapes/decor |
| `com.unity.2d.tilemap.extras` | `4.1.0` | rule/animated tiles and extra brushes |
| `com.unity.addressables` | `2.7.6` | scalable optional world/cosmetic/audio delivery |
| `com.unity.modules.physics2d` | `1.0.0` | 2D hit detection/collision |
| `com.unity.modules.audio` | `1.0.0` | music/SFX |
| `com.unity.modules.imgui` | `1.0.0` | current cloud-safe phone UI layer |
| `com.unity.modules.androidjni` | `1.0.0` | Android runtime integration |

Unity 6 core 2D packages such as **2D Sprite** and **2D Tilemap Editor** are Editor-bound core packages and are not pinned like released registry packages.

## Official Unity 2D feature-set capabilities

The project is approved to use the full Unity 2D feature-set concepts when they improve the game:

- Sprite Editor / Sprite import
- frame-by-frame animation
- 2D Animation / skeletal sprite rigs
- Aseprite import
- PSB layered character import
- Pixel Perfect workflows
- SpriteShape
- Tilemaps
- Rule Tile / Animated Tile / Tilemap Extras
- Physics2D
- Sprite Atlases
- Addressables for large optional content

The current runtime remains orthographic and SpriteRenderer/Physics2D-first even when these authoring tools are used.

## Official Unity sample/reference assets

These are approved **reference/import candidates**, not automatically vendored content. They must only be copied/imported when the connected Unity account has legitimately obtained the package and the license allows the intended use.

### Happy Harvest — 2D Sample Project

Unity Technologies sample demonstrating modern 2D production techniques, including flat sprite workflows, Tilemaps and advanced 2D rendering practices.

Asset Store:
`https://assetstore.unity.com/packages/essentials/tutorial-projects/happy-harvest-2d-sample-project-259218`

Kaninbanker use:
- study world layering
- sprite authoring practices
- Tilemap patterns
- mobile-friendly 2D environment organization

Do not copy its visual identity into Kaninbanker; use the techniques, then build original rabbit/world art.

### Lost Crypt — 2D Sample Project

Official Unity 2D sample listed by Unity's 2D feature-set documentation.

Kaninbanker use:
- study layered 2D presentation
- animation/FX organization
- sprite-rendering patterns

### Dragon Crashers — 2D Sample Project

Official Unity 2D sample listed by Unity's 2D feature-set documentation.

Kaninbanker use:
- study polished 2D character/UI presentation
- study mobile-game menu and animation patterns

### 2D Animation Samples

Official 2D Animation sample content.

Kaninbanker use:
- rabbit bone rigs if we choose skeletal animation for bosses/large characters
- IK/secondary-motion reference
- compare skeletal animation with frame-strip animation before choosing per asset

## Package samples available through Unity Package Manager

When the installed package exposes Samples, these can be imported in Unity Package Manager for development/reference:

- 2D Animation samples
- 2D Pixel Perfect samples
- 2D SpriteShape samples
- 2D Tilemap Extras samples

Do not automatically ship all sample content inside the APK. Import for learning/testing, then only promote assets/code that have a concrete production purpose.

## Kaninbanker production asset families

All real production art belongs under:

`Assets/Kaninbanker/Art2D/`

Planned families:

### Rabbits
- Normal
- Fast
- Gold
- Armored
- Bomb
- Bosses
- future 30+ archetypes
- hit/defeat/telegraph animation strips

### Hammers
- starter hammer
- heavy hammer
- neon hammer
- candy hammer
- lava hammer
- event/season hammers
- swing and impact sprite strips

### FX
- hit rings
- stars
- sparks
- combo bursts
- boss telegraphs
- reward confetti
- medal/chest bursts
- power activation FX
- Reduced-FX variants

### Worlds
- Mega Eng
- Mørk Skov
- Neon City
- Sukkerland
- Lava Pit
- Frosthuler
- Robotfabrik
- Piratøen
- Spøgelsesbyen
- Rummet
- Kaosdimensionen
- Kejserens Arena

Each world can contain:
- background layers
- decorative sprite sheets
- optional Tilemap palette
- SpriteShape profiles where useful
- world-specific FX
- world-specific rabbit skins

### UI
- 9:16 lobby
- gameplay HUD
- four power buttons
- boss health
- results
- World Tour
- Mayhem Pass
- Event Circuit
- Career Book
- settings
- music companion
- tutorial
- achievements/shop/equipment later

### Collectibles and cosmetics
- medals
- trophies
- currencies
- badges
- titles/icons
- collection book cards
- event rewards

## Automatic import policy

`Kaninbanker2DAssetPostprocessor.cs` applies the baseline Android sprite policy to textures under `Assets/Kaninbanker/Art2D/`:

- imported as Sprite
- alpha transparency enabled
- mipmaps disabled
- clamp wrapping
- compressed Android texture settings
- ASTC 6x6 target format
- 2048 maximum baseline texture size

Special assets can receive narrower custom import rules later, but production files must not silently become 3D material/model inputs.

## Sprite Atlas plan

Create atlases by render locality rather than one giant atlas:

- `Atlas_CoreRabbits`
- `Atlas_CoreFX`
- `Atlas_GameplayUI`
- one atlas per World Tour world/theme where useful
- separate boss atlas groups
- separate cosmetics/event groups

Optional seasonal/world content should remain separable for Addressables.

## Addressables plan

The initial APK keeps always-needed gameplay content local. Addressables are reserved for scalable content such as:

- optional worlds
- seasonal events
- large boss art sets
- cosmetic collections
- additional soundtrack packs
- future downloadable sprite atlases

Do not make the current basic round depend on remote network content.

## Canva/Figma production bridge

Design tools must follow the same invariant:

`TRUE 2D mobile arcade game, 9:16 portrait, flat front-facing sprite art, orthographic composition, no 3D render, no perspective camera, no isometric view, no 3D models.`

Approved Canva work is organized into separate 2D folders for Rabbits, UI, FX and Worlds. Figma/FigJam architecture is used as a system/design reference, not as a source of runtime 3D assets.

## License gate

Before adding any third-party or Asset Store package into GitHub/runtime content:

1. identify package/publisher;
2. verify the user's Unity account has legitimate access/license;
3. confirm the package is compatible with Unity 6000.0;
4. reject 3D-only dependencies;
5. import on the development branch only;
6. run source audit;
7. run a fresh Unity Build Automation build;
8. test the APK on Android before treating the asset as production-safe.

No paid/proprietary Asset Store bytes should be copied into the repository merely because a public product page exists.
