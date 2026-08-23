using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Opens the official YouTube Music experience instead of extracting or embedding audio.
    /// The user starts a track in YouTube Music and then returns to the game.
    /// </summary>
    public sealed class KaninbankerExternalMusic : MonoBehaviour
    {
        private const string QueryKey = "Kaninbanker.YouTubeMusicQuery";
        private const string ExternalModeKey = "Kaninbanker.ExternalMusicMode";
        private const string YouTubeMusicHome = "https://music.youtube.com/";
        private const string YouTubeMusicSearch = "https://music.youtube.com/search?q=";

        private KaninbankerAudio audioSystem;
        private string searchQuery;
        private bool externalMode;

        public string SearchQuery
        {
            get => searchQuery;
            set => searchQuery = value ?? string.Empty;
        }

        public bool ExternalMode => externalMode;

        public void Configure(KaninbankerAudio audio)
        {
            audioSystem = audio;
            searchQuery = PlayerPrefs.GetString(QueryKey, "");
            externalMode = PlayerPrefs.GetInt(ExternalModeKey, 0) != 0;
            if (audioSystem != null)
                audioSystem.SetInternalMusicEnabled(!externalMode);
        }

        public void UseInternalMusic()
        {
            externalMode = false;
            SaveMode();
            if (audioSystem != null)
                audioSystem.SetInternalMusicEnabled(true);
        }

        public void UseYouTubeMusic()
        {
            externalMode = true;
            SaveMode();
            if (audioSystem != null)
                audioSystem.SetInternalMusicEnabled(false);
        }

        public void OpenYouTubeMusicHome()
        {
            UseYouTubeMusic();
            Application.OpenURL(YouTubeMusicHome);
        }

        public void SearchYouTubeMusic()
        {
            UseYouTubeMusic();
            string trimmed = (searchQuery ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                Application.OpenURL(YouTubeMusicHome);
                return;
            }

            PlayerPrefs.SetString(QueryKey, trimmed);
            PlayerPrefs.Save();
            Application.OpenURL(YouTubeMusicSearch + Uri.EscapeDataString(trimmed));
        }

        private void SaveMode()
        {
            PlayerPrefs.SetInt(ExternalModeKey, externalMode ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
