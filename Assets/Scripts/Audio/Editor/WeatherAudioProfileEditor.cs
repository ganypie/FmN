using UnityEditor;
using UnityEngine;

using Game.Audio;

namespace Game.Audio.Editor
{
    /// <summary>
    /// Custom inspector for WeatherAudioProfile with simple preview controls.
    /// </summary>
    [CustomEditor(typeof(WeatherAudioProfile))]
    public class WeatherAudioProfileEditor : UnityEditor.Editor
    {
        private WeatherAudioProfile _profile;
        private float _previewIntensity = 0.5f;
        private bool _previewIndoor;

        private void OnEnable()
        {
            _profile = (WeatherAudioProfile)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            _previewIntensity = EditorGUILayout.Slider("Intensity", _previewIntensity, 0f, 1f);
            _previewIndoor = EditorGUILayout.Toggle("Indoor", _previewIndoor);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play Preview"))
                {
                    PlayPreview();
                }
                if (GUILayout.Button("Stop Preview"))
                {
                    StopPreview();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Preview работает в Play Mode. Запусти сцену и используй Play/Stop.", MessageType.Info);
            }
        }

        private void PlayPreview()
        {
            if (_profile == null) return;
            var mgr = WeatherAudioManager.Instance;
            mgr.SetProfile(_profile, 0.25f);
            mgr.SetIntensity(_previewIntensity);
            mgr.SetIndoor(_previewIndoor);
        }

        private void StopPreview()
        {
            // Switch to empty profile to stop everything quickly
            var empty = ScriptableObject.CreateInstance<WeatherAudioProfile>();
            WeatherAudioManager.Instance.SetProfile(empty, 0.2f);
        }
    }
}


