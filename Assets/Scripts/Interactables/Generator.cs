// Generator.cs
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Generator : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public GameObject requiredItem;       // предмет, который нужен для заправки
    public Transform player;              // игрок
    public float interactDistance = 2f;   // радиус взаимодействия
    public AudioClip humSound;            // звук работающего генератора
    public float shakeAmount = 0.05f;     // амплитуда подёргивания
    public float shakeSpeed = 5f;         // скорость подёргивания

    private bool isFilled = false;
    private AudioSource audioSource;
    private Vector3 initialPos;
    private bool isShaking = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        initialPos = transform.position;
    }

    void Update()
    {
        // Простое “подёргивание” генератора
        if (isShaking)
        {
            float yOffset = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.position = initialPos + new Vector3(0, yOffset, 0);
        }
    }

    public bool CanInteract(Transform interactor)
    {
        if (isFilled || interactor == null) return false;
        float dist = Vector3.Distance(interactor.position, transform.position);
        return dist <= interactDistance;
    }

    public void Interact(Transform interactor)
    {
        if (isFilled) return;

        // Проверяем, держит ли игрок requiredItem рядом
        HeldItem held = interactor.GetComponent<HeldItem>();
        if (held != null && held.CurrentItem != null)
        {
            // Проверяем точное совпадение с prefab или по компоненту KanistraItem
            KanistraItem kanistra = held.CurrentItem.GetComponent<KanistraItem>();
            
            if (held.CurrentItem == requiredItem || kanistra != null)
            {
                // удаляем предмет из рук
                held.Drop();

                // если это KanistraItem, уничтожаем его
                if (kanistra != null)
                {
                    Destroy(kanistra.gameObject);
                }

                // включаем генератор
                isFilled = true;
                isShaking = true;

                if (humSound != null && audioSource != null)
                {
                    audioSource.clip = humSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }

                Debug.Log("Генератор заправлен!");
            }
            else
            {
                Debug.Log("Это не то топливо!");
            }
        }
        else
        {
            Debug.Log("Игрок не держит нужный предмет!");
        }
    }

    public bool IsFilled()
    {
        return isFilled;
    }
}
