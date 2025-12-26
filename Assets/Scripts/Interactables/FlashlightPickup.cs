using UnityEngine;

public class FlashlightPickup : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject flashlightOnTable; // фонарик, лежащий на столе
    [SerializeField] private GameObject flashlightInHand;  // фонарик в руке игрока
    [SerializeField] private Collider interactCollider;    // коллайдер для взаимодействия

    private bool isPickedUp = false;

    private void Awake()
    {
        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();
    }

    public bool CanInteract(Transform interactor)
    {
        return !isPickedUp;
    }

    public void Interact(Transform interactor)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        flashlightOnTable.SetActive(false);
        flashlightInHand.SetActive(true);
        interactCollider.enabled = false;
        
        // Если у включенного объекта есть контроллер фонарика — вызываем PickUp, чтобы его состояние было корректным
        var controller = flashlightInHand.GetComponent<FlashlightController>();
        if (controller != null)
        {
            controller.PickUp(interactor);
        }
    }
}
