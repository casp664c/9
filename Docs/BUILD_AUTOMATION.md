# Unity Build Automation setup for Kaninbanker TRUE 2D

Use this repository with Unity Build Automation.

## Current Android build configuration

- Repository: `casp664c/9`
- Branch: `build/kaninbanker-unity-live`
- Project subfolder: leave empty
- Unity version: `6000.0.60f1`
- Platform: Android
- Build format for phone testing: APK
- Development build: off
- Signing for test builds: Unity/default debug signing

Current application version: `0.10.0` / Android versionCode `10`.

## TRUE-2D guarantees applied before each build

`KaninbankerCloudBootstrap` runs before the Android build and:

1. forces `EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D`;
2. locks Android to portrait and disables landscape autorotation;
3. configures package id, ARM64, IL2CPP and API 26 minimum;
4. regenerates `Assets/Kaninbanker/Scenes/Main.unity` from scratch;
5. attaches `KaninbankerGame2D` as the generated scene root;
6. replaces Build Settings with that generated scene.

The scene is regenerated even if a file already exists. This is intentional: a cached scene from an older 3D Unity Cloud workspace must never become build input again.

## Preflight gate

Before Unity spends time on the full Android player build, `KaninbankerPreflightValidator` checks:

- portrait lock
- Unity 2D editor mode
- generated Main scene
- required runtime files
- `com.unity.modules.physics2d`
- absence of legacy Unity 3D Physics and ParticleSystem modules
- absence of the deleted `KaninbankerGame.cs`
- orthographic + SpriteRenderer + Collider2D + Physics2D markers
- no forbidden 3D runtime APIs across the entire `Assets/Kaninbanker/Scripts` folder
- version `0.10.0` / versionCode `10`

Expected successful log marker:

`PREFLIGHT PASS: FULL APP TRUE 2D ONLY`

## GitHub source audit

GitHub Actions runs:

`python scripts/source_audit.py`

The source audit is an additional fast regression gate. It does not replace Unity compilation, but it catches accidental reintroduction of 3D files/modules/APIs before a longer cloud build.

## Build flow

1. Unity Cloud clones `build/kaninbanker-unity-live`.
2. Confirm the log shows the newest revision, not a replayed historical revision.
3. Unity imports the project.
4. Bootstrap forces 2D/portrait and regenerates Main.unity.
5. Preflight requires FULL APP TRUE 2D ONLY.
6. Unity creates the Android APK.
7. Build must end in SUCCESS.
8. Install the APK on a real portrait Android phone and visually confirm front-facing flat 2D gameplay.

## Important: always start a new build after source changes

Do not use Replay/Genspil on an older successful build when validating new commits. Start a fresh build so Unity checks out the current branch revision.

## Custom Android build method

If the Build Automation configuration needs an explicit method, use:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

## Release later

After the TRUE-2D APK is validated on devices:

- use AAB for Google Play
- configure a permanent release keystore outside source control
- increase Android versionCode on every store update
- never commit signing passwords, GitHub tokens, Unity credentials or keystores
