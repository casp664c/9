# Kaninbanker project rules

- Android is the primary and production target.
- The game is permanently **TRUE 2D** and portrait-first. Do not reintroduce a 3D gameplay/runtime path.
- Unity remains the implementation engine. Do not migrate this game to React Native or another runtime.
- Use an orthographic camera, `SpriteRenderer`, 2D colliders and `Physics2D` for gameplay/world presentation.
- Do not use `GameObject.CreatePrimitive`, 3D Physics, MeshRenderer/MeshFilter, 3D colliders, Rigidbody, 3D lights, shadow/LOD quality systems or 3D model assets in the Kaninbanker runtime.
- Keep `com.unity.modules.physics2d`; do not add legacy `com.unity.modules.physics` or `com.unity.modules.particlesystem` unless the project direction is explicitly changed by the user.
- The project must remain buildable through Unity Build Automation without requiring the user to own or connect a PC.
- Prefer code-driven deterministic scene/bootstrap setup so cloud builds do not depend on manual Editor clicks.
- Regenerate the build scene from the TRUE-2D root rather than trusting stale Unity Cloud workspace files.
- Production visual assets must be sprite-based and tested in 9:16 portrait on Android.
- Keep dependencies minimal, purposeful and reversible.
- Run `scripts/source_audit.py` and preserve the Unity preflight 2D gate when changing runtime/build files.
- Never commit secrets, GitHub tokens, Unity credentials, keystores or passwords.
- Validate changes with the strongest available evidence; do not claim Unity compilation or APK success without Unity/CI build logs.
- Preserve GitHub `main` as the stable branch. Develop substantial changes on branches and review before merge.
