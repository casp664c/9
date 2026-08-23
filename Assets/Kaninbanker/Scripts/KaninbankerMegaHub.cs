using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Portrait-safe meta progression hub for the TRUE-2D Android game.
    /// Adds daily quests, permanent reward upgrades, a daily vault and a 24-entry rabbit codex.
    /// It stays outside active gameplay and only uses existing Kaninbanker PlayerPrefs/progression.
    /// </summary>
    public sealed class KaninbankerMegaHub : MonoBehaviour
    {
        private const string CoinsKey = "Kaninbanker2D.Coins";
        private const string XpKey = "Kaninbanker2D.Xp";
        private const string CoinUpgradeKey = "Kaninbanker.MegaHub.CoinUpgrade";
        private const string XpUpgradeKey = "Kaninbanker.MegaHub.XpUpgrade";
        private const string QuestUpgradeKey = "Kaninbanker.MegaHub.QuestUpgrade";
        private const string VaultUpgradeKey = "Kaninbanker.MegaHub.VaultUpgrade";
        private const string DailyStampKey = "Kaninbanker.MegaHub.DailyStamp";
        private const string DailyRoundsKey = "Kaninbanker.MegaHub.DailyRounds";
        private const string DailyScoreKey = "Kaninbanker.MegaHub.DailyScore";
        private const string DailyBestKey = "Kaninbanker.MegaHub.DailyBest";
        private const string VaultClaimKey = "Kaninbanker.MegaHub.VaultClaim";

        private static readonly string[] CodexNames =
        {
            "ENG-KANIN", "TURBO-KANIN", "GULDKANIN", "PANser-KANIN",
            "BOMBE-KANIN", "BOSS-KANIN", "NEON-SPRINTER", "SUKKERHOPPER",
            "LAVA-LØBER", "FROST-KANIN", "GALAKSE-KANIN", "ROYAL KANIN",
            "VOID-HOPPER", "MEGA PANser", "JACKPOT-KANIN", "NATTE-KANIN",
            "STORM-KANIN", "TITAN-KANIN", "KAOS-KANIN", "KRYSTAL-KANIN",
            "TURBO BOSS", "VOID BOSS", "GULDKEJSER", "KANINKEJSER"
        };

        private static readonly int[] CodexXp =
        {
            0, 150, 350, 600, 900, 1250, 1650, 2100,
            2600, 3150, 3750, 4400, 5100, 5850, 6650, 7500,
            8400, 9350, 10350, 11400, 12500, 13700, 15000, 16500
        };

        private KaninbankerGame2D game;
        private bool wasRunning;
        private int roundStartCoins;
        private int roundStartXp;
        private bool open;
        private int tab;
        private Vector2 scroll;
        private string toast = string.Empty;
        private float toastTime;

        private GUIStyle titleStyle;
        private GUIStyle tabStyle;
        private GUIStyle rowStyle;
        private GUIStyle smallStyle;
        private GUIStyle centerStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerMegaHub>() != null)
                return;

            GameObject go = new GameObject("KaninbankerMegaHub");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerMegaHub>();
        }

        private void Start()
        {
            EnsureDailyState();
            ResolveGame();
        }

        private void Update()
        {
            if (game == null)
                ResolveGame();

            EnsureDailyState();
            if (toastTime > 0f)
                toastTime -= Time.unscaledDeltaTime;

            if (game == null)
                return;

            bool running = game.IsRunning;
            if (!wasRunning && running)
            {
                roundStartCoins = PlayerPrefs.GetInt(CoinsKey, 0);
                roundStartXp = PlayerPrefs.GetInt(XpKey, 0);
                open = false;
            }
            else if (wasRunning && !running)
            {
                OnRoundFinished();
            }

            wasRunning = running;
        }

        private void ResolveGame()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            wasRunning = game != null && game.IsRunning;
        }

        private void OnRoundFinished()
        {
            int score = game != null ? Mathf.Max(0, game.Score) : 0;
            PlayerPrefs.SetInt(DailyRoundsKey, PlayerPrefs.GetInt(DailyRoundsKey, 0) + 1);
            PlayerPrefs.SetInt(DailyScoreKey, PlayerPrefs.GetInt(DailyScoreKey, 0) + score);
            PlayerPrefs.SetInt(DailyBestKey, Mathf.Max(PlayerPrefs.GetInt(DailyBestKey, 0), score));

            int currentCoins = PlayerPrefs.GetInt(CoinsKey, 0);
            int currentXp = PlayerPrefs.GetInt(XpKey, 0);
            int earnedCoins = Mathf.Max(0, currentCoins - roundStartCoins);
            int earnedXp = Mathf.Max(0, currentXp - roundStartXp);
            int coinLevel = PlayerPrefs.GetInt(CoinUpgradeKey, 0);
            int xpLevel = PlayerPrefs.GetInt(XpUpgradeKey, 0);
            int bonusCoins = Mathf.RoundToInt(earnedCoins * coinLevel * 0.08f);
            int bonusXp = Mathf.RoundToInt(earnedXp * xpLevel * 0.08f);

            if (bonusCoins > 0)
                PlayerPrefs.SetInt(CoinsKey, currentCoins + bonusCoins);
            if (bonusXp > 0)
                PlayerPrefs.SetInt(XpKey, currentXp + bonusXp);

            PlayerPrefs.Save();
            if (bonusCoins > 0 || bonusXp > 0)
                ShowToast("UPGRADE BONUS  +" + bonusCoins + " MØNTER  +" + bonusXp + " XP");
        }

        private static int TodayStamp()
        {
            DateTime epoch = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Mathf.Max(0, (int)(DateTime.UtcNow.Date - epoch).TotalDays);
        }

        private static void EnsureDailyState()
        {
            int today = TodayStamp();
            if (PlayerPrefs.GetInt(DailyStampKey, -1) == today)
                return;

            PlayerPrefs.SetInt(DailyStampKey, today);
            PlayerPrefs.SetInt(DailyRoundsKey, 0);
            PlayerPrefs.SetInt(DailyScoreKey, 0);
            PlayerPrefs.SetInt(DailyBestKey, 0);
            for (int i = 0; i < 3; i++)
                PlayerPrefs.DeleteKey(QuestClaimKey(today, i));
            PlayerPrefs.Save();
        }

        private static string QuestClaimKey(int day, int slot) => "Kaninbanker.MegaHub.QuestClaim." + day + "." + slot;

        private static int QuestVariant(int slot)
        {
            return (TodayStamp() + slot * 2) % 6;
        }

        private static string QuestName(int slot)
        {
            switch (QuestVariant(slot))
            {
                case 0: return "SPIL 3 RUNDER";
                case 1: return "SAMLET SCORE 120";
                case 2: return "FÅ 70 SCORE I ÉN RUNDE";
                case 3: return "SPIL 5 RUNDER";
                case 4: return "SAMLET SCORE 240";
                default: return "FÅ 120 SCORE I ÉN RUNDE";
            }
        }

        private static int QuestTarget(int slot)
        {
            switch (QuestVariant(slot))
            {
                case 0: return 3;
                case 1: return 120;
                case 2: return 70;
                case 3: return 5;
                case 4: return 240;
                default: return 120;
            }
        }

        private static int QuestProgress(int slot)
        {
            switch (QuestVariant(slot))
            {
                case 0:
                case 3: return PlayerPrefs.GetInt(DailyRoundsKey, 0);
                case 1:
                case 4: return PlayerPrefs.GetInt(DailyScoreKey, 0);
                default: return PlayerPrefs.GetInt(DailyBestKey, 0);
            }
        }

        private static int QuestCoinReward(int slot) => 90 + slot * 35;
        private static int QuestXpReward(int slot) => 120 + slot * 45;

        private void ClaimQuest(int slot)
        {
            int today = TodayStamp();
            string key = QuestClaimKey(today, slot);
            if (PlayerPrefs.GetInt(key, 0) != 0 || QuestProgress(slot) < QuestTarget(slot))
                return;

            int boost = PlayerPrefs.GetInt(QuestUpgradeKey, 0);
            float multiplier = 1f + boost * 0.10f;
            int coins = Mathf.RoundToInt(QuestCoinReward(slot) * multiplier);
            int xp = Mathf.RoundToInt(QuestXpReward(slot) * multiplier);
            PlayerPrefs.SetInt(CoinsKey, PlayerPrefs.GetInt(CoinsKey, 0) + coins);
            PlayerPrefs.SetInt(XpKey, PlayerPrefs.GetInt(XpKey, 0) + xp);
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            ShowToast("QUEST KLARET  +" + coins + " MØNTER  +" + xp + " XP");
        }

        private static int UpgradeCost(string key, int baseCost, int step)
        {
            return baseCost + PlayerPrefs.GetInt(key, 0) * step;
        }

        private void BuyUpgrade(string key, int maxLevel, int baseCost, int step, string label)
        {
            int level = PlayerPrefs.GetInt(key, 0);
            if (level >= maxLevel)
                return;

            int cost = UpgradeCost(key, baseCost, step);
            int coins = PlayerPrefs.GetInt(CoinsKey, 0);
            if (coins < cost)
            {
                ShowToast("MANGLER " + (cost - coins) + " MØNTER");
                return;
            }

            PlayerPrefs.SetInt(CoinsKey, coins - cost);
            PlayerPrefs.SetInt(key, level + 1);
            PlayerPrefs.Save();
            ShowToast(label + " LEVEL " + (level + 1));
        }

        private void ClaimVault()
        {
            int level = PlayerPrefs.GetInt(VaultUpgradeKey, 0);
            if (level <= 0)
            {
                ShowToast("LÅS DAILY VAULT OP I UPGRADES");
                return;
            }

            int today = TodayStamp();
            if (PlayerPrefs.GetInt(VaultClaimKey, -1) == today)
            {
                ShowToast("DAILY VAULT ER ALLEREDE HENTET");
                return;
            }

            int reward = 45 + level * 35;
            PlayerPrefs.SetInt(CoinsKey, PlayerPrefs.GetInt(CoinsKey, 0) + reward);
            PlayerPrefs.SetInt(VaultClaimKey, today);
            PlayerPrefs.Save();
            ShowToast("DAILY VAULT  +" + reward + " MØNTER");
        }

        private void ShowToast(string message)
        {
            toast = message;
            toastTime = 2.1f;
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning)
                return;

            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float margin = Mathf.Max(12f, safe.width * 0.03f);

            if (!open)
            {
                float h = Mathf.Max(50f, safe.height * 0.055f);
                Rect button = new Rect(safe.x + margin, safe.y + safe.height * 0.735f, safe.width - margin * 2f, h);
                if (GUI.Button(button, "MEGA HUB  •  QUESTS  •  UPGRADES  •  CODEX", rowStyle))
                {
                    open = true;
                    scroll = Vector2.zero;
                }
                DrawToast(safe);
                return;
            }

            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.045f, safe.width - margin * 2f, safe.height * 0.90f);
            DrawRect(panel, new Color(0.018f, 0.024f, 0.052f, 0.99f));
            float inner = Mathf.Max(12f, panel.width * 0.035f);
            GUI.Label(new Rect(panel.x + inner, panel.y + inner, panel.width - inner * 2f - 64f, 52f), "KANINBANKER MEGA HUB", titleStyle);
            if (GUI.Button(new Rect(panel.xMax - inner - 58f, panel.y + inner, 58f, 46f), "X"))
            {
                open = false;
                return;
            }

            int coins = PlayerPrefs.GetInt(CoinsKey, 0);
            int xp = PlayerPrefs.GetInt(XpKey, 0);
            GUI.Label(new Rect(panel.x + inner, panel.y + 58f, panel.width - inner * 2f, 34f), "MØNTER " + coins + "   •   XP " + xp, smallStyle);

            float tabY = panel.y + 94f;
            float gap = 6f;
            float tabW = (panel.width - inner * 2f - gap * 2f) / 3f;
            DrawTab(new Rect(panel.x + inner, tabY, tabW, 48f), 0, "DAILY");
            DrawTab(new Rect(panel.x + inner + tabW + gap, tabY, tabW, 48f), 1, "UPGRADES");
            DrawTab(new Rect(panel.x + inner + (tabW + gap) * 2f, tabY, tabW, 48f), 2, "CODEX");

            Rect content = new Rect(panel.x + inner, tabY + 58f, panel.width - inner * 2f, panel.height - 176f);
            if (tab == 0) DrawDaily(content);
            else if (tab == 1) DrawUpgrades(content);
            else DrawCodex(content);

            DrawToast(safe);
        }

        private void DrawTab(Rect rect, int index, string label)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = tab == index ? new Color(0.18f, 0.78f, 1f) : new Color(0.18f, 0.22f, 0.31f);
            if (GUI.Button(rect, label, tabStyle))
            {
                tab = index;
                scroll = Vector2.zero;
            }
            GUI.backgroundColor = old;
        }

        private void DrawDaily(Rect rect)
        {
            float rowH = Mathf.Max(78f, rect.height * 0.19f);
            for (int i = 0; i < 3; i++)
            {
                int progress = QuestProgress(i);
                int target = QuestTarget(i);
                bool claimed = PlayerPrefs.GetInt(QuestClaimKey(TodayStamp(), i), 0) != 0;
                Rect row = new Rect(rect.x, rect.y + i * (rowH + 8f), rect.width, rowH);
                DrawRect(row, new Color(0.055f, 0.075f, 0.12f, 0.98f));
                string status = claimed ? "HENTET" : progress >= target ? "HENT BELØNNING" : progress + "/" + target;
                string text = QuestName(i) + "\n" + status + "  •  +" + QuestCoinReward(i) + " MØNTER  +" + QuestXpReward(i) + " XP";
                GUI.enabled = !claimed && progress >= target;
                if (GUI.Button(new Rect(row.x + 6f, row.y + 6f, row.width - 12f, row.height - 12f), text, rowStyle))
                    ClaimQuest(i);
                GUI.enabled = true;
            }

            int vaultLevel = PlayerPrefs.GetInt(VaultUpgradeKey, 0);
            float vy = rect.y + 3f * (rowH + 8f) + 4f;
            string vault = vaultLevel <= 0 ? "DAILY VAULT  •  LÅST" : "DAILY VAULT LEVEL " + vaultLevel + "  •  HENT DAGENS MØNTER";
            if (GUI.Button(new Rect(rect.x, vy, rect.width, Mathf.Max(58f, rowH * 0.72f)), vault, rowStyle))
                ClaimVault();
        }

        private void DrawUpgrades(Rect rect)
        {
            string[] keys = { CoinUpgradeKey, XpUpgradeKey, QuestUpgradeKey, VaultUpgradeKey };
            string[] names = { "COIN BOOST", "XP BOOST", "QUEST BOOST", "DAILY VAULT" };
            string[] effects = { "+8% rundemønter pr. level", "+8% runde-XP pr. level", "+10% quest-belønning pr. level", "Større daglig møntkiste" };
            int[] max = { 10, 10, 5, 5 };
            int[] baseCost = { 180, 180, 260, 320 };
            int[] step = { 140, 140, 210, 260 };
            float rowH = Mathf.Max(82f, rect.height * 0.20f);

            for (int i = 0; i < keys.Length; i++)
            {
                int level = PlayerPrefs.GetInt(keys[i], 0);
                int cost = UpgradeCost(keys[i], baseCost[i], step[i]);
                string state = level >= max[i] ? "MAX" : cost + " MØNTER";
                Rect row = new Rect(rect.x, rect.y + i * (rowH + 7f), rect.width, rowH);
                DrawRect(row, new Color(0.055f, 0.075f, 0.12f, 0.98f));
                string label = names[i] + "  LV " + level + "/" + max[i] + "\n" + effects[i] + "  •  " + state;
                GUI.enabled = level < max[i];
                if (GUI.Button(new Rect(row.x + 6f, row.y + 6f, row.width - 12f, row.height - 12f), label, rowStyle))
                    BuyUpgrade(keys[i], max[i], baseCost[i], step[i], names[i]);
                GUI.enabled = true;
            }
        }

        private void DrawCodex(Rect rect)
        {
            int xp = PlayerPrefs.GetInt(XpKey, 0);
            int unlocked = 0;
            for (int i = 0; i < CodexNames.Length; i++)
                if (xp >= CodexXp[i]) unlocked++;

            GUI.Label(new Rect(rect.x, rect.y, rect.width, 38f), "OPDAGET " + unlocked + "/" + CodexNames.Length + "  •  Låses op gennem XP", centerStyle);
            Rect viewport = new Rect(rect.x, rect.y + 42f, rect.width, rect.height - 42f);
            float rowH = 64f;
            float contentH = CodexNames.Length * rowH;
            scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0f, 0f, viewport.width - 18f, contentH));
            for (int i = 0; i < CodexNames.Length; i++)
            {
                bool openEntry = xp >= CodexXp[i];
                string label = openEntry ? (i + 1).ToString("00") + "  " + CodexNames[i] + "  ✓" : (i + 1).ToString("00") + "  ???  •  KRÆVER " + CodexXp[i] + " XP";
                GUI.Box(new Rect(0f, i * rowH, viewport.width - 24f, rowH - 6f), label, rowStyle);
            }
            GUI.EndScrollView();
        }

        private void DrawToast(Rect safe)
        {
            if (toastTime <= 0f || string.IsNullOrEmpty(toast))
                return;
            float w = safe.width * 0.82f;
            float h = Mathf.Max(50f, safe.height * 0.055f);
            float x = safe.x + (safe.width - w) * 0.5f;
            float y = safe.y + safe.height * 0.16f;
            DrawRect(new Rect(x, y, w, h), new Color(0.01f, 0.018f, 0.04f, 0.97f));
            GUI.Label(new Rect(x + 8f, y, w - 16f, h), toast, centerStyle);
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
                fontSize = Mathf.Clamp(reference / 19, 24, 50),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            titleStyle.normal.textColor = Color.white;

            tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 34, 15, 29),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            rowStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 38, 13, 26),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 41, 12, 23),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            smallStyle.normal.textColor = new Color(0.82f, 0.90f, 1f);

            centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 39, 13, 25),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            centerStyle.normal.textColor = Color.white;
        }
    }
}
