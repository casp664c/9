using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    public enum KaninbankerGameMode
    {
        Classic,
        Turbo,
        Marathon,
        BossRush
    }

    public enum KaninbankerArenaTheme
    {
        Meadow,
        Night,
        Candy,
        Volcano
    }

    internal enum RabbitKind
    {
        Normal,
        Fast,
        Gold,
        Armored,
        Bomb,
        Boss
    }

    internal enum KaninbankerScreen
    {
        Lobby,
        Playing,
        Results
    }

    /// <summary>
    /// Portrait-first Kaninbanker core. The entire arena, progression layer and HUD are generated
    /// at runtime so Unity Cloud Build can compile a complete Android game without imported art.
    /// The layout is designed around a tall 9:16 phone rather than a landscape desktop viewport.
    /// </summary>
    public sealed class KaninbankerGame : MonoBehaviour
    {
        private const string ModeKey = "Kaninbanker.SelectedMode";
        private const string ThemeKey = "Kaninbanker.SelectedTheme";
        private const string HighScorePrefix = "Kaninbanker.HighScore.";
        private const float ComboWindow = 1.45f;

        private readonly List<RabbitHole> holes = new List<RabbitHole>();
        private readonly List<Renderer> themePrimaryRenderers = new List<Renderer>();
        private readonly List<Renderer> themeSecondaryRenderers = new List<Renderer>();
        private readonly List<Renderer> themeAccentRenderers = new List<Renderer>();

        private Camera gameplayCamera;
        private RabbitHole activeHole;
        private KaninbankerAudio audioSystem;
        private KaninbankerFeedback feedbackSystem;
        private KaninbankerProfile profile;
        private KaninbankerScreen screenState = KaninbankerScreen.Lobby;
        private KaninbankerGameMode selectedMode;
        private KaninbankerArenaTheme selectedTheme;

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
        private int bestComboThisRound;
        private int hits;
        private int misses;
        private int spawnCount;
        private int challengeTarget;
        private int challengeProgress;
        private int slowCharges;
        private int doubleCharges;
        private int shieldCharges;
        private int frenzyCharges;
        private int lastCoinReward;
        private int lastXpReward;
        private bool challengeCompleted;
        private string toastMessage = string.Empty;

        private GUIStyle titleStyle;
        private GUIStyle heroStyle;
        private GUIStyle hudStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;
        private GUIStyle modeButtonStyle;
        private GUIStyle powerButtonStyle;
        private GUIStyle centerStyle;

        public int Score => score;
        public int HighScore => highScore;
        public float TimeLeft => roundTimeLeft;
        public bool IsRunning => screenState == KaninbankerScreen.Playing;

        private float Difficulty01 => roundLength <= 0f ? 0f : Mathf.Clamp01((roundLength - roundTimeLeft) / roundLength);

        private void Awake()
        {
            ConfigurePortraitRuntime();
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            profile = new KaninbankerProfile();
            profile.Load();

            selectedMode = (KaninbankerGameMode)Mathf.Clamp(PlayerPrefs.GetInt(ModeKey, 0), 0, 3);
            selectedTheme = (KaninbankerArenaTheme)Mathf.Clamp(PlayerPrefs.GetInt(ThemeKey, 0), 0, 3);
            highScore = PlayerPrefs.GetInt(HighScorePrefix + selectedMode, 0);

            audioSystem = GetComponent<KaninbankerAudio>();
            if (audioSystem == null)
                audioSystem = gameObject.AddComponent<KaninbankerAudio>();

            feedbackSystem = GetComponent<KaninbankerFeedback>();
            if (feedbackSystem == null)
                feedbackSystem = gameObject.AddComponent<KaninbankerFeedback>();

            BuildRuntimeScene();
            feedbackSystem.Configure(gameplayCamera);
            audioSystem.StartMusic();
            screenState = KaninbankerScreen.Lobby;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                ConfigurePortraitRuntime();
        }

        private static void ConfigurePortraitRuntime()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            if (Screen.orientation != ScreenOrientation.Portrait)
                Screen.orientation = ScreenOrientation.Portrait;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            for (int i = 0; i < holes.Count; i++)
                holes[i].Tick(dt);

            if (toastTimeLeft > 0f)
                toastTimeLeft -= Time.unscaledDeltaTime;

            if (screenState != KaninbankerScreen.Playing)
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
            ConfigurePortraitRuntime();
            score = 0;
            combo = 0;
            bestComboThisRound = 0;
            hits = 0;
            misses = 0;
            spawnCount = 0;
            comboTimeLeft = 0f;
            slowTimeLeft = 0f;
            doubleTimeLeft = 0f;
            shieldTimeLeft = 0f;
            frenzyTimeLeft = 0f;
            roundLength = GetRoundLength(selectedMode);
            roundTimeLeft = roundLength;
            challengeTarget = GetChallengeTarget(selectedMode, profile.Level);
            challengeProgress = 0;
            challengeCompleted = false;
            highScore = PlayerPrefs.GetInt(HighScorePrefix + selectedMode, 0);

            int extraCharge = profile.Level >= 10 ? 1 : 0;
            slowCharges = 1 + extraCharge;
            doubleCharges = 1 + extraCharge;
            shieldCharges = 1 + extraCharge;
            frenzyCharges = 1 + extraCharge;

            HideAllRabbits();
            phaseTimeLeft = 0.28f;
            screenState = KaninbankerScreen.Playing;
            ShowToast(GetModeName(selectedMode) + " START!", 1.2f);
            audioSystem.PlayRoundStart();
        }

        private void FinishRound()
        {
            screenState = KaninbankerScreen.Results;
            HideActiveRabbit(false);
            audioSystem.PlayGameOver();

            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScorePrefix + selectedMode, highScore);
                PlayerPrefs.Save();
            }

            float accuracy01 = hits + misses > 0 ? hits / (float)(hits + misses) : 0f;
            challengeCompleted = challengeProgress >= challengeTarget;
            int coinsBefore = profile.Coins;
            int xpBefore = profile.Xp;
            profile.AwardRound(score, hits, bestComboThisRound, accuracy01, challengeCompleted);
            lastCoinReward = profile.Coins - coinsBefore;
            lastXpReward = profile.Xp - xpBefore;
        }

        private float GetRoundLength(KaninbankerGameMode mode)
        {
            switch (mode)
            {
                case KaninbankerGameMode.Turbo: return 30f;
                case KaninbankerGameMode.Marathon: return 90f;
                case KaninbankerGameMode.BossRush: return 60f;
                default: return 45f;
            }
        }

        private int GetChallengeTarget(KaninbankerGameMode mode, int level)
        {
            int baseTarget;
            switch (mode)
            {
                case KaninbankerGameMode.Turbo: baseTarget = 20; break;
                case KaninbankerGameMode.Marathon: baseTarget = 55; break;
                case KaninbankerGameMode.BossRush: baseTarget = 26; break;
                default: baseTarget = 30; break;
            }
            return baseTarget + Mathf.Min(12, level / 2);
        }

        private float RabbitVisibleTime
        {
            get
            {
                float start;
                float end;
                switch (selectedMode)
                {
                    case KaninbankerGameMode.Turbo:
                        start = 0.58f; end = 0.23f; break;
                    case KaninbankerGameMode.Marathon:
                        start = 0.88f; end = 0.34f; break;
                    case KaninbankerGameMode.BossRush:
                        start = 0.74f; end = 0.29f; break;
                    default:
                        start = 0.80f; end = 0.32f; break;
                }

                float value = Mathf.Lerp(start, end, Difficulty01);
                if (slowTimeLeft > 0f) value *= 1.65f;
                if (frenzyTimeLeft > 0f) value *= 0.76f;
                return value;
            }
        }

        private float BetweenRabbitsTime
        {
            get
            {
                float value = Mathf.Lerp(0.16f, 0.055f, Difficulty01);
                if (selectedMode == KaninbankerGameMode.Turbo) value *= 0.72f;
                if (frenzyTimeLeft > 0f) value *= 0.44f;
                if (slowTimeLeft > 0f) value *= 1.15f;
                return value;
            }
        }

        private void BuildRuntimeScene()
        {
            gameplayCamera = Camera.main;
            if (gameplayCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                gameplayCamera = cameraObject.AddComponent<Camera>();
            }

            // Tall-field composition: a longer Z-axis arena fills a portrait display instead of
            // squeezing a landscape board into the middle of the phone.
            gameplayCamera.transform.position = new Vector3(0f, 13.2f, -14.2f);
            gameplayCamera.transform.rotation = Quaternion.Euler(39f, 0f, 0f);
            gameplayCamera.fieldOfView = 46f;
            gameplayCamera.nearClipPlane = 0.1f;
            gameplayCamera.farClipPlane = 80f;
            gameplayCamera.backgroundColor = GetSkyColor(selectedTheme);

            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.72f);

            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Arena Sun");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

                GameObject fillObject = new GameObject("Arena Fill");
                Light fill = fillObject.AddComponent<Light>();
                fill.type = LightType.Directional;
                fill.intensity = 0.38f;
                fillObject.transform.rotation = Quaternion.Euler(65f, 155f, 0f);
            }

            CreateGround();
            CreateArenaDecor();

            float[] xs = { -2.35f, 0f, 2.35f };
            float[] zs = { 5.0f, 2.5f, 0f, -2.5f, -5.0f };
            int index = 1;
            for (int row = 0; row < zs.Length; row++)
            {
                for (int col = 0; col < xs.Length; col++)
                {
                    holes.Add(CreateHole(index++, new Vector3(xs[col], 0f, zs[row])));
                }
            }

            ApplyThemePalette();
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Portrait Arena Ground";
            ground.transform.localScale = new Vector3(0.92f, 1f, 1.48f);
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
                themePrimaryRenderers.Add(renderer);
            SetColor(ground, GetGroundColor(selectedTheme));

            // Central stripe gives the tall arena visual depth and helps the 5 rows read clearly.
            GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
            path.name = "Arena Center Stripe";
            path.transform.position = new Vector3(0f, 0.025f, 0f);
            path.transform.localScale = new Vector3(1.15f, 0.045f, 13.4f);
            RemoveCollider(path);
            Renderer pathRenderer = path.GetComponent<Renderer>();
            if (pathRenderer != null)
                themeSecondaryRenderers.Add(pathRenderer);
            SetColor(path, GetSecondaryColor(selectedTheme));
        }

        private void CreateArenaDecor()
        {
            // Side fences frame the portrait playfield without consuming tap space.
            for (int i = 0; i < 9; i++)
            {
                float z = -6.4f + i * 1.6f;
                CreateFencePost(-4.45f, z, i);
                CreateFencePost(4.45f, z, i + 9);
            }

            // Four oversized corner totems make every arena feel more like a game world than a test scene.
            CreateTotem(new Vector3(-4.15f, 0f, 6.65f), 0);
            CreateTotem(new Vector3(4.15f, 0f, 6.65f), 1);
            CreateTotem(new Vector3(-4.15f, 0f, -6.65f), 2);
            CreateTotem(new Vector3(4.15f, 0f, -6.65f), 3);
        }

        private void CreateFencePost(float x, float z, int index)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "FencePost_" + index.ToString("00");
            post.transform.position = new Vector3(x, 0.55f, z);
            post.transform.localScale = new Vector3(0.28f, 1.1f, 0.28f);
            RemoveCollider(post);
            Renderer renderer = post.GetComponent<Renderer>();
            if (renderer != null)
                themeSecondaryRenderers.Add(renderer);

            if (index % 2 == 0)
            {
                GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = "FenceOrb_" + index.ToString("00");
                orb.transform.position = new Vector3(x, 1.32f, z);
                orb.transform.localScale = Vector3.one * 0.34f;
                RemoveCollider(orb);
                Renderer orbRenderer = orb.GetComponent<Renderer>();
                if (orbRenderer != null)
                    themeAccentRenderers.Add(orbRenderer);
            }
        }

        private void CreateTotem(Vector3 position, int index)
        {
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "ArenaTotem_" + index;
            trunk.transform.position = position + Vector3.up * 0.7f;
            trunk.transform.localScale = new Vector3(0.48f, 0.7f, 0.48f);
            RemoveCollider(trunk);
            Renderer trunkRenderer = trunk.GetComponent<Renderer>();
            if (trunkRenderer != null)
                themeSecondaryRenderers.Add(trunkRenderer);

            GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "TotemCrown_" + index;
            crown.transform.position = position + Vector3.up * 1.65f;
            crown.transform.localScale = new Vector3(0.9f, 0.62f, 0.9f);
            RemoveCollider(crown);
            Renderer crownRenderer = crown.GetComponent<Renderer>();
            if (crownRenderer != null)
                themeAccentRenderers.Add(crownRenderer);
        }

        private RabbitHole CreateHole(int index, Vector3 position)
        {
            GameObject root = new GameObject("Hole_" + index.ToString("00"));
            root.transform.position = position;

            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localScale = new Vector3(0.92f, 0.075f, 0.92f);
            RemoveCollider(rim);
            SetColor(rim, GetRimColor(selectedTheme));

            GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            soil.name = "HoleDark";
            soil.transform.SetParent(root.transform, false);
            soil.transform.localPosition = new Vector3(0f, 0.045f, 0f);
            soil.transform.localScale = new Vector3(0.70f, 0.03f, 0.70f);
            RemoveCollider(soil);
            SetColor(soil, new Color(0.055f, 0.035f, 0.025f));

            GameObject rabbit = new GameObject("Rabbit");
            rabbit.transform.SetParent(root.transform, false);

            RabbitTarget target = rabbit.AddComponent<RabbitTarget>();
            BoxCollider hitBox = rabbit.AddComponent<BoxCollider>();
            hitBox.center = new Vector3(0f, 0.78f, 0f);
            hitBox.size = new Vector3(1.42f, 2.4f, 1.10f);

            CreatePart(PrimitiveType.Sphere, "Body", rabbit.transform, new Vector3(0f, 0.42f, 0f), new Vector3(0.86f, 1.00f, 0.76f), new Color(0.76f, 0.70f, 0.62f));
            CreatePart(PrimitiveType.Sphere, "Belly", rabbit.transform, new Vector3(0f, 0.36f, -0.33f), new Vector3(0.52f, 0.62f, 0.18f), new Color(0.93f, 0.88f, 0.80f));
            CreatePart(PrimitiveType.Sphere, "Head", rabbit.transform, new Vector3(0f, 1.20f, 0f), new Vector3(0.73f, 0.73f, 0.67f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Capsule, "EarLeft", rabbit.transform, new Vector3(-0.22f, 1.90f, 0f), new Vector3(0.19f, 0.48f, 0.17f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Capsule, "EarRight", rabbit.transform, new Vector3(0.22f, 1.90f, 0f), new Vector3(0.19f, 0.48f, 0.17f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Sphere, "Nose", rabbit.transform, new Vector3(0f, 1.12f, -0.34f), new Vector3(0.16f, 0.12f, 0.12f), new Color(0.92f, 0.38f, 0.48f));
            CreatePart(PrimitiveType.Sphere, "EyeLeft", rabbit.transform, new Vector3(-0.18f, 1.30f, -0.31f), new Vector3(0.10f, 0.12f, 0.07f), new Color(0.025f, 0.025f, 0.03f));
            CreatePart(PrimitiveType.Sphere, "EyeRight", rabbit.transform, new Vector3(0.18f, 1.30f, -0.31f), new Vector3(0.10f, 0.12f, 0.07f), new Color(0.025f, 0.025f, 0.03f));

            // A small top marker is recoloured per rabbit type: gold, bomb, boss etc.
            CreatePart(PrimitiveType.Cube, "TypeMarker", rabbit.transform, new Vector3(0f, 2.30f, 0f), new Vector3(0.46f, 0.18f, 0.46f), new Color(0.35f, 0.80f, 1f));

            RabbitHole hole = root.AddComponent<RabbitHole>();
            hole.Configure(rabbit, rim.GetComponent<Renderer>());
            target.Hole = hole;
            rabbit.SetActive(false);
            return hole;
        }

        private static void CreatePart(PrimitiveType primitive, string partName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            RemoveCollider(part);
            SetColor(part, color);
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            Collider collider = gameObject.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private static void SetColor(GameObject gameObject, Color color)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            if (shader == null)
                return;

            Material material = new Material(shader);
            material.color = color;
            renderer.sharedMaterial = material;
        }

        private void ApplyThemePalette()
        {
            if (gameplayCamera != null)
                gameplayCamera.backgroundColor = GetSkyColor(selectedTheme);

            Color primary = GetGroundColor(selectedTheme);
            Color secondary = GetSecondaryColor(selectedTheme);
            Color accent = GetAccentColor(selectedTheme);
            Color rim = GetRimColor(selectedTheme);

            ApplyColor(themePrimaryRenderers, primary);
            ApplyColor(themeSecondaryRenderers, secondary);
            ApplyColor(themeAccentRenderers, accent);

            for (int i = 0; i < holes.Count; i++)
                holes[i].SetRimColor(rim);
        }

        private static void ApplyColor(List<Renderer> renderers, Color color)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                    renderers[i].material.color = color;
            }
        }

        private void ShowRandomRabbit()
        {
            if (holes.Count == 0)
                return;

            spawnCount++;
            int nextIndex = Random.Range(0, holes.Count);
            activeHole = holes[nextIndex];
            RabbitKind kind = PickRabbitKind();
            int health = GetRabbitHealth(kind);
            activeHole.Show(kind, health);

            float visible = RabbitVisibleTime * GetKindTimeMultiplier(kind);
            phaseTimeLeft = visible;
            audioSystem.PlayRabbitPop(Difficulty01);
        }

        private RabbitKind PickRabbitKind()
        {
            if (selectedMode == KaninbankerGameMode.BossRush && spawnCount % 7 == 0)
                return RabbitKind.Boss;

            if (selectedMode != KaninbankerGameMode.BossRush && spawnCount >= 18 && spawnCount % 19 == 0)
                return RabbitKind.Boss;

            float roll = Random.value;
            float difficulty = Difficulty01;
            float bombChance = Mathf.Lerp(0.05f, 0.11f, difficulty);
            float armoredChance = Mathf.Lerp(0.06f, 0.14f, difficulty);
            float goldChance = 0.08f;
            float fastChance = Mathf.Lerp(0.12f, 0.24f, difficulty);

            if (roll < bombChance) return RabbitKind.Bomb;
            roll -= bombChance;
            if (roll < goldChance) return RabbitKind.Gold;
            roll -= goldChance;
            if (roll < armoredChance) return RabbitKind.Armored;
            roll -= armoredChance;
            if (roll < fastChance) return RabbitKind.Fast;
            return RabbitKind.Normal;
        }

        private int GetRabbitHealth(RabbitKind kind)
        {
            switch (kind)
            {
                case RabbitKind.Armored: return 2;
                case RabbitKind.Boss: return 6 + Mathf.Min(6, profile.Level / 3);
                default: return 1;
            }
        }

        private static float GetKindTimeMultiplier(RabbitKind kind)
        {
            switch (kind)
            {
                case RabbitKind.Fast: return 0.66f;
                case RabbitKind.Gold: return 0.82f;
                case RabbitKind.Armored: return 1.35f;
                case RabbitKind.Boss: return 3.4f;
                default: return 1f;
            }
        }

        private void HideActiveRabbit(bool countMiss)
        {
            if (activeHole != null && activeHole.RabbitObject != null)
            {
                Vector3 feedbackPosition = activeHole.transform.position;
                RabbitKind kind = activeHole.Kind;
                activeHole.Hide();

                if (countMiss)
                {
                    if (kind == RabbitKind.Bomb)
                    {
                        score += 2;
                        ShowToast("BOMBE UNDGÅET +2", 0.65f);
                    }
                    else if (shieldTimeLeft > 0f)
                    {
                        ShowToast("SKJOLD REDDER DIG", 0.65f);
                    }
                    else
                    {
                        misses++;
                        combo = 0;
                        comboTimeLeft = 0f;
                        audioSystem.PlayMiss();
                        feedbackSystem.PlayMiss(feedbackPosition);
                    }
                }
            }

            activeHole = null;
            phaseTimeLeft = BetweenRabbitsTime;
        }

        private void HideAllRabbits()
        {
            for (int i = 0; i < holes.Count; i++)
            {
                if (holes[i] != null)
                    holes[i].Hide();
            }
            activeHole = null;
        }

        private void ReadPointerInput()
        {
            if (gameplayCamera == null)
                return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && !IsReservedUiTouch(touch.position))
                    TryHit(touch.position);
                return;
            }

            if (Input.GetMouseButtonDown(0) && !IsReservedUiTouch(Input.mousePosition))
                TryHit(Input.mousePosition);
        }

        private bool IsReservedUiTouch(Vector2 screenPosition)
        {
            Rect safe = Screen.safeArea;
            float topReserved = safe.height * 0.18f;
            float bottomReserved = safe.height * 0.17f;
            if (screenPosition.y > safe.yMax - topReserved)
                return true;
            if (screenPosition.y < safe.y + bottomReserved)
                return true;
            return false;
        }

        private void TryHit(Vector2 screenPosition)
        {
            Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, 100f))
                return;

            RabbitTarget target = hit.collider.GetComponentInParent<RabbitTarget>();
            if (target == null || activeHole == null || target.Hole != activeHole || !activeHole.IsVisible)
                return;

            RabbitKind kind = activeHole.Kind;
            Vector3 feedbackPosition = activeHole.transform.position;

            if (kind == RabbitKind.Bomb)
            {
                if (shieldTimeLeft > 0f)
                {
                    score += 3;
                    ShowToast("SKJOLD SMADRER BOMBE +3", 0.75f);
                    audioSystem.PlayHit(1);
                    feedbackSystem.PlayHit(feedbackPosition, 2);
                }
                else
                {
                    score = Mathf.Max(0, score - 8);
                    misses++;
                    combo = 0;
                    comboTimeLeft = 0f;
                    ShowToast("BOMBE! -8", 0.85f);
                    audioSystem.PlayMiss();
                    feedbackSystem.PlayMiss(feedbackPosition);
                }

                activeHole.Hide();
                activeHole = null;
                phaseTimeLeft = BetweenRabbitsTime;
                return;
            }

            combo = comboTimeLeft > 0f ? combo + 1 : 1;
            comboTimeLeft = ComboWindow;
            bestComboThisRound = Mathf.Max(bestComboThisRound, combo);

            bool defeated = activeHole.TakeHit();
            int hitValue = GetBasePoints(kind);
            int comboMultiplier = 1 + Mathf.Min(4, combo / 4);
            int powerMultiplier = doubleTimeLeft > 0f ? 2 : 1;
            int frenzyBonus = frenzyTimeLeft > 0f ? 1 : 0;
            int gained = (hitValue + frenzyBonus) * comboMultiplier * powerMultiplier;
            score += gained;

            audioSystem.PlayHit(combo);
            feedbackSystem.PlayHit(feedbackPosition, combo);

            if (!defeated)
            {
                phaseTimeLeft = Mathf.Max(phaseTimeLeft, kind == RabbitKind.Boss ? 1.25f : 0.70f);
                ShowToast(kind == RabbitKind.Boss ? "BOSS HP " + activeHole.Health : "PANser HP " + activeHole.Health, 0.55f);
                return;
            }

            hits++;
            challengeProgress = Mathf.Min(challengeTarget, challengeProgress + 1);
            if (challengeProgress >= challengeTarget && !challengeCompleted)
            {
                challengeCompleted = true;
                ShowToast("MISSION KLARET!", 1.15f);
            }
            else if (kind == RabbitKind.Gold)
            {
                ShowToast("GULDKANIN +" + gained, 0.75f);
            }
            else if (kind == RabbitKind.Boss)
            {
                ShowToast("BOSS KNUST +" + gained, 1.0f);
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (combo >= 3 || kind == RabbitKind.Boss || kind == RabbitKind.Gold)
                Handheld.Vibrate();
#endif

            if (score > highScore)
                highScore = score;

            activeHole.Hide();
            activeHole = null;
            phaseTimeLeft = BetweenRabbitsTime;
        }

        private static int GetBasePoints(RabbitKind kind)
        {
            switch (kind)
            {
                case RabbitKind.Fast: return 2;
                case RabbitKind.Gold: return 6;
                case RabbitKind.Armored: return 3;
                case RabbitKind.Boss: return 4;
                default: return 1;
            }
        }

        private void UsePowerUp(int index)
        {
            if (screenState != KaninbankerScreen.Playing)
                return;

            switch (index)
            {
                case 0:
                    if (slowCharges <= 0) return;
                    slowCharges--;
                    slowTimeLeft = Mathf.Max(slowTimeLeft, 7f);
                    ShowToast("SLOW-MO 7 SEK", 0.8f);
                    break;
                case 1:
                    if (doubleCharges <= 0) return;
                    doubleCharges--;
                    doubleTimeLeft = Mathf.Max(doubleTimeLeft, 10f);
                    ShowToast("DOBBELT SCORE 10 SEK", 0.8f);
                    break;
                case 2:
                    if (shieldCharges <= 0) return;
                    shieldCharges--;
                    shieldTimeLeft = Mathf.Max(shieldTimeLeft, 9f);
                    ShowToast("SKJOLD 9 SEK", 0.8f);
                    break;
                case 3:
                    if (frenzyCharges <= 0) return;
                    frenzyCharges--;
                    frenzyTimeLeft = Mathf.Max(frenzyTimeLeft, 8f);
                    ShowToast("KANIN-FRENZY 8 SEK", 0.8f);
                    break;
            }

            audioSystem.PlayUi();
        }

        private void SelectMode(KaninbankerGameMode mode)
        {
            selectedMode = mode;
            PlayerPrefs.SetInt(ModeKey, (int)selectedMode);
            PlayerPrefs.Save();
            highScore = PlayerPrefs.GetInt(HighScorePrefix + selectedMode, 0);
            audioSystem.PlayUi();
        }

        private void CycleArena()
        {
            selectedTheme = (KaninbankerArenaTheme)(((int)selectedTheme + 1) % 4);
            PlayerPrefs.SetInt(ThemeKey, (int)selectedTheme);
            PlayerPrefs.Save();
            ApplyThemePalette();
            audioSystem.PlayUi();
            ShowToast("ARENA: " + GetThemeName(selectedTheme), 0.9f);
        }

        private void ShowToast(string message, float seconds)
        {
            toastMessage = message;
            toastTimeLeft = seconds;
        }

        private void OnGUI()
        {
            EnsureStyles();
            Rect safe = GetSafeGuiRect();

            if (screenState == KaninbankerScreen.Lobby)
                DrawLobby(safe);
            else if (screenState == KaninbankerScreen.Playing)
                DrawGameplayHud(safe);
            else
                DrawResults(safe);

            if (toastTimeLeft > 0f && !string.IsNullOrEmpty(toastMessage))
                DrawToast(safe);
        }

        private Rect GetSafeGuiRect()
        {
            Rect safe = Screen.safeArea;
            return new Rect(safe.x, Screen.height - safe.yMax, safe.width, safe.height);
        }

        private void DrawLobby(Rect safe)
        {
            float margin = Mathf.Max(16f, safe.width * 0.035f);
            float cardX = safe.x + margin;
            float cardW = safe.width - margin * 2f;
            float top = safe.y + margin;
            float heroH = safe.height * 0.19f;

            DrawCard(new Rect(cardX, top, cardW, heroH), new Color(0.04f, 0.055f, 0.09f, 0.94f));
            GUI.Label(new Rect(cardX + margin, top + heroH * 0.04f, cardW - margin * 2f, heroH * 0.38f), "KANINBANKER", heroStyle);
            GUI.Label(new Rect(cardX + margin, top + heroH * 0.38f, cardW - margin * 2f, heroH * 0.22f), "PORTRAIT MAYHEM", titleStyle);

            float statY = top + heroH * 0.65f;
            float statW = (cardW - margin * 4f) / 3f;
            DrawStat(new Rect(cardX + margin, statY, statW, heroH * 0.27f), "LEVEL", profile.Level.ToString());
            DrawStat(new Rect(cardX + margin * 2f + statW, statY, statW, heroH * 0.27f), "MØNTER", profile.Coins.ToString());
            DrawStat(new Rect(cardX + margin * 3f + statW * 2f, statY, statW, heroH * 0.27f), "TROFÆER", profile.Trophies.ToString());

            float modeY = top + heroH + margin * 0.75f;
            GUI.Label(new Rect(cardX, modeY, cardW, safe.height * 0.045f), "VÆLG SPILTYPE", titleStyle);
            modeY += safe.height * 0.05f;

            float gap = margin * 0.45f;
            float buttonH = safe.height * 0.072f;
            float halfW = (cardW - gap) * 0.5f;
            DrawModeButton(new Rect(cardX, modeY, halfW, buttonH), KaninbankerGameMode.Classic, "CLASSIC", "45 sek / fuld mix");
            DrawModeButton(new Rect(cardX + halfW + gap, modeY, halfW, buttonH), KaninbankerGameMode.Turbo, "TURBO", "30 sek / lynhurtig");
            modeY += buttonH + gap;
            DrawModeButton(new Rect(cardX, modeY, halfW, buttonH), KaninbankerGameMode.Marathon, "MARATHON", "90 sek / udholdenhed");
            DrawModeButton(new Rect(cardX + halfW + gap, modeY, halfW, buttonH), KaninbankerGameMode.BossRush, "BOSS RUSH", "60 sek / boss-kaos");

            float profileY = modeY + buttonH + margin * 0.8f;
            float profileH = safe.height * 0.12f;
            DrawCard(new Rect(cardX, profileY, cardW, profileH), new Color(0.07f, 0.08f, 0.12f, 0.92f));
            GUI.Label(new Rect(cardX + margin, profileY + profileH * 0.07f, cardW - margin * 2f, profileH * 0.30f), "RANG: " + profile.Rank, hudStyle);
            GUI.Label(new Rect(cardX + margin, profileY + profileH * 0.36f, cardW - margin * 2f, profileH * 0.25f), "XP TIL NÆSTE LEVEL: " + profile.LevelXp + " / 450", smallStyle);
            DrawProgressBar(new Rect(cardX + margin, profileY + profileH * 0.68f, cardW - margin * 2f, profileH * 0.16f), profile.LevelProgress01, GetAccentColor(selectedTheme));

            float arenaY = profileY + profileH + margin * 0.65f;
            float arenaH = safe.height * 0.062f;
            if (GUI.Button(new Rect(cardX, arenaY, cardW, arenaH), "ARENA: " + GetThemeName(selectedTheme) + "   - TRYK FOR NÆSTE", buttonStyle))
                CycleArena();

            float startH = safe.height * 0.10f;
            float startY = safe.yMax - margin - startH;
            GUI.backgroundColor = GetAccentColor(selectedTheme);
            if (GUI.Button(new Rect(cardX, startY, cardW, startH), "START " + GetModeName(selectedMode), heroStyle))
            {
                audioSystem.PlayUi();
                StartRound();
            }
            GUI.backgroundColor = Color.white;

            GUI.Label(new Rect(cardX, startY - safe.height * 0.055f, cardW, safe.height * 0.05f), "15 huller - boss-kaniner - bomber - guld - powerups - progression", centerStyle);
        }

        private void DrawGameplayHud(Rect safe)
        {
            float margin = Mathf.Max(12f, safe.width * 0.025f);
            float topH = safe.height * 0.145f;
            DrawCard(new Rect(safe.x + margin, safe.y + margin, safe.width - margin * 2f, topH), new Color(0.03f, 0.04f, 0.07f, 0.93f));

            float cardX = safe.x + margin;
            float cardW = safe.width - margin * 2f;
            float colW = cardW / 3f;
            GUI.Label(new Rect(cardX, safe.y + margin * 1.1f, colW, topH * 0.42f), score.ToString(), heroStyle);
            GUI.Label(new Rect(cardX + colW, safe.y + margin * 1.1f, colW, topH * 0.42f), Mathf.CeilToInt(roundTimeLeft) + "s", heroStyle);
            GUI.Label(new Rect(cardX + colW * 2f, safe.y + margin * 1.1f, colW, topH * 0.42f), combo > 1 ? "x" + combo : "-", heroStyle);
            GUI.Label(new Rect(cardX, safe.y + topH * 0.47f, colW, topH * 0.20f), "SCORE", smallStyle);
            GUI.Label(new Rect(cardX + colW, safe.y + topH * 0.47f, colW, topH * 0.20f), "TID", smallStyle);
            GUI.Label(new Rect(cardX + colW * 2f, safe.y + topH * 0.47f, colW, topH * 0.20f), "COMBO", smallStyle);

            float challengeY = safe.y + topH * 0.72f;
            float challengeW = cardW - margin * 2f;
            GUI.Label(new Rect(cardX + margin, challengeY - topH * 0.09f, challengeW, topH * 0.18f), "MISSION " + challengeProgress + "/" + challengeTarget, smallStyle);
            DrawProgressBar(new Rect(cardX + margin, challengeY + topH * 0.10f, challengeW, topH * 0.12f), challengeTarget > 0 ? challengeProgress / (float)challengeTarget : 0f, challengeCompleted ? new Color(0.35f, 1f, 0.55f) : GetAccentColor(selectedTheme));

            float bottomH = safe.height * 0.145f;
            float bottomY = safe.yMax - bottomH - margin;
            DrawCard(new Rect(cardX, bottomY, cardW, bottomH), new Color(0.03f, 0.04f, 0.07f, 0.94f));

            float gap = Mathf.Max(5f, margin * 0.35f);
            float powerW = (cardW - margin * 2f - gap * 3f) / 4f;
            float powerY = bottomY + bottomH * 0.18f;
            float powerH = bottomH * 0.66f;
            DrawPowerButton(new Rect(cardX + margin, powerY, powerW, powerH), 0, "SLOW", slowCharges, slowTimeLeft);
            DrawPowerButton(new Rect(cardX + margin + (powerW + gap), powerY, powerW, powerH), 1, "x2", doubleCharges, doubleTimeLeft);
            DrawPowerButton(new Rect(cardX + margin + (powerW + gap) * 2f, powerY, powerW, powerH), 2, "SKJOLD", shieldCharges, shieldTimeLeft);
            DrawPowerButton(new Rect(cardX + margin + (powerW + gap) * 3f, powerY, powerW, powerH), 3, "FRENZY", frenzyCharges, frenzyTimeLeft);

            if (activeHole != null && activeHole.Kind == RabbitKind.Boss && activeHole.IsVisible)
            {
                float bossW = safe.width * 0.74f;
                float bossH = safe.height * 0.055f;
                float bossX = safe.x + (safe.width - bossW) * 0.5f;
                float bossY = safe.y + topH + margin * 1.7f;
                DrawCard(new Rect(bossX, bossY, bossW, bossH), new Color(0.24f, 0.02f, 0.04f, 0.92f));
                GUI.Label(new Rect(bossX, bossY, bossW, bossH * 0.55f), "MEGA-BOSS", titleStyle);
                DrawProgressBar(new Rect(bossX + bossW * 0.08f, bossY + bossH * 0.64f, bossW * 0.84f, bossH * 0.18f), activeHole.MaxHealth > 0 ? activeHole.Health / (float)activeHole.MaxHealth : 0f, new Color(1f, 0.18f, 0.14f));
            }
        }

        private void DrawResults(Rect safe)
        {
            float margin = Mathf.Max(18f, safe.width * 0.04f);
            float panelW = safe.width - margin * 2f;
            float panelH = safe.height * 0.72f;
            float panelX = safe.x + margin;
            float panelY = safe.y + safe.height * 0.12f;
            DrawCard(new Rect(panelX, panelY, panelW, panelH), new Color(0.035f, 0.045f, 0.075f, 0.97f));

            GUI.Label(new Rect(panelX + margin, panelY + panelH * 0.04f, panelW - margin * 2f, panelH * 0.10f), "RUNDE FÆRDIG", titleStyle);
            GUI.Label(new Rect(panelX + margin, panelY + panelH * 0.13f, panelW - margin * 2f, panelH * 0.16f), score.ToString(), heroStyle);
            GUI.Label(new Rect(panelX + margin, panelY + panelH * 0.27f, panelW - margin * 2f, panelH * 0.06f), "REKORD " + highScore + "   -   BEDSTE COMBO x" + bestComboThisRound, centerStyle);

            float accuracy = hits + misses > 0 ? hits * 100f / (hits + misses) : 0f;
            float statY = panelY + panelH * 0.37f;
            float statH = panelH * 0.075f;
            DrawResultLine(new Rect(panelX + margin, statY, panelW - margin * 2f, statH), "TRÆFFERE", hits.ToString());
            DrawResultLine(new Rect(panelX + margin, statY + statH, panelW - margin * 2f, statH), "MISS", misses.ToString());
            DrawResultLine(new Rect(panelX + margin, statY + statH * 2f, panelW - margin * 2f, statH), "PRÆCISION", accuracy.ToString("0") + "%");
            DrawResultLine(new Rect(panelX + margin, statY + statH * 3f, panelW - margin * 2f, statH), "BELØNNING", "+" + lastCoinReward + " mønter / +" + lastXpReward + " XP");

            string mission = challengeCompleted ? "MISSION KLARET - BONUS UDBETALT" : "MISSION " + challengeProgress + "/" + challengeTarget;
            GUI.Label(new Rect(panelX + margin, panelY + panelH * 0.70f, panelW - margin * 2f, panelH * 0.07f), mission, challengeCompleted ? titleStyle : centerStyle);

            float buttonH = panelH * 0.105f;
            float buttonY = panelY + panelH - buttonH - margin;
            float gap = margin * 0.5f;
            float half = (panelW - margin * 2f - gap) * 0.5f;
            if (GUI.Button(new Rect(panelX + margin, buttonY, half, buttonH), "LOBBY", buttonStyle))
            {
                audioSystem.PlayUi();
                screenState = KaninbankerScreen.Lobby;
            }

            GUI.backgroundColor = GetAccentColor(selectedTheme);
            if (GUI.Button(new Rect(panelX + margin + half + gap, buttonY, half, buttonH), "SPIL IGEN", buttonStyle))
            {
                audioSystem.PlayUi();
                StartRound();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawModeButton(Rect rect, KaninbankerGameMode mode, string headline, string subtitle)
        {
            bool selected = selectedMode == mode;
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = selected ? GetAccentColor(selectedTheme) : new Color(0.22f, 0.24f, 0.30f);
            if (GUI.Button(rect, headline + "\n" + subtitle, modeButtonStyle))
                SelectMode(mode);
            GUI.backgroundColor = old;
        }

        private void DrawPowerButton(Rect rect, int index, string name, int charges, float activeTime)
        {
            Color old = GUI.backgroundColor;
            if (activeTime > 0f)
                GUI.backgroundColor = GetAccentColor(selectedTheme);
            else if (charges <= 0)
                GUI.backgroundColor = new Color(0.18f, 0.18f, 0.20f);
            else
                GUI.backgroundColor = new Color(0.25f, 0.28f, 0.36f);

            string status = activeTime > 0f ? activeTime.ToString("0.0") + "s" : "x" + charges;
            if (GUI.Button(rect, name + "\n" + status, powerButtonStyle))
                UsePowerUp(index);
            GUI.backgroundColor = old;
        }

        private void DrawStat(Rect rect, string label, string value)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height * 0.48f), value, hudStyle);
            GUI.Label(new Rect(rect.x, rect.y + rect.height * 0.46f, rect.width, rect.height * 0.45f), label, smallStyle);
        }

        private void DrawResultLine(Rect rect, string left, string right)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width * 0.45f, rect.height), left, bodyStyle);
            GUI.Label(new Rect(rect.x + rect.width * 0.45f, rect.y, rect.width * 0.55f, rect.height), right, hudStyle);
        }

        private void DrawToast(Rect safe)
        {
            float width = safe.width * 0.78f;
            float height = safe.height * 0.065f;
            float x = safe.x + (safe.width - width) * 0.5f;
            float y = safe.y + safe.height * 0.20f;
            DrawCard(new Rect(x, y, width, height), new Color(0.02f, 0.02f, 0.035f, 0.90f));
            GUI.Label(new Rect(x + 8f, y, width - 16f, height), toastMessage, titleStyle);
        }

        private static void DrawCard(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void DrawProgressBar(Rect rect, float progress01, Color fillColor)
        {
            Color old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.42f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = fillColor;
            Rect fill = rect;
            fill.width *= Mathf.Clamp01(progress01);
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            int reference = Mathf.Min(Screen.width, Screen.height);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 22, 22, 48),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            titleStyle.normal.textColor = Color.white;

            heroStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 14, 30, 72),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            heroStyle.normal.textColor = Color.white;
            heroStyle.hover.textColor = Color.white;
            heroStyle.active.textColor = Color.white;

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 23, 20, 46),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            hudStyle.normal.textColor = Color.white;

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 29, 17, 36),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            bodyStyle.normal.textColor = new Color(0.86f, 0.88f, 0.94f);

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 34, 15, 30),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            smallStyle.normal.textColor = new Color(0.78f, 0.82f, 0.90f);

            centerStyle = new GUIStyle(bodyStyle)
            {
                alignment = TextAnchor.MiddleCenter
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 26, 18, 38),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            modeButtonStyle = new GUIStyle(buttonStyle)
            {
                fontSize = Mathf.Clamp(reference / 31, 16, 32)
            };

            powerButtonStyle = new GUIStyle(buttonStyle)
            {
                fontSize = Mathf.Clamp(reference / 36, 14, 28)
            };
        }

        private static string GetModeName(KaninbankerGameMode mode)
        {
            switch (mode)
            {
                case KaninbankerGameMode.Turbo: return "TURBO";
                case KaninbankerGameMode.Marathon: return "MARATHON";
                case KaninbankerGameMode.BossRush: return "BOSS RUSH";
                default: return "CLASSIC";
            }
        }

        private static string GetThemeName(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return "NEON NAT";
                case KaninbankerArenaTheme.Candy: return "SUKKERLAND";
                case KaninbankerArenaTheme.Volcano: return "LAVA PIT";
                default: return "MEGA ENG";
            }
        }

        private static Color GetSkyColor(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return new Color(0.015f, 0.025f, 0.09f);
                case KaninbankerArenaTheme.Candy: return new Color(0.38f, 0.14f, 0.38f);
                case KaninbankerArenaTheme.Volcano: return new Color(0.13f, 0.025f, 0.02f);
                default: return new Color(0.08f, 0.19f, 0.30f);
            }
        }

        private static Color GetGroundColor(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return new Color(0.055f, 0.075f, 0.15f);
                case KaninbankerArenaTheme.Candy: return new Color(0.44f, 0.16f, 0.34f);
                case KaninbankerArenaTheme.Volcano: return new Color(0.16f, 0.09f, 0.07f);
                default: return new Color(0.16f, 0.39f, 0.20f);
            }
        }

        private static Color GetSecondaryColor(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return new Color(0.10f, 0.20f, 0.38f);
                case KaninbankerArenaTheme.Candy: return new Color(0.90f, 0.44f, 0.68f);
                case KaninbankerArenaTheme.Volcano: return new Color(0.30f, 0.12f, 0.07f);
                default: return new Color(0.28f, 0.18f, 0.08f);
            }
        }

        private static Color GetAccentColor(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return new Color(0.16f, 0.86f, 1f);
                case KaninbankerArenaTheme.Candy: return new Color(1f, 0.50f, 0.78f);
                case KaninbankerArenaTheme.Volcano: return new Color(1f, 0.28f, 0.08f);
                default: return new Color(0.40f, 0.94f, 0.45f);
            }
        }

        private static Color GetRimColor(KaninbankerArenaTheme theme)
        {
            switch (theme)
            {
                case KaninbankerArenaTheme.Night: return new Color(0.08f, 0.15f, 0.27f);
                case KaninbankerArenaTheme.Candy: return new Color(0.56f, 0.19f, 0.42f);
                case KaninbankerArenaTheme.Volcano: return new Color(0.23f, 0.07f, 0.04f);
                default: return new Color(0.22f, 0.12f, 0.06f);
            }
        }
    }

    internal sealed class RabbitTarget : MonoBehaviour
    {
        public RabbitHole Hole { get; set; }
    }

    internal sealed class RabbitHole : MonoBehaviour
    {
        private GameObject rabbitObject;
        private Transform rabbitTransform;
        private Renderer[] rabbitRenderers;
        private Renderer rimRenderer;
        private Vector3 hiddenPosition;
        private Vector3 visiblePosition;
        private Vector3 targetScale;
        private float animationTime;

        public GameObject RabbitObject => rabbitObject;
        public bool IsVisible { get; private set; }
        public RabbitKind Kind { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public void Configure(GameObject rabbit, Renderer rim)
        {
            rabbitObject = rabbit;
            rabbitTransform = rabbit.transform;
            rabbitRenderers = rabbit.GetComponentsInChildren<Renderer>(true);
            rimRenderer = rim;
            hiddenPosition = new Vector3(0f, -1.25f, 0f);
            visiblePosition = new Vector3(0f, 0.58f, 0f);
            targetScale = Vector3.one;
            rabbitTransform.localPosition = hiddenPosition;
        }

        public void SetRimColor(Color color)
        {
            if (rimRenderer != null && rimRenderer.material != null)
                rimRenderer.material.color = color;
        }

        public void Show(RabbitKind kind, int health)
        {
            if (rabbitObject == null)
                return;

            Kind = kind;
            Health = Mathf.Max(1, health);
            MaxHealth = Health;
            targetScale = GetScaleForKind(kind);
            animationTime = 0f;
            rabbitTransform.localPosition = hiddenPosition;
            rabbitTransform.localScale = targetScale * 0.80f;
            ApplyLook(kind);
            rabbitObject.SetActive(true);
            IsVisible = true;
        }

        public bool TakeHit()
        {
            if (!IsVisible)
                return false;

            Health = Mathf.Max(0, Health - 1);
            animationTime = Mathf.Max(animationTime, 0.06f);
            if (Health > 0)
            {
                rabbitTransform.localScale = targetScale * 1.12f;
                return false;
            }
            return true;
        }

        public void Hide()
        {
            IsVisible = false;
            if (rabbitObject != null)
            {
                rabbitTransform.localPosition = hiddenPosition;
                rabbitTransform.localScale = Vector3.one;
                rabbitObject.SetActive(false);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsVisible || rabbitObject == null)
                return;

            animationTime += deltaTime;
            float pop = 1f - Mathf.Exp(-animationTime * 11f);
            rabbitTransform.localPosition = Vector3.Lerp(hiddenPosition, visiblePosition, pop);

            float wobble = 1f + Mathf.Sin(animationTime * (Kind == RabbitKind.Fast ? 18f : 11f)) * 0.025f;
            float bossPulse = Kind == RabbitKind.Boss ? 1f + Mathf.Sin(animationTime * 5.5f) * 0.035f : 1f;
            rabbitTransform.localScale = targetScale * wobble * bossPulse;
            rabbitTransform.localRotation = Quaternion.Euler(0f, Mathf.Sin(animationTime * 8f) * (Kind == RabbitKind.Fast ? 7f : 3f), 0f);
        }

        private static Vector3 GetScaleForKind(RabbitKind kind)
        {
            switch (kind)
            {
                case RabbitKind.Fast: return Vector3.one * 0.88f;
                case RabbitKind.Gold: return Vector3.one * 1.03f;
                case RabbitKind.Armored: return new Vector3(1.06f, 1.04f, 1.06f);
                case RabbitKind.Bomb: return Vector3.one * 0.96f;
                case RabbitKind.Boss: return Vector3.one * 1.32f;
                default: return Vector3.one;
            }
        }

        private void ApplyLook(RabbitKind kind)
        {
            Color body = new Color(0.76f, 0.70f, 0.62f);
            Color head = new Color(0.84f, 0.79f, 0.72f);
            Color belly = new Color(0.93f, 0.88f, 0.80f);
            Color marker = new Color(0.30f, 0.76f, 1f);

            switch (kind)
            {
                case RabbitKind.Fast:
                    body = new Color(0.42f, 0.72f, 0.98f);
                    head = new Color(0.56f, 0.82f, 1f);
                    marker = new Color(0.10f, 0.95f, 1f);
                    break;
                case RabbitKind.Gold:
                    body = new Color(1f, 0.70f, 0.08f);
                    head = new Color(1f, 0.82f, 0.22f);
                    belly = new Color(1f, 0.93f, 0.58f);
                    marker = new Color(1f, 0.95f, 0.18f);
                    break;
                case RabbitKind.Armored:
                    body = new Color(0.38f, 0.42f, 0.50f);
                    head = new Color(0.52f, 0.56f, 0.64f);
                    belly = new Color(0.72f, 0.76f, 0.84f);
                    marker = new Color(0.74f, 0.82f, 0.92f);
                    break;
                case RabbitKind.Bomb:
                    body = new Color(0.10f, 0.10f, 0.12f);
                    head = new Color(0.16f, 0.16f, 0.18f);
                    belly = new Color(0.42f, 0.05f, 0.06f);
                    marker = new Color(1f, 0.10f, 0.08f);
                    break;
                case RabbitKind.Boss:
                    body = new Color(0.48f, 0.12f, 0.16f);
                    head = new Color(0.65f, 0.18f, 0.22f);
                    belly = new Color(0.92f, 0.40f, 0.18f);
                    marker = new Color(1f, 0.16f, 0.08f);
                    break;
            }

            for (int i = 0; i < rabbitRenderers.Length; i++)
            {
                Renderer renderer = rabbitRenderers[i];
                if (renderer == null || renderer.material == null)
                    continue;

                string part = renderer.gameObject.name;
                if (part == "Body") renderer.material.color = body;
                else if (part == "Head" || part == "EarLeft" || part == "EarRight") renderer.material.color = head;
                else if (part == "Belly") renderer.material.color = belly;
                else if (part == "TypeMarker") renderer.material.color = marker;
                else if (part == "Nose") renderer.material.color = kind == RabbitKind.Bomb ? new Color(1f, 0.08f, 0.05f) : new Color(0.92f, 0.38f, 0.48f);
                else if (part == "EyeLeft" || part == "EyeRight") renderer.material.color = Color.black;
            }
        }
    }
}
