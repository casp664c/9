using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Small portrait settings drawer for device comfort. It stays outside active gameplay and
    /// stores preferences in PlayerPrefs so the performance governor and FX layer can react.
    /// </summary>
    public sealed class KaninbankerSettingsPanel : MonoBehaviour
    {
        public const string FpsKey = "Kaninbanker.Settings.TargetFps";
        public const string ReducedFxKey = "Kaninbanker.Settings.ReducedFx";

        private KaninbankerGame2D game;
        private KaninbankerAudio audioSystem;
        private bool panelOpen;
        private int targetFps;
        private bool reducedFx;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerSettingsPanel>() != null)
                return;

            GameObject go = new GameObject("KaninbankerSettingsPanel");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerSettingsPanel>();
        }

        private void Start()
        {
            targetFps = PlayerPrefs.GetInt(FpsKey, 60);
            reducedFx = PlayerPrefs.GetInt(ReducedFxKey, 0) != 0;
            ResolveGame();
        }

        private void Update()
        {
            if (game == null)
                ResolveGame();
        }

        private void ResolveGame()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            if (game != null)
                audioSystem = game.GetComponent<KaninbankerAudio>();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(FpsKey, targetFps);
            PlayerPrefs.SetInt(ReducedFxKey, reducedFx ? 1 : 0);
            PlayerPrefs.Save();
            Application.targetFrameRate = targetFps;
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float margin = Mathf.Max(14f, safe.width * 0.03f);
            float buttonW = Mathf.Min(safe.width * 0.26f, 190f);
            float buttonH = Mathf.Max(50f, safe.height * 0.052f);

            Rect tab = new Rect(safe.xMax - margin - buttonW, safe.y + safe.height * 0.345f, buttonW, buttonH);
            if (GUI.Button(tab, panelOpen ? "LUK" : "INDSTILLINGER", buttonStyle))
                panelOpen = !panelOpen;

            if (!panelOpen)
                return;

            float panelW = safe.width - margin * 2f;
            float panelH = safe.height * 0.46f;
            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.25f, panelW, panelH);
            DrawRect(panel, new Color(0.025f, 0.035f, 0.055f, 0.988f));

            float inner = Mathf.Max(18f, panel.width * 0.045f);
            float y = panel.y + panel.height * 0.06f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.14f), "INDSTILLINGER", titleStyle);
            y += panel.height * 0.17f;

            float rowH = panel.height * 0.17f;
            DrawRect(new Rect(panel.x + inner, y, panel.width - inner * 2f, rowH), new Color(0.07f, 0.085f, 0.12f, 0.94f));
            GUI.Label(new Rect(panel.x + inner * 1.4f, y, panel.width * 0.43f, rowH), "LYD", bodyStyle);
            if (GUI.Button(new Rect(panel.x + panel.width * 0.57f, y + rowH * 0.16f, panel.width * 0.32f, rowH * 0.68f),
                audioSystem != null && audioSystem.IsMuted ? "FRA" : "TIL", buttonStyle))
            {
                if (audioSystem != null)
                    audioSystem.ToggleMute();
            }
            y += rowH + panel.height * 0.025f;

            DrawRect(new Rect(panel.x + inner, y, panel.width - inner * 2f, rowH), new Color(0.07f, 0.085f, 0.12f, 0.94f));
            GUI.Label(new Rect(panel.x + inner * 1.4f, y, panel.width * 0.43f, rowH), "FRAME RATE", bodyStyle);
            if (GUI.Button(new Rect(panel.x + panel.width * 0.57f, y + rowH * 0.16f, panel.width * 0.32f, rowH * 0.68f),
                targetFps + " FPS", buttonStyle))
            {
                targetFps = targetFps >= 60 ? 30 : 60;
                Save();
            }
            y += rowH + panel.height * 0.025f;

            DrawRect(new Rect(panel.x + inner, y, panel.width - inner * 2f, rowH), new Color(0.07f, 0.085f, 0.12f, 0.94f));
            GUI.Label(new Rect(panel.x + inner * 1.4f, y, panel.width * 0.43f, rowH), "REDUCER FX", bodyStyle);
            if (GUI.Button(new Rect(panel.x + panel.width * 0.57f, y + rowH * 0.16f, panel.width * 0.32f, rowH * 0.68f),
                reducedFx ? "TIL" : "FRA", buttonStyle))
            {
                reducedFx = !reducedFx;
                Save();
            }
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            int reference = Mathf.Min(Screen.width, Screen.height);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 14, 28, 60),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 29, 17, 34),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            bodyStyle.normal.textColor = new Color(0.90f, 0.92f, 0.98f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 36, 14, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
