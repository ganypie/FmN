// Fireplace.cs
using UnityEngine;

public class Fireplace : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject requiredItem;   // объект, который должен быть у игрока в руках
    public GameObject placedWood;     // объект дров, который появляется в камине
    public Transform player;          // ссылка на игрока
    public Collider interactCollider; // коллайдер взаимодействия
    public float interactDistance = 2f;

    private bool woodPlaced = false;

    private void Awake()
    {
        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();

        if (placedWood != null)
            placedWood.SetActive(false); // изначально в камине пусто
    }

    public bool CanInteract(Transform interactor)
    {
        if (woodPlaced || interactor == null) return false;
        float dist = Vector3.Distance(interactor.position, transform.position);
        return dist <= interactDistance;
    }

    public void Interact(Transform interactor)
    {
        if (woodPlaced) return;

        // проверяем, держит ли игрок дрова (любой объект с компонентом PileItem)
        HeldItem held = interactor.GetComponent<HeldItem>();
        if (held != null)
        {
            // Проверяем, есть ли у игрока объект с компонентом PileItem
            PileItem pileItem = held.CurrentItem != null ? held.CurrentItem.GetComponent<PileItem>() : null;
            if (pileItem != null)
            {
                // Проверяем, собрано ли ровно 3 полена
                if (WoodCollector.Instance != null && WoodCollector.Instance.HasCollectedWood(3))
                {
                    // Убираем дрова из рук (все 3 полена)
                    held.ClearAllWood();

                    // Активируем дрова в камине (только если нужно для визуализации)
                    if (placedWood != null)
                        placedWood.SetActive(true);

                    woodPlaced = true;

                    // Сбрасываем счётчик дров
                    if (WoodCollector.Instance != null)
                    {
                        WoodCollector.Instance.Reset();
                    }

                    Debug.Log("[Fireplace] Wood placed successfully (3 pieces)");
                    return;
                }
                else
                {
                    Debug.Log("[Fireplace] Need 3 wood pieces to place in fireplace!");
                    return;
                }
            }

            // Вариант 2: Проверяем через requiredItem (совместимость)
            if (held.CurrentItem == requiredItem)
            {
                // убираем дрова из рук
                held.Drop();

                // активируем дрова в камине
                if (placedWood != null)
                    placedWood.SetActive(true);

                woodPlaced = true;

                // отключаем коллайдер, чтобы нельзя было повторно класть
                if (interactCollider != null)
                    interactCollider.enabled = false;

                Debug.Log("[Fireplace] Wood placed (requiredItem)");
                return;
            }

            Debug.Log("В руках игрока нет нужных дров!");
            return;
        }
    }

    // Метод для TaskManager
    public bool HasWoodBeenPlaced()
    {
        return woodPlaced;
    }
}
