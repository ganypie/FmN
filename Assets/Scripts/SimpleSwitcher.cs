using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleVykl : MonoBehaviour, IInteractable
{
    [Header("Light Settings")]
    [Tooltip("Источники света, которые будут включаться/выключаться")]
    public Light[] targetLights;
    
    [Header("Switch Settings")]
    [Tooltip("Угол поворота переключателя по оси X (в градусах)")]
    public float switchAngle = 45f;
    [Tooltip("Скорость вращения переключателя")]
    public float rotationSpeed = 180f;
    [Tooltip("Радиус взаимодействия")]
    public float interactDistance = 3f;
    
    [Header("Easing Curve")]
    [Tooltip("Кривая скорости анимации переключателя")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Sound Settings")]
    [Tooltip("Источник звука (если не задан, будет создан автоматически)")]
    public AudioSource audioSource;
    [Tooltip("Звук включения света")]
    public AudioClip turnOnSound;
    [Tooltip("Звук выключения света")]
    public AudioClip turnOffSound;
    [Range(0f, 1f)]
    [Tooltip("Громкость звуков")]
    public float soundVolume = 1f;
    [Range(0.5f, 2f)]
    [Tooltip("Высота звука (pitch)")]
    public float soundPitch = 1f;
    
    private float offAngleX;
    private float onAngleX;
    private float targetAngleX;
    private float currentAngleX;
    private bool isLightOn = false;
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
        
        // Проверяем наличие источников света
        if (targetLights == null || targetLights.Length == 0)
        {
            Debug.LogWarning($"SimpleVykl на {gameObject.name}: не заданы источники света (targetLights)!");
        }
    }
    
    void Start()
    {
        // Сохраняем начальный угол по оси X как состояние "выключено"
        Vector3 currentEuler = transform.localEulerAngles;
        offAngleX = currentEuler.x;
        onAngleX = offAngleX + switchAngle;
        
        // Устанавливаем начальное состояние
        targetAngleX = offAngleX;
        currentAngleX = offAngleX;
        
        // Устанавливаем начальное состояние источников света
        SetLightsState(isLightOn);
    }
    
    void Update()
    {
        // Плавное вращение переключателя только по оси X с кривой скорости
        float angleDiff = Mathf.DeltaAngle(currentAngleX, targetAngleX);
        if (Mathf.Abs(angleDiff) > 0.01f)
        {
            float t = Mathf.Clamp01(Mathf.Abs(angleDiff) / switchAngle);
            float speedFactor = speedCurve.Evaluate(t);
            float step = rotationSpeed * speedFactor * Time.deltaTime;
            
            currentAngleX = Mathf.MoveTowardsAngle(currentAngleX, targetAngleX, step);
            
            // Обновляем только ось X, сохраняя Y и Z
            Vector3 euler = transform.localEulerAngles;
            euler.x = currentAngleX;
            transform.localEulerAngles = euler;
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
        ToggleSwitch();
    }
    
    /// <summary>
    /// Переключает состояние переключателя и света
    /// </summary>
    private void ToggleSwitch()
    {
        isLightOn = !isLightOn;
        
        // Устанавливаем целевой угол по оси X
        targetAngleX = isLightOn ? onAngleX : offAngleX;
        
        // Управляем источниками света
        SetLightsState(isLightOn);
        
        // Воспроизводим соответствующий звук
        if (isLightOn)
            PlaySound(turnOnSound);
        else
            PlaySound(turnOffSound);
    }
    
    /// <summary>
    /// Устанавливает состояние всех источников света
    /// </summary>
    private void SetLightsState(bool enabled)
    {
        if (targetLights == null) return;
        
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.enabled = enabled;
            }
        }
    }
    
    /// <summary>
    /// Воспроизводит звук включения или выключения
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        
        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = soundPitch;
        audioSource.Play();
    }
}

