# Official Unity asset/sample sources for Kaninbanker

Kaninbanker is TRUE 2D and Android portrait-first. Prefer official Unity Technologies sources and official Unity Package Manager samples before third-party assets.

## Automatically imported from installed Unity packages

`KaninbankerOfficialUnitySamples` uses Unity's official `UnityEditor.PackageManager.UI.Sample` API to import every non-interactive sample exposed by these pinned packages before `Kaninbanker2DAssetCatalogBuilder` scans project assets:

- `com.unity.2d.animation@10.2.2`
- `com.unity.2d.aseprite@1.1.10`
- `com.unity.2d.pixel-perfect@5.0.3`
- `com.unity.2d.psdimporter@9.1.1`
- `com.unity.2d.spriteshape@10.0.7`
- `com.unity.2d.tilemap.extras@4.1.0`

Unity imports package samples under `Assets/Samples/...`. The Kaninbanker catalog then scans all compatible `Sprite` and `AudioClip` assets and can use them as rabbits, bosses, holes, backgrounds, FX, hammers, UI, music and SFX when category naming matches.

## Official Unity Technologies Asset Store / Unity sources found

These are useful official sources but may require the project owner to add them to a Unity account / accept their Asset Store license before Unity permits download. The cloud build does **not** bypass Unity account ownership or license acceptance.

1. **Lost Crypt - 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/lost-crypt-2d-sample-project-158673
   - Official 2D sample project; strong source for 2D art, animation and presentation workflows.

2. **Happy Harvest - 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/happy-harvest-2d-sample-project-259218
   - Official native-2D/URP sample with characters, environments and production 2D workflows.

3. **Dragon Crashers - URP 2D Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-urp-2d-sample-project-190721
   - Official sample demonstrating Unity's native 2D toolset and 2D Renderer workflows.

4. **Dragon Crashers - UI Toolkit Sample Project** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-ui-toolkit-sample-project-231178
   - Official runtime UI sample; useful for responsive mobile UI, themes and safe-area patterns.

5. **QuizU - A UI Toolkit Sample** — Unity Technologies
   - https://assetstore.unity.com/packages/essentials/tutorial-projects/quizu-a-ui-toolkit-sample-268492
   - Official UI Toolkit sample project.

6. **2D Animation Samples** — Unity Technologies
   - https://assetstore.unity.com/packages/2d/characters/2d-animation-samples-354550
   - Official current 2D character/animation sample asset.

7. **2D Game Kit** — Unity Learn / Unity Technologies
   - https://learn.unity.com/project/2d-game-kit
   - Official 2D learning/game-kit project.

8. **2D Renderer Samples** — Unity Technologies GitHub
   - https://github.com/Unity-Technologies/2d-renderer-samples
   - Official 2D Renderer sample repository. Treat as reference until compatibility/license review is complete before copying source assets.

9. **Unity 6 Resources Hub** — Unity
   - https://unity.com/campaign/unity-6-resources
   - Official hub for Unity 6 documentation, samples, tutorials and assets.

10. **Unity 2D product/resources page**
    - https://unity.com/features/2d
    - Official index for Unity's 2D worldbuilding, characters, graphics, physics and sample resources.

11. **Unity Technologies Asset Store publisher page**
    - https://assetstore.unity.com/publishers/1
    - Canonical Unity-owned Asset Store catalog. Review new 2D/UI/sample releases from this publisher first.

## Intake rule

Do not dump arbitrary Asset Store package source into the public repository merely to inflate size. For every licensed asset actually acquired:

1. verify its Unity/Asset Store license and redistribution constraints;
2. import it through Unity into `Assets/`;
3. keep game-facing art 2D-only;
4. let `Kaninbanker2DAssetCatalogBuilder` classify compatible sprites/audio;
5. add explicit category naming when needed (`rabbit`, `boss`, `bomb`, `hole`, `background`, `foreground`, `fx`, `ui`, `hammer`, `music`, `hit`, `combo`, `miss`, `power`, `reward`);
6. run source audit + Unity preflight + a fresh Android Cloud Build;
7. test the result on a portrait Android device.

The target is a very large production game, but size must come from useful content and systems rather than unreferenced files or prohibited redistribution.
