# Kaninbanker 0.9.0 — TRUE 2D migration

This branch now treats 2D as a hard production requirement, not a camera trick.

## Runtime rules

- Android portrait only (9:16-first).
- `KaninbankerGame2D` is the generated Main scene root.
- Camera projection is orthographic.
- Gameplay visuals use `SpriteRenderer` on a flat XY plane.
- Rabbits and holes are composed from runtime-generated 2D sprites.
- Touch hit testing uses `CircleCollider2D` + `Physics2D.OverlapPoint`.
- Hit feedback and arena atmosphere use flat sprites.
- No 3D primitive creation, 3D raycasts or directional lights are allowed inside the true-2D gameplay runtime.
- Unity Editor Default Behavior Mode is forced to 2D before Cloud Build.
- `com.unity.modules.physics2d` is required by preflight.

## Preserved game scale

The 2D migration keeps the large portrait game direction:

- 15-hole 3x5 portrait arena
- Classic / Turbo / Marathon / Boss Rush
- normal, fast, gold, armored, bomb and boss rabbits
- Slow, x2, Shield and Frenzy powers
- missions, score, combos, coins and XP
- 60-stage World Tour
- rotating Mega Event Circuit
- 50-level Mayhem Pass
- Career/collection panel
- tutorial, settings, external-music panel and performance governor

## Cloud Build gate

`KaninbankerPreflightValidator` verifies:

1. portrait orientation is locked;
2. Unity editor behavior mode is 2D;
3. Main scene exists and is enabled;
4. Physics2D is present;
5. the true-2D runtime contains orthographic/SpriteRenderer/Collider2D/Physics2D markers;
6. executable 3D primitive/raycast/directional-light calls have not leaked into the new runtime.

The legacy `KaninbankerGame.cs` remains in source temporarily as migration history/compatibility code, but the generated Android scene no longer attaches it. All new runtime gameplay work should target `KaninbankerGame2D.cs`.
