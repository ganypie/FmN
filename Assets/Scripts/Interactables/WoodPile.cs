// WoodPile.cs
using UnityEngine;

public class WoodPile : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject woodOnPile;    // дрова на складе
    [SerializeField] private GameObject woodInHand;    // дрова в руках игрока
    [SerializeField] private Collider interactCollider; // коллайдер для взаимодействия

    private bool isPickedUp = false;

    private void Awake()
    {
        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();
    }

    public bool CanInteract(Transform interactor)
    {
        // можно добавить проверку дистанции, если нужно
        return !isPickedUp;
    }

    public void Interact(Transform interactor)
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // скрываем дрова на складе
        if (woodOnPile != null) woodOnPile.SetActive(false);

        // активируем дрова в руках игрока
        if (woodInHand != null) woodInHand.SetActive(true);

        // отключаем коллайдер, чтобы больше нельзя было кликнуть
        if (interactCollider != null) interactCollider.enabled = false;
    }

    // Метод для TaskManager
    public bool IsPickedUp()
    {
        return isPickedUp;
    }
}
