# Official Unity website/content sources for Kaninbanker

Kaninbanker is TRUE 2D and Android portrait-first. Prefer official Unity Technologies sources and official Unity Package Manager samples before third-party assets.

## What "use everything from Unity" means safely

Unity.com is not one downloadable asset archive. It contains the engine, documentation, tutorials, services, samples, Asset Store products, 2D/3D/XR/industry content, paid products and account-gated downloads. Kaninbanker therefore uses an **official-content intake policy**:

- automatically discover all loaded official `com.unity.*` packages;
- automatically import every non-interactive sample exposed by installed TRUE-2D `com.unity.2d.*` packages;
- scan imported project assets for compatible sprites and audio;
- register official Unity-owned web/sample sources in the project;
- keep 3D/HDRP/XR/industry content out of the production runtime unless explicitly reviewed for a 2D use case;
- never bypass Asset Store account ownership or license acceptance;
- never bulk-copy licensed raw Asset Store packages into the public GitHub repository.

## Automatic Unity-owned package discovery

`KaninbankerOfficialUnitySamples` calls Unity's official `PackageInfo.GetAllRegisteredPackages()` API. Every currently loaded package whose name starts with `com.unity.` is discovered automatically.

Cloud Build then applies these rules:

- all installed `com.unity.*` packages are discovered and reported;
- installed `com.unity.2d.*` packages are eligible for automatic non-interactive sample import;
- interactive samples are skipped in batch/cloud mode;
- already imported samples are not duplicated;
- Unity copies imported package samples under `Assets/Samples/...`;
- `Kaninbanker2DAssetCatalogBuilder` runs afterwards and indexes compatible `Sprite` and `AudioClip` assets.

New official Unity 2D packages can therefore participate without another hardcoded package-list edit.

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

## Canonical Unity website surfaces reviewed

These official Unity-owned pages are treated as the primary discovery layer for future Kaninbanker content:

- Unity home: https://www.unity.com
- Unity 2D: https://unity.com/features/2d
- Unity 6 Resources Hub: https://unity.com/campaign/unity-6-resources
- Unity 2D Asset Store: https://assetstore.unity.com/2d
- Unity Technologies Asset Store publisher: https://assetstore.unity.com/publishers/1
- Unity Learn - Create a 2D game: https://learn.unity.com/collection/create-a-2d-game
- Unity Learn - 2D Game Kit: https://learn.unity.com/project/2d-game-kit
- Unity Manual - Set up a project for 2D games: https://docs.unity3d.com/6000.3/Documentation/Manual/setup-project-2d-game.html
- Unity Package Manager Sample API: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PackageManager.UI.Sample.html

Unity's own 2D documentation describes the 2D suite as covering worldbuilding, characters, graphics, physics and more. The Unity 6 Resources Hub groups documentation, best-practice guides, samples, tutorials and assets.

## Machine-readable official Unity web registry

`Assets/Kaninbanker/Editor/KaninbankerUnityOfficialSourceRegistry.cs` is part of every Cloud Build. It records canonical Unity-owned hubs and known official sample sources, and logs whether each source is public or requires Asset Store account/license acquisition.

Known official 2D/UI sources include:

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

Unity Asset Store products must legitimately be added to the project owner's Unity account / `My Assets` and accepted under their applicable license before Unity permits download/import. Kaninbanker does not bypass that step.

Once an acquired asset is actually present under `Assets/`, the Kaninbanker catalog automatically scans and uses compatible 2D sprites/audio without requiring the core gameplay architecture to be rewritten.

## Intake rule for every new Unity-owned asset

1. verify that the source is official Unity/Unity Technologies or intentionally approved;
2. verify its license and redistribution constraints;
3. acquire it through the supported Unity/Asset Store/Package Manager flow;
4. import it into `Assets/`;
5. keep game-facing production art TRUE 2D;
6. allow the automatic catalog to classify sprites/audio;
7. add useful category naming when needed (`rabbit`, `boss`, `bomb`, `hole`, `background`, `foreground`, `fx`, `ui`, `hammer`, `music`, `hit`, `combo`, `miss`, `power`, `reward`);
8. run the TRUE-2D source audit, asset-pipeline audit and official Unity sample audit;
9. run a fresh Unity Android Cloud Build;
10. test on a portrait Android device.

The target is a very large production game, but size must come from useful, licensed content and systems rather than unreferenced files, incompatible 3D samples or prohibited redistribution.
