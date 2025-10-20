using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Simple bootstrap to activate a profile in Play Mode without writing custom code.
    /// </summary>
    public sealed class WeatherAudioBootstrap : MonoBehaviour
    {
        [Header("Profile & Initial State")]
        public WeatherAudioProfile profile;
        [Range(0f,1f)] public float initialIntensity = 0.5f;
        public bool startIndoor = false;

        void Start()
        {
            var m = WeatherAudioManager.Instance;
            if (profile != null) m.SetProfile(profile, 0.5f);
            m.SetIntensity(initialIntensity);
            m.SetIndoor(startIndoor);
        }
    }
}


