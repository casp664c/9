using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// First-run portrait tutorial for the true-2D game.
    /// </summary>
    public sealed class KaninbankerTutorial : MonoBehaviour
    {
        private const string SeenKey = "Kaninbanker.Tutorial.SeenV2D";

        private static readonly string[] Titles =
        {
            "BANK KANINERNE",
            "LÆR TYPERNE",
            "BRUG POWERUPS",
            "BYG DIN KARRIERE"
        };

        private static readonly string[] Bodies =
        {
            "Tryk direkte på den 2D-kanin der popper op. Hold comboen i live for større score. Arenaen er ægte flad 2D med 15 huller på telefonen i højkant.",
            "Guld giver store point. Pansrede kræver flere slag. Bomber skal normalt undgås. Boss-kaniner har flere liv og bliver længere på den flade 2D-bane.",
            "SLOW giver mere reaktionstid. x2 fordobler score. SKJOLD beskytter mod fejl og bomber. FRENZY gør runden hurtigere og mere eksplosiv.",
            "Spil events, World Tour og Mayhem Pass. Saml medaljer, trofæer, levels og collection-unlocks i Karriere-bogen."
        };

        private KaninbankerGame2D game;
        private bool open;
        private int page;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle pageStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerTutorial>() != null)
                return;

            GameObject go = new GameObject("KaninbankerTutorial");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerTutorial>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            open = PlayerPrefs.GetInt(SeenKey, 0) == 0;
        }

        private void Update()
        {
            if (game == null)
                game = FindFirstObjectByType<KaninbankerGame2D>();
        }

        private void CloseTutorial()
        {
            open = false;
            PlayerPrefs.SetInt(SeenKey, 1);
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float margin = Mathf.Max(14f, safe.width * 0.03f);

            if (!open)
            {
                float w = Mathf.Min(safe.width * 0.26f, 190f);
                float h = Mathf.Max(50f, safe.height * 0.052f);
                Rect reopen = new Rect(safe.xMax - margin - w, safe.y + safe.height * 0.275f, w, h);
                if (GUI.Button(reopen, "2D GUIDE", buttonStyle))
                {
                    page = 0;
                    open = true;
                }
                return;
            }

            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.16f,
                safe.width - margin * 2f, safe.height * 0.68f);
            DrawRect(panel, new Color(0.02f, 0.028f, 0.05f, 0.992f));

            float inner = Mathf.Max(20f, panel.width * 0.05f);
            float heroH = panel.height * 0.22f;
            DrawRect(new Rect(panel.x + inner, panel.y + inner, panel.width - inner * 2f, heroH),
                page == 1 ? new Color(0.30f, 0.08f, 0.08f, 0.96f) :
                page == 2 ? new Color(0.16f, 0.08f, 0.32f, 0.96f) :
                page == 3 ? new Color(0.25f, 0.16f, 0.04f, 0.96f) :
                new Color(0.04f, 0.18f, 0.25f, 0.96f));

            string icon = page == 0 ? "2D  🐇  2D" : page == 1 ? "🐇  💣  👑" : page == 2 ? "⏱  x2  🛡  ⚡" : "🏆  ⭐  🥇";
            GUI.Label(new Rect(panel.x + inner, panel.y + inner, panel.width - inner * 2f, heroH), icon, pageStyle);

            float y = panel.y + inner + heroH + panel.height * 0.045f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.11f), Titles[page], titleStyle);
            y += panel.height * 0.12f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.22f), Bodies[page], bodyStyle);

            float navH = panel.height * 0.11f;
            float navY = panel.yMax - inner - navH;
            float gap = inner * 0.45f;
            float half = (panel.width - inner * 2f - gap) * 0.5f;

            if (page > 0)
            {
                if (GUI.Button(new Rect(panel.x + inner, navY, half, navH), "TILBAGE", buttonStyle)) page--;
            }
            else if (GUI.Button(new Rect(panel.x + inner, navY, half, navH), "LUK", buttonStyle))
            {
                CloseTutorial();
            }

            string nextLabel = page >= Titles.Length - 1 ? "KLAR!" : "NÆSTE";
            if (GUI.Button(new Rect(panel.x + inner + half + gap, navY, half, navH), nextLabel, buttonStyle))
            {
                if (page >= Titles.Length - 1) CloseTutorial(); else page++;
            }

            GUI.Label(new Rect(panel.x + inner, navY - panel.height * 0.07f, panel.width - inner * 2f, panel.height * 0.05f),
                (page + 1) + " / " + Titles.Length, pageStyle);
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
            if (titleStyle != null) return;
            int reference = Mathf.Min(Screen.width, Screen.height);
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 14, 28, 60), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = Color.white;
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 26, 18, 36), alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bodyStyle.normal.textColor = new Color(0.90f, 0.92f, 0.98f);
            pageStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 18, 24, 50), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            pageStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(reference / 34, 15, 28), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }
    }
}
