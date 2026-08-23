using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    public sealed class KaninbankerGame : MonoBehaviour
    {
        private const float RoundLength = 30f;
        private const float RabbitVisibleTime = 0.8f;
        private const float BetweenRabbitsTime = 0.18f;

        private readonly List<RabbitHole> holes = new List<RabbitHole>();
        private Camera gameplayCamera;
        private RabbitHole activeHole;
        private float roundTimeLeft;
        private float phaseTimeLeft;
        private int score;
        private bool running;
        private GUIStyle titleStyle;
        private GUIStyle hudStyle;
        private GUIStyle buttonStyle;

        public int Score => score;
        public float TimeLeft => roundTimeLeft;
        public bool IsRunning => running;

        private void Awake()
        {
            BuildRuntimeScene();
            StartRound();
        }

        private void Update()
        {
            if (running)
            {
                roundTimeLeft = Mathf.Max(0f, roundTimeLeft - Time.deltaTime);
                if (roundTimeLeft <= 0f)
                {
                    running = false;
                    HideActiveRabbit();
                    return;
                }

                phaseTimeLeft -= Time.deltaTime;
                if (phaseTimeLeft <= 0f)
                {
                    if (activeHole == null)
                        ShowRandomRabbit();
                    else
                        HideActiveRabbit();
                }
            }

            ReadPointerInput();
        }

        public void StartRound()
        {
            score = 0;
            roundTimeLeft = RoundLength;
            running = true;
            HideAllRabbits();
            phaseTimeLeft = 0.25f;
        }

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
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
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

        private void HideActiveRabbit()
        {
            if (activeHole != null && activeHole.RabbitObject != null)
                activeHole.RabbitObject.SetActive(false);
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
            if (!running || gameplayCamera == null)
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

            score++;
            HideActiveRabbit();
        }

        private void OnGUI()
        {
            EnsureStyles();
            float margin = Mathf.Max(18f, Screen.width * 0.035f);
            float lineHeight = Mathf.Max(42f, Screen.height * 0.065f);

            GUI.Box(new Rect(margin, margin, Screen.width - margin * 2f, lineHeight * 2.25f), GUIContent.none);
            GUI.Label(new Rect(margin, margin, Screen.width - margin * 2f, lineHeight), "KANINBANKER", titleStyle);
            GUI.Label(new Rect(margin * 1.6f, margin + lineHeight, Screen.width * 0.45f, lineHeight), "Score: " + score, hudStyle);
            GUI.Label(new Rect(Screen.width * 0.58f, margin + lineHeight, Screen.width * 0.35f, lineHeight), "Tid: " + Mathf.CeilToInt(roundTimeLeft), hudStyle);

            if (!running)
            {
                float width = Screen.width * 0.58f;
                float height = Mathf.Max(64f, Screen.height * 0.09f);
                var buttonRect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.78f, width, height);
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
                fontStyle = FontStyle.Bold
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
