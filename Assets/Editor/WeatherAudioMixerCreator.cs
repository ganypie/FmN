using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio.Editor
{
    /// <summary>
    /// Helper to create a default AudioMixer with groups and snapshots for indoor/outdoor.
    /// </summary>
    public static class WeatherAudioMixerCreator
    {
        [MenuItem("Tools/WeatherAudio/Create Default Mixer")] 
        public static void CreateDefaultMixer()
        {
            // Use Unity's built-in menu to create an Audio Mixer asset
            bool ok = EditorApplication.ExecuteMenuItem("Assets/Create/Audio Mixer");
            if (!ok)
            {
                EditorUtility.DisplayDialog(
                    "Weather Audio",
                    "Could not invoke 'Assets/Create/Audio Mixer'. Please create an Audio Mixer manually: Project → Right-click → Create → Audio Mixer.\n\n" +
                    "Then add snapshots (Outdoor, Indoor) in the Audio Mixer window and assign them in your WeatherAudioProfile.",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "Weather Audio",
                "Audio Mixer created via 'Assets/Create/Audio Mixer'.\n\n" +
                "Next: Open the mixer and add snapshots (Outdoor, Indoor) manually in the Audio Mixer window, then assign them in your WeatherAudioProfile.",
                "OK");
        }
    }
}


