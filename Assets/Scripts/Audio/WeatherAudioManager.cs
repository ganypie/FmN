using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Central weather audio manager that drives loop layers and transients via an audio backend.
    /// </summary>
    public sealed class WeatherAudioManager : MonoBehaviour
    {
        private static WeatherAudioManager _instance;
        public static WeatherAudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("WeatherAudioManager");
                    _instance = go.AddComponent<WeatherAudioManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Runtime State")]
        [SerializeField] private WeatherAudioProfile _currentProfile;
        [Range(0f, 1f)] [SerializeField] private float _intensity;
		[SerializeField] private bool _indoor;
		// Smooth indoor factor [0..1] that fades instead of snapping volumes
		[SerializeField] private float _indoorBlend;
        [Tooltip("Global weather volume multiplier (to avoid masking footsteps).")]
        [Range(0f,1f)] [SerializeField] private float _weatherMasterVolume = 0.6f;

        [Header("Backend")]
        [SerializeField] private int _poolPreload = 8;

        private IWeatherAudioBackend _backend;
        private readonly Dictionary<string, float> _layerVolumes = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _transientCooldowns = new Dictionary<string, float>();

        private Transform _root;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            _root = transform;
        }

		private void Update()
        {
			// Smoothly approach target indoor blend based on current profile's snapshotTransition
			float indoorTarget = _indoor ? 1f : 0f;
			float indoorLerpSpeed = 0f;
			if (_currentProfile != null)
			{
				// Convert transition time (seconds) to exponential decay per frame
				float t = Mathf.Max(0.01f, _currentProfile.snapshotTransition);
				// Approximate: reach ~95% in transition time
				indoorLerpSpeed = 3f / t;
			}
			_indoorBlend = Mathf.MoveTowards(_indoorBlend, indoorTarget, indoorLerpSpeed * Time.unscaledDeltaTime);

            // Cooldowns for transients
            if (_currentProfile != null && _currentProfile.transients != null)
            {
                for (int i = 0; i < _currentProfile.transients.Length; i++)
                {
                    var t = _currentProfile.transients[i];
                    if (t == null) continue;
                    if (_transientCooldowns.TryGetValue(t.id, out float cd) && cd > 0f)
                    {
                        _transientCooldowns[t.id] = Mathf.Max(0f, cd - Time.unscaledDeltaTime);
                    }
                }
            }

            // Drive layers if a profile is set
			if (_currentProfile != null && _backend != null)
            {
                float globalMul = Mathf.Clamp01(_currentProfile.intensityToGlobalVolume.Evaluate(_intensity)) * _weatherMasterVolume;
				// Blend indoor multiplier smoothly rather than snap
				float indoorMul = Mathf.Lerp(1f, _currentProfile.indoorVolumeMultiplier, _indoorBlend);

                if (_currentProfile.layers != null)
                {
                    for (int i = 0; i < _currentProfile.layers.Length; i++)
                    {
                        var layer = _currentProfile.layers[i];
                        if (layer == null) continue;

                        float layerMul = Mathf.Clamp01(layer.intensityToVolume.Evaluate(_intensity));
                        float targetVol = layer.baseVolume * layerMul * globalMul * indoorMul;

                        AudioClip chosen = null;
                        if (layer.clips != null && layer.clips.Length > 0)
                        {
                            // For loops pick first non-null; could randomize when changing
                            chosen = layer.clips[0];
                        }

                        if (layer.loop)
                        {
                            float pitch = Random.Range(layer.pitchRange.x, layer.pitchRange.y);
                            _backend.SetLoop(layer.id, chosen, targetVol, layer.crossfadeTime, layer.spatialBlend, layer.minDistance, layer.maxDistance, layer.rolloffMode, pitch, layer.targetGroup);
                        }
                        else
                        {
                            // non-loop layers are ignored here
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets or crossfades to a new weather audio profile.
        /// </summary>
        /// <param name="profile">Profile to activate.</param>
        /// <param name="transitionTime">Seconds for crossfade between layer sets.</param>
        /// <example>
        /// WeatherAudioManager.Instance.SetProfile(profile, 2f);
        /// </example>
        public void SetProfile(WeatherAudioProfile profile, float transitionTime = 1f)
        {
            _currentProfile = profile;
            if (_backend == null)
            {
                _backend = new UnityAudioBackend(_root, _currentProfile, _poolPreload);
            }

            // Stop loops not present in new profile
            var toStop = new List<string>(_layerVolumes.Keys);
            for (int i = 0; i < toStop.Count; i++)
            {
                var id = toStop[i];
                if (profile == null || profile.GetLayer(id) == null)
                {
                    _backend.StopLoop(id, transitionTime);
                    _layerVolumes.Remove(id);
                }
            }

            // Initialize all new profile layers with zero target (will Update to target)
            if (profile != null && profile.layers != null)
            {
                foreach (var layer in profile.layers)
                {
                    if (layer == null) continue;
                    _layerVolumes[layer.id] = 0f;
                }
            }
        }

        /// <summary>
        /// Sets target intensity [0..1] with optional smoothing time.
        /// </summary>
        /// <param name="intensity">0..1 intensity.</param>
        /// <param name="smoothTime">Not used directly here; intensity is applied per-frame via curves.</param>
        /// <example>
        /// WeatherAudioManager.Instance.SetIntensity(0.8f);
        /// </example>
        public void SetIntensity(float intensity, float smoothTime = 0.5f)
        {
            _intensity = Mathf.Clamp01(intensity);
        }

        /// <summary>
        /// Sets global weather volume (0..1) to keep footsteps clear.
        /// </summary>
        /// <param name="volume">0..1</param>
        /// <example>
        /// WeatherAudioManager.Instance.SetWeatherVolume(0.5f);
        /// </example>
        public void SetWeatherVolume(float volume)
        {
            _weatherMasterVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Triggers a transient event by id at position with volume multiplier.
        /// </summary>
        /// <param name="transientId">Registered transient id.</param>
        /// <param name="position">World position (or Vector3.zero for 2D).</param>
        /// <param name="volume">Multiplier on baseVolume.</param>
        /// <example>
        /// WeatherAudioManager.Instance.TriggerTransient("thunder_big", player.position, 1f);
        /// </example>
        public void TriggerTransient(string transientId, Vector3 position, float volume = 1f)
        {
            if (_currentProfile == null || _backend == null) return;
            var t = _currentProfile.GetTransient(transientId);
            if (t == null || t.clips == null || t.clips.Length == 0) return;

            // cooldown
            if (_transientCooldowns.TryGetValue(transientId, out float cd) && cd > 0f) return;
            _transientCooldowns[transientId] = t.cooldown;

            var clip = t.clips[Random.Range(0, t.clips.Length)];
            float vol = t.baseVolume * Mathf.Lerp(t.volumeJitter.x, t.volumeJitter.y, Random.value) * Mathf.Clamp01(volume);
            float pitch = Random.Range(t.pitchRange.x, t.pitchRange.y);
            _backend.PlayTransient(transientId, clip, position, vol, t.spatialBlend, t.minDistance, t.maxDistance, t.rolloffMode, pitch, t.targetGroup);
        }

        /// <summary>
        /// Registers the main listener (optional for multi-listener setups).
        /// </summary>
        public void RegisterListener(AudioListener listener)
        {
            // For Unity backend no-op (left for future multi-listener backends)
        }

        /// <summary>
        /// Unregisters a listener.
        /// </summary>
        public void UnregisterListener(AudioListener listener)
        {
            // No-op
        }

        /// <summary>
        /// Sets indoor state (applies mixer snapshot if configured) and affects loop volumes.
        /// </summary>
        /// <param name="indoor">True when player is indoor.</param>
        /// <example>
        /// WeatherAudioManager.Instance.SetIndoor(true);
        /// </example>
        public void SetIndoor(bool indoor)
        {
            _indoor = indoor;
            if (_currentProfile != null && _backend != null)
            {
                _backend.SetIndoor(indoor, _currentProfile.snapshotTransition);
            }
        }
    }
}


