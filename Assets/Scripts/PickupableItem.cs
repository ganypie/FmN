using UnityEngine;

/// <summary>
/// Компонент для предметов, которые можно поднимать и бросать
/// Основан на оригинальном CandlePickUp скрипте
/// </summary>
[RequireComponent(typeof(Collider))]
public class PickupableItem : MonoBehaviour, IInteractable
{
    public enum HandSide { Right, Left }

    [Header("Hand Assignment")]
    [Tooltip("Выберите, к какой руке относится предмет при подборе")]
    [SerializeField] private HandSide handSide = HandSide.Right;

    [Header("Pickup Settings")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private Transform player; // Ссылка на игрока
    [SerializeField] private Transform handPosition; // Позиция в руке игрока

    [Header("Hand Position & Rotation")]
    [SerializeField] private Vector3 handOffset = new Vector3(0.3f, 0.5f, 0.5f); // Позиция относительно игрока
    [SerializeField] private Vector3 handRotation = Vector3.zero; // Поворот в руке (градусы)
    [SerializeField] private Vector3 handScale = Vector3.one; // Масштаб в руке
    [SerializeField] private bool useHandOffset = true; // Использовать смещение руки

    [Header("Physics")]
    [SerializeField] private bool disablePhysicsWhenPicked = true;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardForce = 5f;
    [SerializeField] private bool throwFromHandPosition = true; // Бросать из позиции руки
    [SerializeField] private float groundCheckDistance = 0.1f; // Расстояние для проверки земли
    [SerializeField] private LayerMask groundLayerMask = 1; // Слои, считающиеся землей

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioSource audioSource;

    // Состояние
    private bool isPickedUp = false;
    private bool canPick = false;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Rigidbody rb;
    private Collider col;

    // Сохраненные настройки физики
    private bool wasKinematic;
    private bool wasGravityEnabled;

    void Start()
    {
        // Получаем компоненты
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Находим игрока если не задан
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Создаем AudioSource если его нет
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D звук
        }

        // Сохраняем изначальные настройки
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            wasGravityEnabled = rb.useGravity;
        }

        // Сохраняем изначальные трансформации
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // Создаем позицию руки если не задана
        if (handPosition == null && player != null)
        {
            CreateHandPosition();
        }
    }

    void Update()
    {
        // Не обрабатываем ввод во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        // Обработка глобального запроса броска: только один предмет может быть брошен за одно нажатие G
        HandleGlobalDropInput();
    }

    // --- GLOBAL DROP COORDINATION ---
    // Гарантирует, что при нажатии G будет брошен только один предмет (приоритет Right)
    private static int lastProcessedFrame = -1;
    private static bool lastGState = false;

    private void HandleGlobalDropInput()
    {
        // Выполняем логику только в одном экземпляре в кадре
        if (Time.frameCount == lastProcessedFrame) return;
        lastProcessedFrame = Time.frameCount;

        bool gDown = Input.GetKeyDown(KeyCode.G);
        if (!gDown) return;

        // Найти все поднятые предметы
        PickupableItem[] all;
#if UNITY_2023_2_OR_NEWER
        all = Object.FindObjectsByType<PickupableItem>(FindObjectsSortMode.None);
#else
        all = FindObjectsOfType<PickupableItem>();
#endif

        // Сначала ищем предметы Right
        foreach (var it in all)
        {
            if (it != null && it.IsPickedUp && it.handSide == HandSide.Right)
            {
                it.ForceDrop();
                return; // только один предмет
            }
        }

        // Если нет Right — ищем Left
        foreach (var it in all)
        {
            if (it != null && it.IsPickedUp && it.handSide == HandSide.Left)
            {
                it.ForceDrop();
                return;
            }
        }
    }

    private void CreateHandPosition()
    {
        // Создаем пустой объект для позиции руки
        GameObject handPos = new GameObject($"{name}_HandPosition");
        handPos.transform.SetParent(player);
        handPos.transform.localPosition = handOffset; // Используем настраиваемое смещение
        handPos.transform.localRotation = Quaternion.identity;
        handPosition = handPos.transform;
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null) return false;
        if (isPickedUp) return false; // Нельзя взаимодействовать с уже поднятым предметом

        if (col == null) return false;

        float dist = InteractionUtils.ComputeDistanceToInteractor(col, interactor);
        return dist <= interactDistance;
    }

    public void Interact(Transform interactor)
    {
        if (isPickedUp)
        {
            DropItem();
        }
        else
        {
            PickupItem(interactor);
        }
    }

    private void PickupItem(Transform interactor)
    {
        // Если у игрока уже есть предмет той же руки, выбрасываем его
    PickupableItem[] all;
#if UNITY_2023_2_OR_NEWER
    all = Object.FindObjectsByType<PickupableItem>(FindObjectsSortMode.None);
#else
    all = FindObjectsOfType<PickupableItem>();
#endif
        foreach (var other in all)
        {
            if (other == this) continue;
            if (other.IsPickedUp && other.handSide == this.handSide)
            {
                // Просто сбрасываем предмет без броска
                other.DropItem(false);
            }
        }

        isPickedUp = true;

        // Сохраняем текущие трансформации
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
        originalScale = transform.localScale;

        // Отключаем физику
        if (disablePhysicsWhenPicked && rb != null)
        {
            // Сначала сбрасываем скорости, потом делаем кинематическим
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Отключаем коллайдер (как в оригинальном скрипте)
        if (col != null)
        {
            col.enabled = false;
        }

        // Делаем объект дочерним к позиции руки
        if (handPosition != null)
        {
            transform.SetParent(handPosition);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(handRotation); // Используем настраиваемый поворот
            transform.localScale = handScale; // Используем настраиваемый масштаб
        }

        // Воспроизводим звук
        PlaySound(pickupSound);
    }

    // Новый параметр: applyThrowForce — применять ли силу броска
    public void DropItem(bool applyThrowForce = true)
    {
        if (!isPickedUp) return;

        // Сохраняем текущие трансформации в руке ПЕРЕД сбросом
        Vector3 currentWorldPosition = transform.position;
        Quaternion currentWorldRotation = transform.rotation;
        Vector3 currentWorldScale = transform.lossyScale;

        isPickedUp = false;

        // Возвращаем объект в мир с сохранением текущих трансформаций
        transform.SetParent(originalParent);
        transform.position = currentWorldPosition; // Используем позицию из руки
        transform.rotation = currentWorldRotation; // Используем поворот из руки
        transform.localScale = originalScale; // Восстанавливаем только локальный масштаб

        // Проверяем, не провалится ли предмет сквозь землю
        Vector3 safePosition = GetSafeDropPosition(currentWorldPosition);
        transform.position = safePosition;

        // Восстанавливаем физику
        if (disablePhysicsWhenPicked && rb != null)
        {
            // Сначала восстанавливаем настройки физики
            rb.isKinematic = wasKinematic;
            rb.useGravity = wasGravityEnabled;

            // Добавляем силу броска только если требуется и Rigidbody не кинематический
            if (applyThrowForce && player != null && !rb.isKinematic)
            {
                Vector3 throwDirection = player.forward;
                Vector3 velocity = player.GetComponent<CharacterController>()?.velocity ?? Vector3.zero;
                
                Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwUpwardForce;
                if (velocity.magnitude > 0.1f)
                {
                    throwVelocity += velocity * 0.5f;
                }

                rb.linearVelocity = throwVelocity;
            }
            else if (!applyThrowForce && rb != null && !rb.isKinematic)
            {
                // Просто сбрасываем скорость
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // Восстанавливаем коллайдер
        if (col != null)
        {
            col.enabled = true;
        }

        // Воспроизводим звук
        PlaySound(dropSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Публичные методы для внешнего управления
    public bool IsPickedUp => isPickedUp;
    public void ForceDrop() => DropItem(true);

    // Метод для сброса предмета в изначальную позицию
    public void ResetToOriginalPosition()
    {
        if (isPickedUp)
        {
            DropItem();
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        transform.SetParent(originalParent);

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = wasGravityEnabled;
            
            // Сбрасываем скорости только если Rigidbody не кинематический
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (col != null)
        {
            col.enabled = true;
        }
    }

    // Методы для настройки позиции в руке (можно вызывать из других скриптов)
    public void SetHandOffset(Vector3 offset)
    {
        handOffset = offset;
        if (handPosition != null)
        {
            handPosition.localPosition = handOffset;
        }
    }

    public void SetHandRotation(Vector3 rotation)
    {
        handRotation = rotation;
        if (isPickedUp && handPosition != null)
        {
            transform.localRotation = Quaternion.Euler(handRotation);
        }
    }

    public void SetHandScale(Vector3 scale)
    {
        handScale = scale;
        if (isPickedUp && handPosition != null)
        {
            transform.localScale = handScale;
        }
    }


    // Методы для получения текущих настроек
    public Vector3 GetHandOffset() => handOffset;
    public Vector3 GetHandRotation() => handRotation;
    public Vector3 GetHandScale() => handScale;

    // Метод для обновления позиции руки в реальном времени
    public void UpdateHandPosition()
    {
        if (handPosition != null)
        {
            handPosition.localPosition = handOffset;
        }
        if (isPickedUp)
        {
            transform.localRotation = Quaternion.Euler(handRotation);
            transform.localScale = handScale;
        }
    }

    // Метод для безопасного сброса предмета (предотвращает проваливание)
    private Vector3 GetSafeDropPosition(Vector3 targetPosition)
    {
        // Проверяем, есть ли земля под предметом
        RaycastHit hit;
        Vector3 rayStart = targetPosition + Vector3.up * 0.5f; // Начинаем проверку немного выше
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f, groundLayerMask))
        {
            // Если нашли землю, размещаем предмет на ней
            float bottomY = targetPosition.y - (col != null ? col.bounds.size.y / 2f : 0.5f);
            float groundY = hit.point.y;
            
            if (bottomY < groundY)
            {
                // Предмет проваливается в землю, поднимаем его
                float offset = groundY - bottomY + groundCheckDistance;
                return new Vector3(targetPosition.x, targetPosition.y + offset, targetPosition.z);
            }
        }
        
        // Если землю не нашли, возвращаем исходную позицию
        return targetPosition;
    }
}
