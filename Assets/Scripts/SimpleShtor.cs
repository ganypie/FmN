using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleShtor : MonoBehaviour, IInteractable
{
    [Header("Curtain Type")]
    [Tooltip("Отметьте, если это модель закрытых штор. Если не отмечено - это модель открытых штор.")]
    public bool isClosedModel = false;
    
    [Header("Opposite Model")]
    [Tooltip("Противоположная модель (если это закрытая - укажите открытую, и наоборот)")]
    public GameObject oppositeModel;
    
    [Header("Interaction Settings")]
    [Tooltip("Радиус взаимодействия")]
    public float interactDistance = 3f;
    
    [Header("Sound Settings")]
    [Tooltip("Источник звука (если не задан, будет создан автоматически)")]
    public AudioSource audioSource;
    [Tooltip("Звук открытия штор (раздвигания)")]
    public AudioClip openSound;
    [Tooltip("Звук закрытия штор (сдвигания)")]
    public AudioClip closeSound;
    [Range(0f, 1f)] 
    [Tooltip("Громкость звуков")]
    public float soundVolume = 1f;
    [Range(0.5f, 2f)] 
    [Tooltip("Высота звука (pitch)")]
    public float soundPitch = 1f;
    
    private Collider cachedCollider;
    
    void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        
        // Проверяем, есть ли источник звука, если нет — создаём
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D звук
        }
        
        // Проверяем наличие противоположной модели
        if (oppositeModel == null)
            Debug.LogWarning($"SimpleShtor на {gameObject.name}: не задана противоположная модель (oppositeModel)!");
    }
    
    void Start()
    {
        // Устанавливаем начальное состояние модели в зависимости от типа
        // Если это закрытая модель (isClosedModel = true) - модель должна быть активна
        // Если это открытая модель (isClosedModel = false) - модель должна быть неактивна
        gameObject.SetActive(isClosedModel);
        
        // Убеждаемся, что противоположная модель в правильном состоянии
        if (oppositeModel != null)
        {
            // Противоположная модель должна быть в противоположном состоянии
            oppositeModel.SetActive(!isClosedModel);
        }
    }
    
    public bool CanInteract(Transform interactor)
    {
        if (interactor == null) return false;
        if (cachedCollider == null) return false;
        
        float dist = InteractionUtils.ComputeDistanceToInteractor(cachedCollider, interactor);
        return dist <= interactDistance;
    }
    
    public void Interact(Transform interactor)
    {
        ToggleCurtains();
    }
    
    /// <summary>
    /// Переключает состояние штор: скрывает текущую модель и показывает противоположную
    /// </summary>
    private void ToggleCurtains()
    {
        if (oppositeModel == null)
        {
            Debug.LogWarning($"SimpleShtor на {gameObject.name}: невозможно переключить, не задана противоположная модель!");
            return;
        }
        
        // Скрываем текущую модель
        gameObject.SetActive(false);
        
        // Показываем противоположную модель
        oppositeModel.SetActive(true);
        
        // Воспроизводим соответствующий звук
        // Если это закрытая модель (isClosedModel = true), то мы открываем шторы
        // Если это открытая модель (isClosedModel = false), то мы закрываем шторы
        if (isClosedModel)
            PlaySound(openSound); // Открываем шторы
        else
            PlaySound(closeSound); // Закрываем шторы
    }
    
    /// <summary>
    /// Воспроизводит звук открытия или закрытия
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = soundPitch;
        audioSource.Play();
    }
    
    /// <summary>
    /// Возвращает, является ли эта модель закрытой
    /// </summary>
    public bool IsClosedModel => isClosedModel;
    
    /// <summary>
    /// Возвращает, является ли эта модель открытой
    /// </summary>
    public bool IsOpenModel => !isClosedModel;
}

