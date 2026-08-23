using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Pure 2D hammer overlay for tap feedback. Imported hammer sprites from the generated
    /// asset catalog are preferred; procedural SpriteRenderer geometry remains the fallback.
    /// The selected Hammer Collection skin is applied live without restarting the round.
    /// </summary>
    public sealed class KaninbankerHammer2D : MonoBehaviour
    {
        private KaninbankerGame2D game;
        private Camera gameplayCamera;
        private Transform hammerRoot;
        private SpriteRenderer handle;
        private SpriteRenderer head;
        private SpriteRenderer importedHammer;
        private float swingTime;
        private Vector3 targetWorld;
        private bool reducedFx;
        private int currentSkin = -1;

        private const float SwingDuration = 0.18f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerHammer2D>() != null)
                return;

            GameObject go = new GameObject("KaninbankerHammer2D");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerHammer2D>();
        }

        private void Start()
        {
            Resolve();
            BuildHammer();
            RefreshSkin(true);
        }

        private void Update()
        {
            if (game == null || gameplayCamera == null)
                Resolve();

            if (game == null || gameplayCamera == null)
                return;

            reducedFx = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;
            RefreshSkin(false);

            if (game.IsRunning)
                ReadTap();

            AnimateHammer();
        }

        private void Resolve()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            gameplayCamera = Camera.main;
        }

        private void BuildHammer()
        {
            if (hammerRoot != null)
                return;

            GameObject root = new GameObject("Hammer2D_Root");
            root.transform.SetParent(transform, false);
            hammerRoot = root.transform;

            GameObject handleGo = new GameObject("Hammer2D_Handle");
            handleGo.transform.SetParent(hammerRoot, false);
            handleGo.transform.localPosition = new Vector3(0f, -0.52f, 0f);
            handleGo.transform.localScale = new Vector3(0.20f, 1.45f, 1f);
            handle = handleGo.AddComponent<SpriteRenderer>();
            handle.sprite = Kaninbanker2DArt.Square;
            handle.sortingOrder = 90;

            GameObject headGo = new GameObject("Hammer2D_Head");
            headGo.transform.SetParent(hammerRoot, false);
            headGo.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            headGo.transform.localScale = new Vector3(1.20f, 0.52f, 1f);
            head = headGo.AddComponent<SpriteRenderer>();
            head.sprite = Kaninbanker2DArt.Square;
            head.sortingOrder = 91;

            GameObject importedGo = new GameObject("Hammer2D_ImportedArt");
            importedGo.transform.SetParent(hammerRoot, false);
            importedGo.transform.localPosition = Vector3.zero;
            importedHammer = importedGo.AddComponent<SpriteRenderer>();
            importedHammer.sortingOrder = 92;
            importedHammer.color = Color.white;
            importedHammer.enabled = false;

            hammerRoot.gameObject.SetActive(false);
        }

        private void RefreshSkin(bool force)
        {
            int selected = KaninbankerHammerCollection.SelectedIndex;
            if (!force && selected == currentSkin)
                return;

            currentSkin = selected;
            Kaninbanker2DAssetCatalog catalog = Kaninbanker2DAssetCatalog.Load();
            Sprite imported = catalog != null ? catalog.PickHammer(selected) : null;

            if (imported != null)
            {
                importedHammer.sprite = imported;
                importedHammer.enabled = true;
                ScaleToHeight(importedHammer, 2.0f + (selected % 3) * 0.08f);
                handle.enabled = false;
                head.enabled = false;
            }
            else
            {
                importedHammer.sprite = null;
                importedHammer.enabled = false;
                handle.enabled = true;
                head.enabled = true;
                ApplyFallbackPalette(selected);
            }
        }

        private void ApplyFallbackPalette(int skin)
        {
            Color handleColor;
            Color headColor;
            switch (skin)
            {
                case 1: handleColor = new Color(0.12f, 0.16f, 0.32f); headColor = new Color(0.15f, 0.95f, 1f); break;
                case 2: handleColor = new Color(0.45f, 0.24f, 0.03f); headColor = new Color(1f, 0.78f, 0.08f); break;
                case 3: handleColor = new Color(0.74f, 0.30f, 0.58f); headColor = new Color(1f, 0.55f, 0.82f); break;
                case 4: handleColor = new Color(0.28f, 0.08f, 0.03f); headColor = new Color(1f, 0.24f, 0.04f); break;
                case 5: handleColor = new Color(0.12f, 0.30f, 0.42f); headColor = new Color(0.52f, 0.90f, 1f); break;
                case 6: handleColor = new Color(0.12f, 0.04f, 0.24f); headColor = new Color(0.70f, 0.34f, 1f); break;
                case 7: handleColor = new Color(0.32f, 0.08f, 0.08f); headColor = new Color(1f, 0.18f, 0.18f); break;
                case 8: handleColor = new Color(0.08f, 0.26f, 0.20f); headColor = new Color(0.18f, 1f, 0.56f); break;
                case 9: handleColor = new Color(0.38f, 0.20f, 0.48f); headColor = new Color(0.95f, 0.74f, 1f); break;
                case 10: handleColor = new Color(0.02f, 0.02f, 0.05f); headColor = new Color(0.42f, 0.18f, 0.78f); break;
                case 11: handleColor = new Color(0.46f, 0.22f, 0.02f); headColor = new Color(1f, 0.92f, 0.28f); break;
                default: handleColor = new Color(0.38f, 0.18f, 0.07f); headColor = new Color(1f, 0.72f, 0.10f); break;
            }

            handle.color = handleColor;
            head.color = headColor;
            float widthBoost = 1f + (skin % 4) * 0.05f;
            head.transform.localScale = new Vector3(1.20f * widthBoost, 0.52f, 1f);
        }

        private static void ScaleToHeight(SpriteRenderer renderer, float height)
        {
            if (renderer == null || renderer.sprite == null || renderer.sprite.bounds.size.y <= 0.0001f)
                return;
            float scale = height / renderer.sprite.bounds.size.y;
            renderer.transform.localScale = Vector3.one * scale;
        }

        private void ReadTap()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && !IsReservedUiTouch(touch.position))
                    PlaySwing(touch.position);
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsReservedUiTouch(Input.mousePosition))
                PlaySwing(Input.mousePosition);
        }

        private static bool IsReservedUiTouch(Vector2 screenPoint)
        {
            Rect safe = Screen.safeArea;
            return screenPoint.y > safe.yMax - safe.height * 0.18f ||
                   screenPoint.y < safe.y + safe.height * 0.17f;
        }

        private void PlaySwing(Vector2 screenPoint)
        {
            Vector3 world = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10f));
            targetWorld = new Vector3(world.x, world.y + 0.35f, -0.5f);
            hammerRoot.position = targetWorld;
            hammerRoot.localScale = Vector3.one * (reducedFx ? 0.78f : 1f);
            swingTime = SwingDuration;
            hammerRoot.gameObject.SetActive(true);
        }

        private void AnimateHammer()
        {
            if (hammerRoot == null || !hammerRoot.gameObject.activeSelf)
                return;

            swingTime -= Time.unscaledDeltaTime;
            float t = 1f - Mathf.Clamp01(swingTime / SwingDuration);
            float eased = 1f - (1f - t) * (1f - t);
            float startAngle = targetWorld.x < 0f ? -46f : 46f;
            float endAngle = targetWorld.x < 0f ? 22f : -22f;
            float angle = Mathf.Lerp(startAngle, endAngle, eased);
            hammerRoot.rotation = Quaternion.Euler(0f, 0f, angle);

            float punch = 1f + Mathf.Sin(t * Mathf.PI) * (reducedFx ? 0.04f : 0.12f);
            hammerRoot.localScale = Vector3.one * punch * (reducedFx ? 0.78f : 1f);

            if (swingTime <= 0f)
                hammerRoot.gameObject.SetActive(false);
        }
    }
}
