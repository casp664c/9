# Official Unity asset/sample sources for Kaninbanker

Kaninbanker is TRUE 2D and Android portrait-first. Prefer official Unity Technologies sources and official Unity Package Manager samples before third-party assets.

## Automatic Unity-owned package discovery

`KaninbankerOfficialUnitySamples` now calls Unity's official `PackageInfo.GetAllRegisteredPackages()` API. Every currently loaded package whose name starts with `com.unity.` is discovered automatically.

Cloud Build then applies a safety rule:

- every installed `com.unity.*` package is discovered and reported;
- every installed `com.unity.2d.*` package is eligible for automatic non-interactive sample import;
- 3D/HDRP/XR/Multiplayer/Industry packages are discovery-only and are not auto-imported into Kaninbanker's production build;
- interactive package samples are skipped in batch/cloud mode;
- already-imported samples are not duplicated;
- all imported package samples are copied by Unity under `Assets/Samples/...`;
- `Kaninbanker2DAssetCatalogBuilder` runs afterwards and indexes compatible Sprites and AudioClips.

This means newly installed official Unity 2D packages can participate without another hardcoded package-list edit.

## Explicitly installed official Unity 2D toolset

The project currently pins/includes:

- `com.unity.2d.animation@10.2.2`
- `com.unity.2d.aseprite@1.1.10`
- `com.unity.2d.pixel-perfect@5.0.3`
- `com.unity.2d.psdimporter@9.1.1`
- `com.unity.2d.sprite@1.0.0`
- `com.unity.2d.spriteshape@10.0.7`
- `com.unity.2d.tilemap@1.0.0`
- `com.unity.2d.tilemap.extras@4.1.0`
- `com.unity.addressables@2.7.6`
- `com.unity.modules.physics2d@1.0.0`

The game asset catalog can classify compatible imported content as rabbits, bosses, bombs, holes, backgrounds, foregrounds, FX, UI, hammers, music, hit/combo/miss/power/boss/bomb/reward SFX and generic fallback content.

## Machine-readable official Unity web registry

`Assets/Kaninbanker/Editor/KaninbankerUnityOfficialSourceRegistry.cs` is part of every Cloud Build. It records canonical Unity-owned hubs and known official sample sources, and logs whether each source is public or requires Asset Store account/license acquisition.

Canonical hubs:

- Unity 2D: https://unity.com/features/2d
- Unity 6 Resources Hub: https://unity.com/campaign/unity-6-resources
- Unity Technologies Asset Store publisher: https://assetstore.unity.com/publishers/1

Known official 2D/UI sources:

1. **Lost Crypt - 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/lost-crypt-2d-sample-project-158673
2. **Happy Harvest - 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/happy-harvest-2d-sample-project-259218
3. **Dragon Crashers - URP 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-urp-2d-sample-project-190721
4. **Dragon Crashers - UI Toolkit Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-ui-toolkit-sample-project-231178
5. **QuizU - A UI Toolkit Sample** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/quizu-a-ui-toolkit-sample-268492
6. **2D Animation Samples** — Unity Technologies
   - https://assetstore.unity.com/packages/2d/characters/2d-animation-samples-354550
7. **2D Game Kit** — Unity Learn / Unity Technologies
   - https://learn.unity.com/project/2d-game-kit
8. **Unity 2D Renderer Samples** — Unity Technologies
   - https://github.com/Unity-Technologies/2d-renderer-samples
9. **Unity 2D Tech Demos** — Unity Technologies
   - https://github.com/Unity-Technologies/2d-techdemos

## Important Asset Store boundary

Unity's website is not one downloadable asset bundle. It contains engine downloads, documentation, tutorials, services, 2D/3D content, Asset Store products, paid products and account-gated packages.

Asset Store products must legitimately be added to the project owner's Unity account / `My Assets` and accepted under their applicable license before Unity permits download/import. Kaninbanker does not bypass that step and does not copy licensed raw Asset Store packages into a public repository merely to inflate project size.

Once an acquired asset is actually present under `Assets/`, the Kaninbanker catalog automatically scans and uses compatible 2D sprites/audio without requiring the core gameplay architecture to be rewritten.

## Intake rule

For every licensed Unity asset actually acquired:

1. verify its license and redistribution constraints;
2. import it through Unity into `Assets/`;
3. keep game-facing production art TRUE 2D;
4. allow the automatic catalog to classify sprites/audio;
5. add useful category naming when needed (`rabbit`, `boss`, `bomb`, `hole`, `background`, `foreground`, `fx`, `ui`, `hammer`, `music`, `hit`, `combo`, `miss`, `power`, `reward`);
6. run all GitHub audits;
7. run a fresh Unity Android Cloud Build;
8. test on a portrait Android device.

The target is a very large production game, but size must come from useful, licensed content and systems rather than unreferenced files, incompatible 3D samples or prohibited redistribution.
