using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Sixty-stage offline campaign layered on top of the arcade rounds. The player selects a
    /// stage outside gameplay; the next completed round is evaluated against that stage's score
    /// targets and awards 1-3 stars. Progress persists locally and unlocks the next stage.
    /// </summary>
    public sealed class KaninbankerWorldTour : MonoBehaviour
    {
        private const string Prefix = "Kaninbanker.WorldTour.";
        private const int Chapters = 12;
        private const int StagesPerChapter = 5;
        private const int TotalStages = Chapters * StagesPerChapter;

        private static readonly string[] ChapterNames =
        {
            "GRØN ENG", "MØRK SKOV", "NEON CITY", "SUKKERLAND", "LAVA PIT", "FROSTHULER",
            "ROBOTFABRIK", "PIRATØEN", "SPØGELSESBYEN", "RUMMET", "KAOSDIMENSIONEN", "KEJSERENS ARENA"
        };

        private static readonly string[] ChapterTags =
        {
            "Begynd rejsen", "Hurtigere kaniner", "Neon og combo", "Guldfeber", "Bomber og pres", "Præcision",
            "Pansrede fjender", "Streak-jagt", "Falske mål og kaos", "Ekstrem hastighed", "Alt blandes sammen", "Den ultimative prøve"
        };

        private KaninbankerGame2D game;
        private bool panelOpen;
        private bool wasRunning;
        private int selectedStage;
        private int unlockedStage;
        private int totalStars;
        private float toastTime;
        private string toast = string.Empty;
        private GUIStyle titleStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerWorldTour>() != null) return;
            GameObject go = new GameObject("KaninbankerWorldTour");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerWorldTour>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            unlockedStage = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Unlocked", 0), 0, TotalStages - 1);
            selectedStage = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "Selected", 0), 0, unlockedStage);
            RecalculateStars();
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
            bool running = game.IsRunning;
            if (!running && wasRunning) EvaluateStage(game.Score);
            wasRunning = running;
        }

        private void EvaluateStage(int score)
        {
            int target = GetTarget(selectedStage);
            int stars = score >= target * 2 ? 3 : score >= Mathf.RoundToInt(target * 1.4f) ? 2 : score >= target ? 1 : 0;
            int oldStars = PlayerPrefs.GetInt(StageKey(selectedStage), 0);
            if (stars > oldStars)
            {
                PlayerPrefs.SetInt(StageKey(selectedStage), stars);
                if (selectedStage == unlockedStage && stars > 0 && unlockedStage < TotalStages - 1)
                {
                    unlockedStage++;
                    PlayerPrefs.SetInt(Prefix + "Unlocked", unlockedStage);
                }
                RecalculateStars();
                PlayerPrefs.Save();
                ShowToast("WORLD TOUR  " + stars + " STJERNER!");
            }
            else if (stars == 0) ShowToast("WORLD TOUR: " + score + "/" + target);
        }

        private static string StageKey(int stage) => Prefix + "Stars." + stage;

        private void RecalculateStars()
        {
            totalStars = 0;
            for (int i = 0; i < TotalStages; i++) totalStars += PlayerPrefs.GetInt(StageKey(i), 0);
        }

        private static int GetTarget(int stage)
        {
            int chapter = stage / StagesPerChapter;
            int local = stage % StagesPerChapter;
            return 28 + chapter * 18 + local * (8 + chapter * 2);
        }

        private void SelectStage(int stage)
        {
            if (stage < 0 || stage > unlockedStage || stage >= TotalStages) return;
            selectedStage = stage;
            PlayerPrefs.SetInt(Prefix + "Selected", selectedStage);
            PlayerPrefs.Save();
        }

        private void ShowToast(string message)
        {
            toast = message;
            toastTime = 2.2f;
        }

        private void OnGUI()
        {
            if (game == null || game.IsRunning) return;
            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            float margin = Mathf.Max(14f, safe.width * 0.03f);
            float tabW = Mathf.Min(safe.width * 0.31f, 220f);
            float tabH = Mathf.Max(52f, safe.height * 0.055f);
            Rect tab = new Rect(safe.xMax - margin - tabW, safe.y + safe.height * 0.415f, tabW, tabH);
            if (GUI.Button(tab, panelOpen ? "LUK TOUR" : "WORLD TOUR", buttonStyle)) panelOpen = !panelOpen;

            if (toastTime > 0f)
            {
                Rect toastRect = new Rect(safe.x + margin, safe.y + safe.height * 0.145f, safe.width - margin * 2f, tabH);
                DrawRect(toastRect, new Color(0.04f, 0.16f, 0.23f, 0.96f));
                GUI.Label(toastRect, toast, smallStyle);
            }
            if (!panelOpen) return;

            Rect panel = new Rect(safe.x + margin, safe.y + safe.height * 0.09f, safe.width - margin * 2f, safe.height * 0.82f);
            DrawRect(panel, new Color(0.02f, 0.032f, 0.055f, 0.992f));
            float inner = Mathf.Max(16f, panel.width * 0.04f);
            GUI.Label(new Rect(panel.x + inner, panel.y + panel.height * 0.025f, panel.width - inner * 2f, panel.height * 0.075f), "WORLD TOUR 2D", titleStyle);

            int chapter = selectedStage / StagesPerChapter;
            int target = GetTarget(selectedStage);
            int currentStars = PlayerPrefs.GetInt(StageKey(selectedStage), 0);
            float y = panel.y + panel.height * 0.105f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.065f), "VERDEN " + (chapter + 1) + ": " + ChapterNames[chapter], headingStyle);
            y += panel.height * 0.062f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.05f), ChapterTags[chapter] + "  •  " + totalStars + "/" + (TotalStages * 3) + " STJERNER", smallStyle);
            y += panel.height * 0.075f;

            float stageGap = inner * 0.35f;
            float stageW = (panel.width - inner * 2f - stageGap * 4f) / 5f;
            float stageH = panel.height * 0.105f;
            int chapterStart = chapter * StagesPerChapter;
            for (int i = 0; i < StagesPerChapter; i++)
            {
                int stageIndex = chapterStart + i;
                bool unlocked = stageIndex <= unlockedStage;
                bool selected = stageIndex == selectedStage;
                int stars = PlayerPrefs.GetInt(StageKey(stageIndex), 0);
                Color old = GUI.backgroundColor;
                GUI.backgroundColor = selected ? new Color(0.15f, 0.70f, 1f) : unlocked ? new Color(0.20f, 0.28f, 0.38f) : new Color(0.10f, 0.11f, 0.13f);
                string label = unlocked ? (i + 1) + "\n" + StarsText(stars) : "LOCK";
                if (GUI.Button(new Rect(panel.x + inner + i * (stageW + stageGap), y, stageW, stageH), label, buttonStyle) && unlocked) SelectStage(stageIndex);
                GUI.backgroundColor = old;
            }

            y += stageH + panel.height * 0.035f;
            DrawRect(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.18f), new Color(0.055f, 0.075f, 0.11f, 0.96f));
            GUI.Label(new Rect(panel.x + inner * 1.5f, y + panel.height * 0.015f, panel.width - inner * 3f, panel.height * 0.055f), "STAGE " + (selectedStage + 1) + "  •  SCOREMÅL " + target, headingStyle);
            GUI.Label(new Rect(panel.x + inner * 1.5f, y + panel.height * 0.075f, panel.width - inner * 3f, panel.height * 0.08f), "1 stjerne: " + target + "   •   2: " + Mathf.RoundToInt(target * 1.4f) + "   •   3: " + (target * 2) + "\nDin rekord på stage: " + StarsText(currentStars), smallStyle);

            y += panel.height * 0.215f;
            GUI.Label(new Rect(panel.x + inner, y, panel.width - inner * 2f, panel.height * 0.05f), "VERDENSKORT", headingStyle);
            y += panel.height * 0.06f;
            float chapterGap = inner * 0.25f;
            float chapterW = (panel.width - inner * 2f - chapterGap * 3f) / 4f;
            float chapterH = panel.height * 0.075f;
            for (int i = 0; i < Chapters; i++)
            {
                int row = i / 4;
                int col = i % 4;
                int firstStage = i * StagesPerChapter;
                bool chapterUnlocked = firstStage <= unlockedStage;
                Rect chapterButton = new Rect(panel.x + inner + col * (chapterW + chapterGap), y + row * (chapterH + chapterGap * 0.7f), chapterW, chapterH);
                Color old = GUI.backgroundColor;
                GUI.backgroundColor = i == chapter ? new Color(0.20f, 0.62f, 0.96f) : chapterUnlocked ? new Color(0.20f, 0.25f, 0.32f) : new Color(0.09f, 0.10f, 0.12f);
                if (GUI.Button(chapterButton, chapterUnlocked ? "V" + (i + 1) : "LOCK", buttonStyle) && chapterUnlocked) SelectStage(Mathf.Max(firstStage, Mathf.Min(unlockedStage, firstStage + StagesPerChapter - 1)));
                GUI.backgroundColor = old;
            }

            float footerY = panel.yMax - panel.height * 0.105f;
            GUI.Label(new Rect(panel.x + inner, footerY, panel.width - inner * 2f, panel.height * 0.07f), "Vælg en stage, luk World Tour og spil en 2D-runde. Resultatet tæller automatisk mod den valgte stage.", smallStyle);
        }

        private static string StarsText(int stars)
        {
            if (stars >= 3) return "★★★";
            if (stars == 2) return "★★☆";
            if (stars == 1) return "★☆☆";
            return "☆☆☆";
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
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 13, 30, 68), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = new Color(0.35f, 0.85f, 1f);
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 23, 20, 42), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            headingStyle.normal.textColor = Color.white;
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 29, 17, 34), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            bodyStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(reference / 38, 13, 26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            smallStyle.normal.textColor = new Color(0.86f, 0.90f, 0.96f);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(reference / 39, 13, 25), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }
    }
}
