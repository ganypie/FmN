using UnityEngine;

namespace Game.Audio
{
    /// <summary>
    /// Marks a trigger volume as an "indoor" area. When the player enters, weather audio switches to Indoor state.
    /// Place this on a GameObject with a Trigger Collider that covers the interior space.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class IndoorZone : MonoBehaviour
    {
        [Tooltip("Tag of the player root object that carries the CharacterController/Camera.")]
        public string playerTag = "Player";

        [Tooltip("If true, only the first enter sets indoor, and exit anywhere clears it. If false, supports nested indoor zones via a counter.")]
        public bool simpleMode = false;

        private int _insideCount;

        void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _insideCount++;
            WeatherAudioManager.Instance.SetIndoor(true);
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _insideCount = Mathf.Max(0, _insideCount - 1);
            if (simpleMode || _insideCount == 0)
            {
                WeatherAudioManager.Instance.SetIndoor(false);
            }
        }
    }
}


