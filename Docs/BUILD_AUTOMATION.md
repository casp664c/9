# Unity Build Automation setup for Kaninbanker

Use this repository with Unity Cloud Build Automation.

## First validation build

Create an Android build configuration with:

- Repository: `casp664c/9`
- Branch: `build/kaninbanker-unity-cloud`
- Project subfolder: leave empty
- Unity version: `6000.0.60f1`
- Platform: Android
- Build format: APK for direct phone testing
- Development build: off
- Signing: Unity/default debug signing for the first test build

The project bootstrap creates the gameplay scene automatically when Unity imports the project. It also configures Android package id, ARM64, IL2CPP, API 26 minimum and landscape orientation.

## Expected first-build flow

1. Unity Cloud clones the branch.
2. Unity 6000.0.60f1 imports the project.
3. `KaninbankerCloudBootstrap` configures Android settings and generates `Assets/Kaninbanker/Scenes/Main.unity` if missing.
4. The scene is added to Build Settings.
5. Build Automation creates the Android artifact.
6. Read the build log before merging the branch to `main`.

## If the default cloud build does not pick up the generated scene

Use the custom static build method:

`Kaninbanker.Editor.KaninbankerBuild.BuildAndroidApk`

It creates `Builds/Android/Kaninbanker.apk` and throws an error if Unity reports a failed build.

## Release later

After the APK prototype is proven on a device:

- merge the validated branch to `main`
- switch output to AAB for Google Play
- configure a permanent release keystore
- increase Android version code for every store update
- never commit signing passwords or keystore secrets to GitHub
