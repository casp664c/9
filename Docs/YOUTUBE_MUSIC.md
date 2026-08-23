# YouTube Music companion mode

Kaninbanker supports a phone-first external music workflow without extracting YouTube audio or hiding an embedded YouTube player.

## Player experience

1. Tap **MUSIK** in Kaninbanker.
2. Choose **ÅBN YOUTUBE MUSIC**, or enter a song/artist/playlist search and tap **SØG**.
3. Kaninbanker disables only its internal background music. Game sound effects remain enabled.
4. The official YouTube Music experience opens through the system URL handler.
5. Start a track in YouTube Music and return to Kaninbanker.
6. Unity is configured with `PlayerSettings.muteOtherAudioSources = false`, allowing compatible external audio to continue alongside game SFX.
7. Choose **INTERN MUSIK** in Kaninbanker to restore the built-in procedural soundtrack.

The selected mode and last search text are saved with `PlayerPrefs`.

## Why it works this way

YouTube's API policies prohibit separating YouTube audio from video and prohibit using an API client to provide a hidden/background YouTube player. Therefore Kaninbanker does not download, rip, proxy, or directly stream YouTube tracks as Unity `AudioClip` data.

The game instead launches the official YouTube Music service and lets Android/YouTube Music own playback. Background music availability is therefore controlled by YouTube Music, the user's account/subscription, Android, and the selected track.

## Files

- `Assets/Kaninbanker/Scripts/KaninbankerExternalMusic.cs` – saves the mode/search and opens official YouTube Music URLs.
- `Assets/Kaninbanker/Scripts/KaninbankerMusicPanel.cs` – phone-friendly in-game picker panel.
- `Assets/Kaninbanker/Scripts/KaninbankerAudio.cs` – keeps SFX separate from internal soundtrack state.
- `Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs` – allows external audio sources to continue while the game is foregrounded.

## No API key required

This companion flow does not call the YouTube Data API and does not need a YouTube/Google API key. A future search-results browser inside the game would require a separate compliant YouTube Data API integration and policy review.
