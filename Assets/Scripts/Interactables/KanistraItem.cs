// KanistraItem.cs
using UnityEngine;

public class KanistraItem : MonoBehaviour, IInteractable
{
    [Header("Pickup Settings")]
    public Transform holdPoint;                 // точка удержания предмета
    public float pickupDistance = 2f;           // радиус взаимодействия для поднятия

    [Header("Generator")]
    public Generator generatorReference;        // ссылка на генератор

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

        // Пытаемся найти генератор если не задан
        if (generatorReference == null)
        {
#if UNITY_2023_2_OR_NEWER
            generatorReference = FindFirstObjectByType<Generator>();
#else
            generatorReference = FindObjectOfType<Generator>();
#endif
        }
    }

    void Update()
    {
        bool held = (pickupable != null) ? pickupable.IsPickedUp : isPickedUp;

        // Если канистра не в руках - ничего не делаем
        if (!held) return;
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

    // Сделано публичным, чтобы внешние триггеры (например, KanistraPickup) могли вызвать подбор
    public void PickUp(Transform interactor)
    {
        if (isPickedUp || (pickupable != null && pickupable.IsPickedUp)) return;

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

        if (interactor != null)
        {
            var held = interactor.GetComponent<HeldItem>();
            if (held != null)
                held.Pickup(this.gameObject);
        }

        Debug.Log("[KanistraItem] Picked up by " + (interactor != null ? interactor.name : "unknown"));

        // Воспроизводим звук через AudioPickup систему
        if (audioPickup != null)
        {
            audioPickup.PlayPickupSound(gameObject);
        }
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

        if (interactor != null)
        {
            var held = interactor.GetComponent<HeldItem>();
            if (held != null && held.CurrentItem == this.gameObject)
                held.Drop();
        }

        // Воспроизводим звук выброса через AudioPickup систему
        if (audioPickup != null)
        {
            audioPickup.PlayDropSound(gameObject);
        }

        Debug.Log("[KanistraItem] Dropped");
    }

    public bool IsPickedUp
    {
        get { return (pickupable != null) ? pickupable.IsPickedUp : isPickedUp; }
    }

    // Метод для использования канистры с генератором
    public void UseOnGenerator()
    {
        if (generatorReference != null && generatorReference.CanInteract(transform.parent))
        {
            generatorReference.Interact(transform.parent);
            // После взаимодействия генератор должен будет удалить канистру
            Destroy(gameObject);
        }
    }
}
