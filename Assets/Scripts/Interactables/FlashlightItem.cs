// FlashlightController.cs
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlashlightController : MonoBehaviour, IInteractable
{
    [Header("Light Settings")]
    public Light spotLight;                     
    public Light secondarySpotLight;            
    public KeyCode toggleKey = KeyCode.F;       

    [Header("Pickup Settings")]
    public Transform holdPoint;                 // точка удержания предмета
    public float pickupDistance = 2f;           // радиус взаимодействия для поднятия

    [Header("Audio")]
    public AudioClip soundOn;
    public AudioClip soundOff;
    public AudioClip pickupSound;

    private AudioSource audioSource;
    private bool isOn = false;
    private bool isPickedUp = false;
    private Transform originalParent;
    private PickupableItem pickupable;

    void Awake()
    {
        originalParent = transform.parent;

        pickupable = GetComponent<PickupableItem>();

        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>(true);
        if (secondarySpotLight != null) secondarySpotLight.enabled = false;
        if (spotLight != null) spotLight.enabled = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        bool held = (pickupable != null) ? pickupable.IsPickedUp : isPickedUp;

        if (!held)
        {
            if (isOn) TurnOff();
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            if (isOn) TurnOff();
            else TurnOn();
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

    // Сделано публичным, чтобы внешние триггеры (например, FlashlightPickup) могли вызвать подбор
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

        Debug.Log("[FlashlightController] Picked up by " + (interactor != null ? interactor.name : "unknown"));

        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
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

        if (isOn) TurnOff();
    }

    private void TurnOn()
    {
        isOn = true;
        if (spotLight != null) spotLight.enabled = true;
        if (secondarySpotLight != null) secondarySpotLight.enabled = true;
        if (soundOn != null && audioSource != null) audioSource.PlayOneShot(soundOn);
        Debug.Log("[FlashlightController] Turned ON");
    }

    private void TurnOff()
    {
        isOn = false;
        if (spotLight != null) spotLight.enabled = false;
        if (secondarySpotLight != null) secondarySpotLight.enabled = false;
        if (soundOff != null && audioSource != null) audioSource.PlayOneShot(soundOff);
        Debug.Log("[FlashlightController] Turned OFF");
    }

    // Метод для TaskManager
    public bool IsPickedUp()
    {
        if (pickupable != null) return pickupable.IsPickedUp;
        return isPickedUp;
    }
}
