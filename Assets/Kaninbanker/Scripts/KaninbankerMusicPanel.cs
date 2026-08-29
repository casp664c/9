using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Small phone-friendly music panel. It does not extract YouTube audio.
    /// It opens the official YouTube Music experience so the user can start a track there,
    /// return to Kaninbanker, and keep game SFX mixed with external music when the OS/account permits it.
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

            var go = new GameObject("KaninbankerMusicPanel");
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
            Rect safe = Screen.safeArea;
            float margin = Mathf.Max(14f, safe.width * 0.018f);
            float top = Screen.height - safe.yMax + margin;
            float buttonHeight = Mathf.Max(48f, safe.height * 0.065f);
            float buttonWidth = Mathf.Max(120f, safe.width * 0.16f);
            float right = safe.xMax - margin;

            Rect toggleRect = new Rect(right - buttonWidth, top, buttonWidth, buttonHeight);
            string mode = externalMusic.ExternalMode ? "YT MUSIK" : "MUSIK";
            if (GUI.Button(toggleRect, mode, buttonStyle))
            {
                audioSystem.PlayUi();
                panelOpen = !panelOpen;
            }

            if (!panelOpen)
                return;

            float panelWidth = Mathf.Min(safe.width - margin * 2f, Mathf.Max(480f, safe.width * 0.66f));
            float panelHeight = Mathf.Min(safe.height - margin * 2f, Mathf.Max(300f, safe.height * 0.50f));
            float panelX = safe.x + (safe.width - panelWidth) * 0.5f;
            float panelY = Screen.height - safe.yMax + safe.height * 0.20f;
            Rect panel = new Rect(panelX, panelY, panelWidth, panelHeight);
            GUI.Box(panel, GUIContent.none);

            float inner = Mathf.Max(18f, panelWidth * 0.035f);
            float line = Mathf.Max(44f, panelHeight * 0.13f);
            GUI.Label(new Rect(panelX + inner, panelY + inner * 0.35f, panelWidth - inner * 2f, line), "BAGGRUNDSMUSIK", titleStyle);

            string current = externalMusic.ExternalMode
                ? "YouTube Music valgt – intern musik er slået fra, SFX er stadig aktive."
                : "Intern Kaninbanker-musik er aktiv.";
            GUI.Label(new Rect(panelX + inner, panelY + line * 0.95f, panelWidth - inner * 2f, line), current, textStyle);

            float half = (panelWidth - inner * 3f) * 0.5f;
            Rect internalButton = new Rect(panelX + inner, panelY + line * 1.80f, half, line * 0.86f);
            Rect ytButton = new Rect(panelX + inner * 2f + half, panelY + line * 1.80f, half, line * 0.86f);

            if (GUI.Button(internalButton, "INTERN MUSIK", buttonStyle))
            {
                externalMusic.UseInternalMusic();
                audioSystem.PlayUi();
            }

            if (GUI.Button(ytButton, "ÅBN YOUTUBE MUSIC", buttonStyle))
            {
                audioSystem.PlayUi();
                externalMusic.OpenYouTubeMusicHome();
            }

            GUI.Label(new Rect(panelX + inner, panelY + line * 2.75f, panelWidth - inner * 2f, line * 0.72f), "Søg efter sang, artist eller playlist:", textStyle);
            Rect searchRect = new Rect(panelX + inner, panelY + line * 3.35f, panelWidth - inner * 2f - half * 0.45f, line * 0.78f);
            Rect searchButton = new Rect(searchRect.xMax + inner * 0.5f, searchRect.y, half * 0.45f - inner * 0.5f, searchRect.height);

            externalMusic.SearchQuery = GUI.TextField(searchRect, externalMusic.SearchQuery, 80, fieldStyle);
            if (GUI.Button(searchButton, "SØG", buttonStyle))
            {
                audioSystem.PlayUi();
                externalMusic.SearchYouTubeMusic();
            }

            GUI.Label(
                new Rect(panelX + inner, panelY + line * 4.28f, panelWidth - inner * 2f, line * 0.95f),
                "Start musikken i YouTube Music og gå tilbage til spillet. Om musikken kan fortsætte i baggrunden styres af YouTube Music/Android og din konto.",
                textStyle);

            Rect closeRect = new Rect(panelX + panelWidth - inner - half * 0.45f, panelY + panelHeight - inner - line * 0.72f, half * 0.45f, line * 0.72f);
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

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(22, Screen.height / 32),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(15, Screen.height / 48),
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(14, Screen.height / 52),
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            fieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Mathf.Max(16, Screen.height / 46),
                alignment = TextAnchor.MiddleLeft
            };
        }
    }
}
