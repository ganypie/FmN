using UnityEngine;

/// <summary>
/// Скрипт дров (PileItem) — полная аналогия FlashlightController.
/// Поддерживает PickupableItem и прямой подъём через HeldItem.
/// При размещении в камине дрова исчезают из рук и появляются в камине.
/// Звуки управляются через AudioPickup скрипт.
/// </summary>
public class PileItem : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    public Transform holdPoint;              // точка удержания предметов
    public float pickupDistance = 2f;        // радиус взаимодействия для поднятия

    [Header("Fireplace Integration")]
    public Fireplace fireplace;              // ссылка на камин (для проверки размещения)

    private bool isPickedUp = false;
    private Transform originalParent;
    private PickupableItem pickupable;
    private AudioPickup audioPickup;

    void Awake()
    {
        originalParent = transform.parent;
        pickupable = GetComponent<PickupableItem>();

        // Ищем AudioPickup в сцене
#if UNITY_2023_2_OR_NEWER
        audioPickup = FindFirstObjectByType<AudioPickup>();
#else
        audioPickup = FindObjectOfType<AudioPickup>();
#endif

        // Если fireplace не назначен, ищем в сцене
        if (fireplace == null)
        {
#if UNITY_2023_2_OR_NEWER
            fireplace = FindFirstObjectByType<Fireplace>();
#else
            fireplace = FindObjectOfType<Fireplace>();
#endif
        }
    }

    void Update()
    {
        // Если дрова размещены в камине, исчезаем
        if (fireplace != null && fireplace.HasWoodBeenPlaced())
        {
            gameObject.SetActive(false);
            return;
        }
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null || isPickedUp) return false;
        float dist = Vector3.Distance(transform.position, interactor.position);
        return dist <= pickupDistance;
    }

    public void Interact(Transform interactor)
    {
        PickUp(interactor);
    }

    public void PickUp(Transform interactor)
    {
        if (isPickedUp || (pickupable != null && pickupable.IsPickedUp)) return;

        // Проверяем WoodCollector — если уже 3 полена, отказываем
        if (WoodCollector.Instance != null && WoodCollector.Instance.HasCollectedWood(3))
        {
            Debug.Log("[PileItem] Cannot pick up more wood. Maximum 3 already collected.");
            return;
        }

        // Если у нас есть PickupableItem, делегируем ему подбор
        if (pickupable != null)
        {
            pickupable.Interact(interactor);
        }
        else
        {
            isPickedUp = true;
            if (holdPoint != null)
            {
                transform.SetParent(holdPoint);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // Обновляем WoodCollector и HeldItem для дров
        if (interactor != null)
        {
            var held = interactor.GetComponent<HeldItem>();
            if (held != null)
            {
                // Используем PickupWood вместо Pickup — дрова "прилипают"
                // PickupWood сам позиционирует полено в зависимости от количества
                bool success = held.PickupWood(this.gameObject);
                if (success && WoodCollector.Instance != null)
                {
                    WoodCollector.Instance.AddWood();
                    Debug.Log("[PileItem] Added to WoodCollector and positioned in hand");
                }
                else if (!success)
                {
                    Debug.Log("[PileItem] Failed to pickup wood (HeldItem.PickupWood returned false)");
                }
            }
            else
            {
                Debug.LogWarning("[PileItem] No HeldItem component found on interactor!");
            }
        }

        // Воспроизводим звук через AudioPickup систему
        if (audioPickup != null)
        {
            audioPickup.PlayPickupSound(gameObject);
        }

        Debug.Log("[PileItem] Picked up by " + (interactor != null ? interactor.name : "unknown"));
    }

    public void Drop(Transform interactor)
    {
        if (pickupable != null)
        {
            if (pickupable.IsPickedUp) pickupable.ForceDrop();
        }
        else
        {
            isPickedUp = false;
            transform.SetParent(originalParent);
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = true;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
        }

        // Дрова "прилипают" — их нельзя выбросить просто так
        // Логика: если игрок попытается выбросить (G), ничего не произойдёт
        // Дрова исчезают только при размещении в камине
        Debug.Log("[PileItem] Drop attempt blocked - wood is stuck until placed in fireplace");
    }

    // Метод для TaskManager — проверка, подняты ли дрова
    public bool IsPickedUp()
    {
        if (pickupable != null) return pickupable.IsPickedUp;
        return isPickedUp;
    }
}
