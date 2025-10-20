using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Backend abstraction for different audio middlewares (Unity Audio, FMOD, Wwise).
    /// </summary>
    public interface IWeatherAudioBackend
    {
        /// <summary>
        /// Sets or crossfades to loop layer content.
        /// </summary>
        void SetLoop(string layerId, AudioClip clip, float targetVolume, float crossfadeTime, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, UnityEngine.Audio.AudioMixerGroup mixerGroup);

        /// <summary>
        /// Fades out and stops a loop layer.
        /// </summary>
        void StopLoop(string layerId, float fadeTime);

        /// <summary>
        /// Plays a one-shot transient at position.
        /// </summary>
        void PlayTransient(string transientId, AudioClip clip, Vector3 position, float volume, float spatialBlend, float minDistance, float maxDistance, AudioRolloffMode rolloff, float pitch, UnityEngine.Audio.AudioMixerGroup mixerGroup);

        /// <summary>
        /// Applies indoor/outdoor state (e.g., via mixer snapshots).
        /// </summary>
        void SetIndoor(bool indoor, float transitionTime);
    }
}


