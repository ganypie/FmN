using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// WeatherAudioAdapter подключает WeatherAudioManager к WeatherController,
    /// но НЕ вмешивается в громкость, если не вызываются методы Apply*().
    /// Полностью совместим с интро и плавным управлением звуком.
    /// </summary>
    public sealed class WeatherAudioAdapter : MonoBehaviour
    {
        [Header("References")]
        public WeatherController weatherController;

        [Header("Profiles per Weather State")]
        public WeatherAudioProfile clearProfile;
        public WeatherAudioProfile rainProfile;
        public WeatherAudioProfile fogProfile;

        [Header("Intensity per State")]
        [Range(0f, 1f)] public float clearIntensity = 0f;
        [Range(0f, 1f)] public float rainIntensity = 1f;
        [Range(0f, 1f)] public float fogIntensity = 0.2f;

        [Header("Behavior")]
        [Tooltip("Crossfade duration when switching profiles.")]
        [Range(0f, 5f)] public float transitionTime = 0.5f;

        [Tooltip("If enabled, intensity becomes 0 if weather system GameObject is inactive.")]
        public bool bindToSystemActive = true;

        void Start()
        {
            if (weatherController == null)
                weatherController = FindObjectOfType<WeatherController>();

            // Один раз инициализируем звук под текущую погоду
            ApplyInitial();
        }

        /// <summary>
        /// Инициализация — применяется единожды при старте сцены
        /// </summary>
        private void ApplyInitial()
        {
            if (weatherController == null) 
                return;

            var mgr = WeatherAudioManager.Instance;

            switch (weatherController.currentWeather)
            {
                case WeatherController.WeatherType.Rain:
                    if (rainProfile != null) mgr.SetProfile(rainProfile, 0f);
                    mgr.SetIntensity(bindToSystemActive ? rainIntensity : 0f);
                    break;

                case WeatherController.WeatherType.Fog:
                    if (fogProfile != null) mgr.SetProfile(fogProfile, 0f);
                    mgr.SetIntensity(bindToSystemActive ? fogIntensity : 0f);
                    break;

                case WeatherController.WeatherType.Clear:
                default:
                    if (clearProfile != null) mgr.SetProfile(clearProfile, 0f);
                    mgr.SetIntensity(clearIntensity);
                    break;
            }
        }

        /// <summary>
        /// Прямой метод для плавного изменения громкости дождя
        /// без смены профиля.
        /// Используется в интро.
        /// </summary>
        public void ApplyRainIntensity(float value)
        {
            if (rainProfile != null)
                WeatherAudioManager.Instance.SetProfile(rainProfile, transitionTime);

            WeatherAudioManager.Instance.SetIntensity(Mathf.Clamp01(value));
        }
    }
}
