using UnityEngine;

/// <summary>
/// Скрипт для подъёма дров (PilePickup) — полная аналогия FlashlightPickup.
/// По умолчанию объект является "дровами на столе" и "дровами в руке" одновременно.
/// При подборе дровами нескольких штук они позиционируются рядом или поверх друг друга.
/// </summary>
public class PilePickup : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private Collider interactCollider; // коллайдер для взаимодействия (по умолчанию GetComponent)
    [SerializeField] private Vector3 woodOffset1 = new Vector3(0.2f, 0.1f, 0);     // смещение 1-го полена
    [SerializeField] private Vector3 woodOffset2 = new Vector3(-0.2f, 0.2f, 0);    // смещение 2-го полена
    [SerializeField] private Vector3 woodOffset3 = new Vector3(0, 0.35f, 0);       // смещение 3-го полена (поверх)

    private bool isPickedUp = false;
    private PileItem pileController;

    private void Awake()
    {
        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();
        
        pileController = GetComponent<PileItem>();
    }

    public bool CanInteract(Transform interactor)
    {
        // Нельзя взаимодействовать, если уже подняты
        if (isPickedUp) return false;
        
        // Нельзя взаимодействовать, если уже собрано 3 полена
        if (WoodCollector.Instance != null && WoodCollector.Instance.HasCollectedWood(3))
            return false;
        
        return true;
    }

    public void Interact(Transform interactor)
    {
        if (isPickedUp) return;
        
        // Проверяем максимум дров
        if (WoodCollector.Instance != null && WoodCollector.Instance.HasCollectedWood(3))
        {
            Debug.Log("[PilePickup] Cannot pick up more wood. Maximum 3 already collected.");
            return;
        }

        isPickedUp = true;
        
        // Деактивируем коллайдер взаимодействия (дрова на столе больше недоступны)
        if (interactCollider != null)
            interactCollider.enabled = false;

        // Если есть компонент PileItem — вызываем PickUp
        if (pileController != null)
        {
            pileController.PickUp(interactor);
        }

        Debug.Log("[PilePickup] Wood picked up");
    }
}
