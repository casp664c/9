using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Persistent meta-game layer for the portrait build. It tracks daily play, score missions,
    /// login streaks and a long-running Mayhem Pass without requiring a backend or extra packages.
    /// The panel only appears outside active gameplay, so it does not cover the tap arena.
    /// </summary>
    public sealed class KaninbankerMayhemPass : MonoBehaviour
    {
        private const string Prefix = "Kaninbanker.MayhemPass.";
        private const int PassXpPerLevel = 1000;
        private const int MaxPassLevel = 50;

        private KaninbankerGame game;
        private bool panelOpen;
        private bool wasRunning;
        private int lastScore;
        private int dayId;
        private int dailyRounds;
        private int dailyScore;
        private int dailyBest;
        private int passXp;
        private int tokens;
        private int loginStreak;
        private string banner = string.Empty;
        private float bannerTime;

        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;
        private GUIStyle numberStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerMayhemPass>() != null)
                return;

            GameObject go = new GameObject("KaninbankerMayhemPass");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerMayhemPass>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame>();
            LoadOrRollDailyState();
            ApplyDailyLogin();
            if (game != null)
            {
                wasRunning = game.IsRunning;
                lastScore = game.Score;
            }
        }

        private void Update()
        {
            if (bannerTime > 0f)
                bannerTime -= Time.unscaledDeltaTime;

            if (game == null)
            {
                game = FindFirstObjectByType<KaninbankerGame>();
                return;
            }

            bool running = game.IsRunning;
            if (running)
            {
                if (!wasRunning)
                    lastScore = game.Score;

                if (game.Score > lastScore)
                    lastScore = game.Score;
            }
            else if (wasRunning)
            {
                FinishTrackedRound(lastScore);
            }

            wasRunning = running;
        }

        private static int CurrentDayId()
        {
            DateTime epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Mathf.Max(0, (int)(DateTime.UtcNow.Date - epoch).TotalDays);
        }

        private void LoadOrRollDailyState()
        {
            dayId = CurrentDayId();
            int savedDay = PlayerPrefs.GetInt(Prefix + "Day", -1);
            passXp = PlayerPrefs.GetInt(Prefix + "PassXp", 0);
            tokens = PlayerPrefs.GetInt(Prefix + "Tokens", 0);
            loginStreak = PlayerPrefs.GetInt(Prefix + "LoginStreak", 0);

            if (savedDay != dayId)
            {
                dailyRounds = 0;
                dailyScore = 0;
                dailyBest = 0;
                PlayerPrefs.SetInt(Prefix + "Day", dayId);
                PlayerPrefs.SetInt(Prefix + "DailyRounds", 0);
                PlayerPrefs.SetInt(Prefix + "DailyScore", 0);
                PlayerPrefs.SetInt(Prefix + "DailyBest", 0);
                PlayerPrefs.SetInt(Prefix + "ClaimRounds", 0);
                PlayerPrefs.SetInt(Prefix + "ClaimScore", 0);
                PlayerPrefs.SetInt(Prefix + "ClaimBest", 0);
            }
            else
            {
                dailyRounds = PlayerPrefs.GetInt(Prefix + "DailyRounds", 0);
                dailyScore = PlayerPrefs.GetInt(Prefix + "DailyScore", 0);
                dailyBest = PlayerPrefs.GetInt(Prefix + "DailyBest", 0);
            }
        }

        private void ApplyDailyLogin()
        {
            int lastLogin = PlayerPrefs.GetInt(Prefix + "LastLoginDay", -1000);
            if (lastLogin == dayId)
                return;

            loginStreak = lastLogin == dayId - 1 ? loginStreak + 1 : 1;
            int reward = 40 + Mathf.Min(160, loginStreak * 10);
            tokens += reward;
            passXp += 120;

            PlayerPrefs.SetInt(Prefix + "LastLoginDay", dayId);
            PlayerPrefs.SetInt(Prefix + "LoginStreak", loginStreak);
            SaveMeta();
            ShowBanner("DAGLIG BONUS +" + reward + " MAYHEM TOKENS");
        }

        private void FinishTrackedRound(int finalScore)
        {
            dailyRounds++;
            dailyScore += Mathf.Max(0, finalScore);
            dailyBest = Mathf.Max(dailyBest, finalScore);

            int earnedXp = 80 + Mathf.Clamp(finalScore * 2, 0, 900);
            passXp += earnedXp;
            tokens += Mathf.Clamp(finalScore / 4, 5, 120);

            PlayerPrefs.SetInt(Prefix + "DailyRounds", dailyRounds);
            PlayerPrefs.SetInt(Prefix + "DailyScore", dailyScore);
            PlayerPrefs.SetInt(Prefix + "DailyBest", dailyBest);

            TryAutoClaim("ClaimRounds", dailyRounds >= 3, 250, 200, "3 RUNDER KLARET");
            TryAutoClaim("ClaimScore", dailyScore >= 300, 350, 260, "300 DAGLIG SCORE");
            TryAutoClaim("ClaimBest", dailyBest >= 120, 500, 350, "120 SCORE I EN RUNDE");
            SaveMeta();
        }

        private void TryAutoClaim(string key, bool complete, int tokenReward, int xpReward, string label)
        {
            if (!complete || PlayerPrefs.GetInt(Prefix + key, 0) != 0)
                return;

            PlayerPrefs.SetInt(Prefix + key, 1);
            tokens += tokenReward;
            passXp += xpReward;
            ShowBanner(label + "  +" + tokenReward + " TOKENS");
        }

        private void SaveMeta()
        {
            PlayerPrefs.SetInt(Prefix + "PassXp", passXp);
            PlayerPrefs.SetInt(Prefix + "Tokens", tokens);
            PlayerPrefs.SetInt(Prefix + "LoginStreak", loginStreak);
            PlayerPrefs.Save();
        }

        private void ShowBanner(string text)
        {
            banner = text;
            bannerTime = 2.2f;
        }

        private int PassLevel => Mathf.Clamp(1 + passXp / PassXpPerLevel, 1, MaxPassLevel);
        private int PassLevelXp => passXp % PassXpPerLevel;

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect safe = Screen.safeArea;
            Rect guiSafe = new Rect(safe.x, Screen.height - safe.yMax, safe.width, safe.height);
            float margin = Mathf.Max(14f, guiSafe.width * 0.03f);

            float tabW = Mathf.Min(guiSafe.width * 0.31f, 220f);
            float tabH = Mathf.Max(52f, guiSafe.height * 0.055f);
            Rect tab = new Rect(guiSafe.xMax - margin - tabW, guiSafe.y + guiSafe.height * 0.205f, tabW, tabH);
            if (GUI.Button(tab, panelOpen ? "LUK PASS" : "MAYHEM PASS", buttonStyle))
                panelOpen = !panelOpen;

            if (bannerTime > 0f)
            {
                Rect toast = new Rect(guiSafe.x + margin, guiSafe.y + guiSafe.height * 0.15f, guiSafe.width - margin * 2f, tabH);
                DrawRect(toast, new Color(0.08f, 0.04f, 0.14f, 0.94f));
                GUI.Label(toast, banner, smallStyle);
            }

            if (!panelOpen)
                return;

            Rect panel = new Rect(guiSafe.x + margin, guiSafe.y + guiSafe.height * 0.12f,
                guiSafe.width - margin * 2f, guiSafe.height * 0.76f);
            DrawRect(panel, new Color(0.025f, 0.03f, 0.055f, 0.985f));

            float inner = Mathf.Max(16f, panel.width * 0.04f);
            float y = panel.y + inner * 0.6f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.09f), "MAYHEM PASS", titleStyle);
            y += panel.height * 0.085f;

            GUI.Label(new Rect(panel.x + inner, y, panel.width * 0.48f, panel.height * 0.07f), "LEVEL " + PassLevel, numberStyle);
            GUI.Label(new Rect(panel.x + panel.width * 0.52f, y, panel.width * 0.43f, panel.height * 0.07f), tokens + " TOKENS", numberStyle);
            y += panel.height * 0.075f;

            DrawProgress(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.022f), PassLevelXp / (float)PassXpPerLevel);
            y += panel.height * 0.045f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.05f),
                PassLevelXp + " / " + PassXpPerLevel + " XP  •  LOGIN STREAK " + loginStreak + " DAGE", smallStyle);
            y += panel.height * 0.075f;

            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.06f), "DAGLIGE MAYHEM-MISSIONER", bodyStyle);
            y += panel.height * 0.065f;

            y = DrawMission(panel, inner, y, "SPIL 3 RUNDER", dailyRounds, 3, "ClaimRounds");
            y = DrawMission(panel, inner, y, "SAML 300 SCORE", dailyScore, 300, "ClaimScore");
            y = DrawMission(panel, inner, y, "RAM 120 SCORE I ÉN RUNDE", dailyBest, 120, "ClaimBest");

            y += panel.height * 0.025f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.055f), "SÆSON 1: KANIN-KAOS", bodyStyle);
            y += panel.height * 0.06f;
            string unlock = PassLevel >= 40 ? "MYTISK" : PassLevel >= 25 ? "ELITE" : PassLevel >= 10 ? "PRO" : "ROOKIE";
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.10f),
                "Sæsonrang: " + unlock + "\n50 levels • daglige missioner • login streak • permanente statistikker", smallStyle);
        }

        private float DrawMission(Rect panel, float inner, float y, string label, int value, int target, string claimKey)
        {
            float rowH = panel.height * 0.09f;
            bool done = value >= target;
            bool claimed = PlayerPrefs.GetInt(Prefix + claimKey, 0) != 0;
            Rect row = new Rect(panel.x + inner, y, panel.width - inner * 2f, rowH);
            DrawRect(row, done ? new Color(0.08f, 0.24f, 0.16f, 0.90f) : new Color(0.08f, 0.09f, 0.14f, 0.92f));
            string suffix = claimed ? "  ✓ BELØNNET" : done ? "  ✓ KLAR" : "  " + Mathf.Min(value, target) + "/" + target;
            GUI.Label(new Rect(row.x + inner * 0.5f, row.y, row.width - inner, row.height), label + suffix, smallStyle);
            return y + rowH + panel.height * 0.018f;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void DrawProgress(Rect rect, float value)
        {
            DrawRect(rect, new Color(0f, 0f, 0f, 0.55f));
            Rect fill = rect;
            fill.width *= Mathf.Clamp01(value);
            DrawRect(fill, new Color(0.78f, 0.22f, 1f, 1f));
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
            titleStyle.normal.textColor = Color.white;

            numberStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 22, 21, 42),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            numberStyle.normal.textColor = new Color(0.94f, 0.82f, 1f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 27, 18, 34),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            bodyStyle.normal.textColor = Color.white;

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 34, 15, 28),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            smallStyle.normal.textColor = new Color(0.92f, 0.93f, 1f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 36, 14, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
