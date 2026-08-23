# Kaninbanker — TRUE 2D Art Direction

## Goal

Kaninbanker must read immediately as a bold, flat, high-energy 2D mobile arcade game on a 9:16 Android phone. The target is not pseudo-3D, isometric, perspective or model-rendered art.

## Camera and composition

- portrait 9:16
- orthographic camera
- front-facing arena
- no vanishing point
- no perspective floor/road receding into distance
- no 3D camera tilt
- rabbits face the player
- holes read as flat 2D ovals/shapes
- active gameplay occupies the middle 65–70% of the safe area
- top HUD stays compact
- power buttons stay thumb-reachable near the lower safe area

## Visual language

- large readable silhouettes
- thick graphic shapes
- bright arcade palette
- strong value contrast between rabbit, hole and background
- flat shading or hand-drawn 2D shading only
- outlines/highlights may imply volume, but assets remain sprites
- exaggerated squash/stretch in animation
- explosive 2D hit shapes, stars, speed lines, rings and score bursts
- no mesh lighting or model material pipeline

## Rabbit sprite rules

Every rabbit family should preserve:

- same front-facing base proportions
- same bottom-center anchor convention
- readable ears/face at phone scale
- type identity visible without relying only on color
- transparent background

Suggested production states per rabbit:

1. hidden/anticipation
2. pop-up
3. idle A
4. idle B
5. hit/squash
6. defeated/drop
7. special telegraph where required

Bosses can use larger sheets and phase-specific overlays, but remain 2D sprites.

## Hammer and impact FX

- hammer is a 2D overlay/sprite animation, never a 3D model
- impact is centered on the tap point
- use layered rings, stars, sparks and short smear frames
- large combos may add screen-edge flashes and 2D shock rings
- Reduced FX mode removes extra layers but preserves hit readability

## World themes

### Mega Eng
- saturated grass greens
- warm sun/yellow accents
- flower/leaf sprite decorations

### Neon Nat
- deep blue/purple field
- cyan/magenta signage shapes
- glow implied through layered transparent sprites

### Sukkerland
- pastel candy palette
- rounded candy/confetti silhouettes
- bright highlight shapes

### Lava Pit
- dark charcoal/red field
- orange/yellow crack sprites
- ember sprite drift

Future worlds follow the same rule: all atmosphere is created from flat sprite layers, tile-like shapes, decals and screen-space effects.

## Animation pipeline

Follow a strip-first workflow to avoid visual drift:

1. approve one seed frame in actual game scale;
2. generate/draw the entire animation strip in one pass;
3. keep transparent background;
4. preserve silhouette, direction, palette and proportions;
5. normalize to fixed frame dimensions;
6. use one shared bottom-center anchor;
7. preview the full strip before Unity import;
8. import as Sprite (2D and UI);
9. test at 9:16 phone scale;
10. only then add to the production sprite catalog/atlas.

## Sprite Atlas strategy

When imported production art replaces procedural sprites:

- group by gameplay locality: core rabbits, bosses, UI, FX, world themes, cosmetics
- avoid one unbounded mega-atlas
- keep frequently co-rendered sprites together
- keep optional seasonal/world content separable for later Addressables use
- do not ship raw source PSD/AI files in runtime content

## UI direction

- big touch targets
- high contrast
- rounded/graphic arcade panels
- no desktop-style dense controls
- one-hand portrait ergonomics
- safe-area aware
- active playfield remains visually dominant

## Design-tool prompt guardrail

Whenever Canva, Figma or an AI design tool is used, the brief must include:

`TRUE 2D mobile arcade game, 9:16 portrait, flat front-facing sprite art, orthographic composition, no 3D render, no perspective camera, no isometric view, no 3D models.`

Any generated reference that violates those rules is rejected as art direction, even if it looks polished.

## Approval checklist

A visual asset/reference is approved only if:

- it clearly reads as 2D at first glance
- there is no perspective floor/depth camera framing
- characters can be represented as transparent sprites
- silhouettes remain readable on a phone
- touch targets are not obscured
- effects do not hide rabbit state
- it fits the 9:16 safe-area composition
