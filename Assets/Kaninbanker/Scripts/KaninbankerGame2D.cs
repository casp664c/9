using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    public enum Kaninbanker2DMode
    {
        Classic,
        Turbo,
        Marathon,
        BossRush
    }

    internal enum RabbitKind2D
    {
        Normal,
        Fast,
        Gold,
        Armored,
        Bomb,
        Boss
    }

    internal enum Kaninbanker2DScreen
    {
        Lobby,
        Playing,
        Results
    }

    /// <summary>
    /// True 2D portrait runtime. No meshes, 3D primitives, perspective camera, 3D lights,
    /// 3D colliders or Physics.Raycast are used by gameplay. Everything is SpriteRenderer +
    /// Collider2D + Physics2D on a flat XY plane designed for 9:16 Android phones.
    /// </summary>
    public sealed class KaninbankerGame2D : MonoBehaviour
    {
        private const string ModeKey = "Kaninbanker2D.Mode";
        private const string ThemeKey = "Kaninbanker2D.Theme";
        private const string HighScorePrefix = "Kaninbanker2D.HighScore.";
        private const string CoinsKey = "Kaninbanker2D.Coins";
        private const string XpKey = "Kaninbanker2D.Xp";
        private const float ComboWindow = 1.45f;

        private readonly List<KaninbankerHole2D> holes = new List<KaninbankerHole2D>();
        private readonly List<SpriteRenderer> palettePrimary = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> paletteSecondary = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> paletteAccent = new List<SpriteRenderer>();

        private Camera gameplayCamera;
        private KaninbankerHole2D activeHole;
        private KaninbankerAudio audioSystem;
        private KaninbankerFeedback feedbackSystem;
        private Kaninbanker2DScreen screenState = Kaninbanker2DScreen.Lobby;
        private Kaninbanker2DMode selectedMode;
        private int selectedTheme;

        private float roundLength;
        private float roundTimeLeft;
        private float phaseTimeLeft;
        private float comboTimeLeft;
        private float slowTimeLeft;
        private float doubleTimeLeft;
        private float shieldTimeLeft;
        private float frenzyTimeLeft;
        private float toastTimeLeft;
        private int score;
        private int highScore;
        private int combo;
        private int bestCombo;
        private int hits;
        private int misses;
        private int spawnCount;
        private int missionTarget;
        private int missionProgress;
        private int slowCharges;
        private int doubleCharges;
        private int shieldCharges;
        private int frenzyCharges;
        private int coins;
        private int xp;
        private int rewardCoins;
        private int rewardXp;
        private string toast = string.Empty;

        private GUIStyle titleStyle;
        private GUIStyle heroStyle;
        private GUIStyle hudStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;
        private GUIStyle powerStyle;
        private GUIStyle centerStyle;

        public int Score => score;
        public int HighScore => highScore;
        public bool IsRunning => screenState == Kaninbanker2DScreen.Playing;
        public float TimeLeft => roundTimeLeft;

        private float Difficulty01 => roundLength <= 0f ? 0f : Mathf.Clamp01((roundLength - roundTimeLeft) / roundLength);

        private void Awake()
        {
            ConfigurePortraitRuntime();
            Application.targetFrameRate = PlayerPrefs.GetInt(KaninbankerSettingsPanel.FpsKey, 60);
            QualitySettings.vSyncCount = 0;

            selectedMode = (Kaninbanker2DMode)Mathf.Clamp(PlayerPrefs.GetInt(ModeKey, 0), 0, 3);
            selectedTheme = Mathf.Clamp(PlayerPrefs.GetInt(ThemeKey, 0), 0, 3);
            coins = PlayerPrefs.GetInt(CoinsKey, 0);
            xp = PlayerPrefs.GetInt(XpKey, 0);
            highScore = PlayerPrefs.GetInt(HighScorePrefix + (int)selectedMode, 0);

            audioSystem = GetComponent<KaninbankerAudio>();
            if (audioSystem == null)
                audioSystem = gameObject.AddComponent<KaninbankerAudio>();

            feedbackSystem = GetComponent<KaninbankerFeedback>();
            if (feedbackSystem == null)
                feedbackSystem = gameObject.AddComponent<KaninbankerFeedback>();

            BuildTrue2DScene();
            feedbackSystem.Configure(gameplayCamera);
            audioSystem.StartMusic();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
                ConfigurePortraitRuntime();
        }

        private static void ConfigurePortraitRuntime()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < holes.Count; i++)
                holes[i].Tick(dt);

            if (toastTimeLeft > 0f)
                toastTimeLeft -= Time.unscaledDeltaTime;

            if (!IsRunning)
                return;

            roundTimeLeft = Mathf.Max(0f, roundTimeLeft - dt);
            comboTimeLeft = Mathf.Max(0f, comboTimeLeft - dt);
            slowTimeLeft = Mathf.Max(0f, slowTimeLeft - dt);
            doubleTimeLeft = Mathf.Max(0f, doubleTimeLeft - dt);
            shieldTimeLeft = Mathf.Max(0f, shieldTimeLeft - dt);
            frenzyTimeLeft = Mathf.Max(0f, frenzyTimeLeft - dt);

            if (comboTimeLeft <= 0f)
                combo = 0;

            if (roundTimeLeft <= 0f)
            {
                FinishRound();
                return;
            }

            phaseTimeLeft -= dt;
            if (phaseTimeLeft <= 0f)
            {
                if (activeHole == null)
                    ShowRandomRabbit();
                else
                    HideActiveRabbit(true);
            }

            ReadPointerInput();
        }

        public void StartRound()
        {
            score = 0;
            combo = 0;
            bestCombo = 0;
            hits = 0;
            misses = 0;
            spawnCount = 0;
            comboTimeLeft = 0f;
            slowTimeLeft = 0f;
            doubleTimeLeft = 0f;
            shieldTimeLeft = 0f;
            frenzyTimeLeft = 0f;
            roundLength = GetRoundLength();
            roundTimeLeft = roundLength;
            missionTarget = selectedMode == Kaninbanker2DMode.Turbo ? 20 : selectedMode == Kaninbanker2DMode.Marathon ? 55 : selectedMode == Kaninbanker2DMode.BossRush ? 26 : 30;
            missionProgress = 0;
            slowCharges = doubleCharges = shieldCharges = frenzyCharges = 1 + (xp >= 5000 ? 1 : 0);
            highScore = PlayerPrefs.GetInt(HighScorePrefix + (int)selectedMode, 0);
            HideAllRabbits();
            phaseTimeLeft = 0.25f;
            screenState = Kaninbanker2DScreen.Playing;
            ShowToast("2D " + GetModeName() + " START!", 1.1f);
            audioSystem.PlayRoundStart();
        }

        private void FinishRound()
        {
            screenState = Kaninbanker2DScreen.Results;
            HideActiveRabbit(false);
            audioSystem.PlayGameOver();

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScorePrefix + (int)selectedMode, highScore);
            }

            rewardCoins = Mathf.Max(3, score / 4) + (missionProgress >= missionTarget ? 25 : 0);
            rewardXp = Mathf.Max(10, score * 2) + hits * 3 + bestCombo * 4;
            coins += rewardCoins;
            xp += rewardXp;
            PlayerPrefs.SetInt(CoinsKey, coins);
            PlayerPrefs.SetInt(XpKey, xp);
            PlayerPrefs.Save();
        }

        private float GetRoundLength()
        {
            switch (selectedMode)
            {
                case Kaninbanker2DMode.Turbo: return 30f;
                case Kaninbanker2DMode.Marathon: return 90f;
                case Kaninbanker2DMode.BossRush: return 60f;
                default: return 45f;
            }
        }

        private float VisibleTime
        {
            get
            {
                float start = selectedMode == Kaninbanker2DMode.Turbo ? 0.58f : 0.82f;
                float end = selectedMode == Kaninbanker2DMode.Turbo ? 0.23f : 0.31f;
                float value = Mathf.Lerp(start, end, Difficulty01);
                if (slowTimeLeft > 0f) value *= 1.65f;
                if (frenzyTimeLeft > 0f) value *= 0.72f;
                return value;
            }
        }

        private float GapTime
        {
            get
            {
                float value = Mathf.Lerp(0.16f, 0.05f, Difficulty01);
                if (frenzyTimeLeft > 0f) value *= 0.45f;
                return value;
            }
        }

        private void BuildTrue2DScene()
        {
            gameplayCamera = Camera.main;
            if (gameplayCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera 2D");
                cameraObject.tag = "MainCamera";
                gameplayCamera = cameraObject.AddComponent<Camera>();
            }

            gameplayCamera.orthographic = true;
            gameplayCamera.orthographicSize = 8.9f;
            gameplayCamera.transform.position = new Vector3(0f, 0f, -10f);
            gameplayCamera.transform.rotation = Quaternion.identity;
            gameplayCamera.nearClipPlane = 0.1f;
            gameplayCamera.farClipPlane = 50f;
            gameplayCamera.backgroundColor = GetBackgroundColor();

            CreateFlatBackdrop();
            CreateFlatDecor();

            float[] xs = { -2.45f, 0f, 2.45f };
            float[] ys = { 5.05f, 2.55f, 0f, -2.55f, -5.05f };
            int index = 1;
            for (int row = 0; row < ys.Length; row++)
            {
                for (int col = 0; col < xs.Length; col++)
                    holes.Add(CreateHole2D(index++, new Vector2(xs[col], ys[row])));
            }

            ApplyTheme();
        }

        private void CreateFlatBackdrop()
        {
            SpriteRenderer bg = CreateSpriteObject("2D_Background", transform, Vector2.zero, new Vector2(8.2f, 17.8f), Kaninbanker2DArt.Square, GetBackgroundColor(), -20);
            palettePrimary.Add(bg);

            SpriteRenderer lane = CreateSpriteObject("2D_CenterLane", transform, Vector2.zero, new Vector2(2.0f, 13.4f), Kaninbanker2DArt.Square, GetSecondaryColor(), -10);
            paletteSecondary.Add(lane);

            for (int i = 0; i < 6; i++)
            {
                float y = -6.7f + i * 2.65f;
                SpriteRenderer stripe = CreateSpriteObject("2D_Stripe_" + i, transform, new Vector2(0f, y), new Vector2(8.8f, 0.10f), Kaninbanker2DArt.Square, GetAccentColor(), -8);
                stripe.transform.rotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 2.5f : -2.5f);
                paletteAccent.Add(stripe);
            }
        }

        private void CreateFlatDecor()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 7; i++)
                {
                    float y = -6.0f + i * 2f;
                    SpriteRenderer dot = CreateSpriteObject("2D_SideOrb_" + side + "_" + i, transform,
                        new Vector2(side * 3.75f, y), Vector2.one * (i % 3 == 0 ? 0.55f : 0.34f),
                        Kaninbanker2DArt.Circle, GetAccentColor(), -4);
                    paletteAccent.Add(dot);
                }
            }
        }

        private KaninbankerHole2D CreateHole2D(int index, Vector2 position)
        {
            GameObject root = new GameObject("Hole2D_" + index.ToString("00"));
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(position.x, position.y, 0f);

            SpriteRenderer rim = CreateSpriteObject("Rim", root.transform, Vector2.zero, new Vector2(1.72f, 0.84f), Kaninbanker2DArt.Circle, GetRimColor(), 2);
            SpriteRenderer dark = CreateSpriteObject("HoleDark", root.transform, new Vector2(0f, -0.03f), new Vector2(1.42f, 0.58f), Kaninbanker2DArt.Circle, new Color(0.045f, 0.025f, 0.035f), 3);
            dark.sortingOrder = 3;

            GameObject rabbit = new GameObject("Rabbit2D");
            rabbit.transform.SetParent(root.transform, false);
            rabbit.transform.localPosition = new Vector3(0f, 0.46f, 0f);

            CircleCollider2D collider = rabbit.AddComponent<CircleCollider2D>();
            collider.radius = 0.72f;
            collider.offset = new Vector2(0f, 0.32f);
            KaninbankerTarget2D target = rabbit.AddComponent<KaninbankerTarget2D>();

            SpriteRenderer body = CreateSpriteObject("Body", rabbit.transform, new Vector2(0f, 0.20f), new Vector2(1.20f, 1.32f), Kaninbanker2DArt.Circle, new Color(0.88f, 0.76f, 0.62f), 12);
            SpriteRenderer belly = CreateSpriteObject("Belly", rabbit.transform, new Vector2(0f, 0.08f), new Vector2(0.64f, 0.72f), Kaninbanker2DArt.Circle, new Color(1f, 0.92f, 0.82f), 13);
            SpriteRenderer head = CreateSpriteObject("Head", rabbit.transform, new Vector2(0f, 0.82f), new Vector2(1.02f, 1.02f), Kaninbanker2DArt.Circle, new Color(0.92f, 0.82f, 0.68f), 14);
            CreateSpriteObject("EarL", rabbit.transform, new Vector2(-0.26f, 1.50f), new Vector2(0.30f, 0.78f), Kaninbanker2DArt.Circle, new Color(0.92f, 0.82f, 0.68f), 13);
            CreateSpriteObject("EarR", rabbit.transform, new Vector2(0.26f, 1.50f), new Vector2(0.30f, 0.78f), Kaninbanker2DArt.Circle, new Color(0.92f, 0.82f, 0.68f), 13);
            CreateSpriteObject("EyeL", rabbit.transform, new Vector2(-0.20f, 0.92f), new Vector2(0.13f, 0.17f), Kaninbanker2DArt.Circle, new Color(0.03f, 0.03f, 0.05f), 16);
            CreateSpriteObject("EyeR", rabbit.transform, new Vector2(0.20f, 0.92f), new Vector2(0.13f, 0.17f), Kaninbanker2DArt.Circle, new Color(0.03f, 0.03f, 0.05f), 16);
            SpriteRenderer marker = CreateSpriteObject("TypeMarker", rabbit.transform, new Vector2(0f, 1.84f), new Vector2(0.34f, 0.22f), Kaninbanker2DArt.Square, new Color(0.20f, 0.75f, 1f), 18);

            KaninbankerHole2D hole = root.AddComponent<KaninbankerHole2D>();
            hole.Configure(rabbit, rim, body, belly, head, marker);
            target.Hole = hole;
            rabbit.SetActive(false);
            return hole;
        }

        internal static SpriteRenderer CreateSpriteObject(string name, Transform parent, Vector2 localPosition, Vector2 size, Sprite sprite, Color color, int order)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
            return renderer;
        }

        private void ShowRandomRabbit()
        {
            if (holes.Count == 0) return;
            spawnCount++;
            activeHole = holes[Random.Range(0, holes.Count)];
            RabbitKind2D kind = PickRabbitKind();
            int hp = kind == RabbitKind2D.Armored ? 2 : kind == RabbitKind2D.Boss ? 6 + Mathf.Min(6, xp / 2500) : 1;
            activeHole.Show(kind, hp);
            phaseTimeLeft = VisibleTime * (kind == RabbitKind2D.Fast ? 0.65f : kind == RabbitKind2D.Boss ? 3.2f : kind == RabbitKind2D.Armored ? 1.35f : 1f);
            audioSystem.PlayRabbitPop(Difficulty01);
        }

        private RabbitKind2D PickRabbitKind()
        {
            if (selectedMode == Kaninbanker2DMode.BossRush && spawnCount % 7 == 0) return RabbitKind2D.Boss;
            if (spawnCount >= 18 && spawnCount % 19 == 0) return RabbitKind2D.Boss;
            float r = Random.value;
            if (r < Mathf.Lerp(0.05f, 0.11f, Difficulty01)) return RabbitKind2D.Bomb;
            if (r < 0.14f) return RabbitKind2D.Gold;
            if (r < 0.26f) return RabbitKind2D.Armored;
            if (r < Mathf.Lerp(0.40f, 0.54f, Difficulty01)) return RabbitKind2D.Fast;
            return RabbitKind2D.Normal;
        }

        private void HideActiveRabbit(bool countMiss)
        {
            if (activeHole != null)
            {
                Vector3 p = activeHole.transform.position;
                RabbitKind2D kind = activeHole.Kind;
                activeHole.Hide();
                if (countMiss)
                {
                    if (kind == RabbitKind2D.Bomb)
                    {
                        score += 2;
                        ShowToast("BOMBE UNDGÅET +2", 0.65f);
                    }
                    else if (shieldTimeLeft <= 0f)
                    {
                        misses++;
                        combo = 0;
                        audioSystem.PlayMiss();
                        feedbackSystem.PlayMiss(p);
                    }
                }
            }
            activeHole = null;
            phaseTimeLeft = GapTime;
        }

        private void HideAllRabbits()
        {
            for (int i = 0; i < holes.Count; i++) holes[i].Hide();
            activeHole = null;
        }

        private void ReadPointerInput()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && !IsReservedUiTouch(touch.position))
                    TryHit2D(touch.position);
                return;
            }
            if (Input.GetMouseButtonDown(0) && !IsReservedUiTouch(Input.mousePosition))
                TryHit2D(Input.mousePosition);
        }

        private bool IsReservedUiTouch(Vector2 screenPoint)
        {
            Rect safe = Screen.safeArea;
            return screenPoint.y > safe.yMax - safe.height * 0.18f || screenPoint.y < safe.y + safe.height * 0.17f;
        }

        private void TryHit2D(Vector2 screenPoint)
        {
            Vector3 world = gameplayCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10f));
            Collider2D hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
            if (hit == null) return;
            KaninbankerTarget2D target = hit.GetComponent<KaninbankerTarget2D>();
            if (target == null || activeHole == null || target.Hole != activeHole || !activeHole.IsVisible) return;

            RabbitKind2D kind = activeHole.Kind;
            Vector3 p = activeHole.transform.position;
            if (kind == RabbitKind2D.Bomb)
            {
                if (shieldTimeLeft > 0f)
                {
                    score += 3;
                    ShowToast("SKJOLD SMADRER BOMBE +3", 0.7f);
                    feedbackSystem.PlayHit(p, 3);
                }
                else
                {
                    score = Mathf.Max(0, score - 8);
                    misses++;
                    combo = 0;
                    ShowToast("BOMBE! -8", 0.8f);
                    feedbackSystem.PlayMiss(p);
                }
                activeHole.Hide();
                activeHole = null;
                phaseTimeLeft = GapTime;
                return;
            }

            combo = comboTimeLeft > 0f ? combo + 1 : 1;
            comboTimeLeft = ComboWindow;
            bestCombo = Mathf.Max(bestCombo, combo);
            bool defeated = activeHole.TakeHit();
            int basePoints = kind == RabbitKind2D.Gold ? 6 : kind == RabbitKind2D.Boss ? 4 : kind == RabbitKind2D.Armored ? 3 : kind == RabbitKind2D.Fast ? 2 : 1;
            int gained = basePoints * (1 + Mathf.Min(4, combo / 4)) * (doubleTimeLeft > 0f ? 2 : 1);
            score += gained;
            audioSystem.PlayHit(combo);
            feedbackSystem.PlayHit(p, combo);

            if (!defeated)
            {
                phaseTimeLeft = Mathf.Max(phaseTimeLeft, kind == RabbitKind2D.Boss ? 1.2f : 0.65f);
                ShowToast(kind == RabbitKind2D.Boss ? "BOSS HP " + activeHole.Health : "PANser HP " + activeHole.Health, 0.55f);
                return;
            }

            hits++;
            missionProgress = Mathf.Min(missionTarget, missionProgress + 1);
            if (kind == RabbitKind2D.Gold) ShowToast("GULD +" + gained, 0.65f);
            if (kind == RabbitKind2D.Boss) ShowToast("2D BOSS KNUST!", 0.9f);
            if (score > highScore) highScore = score;
            activeHole.Hide();
            activeHole = null;
            phaseTimeLeft = GapTime;
        }

        private void UsePower(int index)
        {
            if (!IsRunning) return;
            if (index == 0 && slowCharges > 0) { slowCharges--; slowTimeLeft = 7f; ShowToast("SLOW 7 SEK", 0.7f); }
            else if (index == 1 && doubleCharges > 0) { doubleCharges--; doubleTimeLeft = 10f; ShowToast("x2 10 SEK", 0.7f); }
            else if (index == 2 && shieldCharges > 0) { shieldCharges--; shieldTimeLeft = 9f; ShowToast("SKJOLD 9 SEK", 0.7f); }
            else if (index == 3 && frenzyCharges > 0) { frenzyCharges--; frenzyTimeLeft = 8f; ShowToast("FRENZY 8 SEK", 0.7f); }
            audioSystem.PlayUi();
        }

        private void ShowToast(string message, float seconds)
        {
            toast = message;
            toastTimeLeft = seconds;
        }

        private void OnGUI()
        {
            EnsureStyles();
            Rect raw = Screen.safeArea;
            Rect safe = new Rect(raw.x, Screen.height - raw.yMax, raw.width, raw.height);
            if (screenState == Kaninbanker2DScreen.Lobby) DrawLobby(safe);
            else if (screenState == Kaninbanker2DScreen.Playing) DrawHud(safe);
            else DrawResults(safe);
            if (toastTimeLeft > 0f) DrawToast(safe);
        }

        private void DrawLobby(Rect safe)
        {
            float m = Mathf.Max(16f, safe.width * 0.04f);
            float w = safe.width - m * 2f;
            DrawRect(new Rect(safe.x + m, safe.y + safe.height * 0.08f, w, safe.height * 0.74f), new Color(0.025f, 0.035f, 0.065f, 0.96f));
            GUI.Label(new Rect(safe.x + m, safe.y + safe.height * 0.10f, w, safe.height * 0.10f), "KANINBANKER 2D", heroStyle);
            GUI.Label(new Rect(safe.x + m, safe.y + safe.height * 0.19f, w, safe.height * 0.05f), "ÆGTE FLAD 2D • 15 HULLER • PORTRAIT", centerStyle);
            GUI.Label(new Rect(safe.x + m, safe.y + safe.height * 0.25f, w, safe.height * 0.045f), "MØNTER " + coins + "   •   XP " + xp + "   •   REKORD " + highScore, centerStyle);

            float y = safe.y + safe.height * 0.32f;
            float gap = m * 0.35f;
            float bh = safe.height * 0.07f;
            float half = (w - gap) * 0.5f;
            DrawMode(new Rect(safe.x + m, y, half, bh), Kaninbanker2DMode.Classic, "CLASSIC");
            DrawMode(new Rect(safe.x + m + half + gap, y, half, bh), Kaninbanker2DMode.Turbo, "TURBO");
            y += bh + gap;
            DrawMode(new Rect(safe.x + m, y, half, bh), Kaninbanker2DMode.Marathon, "MARATHON");
            DrawMode(new Rect(safe.x + m + half + gap, y, half, bh), Kaninbanker2DMode.BossRush, "BOSS RUSH");

            y += bh + gap * 1.4f;
            if (GUI.Button(new Rect(safe.x + m, y, w, bh), "2D TEMA: " + GetThemeName() + "  •  SKIFT", buttonStyle))
            {
                selectedTheme = (selectedTheme + 1) % 4;
                PlayerPrefs.SetInt(ThemeKey, selectedTheme);
                ApplyTheme();
                audioSystem.PlayUi();
            }

            float startH = safe.height * 0.105f;
            float startY = safe.yMax - m - startH;
            GUI.backgroundColor = GetAccentColor();
            if (GUI.Button(new Rect(safe.x + m, startY, w, startH), "START 2D", heroStyle)) StartRound();
            GUI.backgroundColor = Color.white;
        }

        private void DrawHud(Rect safe)
        {
            float m = Mathf.Max(12f, safe.width * 0.025f);
            float w = safe.width - m * 2f;
            float topH = safe.height * 0.145f;
            DrawRect(new Rect(safe.x + m, safe.y + m, w, topH), new Color(0.02f, 0.025f, 0.055f, 0.96f));
            float col = w / 3f;
            GUI.Label(new Rect(safe.x + m, safe.y + m, col, topH * 0.50f), score.ToString(), heroStyle);
            GUI.Label(new Rect(safe.x + m + col, safe.y + m, col, topH * 0.50f), Mathf.CeilToInt(roundTimeLeft) + "s", heroStyle);
            GUI.Label(new Rect(safe.x + m + col * 2f, safe.y + m, col, topH * 0.50f), combo > 1 ? "x" + combo : "-", heroStyle);
            GUI.Label(new Rect(safe.x + m, safe.y + m + topH * 0.43f, col, topH * 0.20f), "SCORE", smallStyle);
            GUI.Label(new Rect(safe.x + m + col, safe.y + m + topH * 0.43f, col, topH * 0.20f), "TID", smallStyle);
            GUI.Label(new Rect(safe.x + m + col * 2f, safe.y + m + topH * 0.43f, col, topH * 0.20f), "COMBO", smallStyle);
            GUI.Label(new Rect(safe.x + m, safe.y + m + topH * 0.67f, w, topH * 0.18f), "MISSION " + missionProgress + "/" + missionTarget, smallStyle);
            DrawProgress(new Rect(safe.x + m * 2f, safe.y + m + topH * 0.86f, w - m * 2f, topH * 0.10f), missionTarget > 0 ? missionProgress / (float)missionTarget : 0f, GetAccentColor());

            float bottomH = safe.height * 0.145f;
            float by = safe.yMax - bottomH - m;
            DrawRect(new Rect(safe.x + m, by, w, bottomH), new Color(0.02f, 0.025f, 0.055f, 0.96f));
            float gap = Mathf.Max(4f, m * 0.3f);
            float pw = (w - m * 2f - gap * 3f) / 4f;
            float py = by + bottomH * 0.17f;
            float ph = bottomH * 0.68f;
            DrawPower(new Rect(safe.x + m * 2f, py, pw, ph), 0, "SLOW", slowCharges, slowTimeLeft);
            DrawPower(new Rect(safe.x + m * 2f + (pw + gap), py, pw, ph), 1, "x2", doubleCharges, doubleTimeLeft);
            DrawPower(new Rect(safe.x + m * 2f + (pw + gap) * 2f, py, pw, ph), 2, "SKJOLD", shieldCharges, shieldTimeLeft);
            DrawPower(new Rect(safe.x + m * 2f + (pw + gap) * 3f, py, pw, ph), 3, "FRENZY", frenzyCharges, frenzyTimeLeft);
        }

        private void DrawResults(Rect safe)
        {
            float m = Mathf.Max(18f, safe.width * 0.04f);
            Rect panel = new Rect(safe.x + m, safe.y + safe.height * 0.16f, safe.width - m * 2f, safe.height * 0.66f);
            DrawRect(panel, new Color(0.025f, 0.035f, 0.065f, 0.98f));
            GUI.Label(new Rect(panel.x, panel.y + panel.height * 0.06f, panel.width, panel.height * 0.10f), "2D RUNDE FÆRDIG", titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + panel.height * 0.16f, panel.width, panel.height * 0.16f), score.ToString(), heroStyle);
            GUI.Label(new Rect(panel.x, panel.y + panel.height * 0.34f, panel.width, panel.height * 0.07f), "REKORD " + highScore + "  •  COMBO x" + bestCombo, centerStyle);
            GUI.Label(new Rect(panel.x, panel.y + panel.height * 0.44f, panel.width, panel.height * 0.07f), "TRÆFFERE " + hits + "  •  MISS " + misses, centerStyle);
            GUI.Label(new Rect(panel.x, panel.y + panel.height * 0.54f, panel.width, panel.height * 0.07f), "+" + rewardCoins + " MØNTER  •  +" + rewardXp + " XP", centerStyle);
            float bh = panel.height * 0.12f;
            float gap = m * 0.4f;
            float half = (panel.width - m * 2f - gap) * 0.5f;
            float by = panel.yMax - bh - m;
            if (GUI.Button(new Rect(panel.x + m, by, half, bh), "LOBBY", buttonStyle)) screenState = Kaninbanker2DScreen.Lobby;
            GUI.backgroundColor = GetAccentColor();
            if (GUI.Button(new Rect(panel.x + m + half + gap, by, half, bh), "SPIL IGEN", buttonStyle)) StartRound();
            GUI.backgroundColor = Color.white;
        }

        private void DrawMode(Rect rect, Kaninbanker2DMode mode, string label)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = selectedMode == mode ? GetAccentColor() : new Color(0.22f, 0.25f, 0.34f);
            if (GUI.Button(rect, label, buttonStyle))
            {
                selectedMode = mode;
                PlayerPrefs.SetInt(ModeKey, (int)mode);
                highScore = PlayerPrefs.GetInt(HighScorePrefix + (int)mode, 0);
                audioSystem.PlayUi();
            }
            GUI.backgroundColor = old;
        }

        private void DrawPower(Rect rect, int index, string label, int charges, float active)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = active > 0f ? GetAccentColor() : charges > 0 ? new Color(0.25f, 0.28f, 0.38f) : new Color(0.14f, 0.14f, 0.17f);
            string status = active > 0f ? active.ToString("0.0") + "s" : "x" + charges;
            if (GUI.Button(rect, label + "\n" + status, powerStyle)) UsePower(index);
            GUI.backgroundColor = old;
        }

        private void DrawToast(Rect safe)
        {
            float w = safe.width * 0.72f;
            float h = safe.height * 0.06f;
            float x = safe.x + (safe.width - w) * 0.5f;
            float y = safe.y + safe.height * 0.20f;
            DrawRect(new Rect(x, y, w, h), new Color(0.01f, 0.015f, 0.035f, 0.94f));
            GUI.Label(new Rect(x, y, w, h), toast, titleStyle);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void DrawProgress(Rect rect, float progress, Color color)
        {
            DrawRect(rect, new Color(0f, 0f, 0f, 0.55f));
            rect.width *= Mathf.Clamp01(progress);
            DrawRect(rect, color);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            int r = Mathf.Min(Screen.width, Screen.height);
            titleStyle = MakeLabel(Mathf.Clamp(r / 22, 22, 48), FontStyle.Bold);
            heroStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(r / 14, 30, 70), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            heroStyle.normal.textColor = Color.white;
            hudStyle = MakeLabel(Mathf.Clamp(r / 23, 20, 44), FontStyle.Bold);
            smallStyle = MakeLabel(Mathf.Clamp(r / 35, 14, 28), FontStyle.Bold);
            centerStyle = MakeLabel(Mathf.Clamp(r / 30, 16, 34), FontStyle.Normal);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(r / 29, 17, 34), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            powerStyle = new GUIStyle(GUI.skin.button) { fontSize = Mathf.Clamp(r / 38, 14, 26), fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }

        private static GUIStyle MakeLabel(int size, FontStyle style)
        {
            GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            s.normal.textColor = Color.white;
            return s;
        }

        private void ApplyTheme()
        {
            if (gameplayCamera != null) gameplayCamera.backgroundColor = GetBackgroundColor();
            for (int i = 0; i < palettePrimary.Count; i++) if (palettePrimary[i] != null) palettePrimary[i].color = GetBackgroundColor();
            for (int i = 0; i < paletteSecondary.Count; i++) if (paletteSecondary[i] != null) paletteSecondary[i].color = GetSecondaryColor();
            for (int i = 0; i < paletteAccent.Count; i++) if (paletteAccent[i] != null) paletteAccent[i].color = GetAccentColor();
            for (int i = 0; i < holes.Count; i++) holes[i].SetRimColor(GetRimColor());
        }

        private string GetModeName() => selectedMode == Kaninbanker2DMode.Turbo ? "TURBO" : selectedMode == Kaninbanker2DMode.Marathon ? "MARATHON" : selectedMode == Kaninbanker2DMode.BossRush ? "BOSS RUSH" : "CLASSIC";
        private string GetThemeName() => selectedTheme == 1 ? "NEON NAT" : selectedTheme == 2 ? "SUKKERLAND" : selectedTheme == 3 ? "LAVA" : "ENG";
        private Color GetBackgroundColor() => selectedTheme == 1 ? new Color(0.025f, 0.035f, 0.10f) : selectedTheme == 2 ? new Color(0.22f, 0.07f, 0.18f) : selectedTheme == 3 ? new Color(0.16f, 0.035f, 0.015f) : new Color(0.06f, 0.16f, 0.12f);
        private Color GetSecondaryColor() => selectedTheme == 1 ? new Color(0.08f, 0.22f, 0.42f) : selectedTheme == 2 ? new Color(0.45f, 0.14f, 0.38f) : selectedTheme == 3 ? new Color(0.42f, 0.10f, 0.025f) : new Color(0.11f, 0.28f, 0.17f);
        private Color GetAccentColor() => selectedTheme == 1 ? new Color(0.10f, 0.92f, 1f) : selectedTheme == 2 ? new Color(1f, 0.25f, 0.78f) : selectedTheme == 3 ? new Color(1f, 0.30f, 0.05f) : new Color(0.30f, 0.92f, 0.48f);
        private Color GetRimColor() => selectedTheme == 1 ? new Color(0.10f, 0.42f, 0.72f) : selectedTheme == 2 ? new Color(0.76f, 0.26f, 0.58f) : selectedTheme == 3 ? new Color(0.66f, 0.16f, 0.05f) : new Color(0.16f, 0.36f, 0.20f);
    }

    internal sealed class KaninbankerTarget2D : MonoBehaviour
    {
        public KaninbankerHole2D Hole;
    }

    internal sealed class KaninbankerHole2D : MonoBehaviour
    {
        private GameObject rabbit;
        private SpriteRenderer rim;
        private SpriteRenderer body;
        private SpriteRenderer belly;
        private SpriteRenderer head;
        private SpriteRenderer marker;
        private Vector3 targetScale = Vector3.one;
        private float pop;

        public RabbitKind2D Kind { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public bool IsVisible => rabbit != null && rabbit.activeSelf;

        public void Configure(GameObject rabbitObject, SpriteRenderer rimRenderer, SpriteRenderer bodyRenderer, SpriteRenderer bellyRenderer, SpriteRenderer headRenderer, SpriteRenderer markerRenderer)
        {
            rabbit = rabbitObject;
            rim = rimRenderer;
            body = bodyRenderer;
            belly = bellyRenderer;
            head = headRenderer;
            marker = markerRenderer;
        }

        public void Show(RabbitKind2D kind, int health)
        {
            Kind = kind;
            Health = MaxHealth = Mathf.Max(1, health);
            Color main = kind == RabbitKind2D.Gold ? new Color(1f, 0.72f, 0.08f) : kind == RabbitKind2D.Bomb ? new Color(0.16f, 0.16f, 0.20f) : kind == RabbitKind2D.Boss ? new Color(0.84f, 0.16f, 0.18f) : kind == RabbitKind2D.Armored ? new Color(0.42f, 0.52f, 0.62f) : kind == RabbitKind2D.Fast ? new Color(0.35f, 0.86f, 0.96f) : new Color(0.88f, 0.76f, 0.62f);
            body.color = main;
            head.color = Color.Lerp(main, Color.white, 0.15f);
            belly.color = kind == RabbitKind2D.Bomb ? new Color(0.88f, 0.12f, 0.10f) : new Color(1f, 0.92f, 0.82f);
            marker.color = kind == RabbitKind2D.Gold ? Color.yellow : kind == RabbitKind2D.Bomb ? Color.red : kind == RabbitKind2D.Boss ? new Color(1f, 0.1f, 0.12f) : kind == RabbitKind2D.Armored ? new Color(0.72f, 0.82f, 0.95f) : new Color(0.15f, 0.80f, 1f);
            targetScale = kind == RabbitKind2D.Boss ? Vector3.one * 1.32f : Vector3.one;
            pop = 0f;
            rabbit.transform.localScale = Vector3.one * 0.15f;
            rabbit.SetActive(true);
        }

        public void Tick(float dt)
        {
            if (!IsVisible) return;
            pop = Mathf.Clamp01(pop + dt * 10f);
            float elastic = 1f + Mathf.Sin(pop * Mathf.PI) * 0.12f;
            rabbit.transform.localScale = Vector3.Lerp(Vector3.one * 0.15f, targetScale * elastic, pop);
        }

        public bool TakeHit()
        {
            Health = Mathf.Max(0, Health - 1);
            if (Health <= 0) return true;
            rabbit.transform.localScale *= 0.88f;
            return false;
        }

        public void Hide()
        {
            if (rabbit != null) rabbit.SetActive(false);
        }

        public void SetRimColor(Color color)
        {
            if (rim != null) rim.color = color;
        }
    }

    internal static class Kaninbanker2DArt
    {
        private static Sprite circle;
        private static Sprite square;
        public static Sprite Circle => circle != null ? circle : (circle = CreateCircleSprite());
        public static Sprite Square => square != null ? square : (square = CreateSquareSprite());

        private static Sprite CreateCircleSprite()
        {
            const int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.name = "Kaninbanker2D_Circle";
            tex.filterMode = FilterMode.Bilinear;
            Color32[] pixels = new Color32[size * size];
            Vector2 c = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    float a = Mathf.Clamp01((radius - d) / 2.2f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateSquareSprite()
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.name = "Kaninbanker2D_Square";
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
        }
    }
}
