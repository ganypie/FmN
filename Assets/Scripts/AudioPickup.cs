using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Система звуков подбора предметов.
/// Позволяет назначать звуки разным предметам через инспектор.
/// Срабатывает при взаимодействии игрока с предметом (тэг Interactable).
/// </summary>
public class AudioPickup : MonoBehaviour
{
    [System.Serializable]
    public class PickupAudioEntry
    {
        [Tooltip("Предмет, у которого воспроизводится звук при подборе")]
        public GameObject item;
        
        [Tooltip("Звук, который воспроизводится при подборе")]
        public AudioClip pickupSound;
        
        [Tooltip("Звук при выбросе (опционально)")]
        public AudioClip dropSound;
    }

    [SerializeField] private List<PickupAudioEntry> audioEntries = new List<PickupAudioEntry>();
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float spatialBlend = 1f; // 0 = 2D, 1 = 3D

    private Dictionary<GameObject, PickupAudioEntry> audioMap;

    void Awake()
    {
        // Создаем AudioSource если его нет
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;

        // Заполняем словарь для быстрого поиска
        audioMap = new Dictionary<GameObject, PickupAudioEntry>();
        foreach (var entry in audioEntries)
        {
            if (entry.item != null)
            {
                audioMap[entry.item] = entry;
            }
        }

        // Подписываемся на все Interactable предметы в сцене
        SubscribeToInteractables();
    }

    private void SubscribeToInteractables()
    {
        // Ищем все объекты с тэгом Interactable и добавляем им слушатели
#if UNITY_2023_2_OR_NEWER
        GameObject[] interactables = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
#else
        GameObject[] interactables = FindObjectsOfType<GameObject>();
#endif

        foreach (var obj in interactables)
        {
            if (obj.CompareTag("Interactable"))
            {
                var interactable = obj.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // Проверяем, есть ли это в нашем списке
                    if (audioMap.ContainsKey(obj))
                    {
                        Debug.Log($"[AudioPickup] Registered audio for {obj.name}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Воспроизводит звук подбора для указанного предмета
    /// </summary>
    public void PlayPickupSound(GameObject item)
    {
        if (item == null) return;

        if (audioMap.TryGetValue(item, out var entry) && entry.pickupSound != null)
        {
            audioSource.PlayOneShot(entry.pickupSound);
            Debug.Log($"[AudioPickup] Playing pickup sound for {item.name}");
        }
    }

    /// <summary>
    /// Воспроизводит звук выброса для указанного предмета
    /// </summary>
    public void PlayDropSound(GameObject item)
    {
        if (item == null) return;

        if (audioMap.TryGetValue(item, out var entry) && entry.dropSound != null)
        {
            audioSource.PlayOneShot(entry.dropSound);
            Debug.Log($"[AudioPickup] Playing drop sound for {item.name}");
        }
    }

    /// <summary>
    /// Получает запись о звуках для предмета
    /// </summary>
    public PickupAudioEntry GetAudioEntry(GameObject item)
    {
        if (audioMap.TryGetValue(item, out var entry))
            return entry;
        return null;
    }

    /// <summary>
    /// Добавляет новую запись в런время (если нужно динамически)
    /// </summary>
    public void AddAudioEntry(GameObject item, AudioClip pickupClip, AudioClip dropClip = null)
    {
        var entry = new PickupAudioEntry { item = item, pickupSound = pickupClip, dropSound = dropClip };
        audioEntries.Add(entry);
        if (item != null)
            audioMap[item] = entry;
    }
}
