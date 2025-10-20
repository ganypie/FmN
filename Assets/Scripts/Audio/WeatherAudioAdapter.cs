using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Non-invasive adapter that ties WeatherAudioManager to an existing WeatherController.
    /// Keeps weather sounds active only when the corresponding weather is set/visible.
    /// </summary>
    public sealed class WeatherAudioAdapter : MonoBehaviour
    {
        [Header("References")]
        public WeatherController weatherController;

        [Header("Profiles per Weather State (optional)")]
        public WeatherAudioProfile clearProfile;
        public WeatherAudioProfile rainProfile;
        public WeatherAudioProfile fogProfile;

        [Header("Intensity per State")]
        [Range(0f,1f)] public float clearIntensity = 0f;
        [Range(0f,1f)] public float rainIntensity = 1f;
        [Range(0f,1f)] public float fogIntensity = 0.2f;

        [Header("Behavior")]
        [Tooltip("Crossfade time when profile/state changes.")]
        [Range(0f,5f)] public float transitionTime = 0.5f;

        [Tooltip("If true, intensity is forced to 0 when corresponding GameObject (e.g., rainSystems) is not active in hierarchy.")]
        public bool bindToSystemActive = true;

        private WeatherController.WeatherType _lastState;

        void Start()
        {
            if (weatherController == null)
                weatherController = FindObjectOfType<WeatherController>();
            Apply(true);
        }

        void Update()
        {
            if (weatherController == null) return;
            // Enforce binding every frame to override any other initializers (e.g., old bootstrap)
            Apply(false);
        }

        private void Apply(bool force)
        {
            if (weatherController == null) return;

            var mgr = WeatherAudioManager.Instance;
            var state = weatherController.currentWeather;

            WeatherAudioProfile profile = null;
            float intensity = 0f;

            switch (state)
            {
                case WeatherController.WeatherType.Rain:
                    profile = rainProfile != null ? rainProfile : clearProfile;
                    intensity = rainIntensity;
                    // Optionally respect active state of rain systems
                    if (bindToSystemActive && weatherController.rainSystems != null && !weatherController.rainSystems.activeInHierarchy)
                        intensity = 0f;
                    break;
                case WeatherController.WeatherType.Fog:
                    profile = fogProfile != null ? fogProfile : clearProfile;
                    intensity = fogIntensity;
                    if (bindToSystemActive && weatherController.fogSystem != null && !weatherController.fogSystem.activeInHierarchy)
                        intensity = 0f;
                    break;
                case WeatherController.WeatherType.Clear:
                default:
                    profile = clearProfile;
                    intensity = clearIntensity;
                    break;
            }

            if (profile != null)
                mgr.SetProfile(profile, transitionTime);
            else
                mgr.SetProfile(null, transitionTime);

            mgr.SetIntensity(intensity);
            _lastState = state;
        }
    }
}


