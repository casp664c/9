using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Pure 2D hammer overlay for tap feedback. Imported hammer sprites from the generated
    /// asset catalog are preferred; procedural SpriteRenderer geometry remains the fallback.
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
        }

        private void Update()
        {
            if (game == null || gameplayCamera == null)
                Resolve();

            if (game == null || gameplayCamera == null)
                return;

            reducedFx = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;

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
            handle.color = new Color(0.38f, 0.18f, 0.07f, 0.98f);
            handle.sortingOrder = 90;

            GameObject headGo = new GameObject("Hammer2D_Head");
            headGo.transform.SetParent(hammerRoot, false);
            headGo.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            headGo.transform.localScale = new Vector3(1.20f, 0.52f, 1f);
            head = headGo.AddComponent<SpriteRenderer>();
            head.sprite = Kaninbanker2DArt.Square;
            head.color = new Color(1f, 0.72f, 0.10f, 0.98f);
            head.sortingOrder = 91;

            Kaninbanker2DAssetCatalog catalog = Kaninbanker2DAssetCatalog.Load();
            Sprite sprite = catalog != null ? catalog.PickHammer(71) : null;
            if (sprite != null)
            {
                GameObject importedGo = new GameObject("Hammer2D_ImportedArt");
                importedGo.transform.SetParent(hammerRoot, false);
                importedGo.transform.localPosition = Vector3.zero;
                importedHammer = importedGo.AddComponent<SpriteRenderer>();
                importedHammer.sprite = sprite;
                importedHammer.color = Color.white;
                importedHammer.sortingOrder = 92;
                ScaleToHeight(importedHammer, 2.0f);
                handle.enabled = false;
                head.enabled = false;
            }

            hammerRoot.gameObject.SetActive(false);
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
