using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// Unity Audio backend implementing IWeatherAudioBackend using pooled AudioSources and Mixer Snapshots.
    /// </summary>
    public sealed class UnityAudioBackend : IWeatherAudioBackend
    {
        private readonly Transform _root;
        private readonly Dictionary<string, PooledAudioSource> _activeLoops = new Dictionary<string, PooledAudioSource>();
        private readonly WeatherAudioProfile _profile;
        private AudioListener _listener;

        public UnityAudioBackend(Transform root, WeatherAudioProfile profile, int poolPreload)
        {
            _root = root;
            _profile = profile;
            PooledAudioSource.Pool.Initialize(root, poolPreload);
        }

        public void SetLoop(string layerId, AudioClip clip, float targetVolume, float crossfadeTime, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, AudioMixerGroup mixerGroup)
        {
            if (clip == null)
            {
                StopLoop(layerId, crossfadeTime);
                return;
            }

            if (!_activeLoops.TryGetValue(layerId, out var loopSrc))
            {
                loopSrc = PooledAudioSource.Pool.Get();
                if (_root != null) loopSrc.transform.SetParent(_root, false);
                _activeLoops[layerId] = loopSrc;
            }

            // Ensure loop follows listener to avoid 3D falloff silence
            if (_listener == null) _listener = Object.FindObjectOfType<AudioListener>();
            if (_listener != null)
            {
                loopSrc.transform.position = _listener.transform.position;
            }

            loopSrc.PlayLoop(clip, targetVolume, crossfadeTime, spatialBlend, minDistance, maxDistance, rolloff, pitch, mixerGroup);
        }

        public void StopLoop(string layerId, float fadeTime)
        {
            if (_activeLoops.TryGetValue(layerId, out var loopSrc))
            {
                loopSrc.FadeOutAndRelease(fadeTime);
                _activeLoops.Remove(layerId);
            }
        }

        public void PlayTransient(string transientId, AudioClip clip, Vector3 position, float volume, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, AudioMixerGroup mixerGroup)
        {
            if (clip == null) return;
            var src = PooledAudioSource.Pool.Get();
            if (_root != null) src.transform.SetParent(_root, false);
            src.transform.position = position;
            src.PlayClip(clip, volume, spatialBlend, minDistance, maxDistance, rolloff, pitch, mixerGroup);
            src.FadeOutAndRelease(clip.length);
        }

        public void SetIndoor(bool indoor, float transitionTime)
        {
            if (_profile == null || _profile.targetMixer == null)
                return;

            var snapshot = indoor ? _profile.indoorSnapshot : _profile.outdoorSnapshot;
            if (snapshot != null)
            {
                snapshot.TransitionTo(Mathf.Max(0.01f, transitionTime));
            }
        }
    }
}


