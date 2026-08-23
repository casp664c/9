using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Offline live-ops style event circuit. A new themed event rotates every three UTC days.
    /// Rounds earn Event Points, milestone chests, medals and a persistent event streak.
    /// </summary>
    public sealed class KaninbankerEventCircuit : MonoBehaviour
    {
        private const string Prefix = "Kaninbanker.EventCircuit.";
        private const int RotationDays = 3;

        private static readonly string[] EventNames =
        {
            "GULD-KANIN FEVER", "BOSS APOKALYPSE", "TURBO TUNNEL", "LAVA MAYHEM",
            "NEON NAT", "BOMBE PANIK", "MEGA MARATHON", "KANIN KEJSER CUP"
        };

        private static readonly string[] EventDescriptions =
        {
            "Jag høje scores og fyld eventmåleren med guldpoint.",
            "Hver runde tæller mod den globale boss-medalje.",
            "Korte, aggressive runs giver ekstra eventprogression.",
            "Overlev kaos og byg en lang combo-streak.",
            "Neon-event med fokus på præcision og hurtige hits.",
            "Undgå fejl, hold comboen i live og jag eventpoint.",
            "Lange runs giver den største eventpoint-bank.",
            "Saml medaljer og arbejd mod kejser-rangen."
        };

        private KaninbankerGame2D game;
        private bool panelOpen;
        private bool wasRunning;
        private int eventId;
        private int eventEpoch;
        private int eventPoints;
        private int lifetimePoints;
        private int medals;
        private int eventRounds;
        private int eventBest;
        private int claimedMask;
        private int eventStreak;
        private float toastTime;
        private string toast = string.Empty;
        private GUIStyle titleStyle;
        private GUIStyle numberStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerEventCircuit>() != null) return;
            GameObject go = new GameObject("KaninbankerEventCircuit");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerEventCircuit>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            LoadState();
            if (game != null) wasRunning = game.IsRunning;
        }

        private void Update()
        {
            if (toastTime > 0f) toastTime -= Time.unscaledDeltaTime;
            if (game == null)
            {
                game = FindFirstObjectByType<KaninbankerGame2D>();
                return;
            }
            RefreshRotationIfNeeded();
            bool running = game.IsRunning;
            if (!running && wasRunning) RegisterRound(game.Score);
            wasRunning = running;
        }

        private static int CurrentDayId()
        {
            DateTime epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Mathf.Max(0, (int)(DateTime.UtcNow.Date - epoch).TotalDays);
        }

        private static int CurrentEpoch() => CurrentDayId() / RotationDays;

        private void LoadState()
        {
            eventEpoch = CurrentEpoch();
            int savedEpoch = PlayerPrefs.GetInt(Prefix + "Epoch", -1);
            lifetimePoints = PlayerPrefs.GetInt(Prefix + "LifetimePoints", 0);
            medals = PlayerPrefs.GetInt(Prefix + "Medals", 0);
            eventStreak = PlayerPrefs.GetInt(Prefix + "Streak", 0);
            if (savedEpoch != eventEpoch)
            {
                int previousEpoch = PlayerPrefs.GetInt(Prefix + "Epoch", eventEpoch - 2);
                eventStreak = previousEpoch == eventEpoch - 1 ? eventStreak + 1 : 1;
                ResetEventProgress();
            }
            else
            {
                eventPoints = PlayerPrefs.GetInt(Prefix + "Points", 0);
                eventRounds = PlayerPrefs.GetInt(Prefix + "Rounds", 0);
                eventBest = PlayerPrefs.GetInt(Prefix + "Best", 0);
                claimedMask = PlayerPrefs.GetInt(Prefix + "ClaimedMask", 0);
            }
            eventId = Mathf.Abs(eventEpoch) % EventNames.Length;
            Save();
        }

        private void RefreshRotationIfNeeded()
        {
            int now = CurrentEpoch();
            if (now == eventEpoch) return;
            eventEpoch = now;
            eventId = Mathf.Abs(eventEpoch) % EventNames.Length;
            eventStreak++;
            ResetEventProgress();
            ShowToast("NYT 2D EVENT: " + EventNames[eventId]);
        }

        private void ResetEventProgress()
        {
            eventPoints = 0;
            eventRounds = 0;
            eventBest = 0;
            claimedMask = 0;
            Save();
        }

        private void RegisterRound(int score)
        {
            score = Mathf.Max(0, score);
            eventRounds++;
            eventBest = Mathf.Max(eventBest, score);
            int points = 25 + Mathf.Clamp(score * 2, 0, 1000);
            if (score >= 100) points += 100;
            if (score >= 200) points += 200;
            if (eventRounds % 5 == 0) points += 150;
            eventPoints += points;
            lifetimePoints += points;
            ClaimMilestones();
            Save();
        }

        private void ClaimMilestones()
        {
            int[] thresholds = { 500, 1500, 3500, 7000, 12000 };
            for (int i = 0; i < thresholds.Length; i++)
            {
                int bit = 1 << i;
                if (eventPoints < thresholds[i] || (claimedMask & bit) != 0) continue;
                claimedMask |= bit;
                medals += i + 1;
                lifetimePoints += 250 * (i + 1);
                ShowToast("EVENT-KISTE " + (i + 1) + "  +" + (i + 1) + " MEDALJE");
            }
        }

        private void Save()
        {
            PlayerPrefs.SetInt(Prefix + "Epoch", eventEpoch);
            PlayerPrefs.SetInt(Prefix + "Points", eventPoints);
            PlayerPrefs.SetInt(Prefix + "Rounds", eventRounds);
            PlayerPrefs.SetInt(Prefix + "Best", eventBest);
            PlayerPrefs.SetInt(Prefix + "ClaimedMask", claimedMask);
            PlayerPrefs.SetInt(Prefix + "LifetimePoints", lifetimePoints);
            PlayerPrefs.SetInt(Prefix + "Medals", medals);
            PlayerPrefs.SetInt(Prefix + "Streak", eventStreak);
            PlayerPrefs.Save();
        }

        private void ShowToast(string message)
        {
            toast = message;
            toastTime = 2.4f;
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning) return;
            EnsureStyles();
            Rect safe = Screen.safeArea;
            Rect guiSafe = new Rect(safe.x, Screen.height - safe.yMax, safe.width, safe.height);
            float margin = Mathf.Max(14f, guiSafe.width * 0.03f);
            float tabW = Mathf.Min(guiSafe.width * 0.31f, 220f);
            float tabH = Mathf.Max(52f, guiSafe.height * 0.055f);
            Rect tab = new Rect(guiSafe.x + margin, guiSafe.y + guiSafe.height * 0.205f, tabW, tabH);
            if (GUI.Button(tab, panelOpen ? "LUK EVENT" : "MEGA EVENT", buttonStyle)) panelOpen = !panelOpen;

            if (toastTime > 0f)
            {
                Rect toastRect = new Rect(guiSafe.x + margin, guiSafe.y + guiSafe.height * 0.145f, guiSafe.width - margin * 2f, tabH);
                DrawRect(toastRect, new Color(0.18f, 0.04f, 0.03f, 0.96f));
                GUI.Label(toastRect, toast, smallStyle);
            }
            if (!panelOpen) return;

            Rect panel = new Rect(guiSafe.x + margin, guiSafe.y + guiSafe.height * 0.12f, guiSafe.width - margin * 2f, guiSafe.height * 0.76f);
            DrawRect(panel, new Color(0.035f, 0.025f, 0.045f, 0.988f));
            float inner = Mathf.Max(16f, panel.width * 0.04f);
            float y = panel.y + panel.height * 0.035f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.09f), EventNames[eventId] + " 2D", titleStyle);
            y += panel.height * 0.095f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.075f), EventDescriptions[eventId], bodyStyle);
            y += panel.height * 0.095f;

            float statW = (panel.width - inner * 2.4f) / 3f;
            DrawStat(new Rect(panel.x + inner, y, statW, panel.height * 0.12f), eventPoints.ToString(), "EVENT POINT");
            DrawStat(new Rect(panel.x + inner * 1.2f + statW, y, statW, panel.height * 0.12f), medals.ToString(), "MEDALJER");
            DrawStat(new Rect(panel.x + inner * 1.4f + statW * 2f, y, statW, panel.height * 0.12f), eventStreak.ToString(), "STREAK");
            y += panel.height * 0.15f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.05f), "RUNDER " + eventRounds + "   •   BEDSTE SCORE " + eventBest + "   •   LIVSTIDSPOINT " + lifetimePoints, smallStyle);
            y += panel.height * 0.075f;

            int[] thresholds = { 500, 1500, 3500, 7000, 12000 };
            for (int i = 0; i < thresholds.Length; i++)
            {
                bool claimed = (claimedMask & (1 << i)) != 0;
                float rowH = panel.height * 0.085f;
                Rect row = new Rect(panel.x + inner, y, panel.width - inner * 2f, rowH);
                DrawRect(row, claimed ? new Color(0.10f, 0.25f, 0.13f, 0.92f) : new Color(0.11f, 0.08f, 0.13f, 0.94f));
                GUI.Label(new Rect(row.x + inner * 0.45f, row.y, row.width * 0.52f, row.height), "KISTE " + (i + 1) + "  •  " + thresholds[i] + " POINT", smallStyle);
                GUI.Label(new Rect(row.x + row.width * 0.56f, row.y, row.width * 0.40f, row.height), claimed ? "✓ HENTET" : Mathf.Min(eventPoints, thresholds[i]) + "/" + thresholds[i], smallStyle);
                y += rowH + panel.height * 0.012f;
            }
        }

        private void DrawStat(Rect rect, string number, string label)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height * 0.60f), number, numberStyle);
            GUI.Label(new Rect(rect.x, rect.y + rect.height * 0.55f, rect.width, rect.height * 0.40f), label, smallStyle);
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
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 13, 30, 68), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            titleStyle.normal.textColor = new Color(1f, 0.82f, 0.20f);
            numberStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 18, 24, 50), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            numberStyle.normal.textColor = Color.white;
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 28, 17, 34), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bodyStyle.normal.textColor = new Color(0.92f, 0.93f, 1f);
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 34, 15, 28), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            smallStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(reference / 36, 14, 26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }
    }
}
