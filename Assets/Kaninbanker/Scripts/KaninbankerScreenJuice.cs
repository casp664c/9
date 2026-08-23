using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Lightweight screen-space reward feedback driven by public score state. It adds edge flashes
    /// and combo-like pulses without owning gameplay rules. Reduced FX mode keeps it subtle.
    /// </summary>
    public sealed class KaninbankerScreenJuice : MonoBehaviour
    {
        private KaninbankerGame game;
        private int lastScore;
        private bool wasRunning;
        private float flash;
        private float bigFlash;
        private GUIStyle scoreStyle;
        private string popText = string.Empty;
        private float popTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerScreenJuice>() != null)
                return;

            GameObject go = new GameObject("KaninbankerScreenJuice");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerScreenJuice>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame>();
            if (game != null)
            {
                lastScore = game.Score;
                wasRunning = game.IsRunning;
            }
        }

        private void Update()
        {
            if (game == null)
            {
                game = FindFirstObjectByType<KaninbankerGame>();
                return;
            }

            bool reduced = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;
            flash = Mathf.Max(0f, flash - Time.unscaledDeltaTime * (reduced ? 4.5f : 2.8f));
            bigFlash = Mathf.Max(0f, bigFlash - Time.unscaledDeltaTime * (reduced ? 5.5f : 2.1f));
            popTime = Mathf.Max(0f, popTime - Time.unscaledDeltaTime);

            bool running = game.IsRunning;
            if (running && !wasRunning)
                lastScore = game.Score;

            if (running && game.Score > lastScore)
            {
                int gained = game.Score - lastScore;
                flash = Mathf.Max(flash, reduced ? 0.15f : 0.48f);
                if (gained >= 10)
                    bigFlash = Mathf.Max(bigFlash, reduced ? 0.12f : 0.70f);
                popText = "+" + gained;
                popTime = reduced ? 0.28f : 0.55f;
                lastScore = game.Score;
            }

            wasRunning = running;
        }

        private void OnGUI()
        {
            if (game == null || !game.IsRunning)
                return;

            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);

            if (flash > 0f)
            {
                float thickness = Mathf.Max(5f, safe.width * 0.018f);
                Color old = GUI.color;
                GUI.color = new Color(1f, 0.50f, 0.08f, flash * 0.36f);
                GUI.DrawTexture(new Rect(safe.x, safe.y, safe.width, thickness), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(safe.x, safe.yMax - thickness, safe.width, thickness), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(safe.x, safe.y, thickness, safe.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(safe.xMax - thickness, safe.y, thickness, safe.height), Texture2D.whiteTexture);
                GUI.color = old;
            }

            if (bigFlash > 0f)
            {
                Color old = GUI.color;
                GUI.color = new Color(0.55f, 0.16f, 1f, bigFlash * 0.09f);
                GUI.DrawTexture(safe, Texture2D.whiteTexture);
                GUI.color = old;
            }

            if (popTime > 0f && !string.IsNullOrEmpty(popText))
            {
                EnsureStyle();
                float width = safe.width * 0.30f;
                float height = safe.height * 0.065f;
                float x = safe.x + (safe.width - width) * 0.5f;
                float y = safe.y + safe.height * 0.26f - (0.55f - popTime) * safe.height * 0.045f;
                Color old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(popTime * 2.2f));
                GUI.Label(new Rect(x, y, width, height), popText, scoreStyle);
                GUI.color = old;
            }
        }

        private void EnsureStyle()
        {
            if (scoreStyle != null)
                return;

            int reference = Mathf.Min(Screen.width, Screen.height);
            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 11, 34, 78),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            scoreStyle.normal.textColor = new Color(1f, 0.82f, 0.16f);
        }
    }
}
