using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// Weather audio profile that defines layered loops and transients with mixer routing and intensity curves.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Audio/Weather Audio Profile", fileName = "WeatherAudioProfile")]
    public class WeatherAudioProfile : ScriptableObject
    {
        [Header("Global Settings")]
        [Tooltip("Higher priority profiles may override lower ones in conflict situations.")]
        public int priority = 0;

        [Tooltip("Hard limit of simultaneously playing sources for this profile (loops + transients).")]
        [Range(1, 64)] public int maxSimultaneousSources = 16;

        [Tooltip("Global indoor multiplier applied to all loop layers when indoor is true.")]
        [Range(0f, 1f)] public float indoorVolumeMultiplier = 0.35f;

        [Header("Ducking (Mixer Snapshots)")]
        [Tooltip("Target mixer used for routing. Optional but recommended for ducking.")]
        public AudioMixer targetMixer;

        [Tooltip("Mixer snapshot for outdoor.")]
        public AudioMixerSnapshot outdoorSnapshot;

        [Tooltip("Mixer snapshot for indoor (may include LPF/volume changes).")]
        public AudioMixerSnapshot indoorSnapshot;

        [Tooltip("Seconds to transition between indoor/outdoor snapshots.")]
        [Range(0.01f, 5f)] public float snapshotTransition = 0.35f;

        [Header("Intensity Mapping")]
        [Tooltip("Maps intensity [0..1] to overall volume multiplier.")]
        public AnimationCurve intensityToGlobalVolume = AnimationCurve.Linear(0, 0, 1, 1);

        [Serializable]
        public class WeatherAudioLayer
        {
            [Tooltip("Unique layer identifier (e.g., AMB, PRECIP_NEAR, PRECIP_FAR, WIND_LOW, WIND_GUSTS)")]
            public string id = "AMB";

            [Tooltip("Clips for this layer. When loop=true, one will be chosen and looped.")]
            public AudioClip[] clips;

            [Tooltip("If true, plays as continuous loop.")]
            public bool loop = true;

            [Tooltip("Base volume before curves/multipliers.")]
            [Range(0f, 1f)] public float baseVolume = 0.5f;

            [Tooltip("Pitch randomization range.")]
            public Vector2 pitchRange = new Vector2(0.98f, 1.02f);

            [Tooltip("AudioSource.spatialBlend (0=2D,1=3D)")]
            [Range(0f, 1f)] public float spatialBlend = 0f;

            [Tooltip("AudioSource.minDistance for 3D falloff.")]
            public float minDistance = 5f;

            [Tooltip("AudioSource.maxDistance for 3D falloff.")]
            public float maxDistance = 50f;

            [Tooltip("AudioSource.rolloffMode.")]
            public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;

            [Tooltip("Maps intensity [0..1] to layer volume multiplier.")]
            public AnimationCurve intensityToVolume = AnimationCurve.Linear(0, 0, 1, 1);

            [Tooltip("Seconds for crossfade when switching profile or changing clip.")]
            [Range(0f, 5f)] public float crossfadeTime = 0.35f;

            [Tooltip("Mixer group routing for this layer.")]
            public AudioMixerGroup targetGroup;
        }

        [Serializable]
        public class WeatherTransient
        {
            [Tooltip("Unique transient identifier (e.g., thunder_big, rain_metal_drip)")]
            public string id = "thunder";

            [Tooltip("Clips for random selection.")]
            public AudioClip[] clips;

            [Range(0f, 1f)] public float baseVolume = 0.8f;
            public Vector2 volumeJitter = new Vector2(0.9f, 1.1f);
            public Vector2 pitchRange = new Vector2(0.95f, 1.05f);

            [Range(0f, 1f)] public float spatialBlend = 0f;
            public float minDistance = 10f;
            public float maxDistance = 200f;
            public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

            [Tooltip("Mixer group routing for this transient.")]
            public AudioMixerGroup targetGroup;

            [Tooltip("Seconds to prevent spamming the same transient.")]
            [Range(0f, 10f)] public float cooldown = 1.0f;
        }

        [Header("Layers (Loops)")]
        public WeatherAudioLayer[] layers;

        [Header("Transient Events")]
        public WeatherTransient[] transients;

        /// <summary>
        /// Returns a layer by id (case-sensitive). Null if not found.
        /// </summary>
        public WeatherAudioLayer GetLayer(string layerId)
        {
            if (layers == null) return null;
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null && layers[i].id == layerId) return layers[i];
            }
            return null;
        }

        /// <summary>
        /// Returns transient by id (case-sensitive). Null if not found.
        /// </summary>
        public WeatherTransient GetTransient(string transientId)
        {
            if (transients == null) return null;
            for (int i = 0; i < transients.Length; i++)
            {
                if (transients[i] != null && transients[i].id == transientId) return transients[i];
            }
            return null;
        }
    }
}


