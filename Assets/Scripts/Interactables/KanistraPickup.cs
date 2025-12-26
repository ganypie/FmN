// KanistraPickup.cs
using UnityEngine;

public class KanistraPickup : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject kanistraOnTable;   // канистра, лежащая на столе/поверхности
    [SerializeField] private GameObject kanistraInHand;    // канистра в руке игрока
    [SerializeField] private Collider interactCollider;    // коллайдер для взаимодействия

    [Header("Generator")]
    [SerializeField] private Generator generatorReference; // ссылка на генератор

    private bool isPickedUp = false;

    private void Awake()
    {
        if (interactCollider == null)
            interactCollider = GetComponent<Collider>();

        // Пытаемся найти генератор если не задан
        if (generatorReference == null)
        {
            generatorReference = FindObjectOfType<Generator>();
        }
    }

    public bool CanInteract(Transform interactor)
    {
        return !isPickedUp;
    }

    public void Interact(Transform interactor)
    {
        if (isPickedUp) return;

        isPickedUp = true;
        kanistraOnTable.SetActive(false);
        kanistraInHand.SetActive(true);
        interactCollider.enabled = false;
        
        // Если у включенного объекта есть контроллер канистры — вызываем PickUp, чтобы его состояние было корректным
        var controller = kanistraInHand.GetComponent<KanistraItem>();
        if (controller != null)
        {
            controller.PickUp(interactor);

            // Если генератор доступен, связываем его с контроллером
            if (generatorReference != null)
            {
                controller.generatorReference = generatorReference;
            }
        }
    }
}
