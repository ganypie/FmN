using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// Lightweight pooled audio source with simple fade in/out utilities.
    /// </summary>
    public sealed class PooledAudioSource : MonoBehaviour
    {
        private AudioSource _source;
        private float _targetVolume;
        private float _fadeSpeed;
        private bool _autoReleaseWhenSilent;
        private bool _inUse;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        private void Update()
        {
            if (!_inUse) return;

            if (!Mathf.Approximately(_source.volume, _targetVolume))
            {
                _source.volume = Mathf.MoveTowards(_source.volume, _targetVolume, _fadeSpeed * Time.unscaledDeltaTime);
            }

            if (_autoReleaseWhenSilent && _source.volume <= 0.001f)
            {
                if (_source.isPlaying) _source.Stop();
                Release();
            }
        }

        /// <summary>
        /// Plays a clip immediately, configuring mixer routing and 2D/3D settings.
        /// </summary>
        public void PlayClip(AudioClip clip, float volume, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, AudioMixerGroup mixerGroup)
        {
            _source.clip = clip;
            _source.outputAudioMixerGroup = mixerGroup;
            _source.spatialBlend = spatialBlend;
            _source.minDistance = minDistance;
            _source.maxDistance = maxDistance;
            _source.rolloffMode = rolloff;
            _source.pitch = pitch;
            _source.volume = volume;
            _source.loop = false;
            _source.Play();
        }

        /// <summary>
        /// Starts/updates loop playback with optional crossfade.
        /// </summary>
        public void PlayLoop(AudioClip clip, float targetVolume, float crossfadeTime, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, AudioMixerGroup mixerGroup)
        {
            _source.outputAudioMixerGroup = mixerGroup;
            _source.spatialBlend = spatialBlend;
            _source.minDistance = minDistance;
            _source.maxDistance = maxDistance;
            _source.rolloffMode = rolloff;
            _source.pitch = pitch;
            _source.loop = true;

            if (_source.clip != clip)
            {
                _source.clip = clip;
                if (clip != null)
                {
                    _source.time = Random.Range(0f, Mathf.Max(0.01f, clip.length));
                }
                if (!_source.isPlaying && clip != null)
                {
                    _source.volume = 0f;
                    _source.Play();
                }
            }

            _targetVolume = targetVolume;
            _fadeSpeed = crossfadeTime > 0f ? Mathf.Max(0.001f, targetVolume / crossfadeTime) : 1000f;
            _autoReleaseWhenSilent = false;
        }

        /// <summary>
        /// Fades out and releases this pooled source when silent.
        /// </summary>
        public void FadeOutAndRelease(float fadeTime)
        {
            _targetVolume = 0f;
            _fadeSpeed = fadeTime > 0f ? Mathf.Max(0.001f, _source.volume / fadeTime) : 1000f;
            _autoReleaseWhenSilent = true;
        }

        internal void MarkInUse()
        {
            _inUse = true;
            _autoReleaseWhenSilent = false;
        }

        internal void Release()
        {
            _inUse = false;
            _source.Stop();
            _source.clip = null;
            _source.outputAudioMixerGroup = null;
            _source.volume = 0f;
            Pool.Release(this);
        }

        // -------- Pool --------
        public static class Pool
        {
            private static readonly Stack<PooledAudioSource> _pool = new Stack<PooledAudioSource>(32);
            private static Transform _root;

            public static void Initialize(Transform root, int preload)
            {
                _root = root;
                for (int i = 0; i < preload; i++)
                {
                    var inst = CreateInstance();
                    _pool.Push(inst);
                }
            }

            public static PooledAudioSource Get()
            {
                var inst = _pool.Count > 0 ? _pool.Pop() : CreateInstance();
                inst.MarkInUse();
                return inst;
            }

            public static void Release(PooledAudioSource src)
            {
                _pool.Push(src);
            }

            private static PooledAudioSource CreateInstance()
            {
                var go = new GameObject("PooledAudioSource");
                if (_root != null) go.transform.SetParent(_root, false);
                var src = go.AddComponent<PooledAudioSource>();
                return src;
            }
        }
    }
}


