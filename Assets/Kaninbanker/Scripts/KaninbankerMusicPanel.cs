using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Portrait phone-friendly companion panel for external music. It never extracts YouTube audio;
    /// it opens the official YouTube Music experience and keeps Kaninbanker's SFX separate.
    /// </summary>
    public sealed class KaninbankerMusicPanel : MonoBehaviour
    {
        private KaninbankerAudio audioSystem;
        private KaninbankerExternalMusic externalMusic;
        private bool panelOpen;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle buttonStyle;
        private GUIStyle fieldStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerMusicPanel>() != null)
                return;

            GameObject go = new GameObject("KaninbankerMusicPanel");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerMusicPanel>();
        }

        private void Start()
        {
            KaninbankerGame game = FindFirstObjectByType<KaninbankerGame>();
            if (game == null)
                return;

            audioSystem = game.GetComponent<KaninbankerAudio>();
            if (audioSystem == null)
                audioSystem = game.gameObject.AddComponent<KaninbankerAudio>();

            externalMusic = game.GetComponent<KaninbankerExternalMusic>();
            if (externalMusic == null)
                externalMusic = game.gameObject.AddComponent<KaninbankerExternalMusic>();

            externalMusic.Configure(audioSystem);
        }

        private void OnGUI()
        {
            if (audioSystem == null || externalMusic == null)
                return;

            EnsureStyles();
            Rect safePixels = Screen.safeArea;
            Rect safe = new Rect(safePixels.x, Screen.height - safePixels.yMax, safePixels.width, safePixels.height);
            float margin = Mathf.Max(12f, safe.width * 0.025f);
            float buttonHeight = Mathf.Max(44f, safe.height * 0.052f);
            float buttonWidth = Mathf.Clamp(safe.width * 0.22f, 112f, 220f);

            Rect toggleRect = new Rect(safe.xMax - margin - buttonWidth, safe.y + margin, buttonWidth, buttonHeight);
            string mode = externalMusic.ExternalMode ? "YT MUSIK" : "MUSIK";
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = externalMusic.ExternalMode ? new Color(0.92f, 0.18f, 0.20f) : new Color(0.20f, 0.24f, 0.34f);
            if (GUI.Button(toggleRect, mode, buttonStyle))
            {
                audioSystem.PlayUi();
                panelOpen = !panelOpen;
            }
            GUI.backgroundColor = oldBackground;

            if (!panelOpen)
                return;

            // A wide bottom-sheet layout reads naturally on a tall TikTok/Reels-style screen.
            float panelWidth = safe.width - margin * 2f;
            float panelHeight = Mathf.Min(safe.height * 0.52f, 620f);
            float panelX = safe.x + margin;
            float panelY = safe.yMax - panelHeight - margin;
            Rect panel = new Rect(panelX, panelY, panelWidth, panelHeight);

            Color oldColor = GUI.color;
            GUI.color = new Color(0.025f, 0.032f, 0.055f, 0.98f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = oldColor;

            float inner = Mathf.Max(16f, panelWidth * 0.035f);
            float line = panelHeight * 0.125f;
            GUI.Label(new Rect(panelX + inner, panelY + inner * 0.30f, panelWidth - inner * 2f, line), "BAGGRUNDSMUSIK", titleStyle);

            string current = externalMusic.ExternalMode
                ? "YouTube Music valgt. Intern soundtrack er fra, men Kaninbanker-SFX fortsætter."
                : "Kaninbankers interne arcade-musik er aktiv.";
            GUI.Label(new Rect(panelX + inner, panelY + line * 0.88f, panelWidth - inner * 2f, line * 0.82f), current, textStyle);

            float gap = inner * 0.55f;
            float half = (panelWidth - inner * 2f - gap) * 0.5f;
            Rect internalButton = new Rect(panelX + inner, panelY + line * 1.80f, half, line * 0.80f);
            Rect ytButton = new Rect(panelX + inner + half + gap, panelY + line * 1.80f, half, line * 0.80f);

            if (GUI.Button(internalButton, "INTERN MUSIK", buttonStyle))
            {
                externalMusic.UseInternalMusic();
                audioSystem.PlayUi();
            }

            GUI.backgroundColor = new Color(0.92f, 0.18f, 0.20f);
            if (GUI.Button(ytButton, "ÅBN YOUTUBE MUSIC", buttonStyle))
            {
                audioSystem.PlayUi();
                externalMusic.OpenYouTubeMusicHome();
            }
            GUI.backgroundColor = oldBackground;

            GUI.Label(new Rect(panelX + inner, panelY + line * 2.78f, panelWidth - inner * 2f, line * 0.62f), "Søg efter sang, artist eller playlist", textStyle);
            float searchButtonW = panelWidth * 0.22f;
            Rect searchRect = new Rect(panelX + inner, panelY + line * 3.35f, panelWidth - inner * 2f - searchButtonW - gap, line * 0.74f);
            Rect searchButton = new Rect(searchRect.xMax + gap, searchRect.y, searchButtonW, searchRect.height);

            externalMusic.SearchQuery = GUI.TextField(searchRect, externalMusic.SearchQuery, 80, fieldStyle);
            if (GUI.Button(searchButton, "SØG", buttonStyle))
            {
                audioSystem.PlayUi();
                externalMusic.SearchYouTubeMusic();
            }

            GUI.Label(
                new Rect(panelX + inner, panelY + line * 4.15f, panelWidth - inner * 2f, line * 1.05f),
                "Start musikken i YouTube Music og gå tilbage til spillet. Om den fortsætter i baggrunden styres af Android, YouTube Music og din konto.",
                textStyle);

            float closeW = panelWidth * 0.32f;
            Rect closeRect = new Rect(panelX + panelWidth - inner - closeW, panelY + panelHeight - inner - line * 0.68f, closeW, line * 0.68f);
            if (GUI.Button(closeRect, "LUK", buttonStyle))
            {
                audioSystem.PlayUi();
                panelOpen = false;
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            int reference = Mathf.Min(Screen.width, Screen.height);
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 24, 22, 42),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            titleStyle.normal.textColor = Color.white;

            textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(reference / 34, 15, 30),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            textStyle.normal.textColor = new Color(0.86f, 0.88f, 0.94f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Clamp(reference / 34, 14, 30),
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            fieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Mathf.Clamp(reference / 32, 15, 32),
                alignment = TextAnchor.MiddleLeft
            };
        }
    }
}
