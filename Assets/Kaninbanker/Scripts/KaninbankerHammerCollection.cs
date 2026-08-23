using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Portrait-safe 2D hammer collection. Hammer skins unlock from existing Kaninbanker XP,
    /// so the system adds long-term collection progression without introducing another currency.
    /// Imported hammer sprites are selected by KaninbankerHammer2D when available.
    /// </summary>
    public sealed class KaninbankerHammerCollection : MonoBehaviour
    {
        public const string SelectedHammerKey = "Kaninbanker2D.SelectedHammer";
        private const string XpKey = "Kaninbanker2D.Xp";

        private static readonly string[] Names =
        {
            "START HAMMER",
            "NEON BONK",
            "GULDHAMMER",
            "SUKKER SMASH",
            "LAVA KNUSE",
            "FROST BANKER",
            "GALAKSE HAMMER",
            "BOSS BREAKER",
            "TURBO MALLET",
            "ROYAL BONK",
            "VOID CRUSHER",
            "KANINKEJSER"
        };

        private static readonly int[] UnlockXp =
        {
            0, 250, 600, 1000, 1500, 2200, 3000, 4000, 5200, 6600, 8200, 10000
        };

        private KaninbankerGame2D game;
        private bool open;
        private Vector2 scroll;
        private GUIStyle titleStyle;
        private GUIStyle itemStyle;
        private GUIStyle smallStyle;

        public static int SelectedIndex
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(SelectedHammerKey, 0), 0, Names.Length - 1);
            set
            {
                int safe = Mathf.Clamp(value, 0, Names.Length - 1);
                if (!IsUnlocked(safe))
                    return;
                PlayerPrefs.SetInt(SelectedHammerKey, safe);
                PlayerPrefs.Save();
            }
        }

        public static int Count => Names.Length;
        public static string GetName(int index) => Names[Mathf.Clamp(index, 0, Names.Length - 1)];
        public static int GetUnlockXp(int index) => UnlockXp[Mathf.Clamp(index, 0, UnlockXp.Length - 1)];
        public static bool IsUnlocked(int index) => PlayerPrefs.GetInt(XpKey, 0) >= GetUnlockXp(index);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerHammerCollection>() != null)
                return;

            GameObject go = new GameObject("KaninbankerHammerCollection");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerHammerCollection>();
        }

        private void Update()
        {
            if (game == null)
                game = FindFirstObjectByType<KaninbankerGame2D>();
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float margin = Mathf.Max(14f, safe.width * 0.035f);

            if (!open)
            {
                Rect button = new Rect(safe.x + margin, safe.y + safe.height * 0.82f,
                    safe.width - margin * 2f, Mathf.Max(52f, safe.height * 0.065f));
                if (GUI.Button(button, "🔨 HAMMER COLLECTION  " + (SelectedIndex + 1) + "/" + Count, itemStyle))
                    open = true;
                return;
            }

            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.08f,
                safe.width - margin * 2f, safe.height * 0.82f);
            GUI.Box(panel, GUIContent.none);

            float inner = Mathf.Max(12f, panel.width * 0.035f);
            GUI.Label(new Rect(panel.x + inner, panel.y + inner, panel.width - inner * 2f, 56f),
                "HAMMER COLLECTION", titleStyle);

            int xp = PlayerPrefs.GetInt(XpKey, 0);
            GUI.Label(new Rect(panel.x + inner, panel.y + 60f, panel.width - inner * 2f, 40f),
                "XP: " + xp + "   •   Vælg en oplåst 2D-hammer", smallStyle);

            if (GUI.Button(new Rect(panel.xMax - inner - 64f, panel.y + inner, 64f, 48f), "X"))
            {
                open = false;
                return;
            }

            Rect viewport = new Rect(panel.x + inner, panel.y + 108f,
                panel.width - inner * 2f, panel.height - 126f);
            float cellHeight = Mathf.Max(72f, safe.height * 0.085f);
            float contentHeight = Count * cellHeight + 8f;
            scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0f, 0f, viewport.width - 18f, contentHeight));

            for (int i = 0; i < Count; i++)
            {
                bool unlocked = IsUnlocked(i);
                bool selected = SelectedIndex == i;
                string status = selected ? "  ✓ VALGT" : unlocked ? "  • TRYK FOR AT VÆLGE" : "  🔒 KRÆVER " + GetUnlockXp(i) + " XP";
                Rect row = new Rect(0f, i * cellHeight, viewport.width - 24f, cellHeight - 8f);
                GUI.enabled = unlocked;
                if (GUI.Button(row, (i + 1).ToString("00") + "  " + GetName(i) + status, itemStyle) && unlocked)
                    SelectedIndex = i;
                GUI.enabled = true;
            }

            GUI.EndScrollView();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.width / 22, 20, 42),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = Color.white;

            itemStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(Screen.width / 34, 13, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.width / 42, 12, 22),
                alignment = TextAnchor.MiddleLeft
            };
            smallStyle.normal.textColor = new Color(0.85f, 0.90f, 1f);
        }
    }
}
