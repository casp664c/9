using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Persistent career book and collection wall. It turns lifetime play into long-term goals
    /// without requiring network services. Milestones unlock automatically from existing profile,
    /// Mayhem Pass and Event Circuit PlayerPrefs state.
    /// </summary>
    public sealed class KaninbankerCareerBook : MonoBehaviour
    {
        private struct Milestone
        {
            public string Name;
            public string Description;
            public int Current;
            public int Target;

            public Milestone(string name, string description, int current, int target)
            {
                Name = name;
                Description = description;
                Current = current;
                Target = target;
            }
        }

        private KaninbankerGame game;
        private bool panelOpen;
        private int page;
        private Vector2 scroll;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerCareerBook>() != null)
                return;

            GameObject go = new GameObject("KaninbankerCareerBook");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerCareerBook>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame>();
        }

        private void Update()
        {
            if (game == null)
                game = FindFirstObjectByType<KaninbankerGame>();
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect safeRaw = Screen.safeArea;
            Rect safe = new Rect(safeRaw.x, Screen.height - safeRaw.yMax, safeRaw.width, safeRaw.height);
            float margin = Mathf.Max(14f, safe.width * 0.03f);
            float tabW = Mathf.Min(safe.width * 0.31f, 220f);
            float tabH = Mathf.Max(52f, safe.height * 0.055f);
            float tabY = safe.y + safe.height * 0.275f;

            Rect tab = new Rect(safe.x + margin, tabY, tabW, tabH);
            if (GUI.Button(tab, panelOpen ? "LUK KARRIERE" : "KARRIERE", buttonStyle))
                panelOpen = !panelOpen;

            if (!panelOpen)
                return;

            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.105f,
                safe.width - margin * 2f, safe.height * 0.79f);
            DrawRect(panel, new Color(0.025f, 0.035f, 0.048f, 0.988f));

            float inner = Mathf.Max(16f, panel.width * 0.04f);
            GUI.Label(new Rect(panel.x + inner, panel.y + panel.height * 0.025f,
                panel.width - inner * 2f, panel.height * 0.075f), "KANINBANKER KARRIERE", titleStyle);

            float navY = panel.y + panel.height * 0.105f;
            float navH = panel.height * 0.065f;
            float gap = inner * 0.35f;
            float navW = (panel.width - inner * 2f - gap * 2f) / 3f;
            DrawNavButton(new Rect(panel.x + inner, navY, navW, navH), 0, "MILEPÆLE");
            DrawNavButton(new Rect(panel.x + inner + navW + gap, navY, navW, navH), 1, "SAMLING");
            DrawNavButton(new Rect(panel.x + inner + (navW + gap) * 2f, navY, navW, navH), 2, "REKORDER");

            Rect content = new Rect(panel.x + inner, navY + navH + panel.height * 0.025f,
                panel.width - inner * 2f, panel.height * 0.735f);

            if (page == 0)
                DrawMilestones(content);
            else if (page == 1)
                DrawCollection(content);
            else
                DrawRecords(content);
        }

        private void DrawNavButton(Rect rect, int targetPage, string label)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = page == targetPage ? new Color(0.95f, 0.55f, 0.12f) : new Color(0.20f, 0.23f, 0.30f);
            if (GUI.Button(rect, label, buttonStyle))
                page = targetPage;
            GUI.backgroundColor = old;
        }

        private void DrawMilestones(Rect content)
        {
            Milestone[] milestones = BuildMilestones();
            int completed = 0;
            for (int i = 0; i < milestones.Length; i++)
                if (milestones[i].Current >= milestones[i].Target) completed++;

            float headerH = content.height * 0.11f;
            GUI.Label(new Rect(content.x, content.y, content.width, headerH * 0.45f),
                completed + " / " + milestones.Length + " MILEPÆLE KLARET", headingStyle);
            DrawProgress(new Rect(content.x, content.y + headerH * 0.55f, content.width, headerH * 0.18f),
                milestones.Length > 0 ? completed / (float)milestones.Length : 0f);

            float rowY = content.y + headerH;
            float rowH = content.height * 0.083f;
            for (int i = 0; i < milestones.Length; i++)
            {
                Milestone m = milestones[i];
                bool done = m.Current >= m.Target;
                Rect row = new Rect(content.x, rowY, content.width, rowH);
                DrawRect(row, done ? new Color(0.08f, 0.24f, 0.15f, 0.94f) : new Color(0.07f, 0.085f, 0.12f, 0.94f));
                GUI.Label(new Rect(row.x + 10f, row.y, row.width * 0.46f, row.height), m.Name, bodyStyle);
                GUI.Label(new Rect(row.x + row.width * 0.47f, row.y, row.width * 0.34f, row.height), m.Description, smallStyle);
                GUI.Label(new Rect(row.x + row.width * 0.81f, row.y, row.width * 0.18f, row.height),
                    done ? "✓" : Mathf.Min(m.Current, m.Target) + "/" + m.Target, smallStyle);
                rowY += rowH + content.height * 0.012f;
            }
        }

        private Milestone[] BuildMilestones()
        {
            int hits = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalHits", 0);
            int rounds = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalRounds", 0);
            int combo = PlayerPrefs.GetInt("Kaninbanker.Profile.BestCombo", 0);
            int xp = PlayerPrefs.GetInt("Kaninbanker.Profile.Xp", 0);
            int trophies = PlayerPrefs.GetInt("Kaninbanker.Profile.Trophies", 0);
            int eventPoints = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.LifetimePoints", 0);
            int medals = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0);
            int passXp = PlayerPrefs.GetInt("Kaninbanker.MayhemPass.PassXp", 0);
            int streak = PlayerPrefs.GetInt("Kaninbanker.MayhemPass.LoginStreak", 0);

            int level = 1 + xp / 450;
            int passLevel = 1 + passXp / 1000;

            return new[]
            {
                new Milestone("FØRSTE BANK", "Ram 25 kaniner", hits, 25),
                new Milestone("100 BANK", "Ram 100 kaniner", hits, 100),
                new Milestone("MEGA BANKER", "Ram 500 kaniner", hits, 500),
                new Milestone("KANIN-MASKINE", "Ram 2.500 kaniner", hits, 2500),
                new Milestone("10 RUNDER", "Spil 10 runder", rounds, 10),
                new Milestone("100 RUNDER", "Spil 100 runder", rounds, 100),
                new Milestone("MARATONIST", "Spil 500 runder", rounds, 500),
                new Milestone("COMBO 10", "Bedste combo x10", combo, 10),
                new Milestone("COMBO 25", "Bedste combo x25", combo, 25),
                new Milestone("COMBO 50", "Bedste combo x50", combo, 50),
                new Milestone("LEVEL 10", "Nå level 10", level, 10),
                new Milestone("LEVEL 25", "Nå level 25", level, 25),
                new Milestone("LEVEL 50", "Nå level 50", level, 50),
                new Milestone("TROFÆJÆGER", "Saml 5 trofæer", trophies, 5),
                new Milestone("TROFÆKONGE", "Saml 15 trofæer", trophies, 15),
                new Milestone("EVENT START", "5.000 eventpoint", eventPoints, 5000),
                new Milestone("EVENT TITAN", "50.000 eventpoint", eventPoints, 50000),
                new Milestone("MEDALJEVÆG", "Saml 25 medaljer", medals, 25),
                new Milestone("PASS PRO", "Nå Mayhem Pass 10", passLevel, 10),
                new Milestone("PASS LEGENDE", "Nå Mayhem Pass 50", passLevel, 50),
                new Milestone("STREAK 7", "Log ind 7 dage i træk", streak, 7),
                new Milestone("STREAK 30", "Log ind 30 dage i træk", streak, 30)
            };
        }

        private void DrawCollection(Rect content)
        {
            int hits = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalHits", 0);
            int rounds = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalRounds", 0);
            int medals = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0);
            int passXp = PlayerPrefs.GetInt("Kaninbanker.MayhemPass.PassXp", 0);
            int level = 1 + PlayerPrefs.GetInt("Kaninbanker.Profile.Xp", 0) / 450;

            string[] names =
            {
                "TRÆHAMMER", "JERNHAMMER", "GULDHAMMER", "NEONHAMMER",
                "LAVAHAMMER", "BOSSHAMMER", "KEJSERHAMMER", "KOSMISK HAMMER",
                "ENG-MÆRKE", "NAT-MÆRKE", "SUKKER-MÆRKE", "LAVA-MÆRKE",
                "TURBO KRONE", "MARATHON KRONE", "BOSS KRONE", "MAYHEM KRONE",
                "KANINJÆGER", "MEGABANKER", "EVENT TITAN", "PASS LEGENDE",
                "COMBO LORD", "TROFÆKONGE", "ARENA MESTER", "KANINKEJSER"
            };

            float gap = content.width * 0.025f;
            float cellW = (content.width - gap * 2f) / 3f;
            float cellH = content.height * 0.145f;

            for (int i = 0; i < names.Length; i++)
            {
                int row = i / 3;
                int col = i % 3;
                Rect cell = new Rect(content.x + col * (cellW + gap), content.y + row * (cellH + gap * 0.8f), cellW, cellH);
                bool unlocked = IsCollectionUnlocked(i, hits, rounds, medals, passXp, level);
                DrawRect(cell, unlocked ? new Color(0.18f, 0.12f, 0.04f, 0.96f) : new Color(0.055f, 0.06f, 0.08f, 0.96f));
                GUI.Label(new Rect(cell.x + 5f, cell.y + cell.height * 0.10f, cell.width - 10f, cell.height * 0.52f),
                    unlocked ? names[i] : "???", bodyStyle);
                GUI.Label(new Rect(cell.x + 5f, cell.y + cell.height * 0.61f, cell.width - 10f, cell.height * 0.30f),
                    unlocked ? "LÅST OP" : GetUnlockHint(i), smallStyle);
            }
        }

        private static bool IsCollectionUnlocked(int index, int hits, int rounds, int medals, int passXp, int level)
        {
            if (index < 4) return hits >= index * 100 + 25;
            if (index < 8) return hits >= (index - 3) * 500;
            if (index < 12) return rounds >= (index - 7) * 20;
            if (index < 16) return rounds >= (index - 11) * 75;
            if (index < 20) return medals >= (index - 15) * 5;
            if (index < 22) return passXp >= (index - 19) * 10000;
            return level >= (index - 20) * 10;
        }

        private static string GetUnlockHint(int index)
        {
            if (index < 8) return "FLERE HITS";
            if (index < 16) return "FLERE RUNDER";
            if (index < 20) return "EVENT MEDALJER";
            if (index < 22) return "MAYHEM PASS";
            return "HØJERE LEVEL";
        }

        private void DrawRecords(Rect content)
        {
            int hits = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalHits", 0);
            int rounds = PlayerPrefs.GetInt("Kaninbanker.Profile.TotalRounds", 0);
            int combo = PlayerPrefs.GetInt("Kaninbanker.Profile.BestCombo", 0);
            int trophies = PlayerPrefs.GetInt("Kaninbanker.Profile.Trophies", 0);
            int coins = PlayerPrefs.GetInt("Kaninbanker.Profile.Coins", 0);
            int xp = PlayerPrefs.GetInt("Kaninbanker.Profile.Xp", 0);
            int medals = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.Medals", 0);
            int eventPoints = PlayerPrefs.GetInt("Kaninbanker.EventCircuit.LifetimePoints", 0);

            string[] labels =
            {
                "TOTAL HITS", "TOTAL RUNDER", "BEDSTE COMBO", "TROFÆER",
                "MØNTER", "TOTAL XP", "EVENT MEDALJER", "EVENT POINT",
                "CLASSIC REKORD", "TURBO REKORD", "MARATHON REKORD", "BOSS RUSH REKORD"
            };
            int[] values =
            {
                hits, rounds, combo, trophies, coins, xp, medals, eventPoints,
                PlayerPrefs.GetInt("Kaninbanker.HighScore.Classic", 0),
                PlayerPrefs.GetInt("Kaninbanker.HighScore.Turbo", 0),
                PlayerPrefs.GetInt("Kaninbanker.HighScore.Marathon", 0),
                PlayerPrefs.GetInt("Kaninbanker.HighScore.BossRush", 0)
            };

            float rowH = content.height * 0.072f;
            for (int i = 0; i < labels.Length; i++)
            {
                Rect row = new Rect(content.x, content.y + i * rowH, content.width, rowH * 0.88f);
                DrawRect(row, i % 2 == 0 ? new Color(0.07f, 0.085f, 0.12f, 0.94f) : new Color(0.055f, 0.07f, 0.10f, 0.94f));
                GUI.Label(new Rect(row.x + 10f, row.y, row.width * 0.60f, row.height), labels[i], bodyStyle);
                GUI.Label(new Rect(row.x + row.width * 0.62f, row.y, row.width * 0.35f, row.height), values[i].ToString(), headingStyle);
            }
        }

        private static void DrawProgress(Rect rect, float progress)
        {
            DrawRect(rect, new Color(0f, 0f, 0f, 0.5f));
            Rect fill = rect;
            fill.width *= Mathf.Clamp01(progress);
            DrawRect(fill, new Color(1f, 0.55f, 0.12f, 1f));
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
                fontSize = Mathf.Clamp(reference / 13, 30, 68),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(1f, 0.68f, 0.18f);

            headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 23, 20, 42),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            headingStyle.normal.textColor = Color.white;

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 30, 16, 32),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            bodyStyle.normal.textColor = Color.white;

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 38, 13, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            smallStyle.normal.textColor = new Color(0.83f, 0.86f, 0.92f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 36, 14, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
