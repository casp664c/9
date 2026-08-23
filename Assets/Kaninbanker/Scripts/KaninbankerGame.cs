using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    public sealed class KaninbankerGame : MonoBehaviour
    {
        private const string HighScoreKey = "Kaninbanker.HighScore";
        private const float RoundLength = 30f;
        private const float BaseRabbitVisibleTime = 0.82f;
        private const float MinimumRabbitVisibleTime = 0.38f;
        private const float BaseBetweenRabbitsTime = 0.18f;
        private const float MinimumBetweenRabbitsTime = 0.08f;
        private const float ComboWindow = 1.35f;

        private readonly List<RabbitHole> holes = new List<RabbitHole>();
        private Camera gameplayCamera;
        private RabbitHole activeHole;
        private float roundTimeLeft;
        private float phaseTimeLeft;
        private float comboTimeLeft;
        private int score;
        private int highScore;
        private int combo;
        private int hits;
        private int misses;
        private bool running;
        private GUIStyle titleStyle;
        private GUIStyle hudStyle;
        private GUIStyle smallStyle;
        private GUIStyle buttonStyle;

        public int Score => score;
        public int HighScore => highScore;
        public float TimeLeft => roundTimeLeft;
        public bool IsRunning => running;

        private void Awake()
        {
            highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            BuildRuntimeScene();
            StartRound();
        }

        private void Update()
        {
            if (!running)
                return;

            roundTimeLeft = Mathf.Max(0f, roundTimeLeft - Time.deltaTime);
            comboTimeLeft = Mathf.Max(0f, comboTimeLeft - Time.deltaTime);
            if (comboTimeLeft <= 0f)
                combo = 0;

            if (roundTimeLeft <= 0f)
            {
                FinishRound();
                return;
            }

            phaseTimeLeft -= Time.deltaTime;
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
            hits = 0;
            misses = 0;
            comboTimeLeft = 0f;
            roundTimeLeft = RoundLength;
            running = true;
            HideAllRabbits();
            phaseTimeLeft = 0.25f;
        }

        private void FinishRound()
        {
            running = false;
            HideActiveRabbit(false);
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt(HighScoreKey, highScore);
                PlayerPrefs.Save();
            }
        }

        private float Difficulty01 => Mathf.Clamp01((RoundLength - roundTimeLeft) / RoundLength);
        private float RabbitVisibleTime => Mathf.Lerp(BaseRabbitVisibleTime, MinimumRabbitVisibleTime, Difficulty01);
        private float BetweenRabbitsTime => Mathf.Lerp(BaseBetweenRabbitsTime, MinimumBetweenRabbitsTime, Difficulty01);

        private void BuildRuntimeScene()
        {
            Application.targetFrameRate = 60;

            gameplayCamera = Camera.main;
            if (gameplayCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                gameplayCamera = cameraObject.AddComponent<Camera>();
            }

            gameplayCamera.transform.position = new Vector3(0f, 9.2f, -9.5f);
            gameplayCamera.transform.rotation = Quaternion.Euler(42f, 0f, 0f);
            gameplayCamera.fieldOfView = 55f;
            gameplayCamera.backgroundColor = new Color(0.11f, 0.18f, 0.28f);

            if (FindFirstObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            CreateGround();

            var positions = new[]
            {
                new Vector3(-2.7f, 0f, 2.25f), new Vector3(0f, 0f, 2.25f), new Vector3(2.7f, 0f, 2.25f),
                new Vector3(-2.7f, 0f, 0f),    new Vector3(0f, 0f, 0f),    new Vector3(2.7f, 0f, 0f),
                new Vector3(-2.7f, 0f, -2.25f),new Vector3(0f, 0f, -2.25f),new Vector3(2.7f, 0f, -2.25f)
            };

            for (int i = 0; i < positions.Length; i++)
                holes.Add(CreateHole(i + 1, positions[i]));
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(1.25f, 1f, 1.25f);
            SetColor(ground, new Color(0.18f, 0.42f, 0.22f));
        }

        private RabbitHole CreateHole(int index, Vector3 position)
        {
            var root = new GameObject($"Hole_{index:00}");
            root.transform.position = position;

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Rim";
            rim.transform.SetParent(root.transform, false);
            rim.transform.localScale = new Vector3(1.05f, 0.08f, 1.05f);
            SetColor(rim, new Color(0.22f, 0.12f, 0.06f));

            var rabbit = new GameObject("Rabbit");
            rabbit.transform.SetParent(root.transform, false);
            rabbit.transform.localPosition = new Vector3(0f, 0.65f, 0f);

            var target = rabbit.AddComponent<RabbitTarget>();
            var hitBox = rabbit.AddComponent<BoxCollider>();
            hitBox.center = new Vector3(0f, 0.65f, 0f);
            hitBox.size = new Vector3(1.35f, 2.15f, 1.05f);

            CreatePart(PrimitiveType.Sphere, "Body", rabbit.transform, new Vector3(0f, 0.35f, 0f), new Vector3(0.9f, 1.05f, 0.78f), new Color(0.76f, 0.70f, 0.62f));
            CreatePart(PrimitiveType.Sphere, "Head", rabbit.transform, new Vector3(0f, 1.10f, 0f), new Vector3(0.76f, 0.76f, 0.70f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Capsule, "EarLeft", rabbit.transform, new Vector3(-0.23f, 1.78f, 0f), new Vector3(0.20f, 0.48f, 0.18f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Capsule, "EarRight", rabbit.transform, new Vector3(0.23f, 1.78f, 0f), new Vector3(0.20f, 0.48f, 0.18f), new Color(0.84f, 0.79f, 0.72f));
            CreatePart(PrimitiveType.Sphere, "Nose", rabbit.transform, new Vector3(0f, 1.02f, -0.34f), new Vector3(0.16f, 0.12f, 0.12f), new Color(0.85f, 0.36f, 0.42f));

            var hole = root.AddComponent<RabbitHole>();
            hole.RabbitObject = rabbit;
            target.Hole = hole;
            rabbit.SetActive(false);
            return hole;
        }

        private static void CreatePart(PrimitiveType primitive, string partName, Transform parent, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            var collider = part.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            SetColor(part, color);
        }

        private static void SetColor(GameObject gameObject, Color color)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer == null)
                return;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            if (shader == null)
                return;

            var material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
        }

        private void ShowRandomRabbit()
        {
            if (holes.Count == 0)
                return;

            activeHole = holes[Random.Range(0, holes.Count)];
            activeHole.RabbitObject.SetActive(true);
            phaseTimeLeft = RabbitVisibleTime;
        }

        private void HideActiveRabbit(bool countMiss)
        {
            if (activeHole != null && activeHole.RabbitObject != null)
            {
                activeHole.RabbitObject.SetActive(false);
                if (countMiss)
                {
                    misses++;
                    combo = 0;
                    comboTimeLeft = 0f;
                }
            }

            activeHole = null;
            phaseTimeLeft = BetweenRabbitsTime;
        }

        private void HideAllRabbits()
        {
            foreach (var hole in holes)
            {
                if (hole != null && hole.RabbitObject != null)
                    hole.RabbitObject.SetActive(false);
            }
            activeHole = null;
        }

        private void ReadPointerInput()
        {
            if (gameplayCamera == null)
                return;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                    TryHit(touch.position);
                return;
            }

            if (Input.GetMouseButtonDown(0))
                TryHit(Input.mousePosition);
        }

        private void TryHit(Vector2 screenPosition)
        {
            var ray = gameplayCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit, 100f))
                return;

            var target = hit.collider.GetComponentInParent<RabbitTarget>();
            if (target == null || activeHole == null || target.Hole != activeHole || !activeHole.RabbitObject.activeSelf)
                return;

            combo = comboTimeLeft > 0f ? combo + 1 : 1;
            comboTimeLeft = ComboWindow;
            score += Mathf.Min(combo, 5);
            hits++;

#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif

            if (score > highScore)
                highScore = score;

            HideActiveRabbit(false);
        }

        private void OnGUI()
        {
            EnsureStyles();
            Rect safe = Screen.safeArea;
            float margin = Mathf.Max(18f, safe.width * 0.025f);
            float lineHeight = Mathf.Max(42f, safe.height * 0.065f);
            float left = safe.x + margin;
            float top = Screen.height - safe.yMax + margin;
            float width = safe.width - margin * 2f;

            GUI.Box(new Rect(left, top, width, lineHeight * 2.6f), GUIContent.none);
            GUI.Label(new Rect(left, top, width, lineHeight), "KANINBANKER", titleStyle);
            GUI.Label(new Rect(left + margin, top + lineHeight, width * 0.32f, lineHeight), "Score: " + score, hudStyle);
            GUI.Label(new Rect(left + width * 0.38f, top + lineHeight, width * 0.25f, lineHeight), "Tid: " + Mathf.CeilToInt(roundTimeLeft), hudStyle);
            GUI.Label(new Rect(left + width * 0.68f, top + lineHeight, width * 0.28f, lineHeight), combo > 1 ? "Combo x" + combo : "", hudStyle);

            if (!running)
            {
                float panelWidth = safe.width * 0.62f;
                float panelHeight = Mathf.Max(160f, safe.height * 0.28f);
                float panelX = safe.x + (safe.width - panelWidth) * 0.5f;
                float panelY = Screen.height - safe.yMax + safe.height * 0.42f;
                GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), GUIContent.none);
                GUI.Label(new Rect(panelX, panelY + 8f, panelWidth, lineHeight), "Runden er slut", titleStyle);
                GUI.Label(new Rect(panelX + margin, panelY + lineHeight, panelWidth - margin * 2f, lineHeight), $"Score {score}   Rekord {highScore}", hudStyle);
                GUI.Label(new Rect(panelX + margin, panelY + lineHeight * 1.65f, panelWidth - margin * 2f, lineHeight), $"Træffere {hits}   Miss {misses}", smallStyle);

                float buttonHeight = Mathf.Max(58f, safe.height * 0.085f);
                var buttonRect = new Rect(panelX + margin, panelY + panelHeight - buttonHeight - margin, panelWidth - margin * 2f, buttonHeight);
                if (GUI.Button(buttonRect, "Spil igen", buttonStyle))
                    StartRound();
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(24, Screen.height / 25),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(20, Screen.height / 32),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(16, Screen.height / 42),
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(20, Screen.height / 34),
                fontStyle = FontStyle.Bold
            };
        }
    }

    public sealed class RabbitHole : MonoBehaviour
    {
        public GameObject RabbitObject { get; set; }
    }

    public sealed class RabbitTarget : MonoBehaviour
    {
        public RabbitHole Hole { get; set; }
    }
}
