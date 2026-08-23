using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// True-2D career and collection panel. Keeps long-term goals visible outside active rounds.
    /// </summary>
    public sealed class KaninbankerCareerBook : MonoBehaviour
    {
        private KaninbankerGame2D game;
        private bool open;
        private int page;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerCareerBook>() != null) return;
            GameObject go = new GameObject("KaninbankerCareerBook");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerCareerBook>();
        }

        private void Start() => game = FindFirstObjectByType<KaninbankerGame2D>();
        private void Update() { if (game == null) game = FindFirstObjectByType<KaninbankerGame2D>(); }

        private void OnGUI()
        {
            if (game == null || game.IsRunning) return;
            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float m = Mathf.Max(14f, safe.width * 0.03f);
            float tw = Mathf.Min(safe.width * 0.31f, 220f);
            float th = Mathf.Max(52f, safe.height * 0.055f);
            if (GUI.Button(new Rect(safe.x + m, safe.y + safe.height * 0.275f, tw, th), open ? "LUK KARRIERE" : "KARRIERE 2D", buttonStyle)) open = !open;
            if (!open) return;

            Rect panel = new Rect(safe.x + m, safe.y + safe.height * 0.10f, safe.width - m * 2f, safe.height * 0.80f);
            Draw(panel, new Color(0.025f, 0.035f, 0.05f, 0.99f));
            float inner = Mathf.Max(16f, panel.width * 0.04f);
            GUI.Label(new Rect(panel.x + inner, panel.y + panel.height * 0.025f, panel.width - inner * 2f, panel.height * 0.08f), "KANINBANKER 2D KARRIERE", titleStyle);

            float navY = panel.y + panel.height * 0.11f;
            float navH = panel.height * 0.07f;
            float gap = inner * 0.35f;
            float navW = (panel.width - inner * 2f - gap * 2f) / 3f;
            Nav(new Rect(panel.x + inner, navY, navW, navH), 0, "MILEPÆLE");
            Nav(new Rect(panel.x + inner + navW + gap, navY, navW, navH), 1, "SAMLING");
            Nav(new Rect(panel.x + inner + (navW + gap) * 2f, navY, navW, navH), 2, "REKORDER");

            Rect content = new Rect(panel.x + inner, navY + navH + panel.height * 0.025f, panel.width - inner * 2f, panel.height * 0.70f);
            if (page == 0) DrawMilestones(content); else if (page == 1) DrawCollection(content); else DrawRecords(content);
        }

        private void Nav(Rect rect, int target, string label)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = page == target ? new Color(0.2f, 0.8f, 1f) : new Color(0.2f, 0.23f, 0.3f);
            if (GUI.Button(rect, label, buttonStyle)) page = target;
            GUI.backgroundColor = old;
        }

        private void DrawMilestones(Rect c)
        {
            int score = game.HighScore;
            int xp = PlayerPrefs.GetInt("Kaninbanker2D.Xp", 0);
            int eventPoints = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.LifetimePoints", 0);
            int medals = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0);
            int passXp = PlayerPrefs.GetInt("Kaninbanker.MayhemPass.PassXp", 0);
            string[] labels = { "FØRSTE 50", "REKORD 100", "REKORD 250", "XP 5.000", "XP 25.000", "EVENT 5.000", "EVENT 50.000", "10 MEDALJER", "25 MEDALJER", "PASS LEVEL 10", "PASS LEVEL 25", "PASS LEVEL 50" };
            int[] current = { score, score, score, xp, xp, eventPoints, eventPoints, medals, medals, passXp / 1000, passXp / 1000, passXp / 1000 };
            int[] target = { 50, 100, 250, 5000, 25000, 5000, 50000, 10, 25, 10, 25, 50 };
            float rh = c.height / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                bool done = current[i] >= target[i];
                Rect r = new Rect(c.x, c.y + i * rh, c.width, rh * 0.88f);
                Draw(r, done ? new Color(0.08f, 0.24f, 0.15f, 0.94f) : new Color(0.07f, 0.085f, 0.12f, 0.94f));
                GUI.Label(new Rect(r.x + 8f, r.y, r.width * 0.55f, r.height), labels[i], bodyStyle);
                GUI.Label(new Rect(r.x + r.width * 0.58f, r.y, r.width * 0.38f, r.height), done ? "✓ KLAR" : Mathf.Min(current[i], target[i]) + "/" + target[i], smallStyle);
            }
        }

        private void DrawCollection(Rect c)
        {
            int xp = PlayerPrefs.GetInt("Kaninbanker2D.Xp", 0);
            int medals = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0);
            int passLevel = 1 + PlayerPrefs.GetInt("Kaninbanker.MayhemPass.PassXp", 0) / 1000;
            string[] items = { "TRÆHAMMER", "JERNHAMMER", "GULDHAMMER", "NEONHAMMER", "LAVAHAMMER", "BOSSHAMMER", "2D KRONE", "NEON KRONE", "LAVA KRONE", "EVENT MESTER", "PASS MESTER", "KANINKEJSER" };
            float gap = c.width * 0.025f;
            float w = (c.width - gap * 2f) / 3f;
            float h = c.height * 0.22f;
            for (int i = 0; i < items.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                bool unlocked = i < 6 ? xp >= (i + 1) * 1200 : i < 10 ? medals >= (i - 5) * 3 : passLevel >= (i - 8) * 10;
                Rect r = new Rect(c.x + col * (w + gap), c.y + row * (h + gap), w, h);
                Draw(r, unlocked ? new Color(0.17f, 0.12f, 0.04f, 0.96f) : new Color(0.055f, 0.06f, 0.08f, 0.96f));
                GUI.Label(new Rect(r.x + 5f, r.y + r.height * 0.12f, r.width - 10f, r.height * 0.45f), unlocked ? items[i] : "???", bodyStyle);
                GUI.Label(new Rect(r.x + 5f, r.y + r.height * 0.60f, r.width - 10f, r.height * 0.25f), unlocked ? "LÅST OP" : "SPIL MERE", smallStyle);
            }
        }

        private void DrawRecords(Rect c)
        {
            string[] labels = { "2D REKORD", "2D XP", "2D MØNTER", "EVENT POINT", "EVENT MEDALJER", "MAYHEM PASS XP", "WORLD TOUR STAGE" };
            int[] values = { game.HighScore, PlayerPrefs.GetInt("Kaninbanker2D.Xp", 0), PlayerPrefs.GetInt("Kaninbanker2D.Coins", 0), PlayerPrefs.GetInt("Kaninbanker.EventCircuit.LifetimePoints", 0), PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0), PlayerPrefs.GetInt("Kaninbanker.MayhemPass.PassXp", 0), PlayerPrefs.GetInt("Kaninbanker.WorldTour.Unlocked", 0) + 1 };
            float rh = c.height / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                Rect r = new Rect(c.x, c.y + i * rh, c.width, rh * 0.86f);
                Draw(r, i % 2 == 0 ? new Color(0.07f, 0.085f, 0.12f, 0.94f) : new Color(0.055f, 0.07f, 0.10f, 0.94f));
                GUI.Label(new Rect(r.x + 10f, r.y, r.width * 0.58f, r.height), labels[i], bodyStyle);
                GUI.Label(new Rect(r.x + r.width * 0.60f, r.y, r.width * 0.35f, r.height), values[i].ToString(), smallStyle);
            }
        }

        private static void Draw(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            int r = Mathf.Min(Screen.width, Screen.height);
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(r / 14, 28, 60), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = new Color(0.25f, 0.88f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(r / 30, 16, 32), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bodyStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(r / 38, 13, 25), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            smallStyle.normal.textColor = new Color(0.88f, 0.91f, 0.98f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(r / 38, 13, 26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }
    }
}
