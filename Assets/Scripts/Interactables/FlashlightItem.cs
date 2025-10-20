using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlashlightController : MonoBehaviour
{
    [Header("Light Settings")]
    public Light spotLight;                     // основной свет фонаря
    public Light secondarySpotLight;            // дополнительный свет для реалистичности
    public KeyCode toggleKey = KeyCode.F;       // клавиша включения/выключения

    [Header("Audio")]
    public AudioClip soundOn;
    public AudioClip soundOff;

    private AudioSource audioSource;
    private PickupableItem pickupable;
    private bool isOn = false;

    void Awake()
    {
        pickupable = GetComponent<PickupableItem>();
        if (pickupable == null)
            Debug.LogError("❌ FlashlightController: не найден PickupableItem!");

        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>(true);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (spotLight != null) spotLight.enabled = false;
        if (secondarySpotLight != null) secondarySpotLight.enabled = false;
    }

    void Update()
    {
        // фонарик работает только если предмет в руке
        if (pickupable == null || !pickupable.IsPickedUp)
        {
            if (isOn)
                TurnOff();
            return;
        }

        // нажатие F — включение/выключение
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOn) TurnOff();
            else TurnOn();
        }
    }

    private void TurnOn()
    {
        isOn = true;

        if (spotLight != null) spotLight.enabled = true;
        if (secondarySpotLight != null) secondarySpotLight.enabled = true;

        if (audioSource != null && soundOn != null)
            audioSource.PlayOneShot(soundOn);
    }

    private void TurnOff()
    {
        isOn = false;

        if (spotLight != null) spotLight.enabled = false;
        if (secondarySpotLight != null) secondarySpotLight.enabled = false;

        if (audioSource != null && soundOff != null)
            audioSource.PlayOneShot(soundOff);
    }
}
