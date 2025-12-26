// Car.cs
using UnityEngine;

public class Car : MonoBehaviour, IInteractable
{
    [Header("References")]
    public Fireplace fireplace;          // ссылка на камин
    public Transform player;             // игрок
    public float checkDistance = 3f;     // дистанция, на которой звук сменяется на фары

    [Header("Audio & Lights")]
    public AudioClip alarmSound;         // звук сигнализации
    public AudioClip lightsSound;        // звук включения фар
    public Light[] lights;               // фары автомобиля

    private AudioSource audioSource;
    private bool alarmActive = false;
    private bool lightsActive = false;
    private bool alarmChecked = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        if (lights != null)
        {
            foreach (var light in lights)
                light.enabled = false;
        }
    }

    void Update()
    {
        if (fireplace != null && fireplace.HasWoodBeenPlaced() && !alarmActive)
        {
            // включаем сигнализацию
            if (alarmSound != null)
            {
                audioSource.clip = alarmSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            alarmActive = true;
        }

        if (alarmActive && player != null)
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= checkDistance && !lightsActive)
            {
                // выключаем сигнализацию
                audioSource.Stop();

                // включаем фары
                if (lights != null)
                {
                    foreach (var light in lights)
                        light.enabled = true;
                }

                // включаем звук фар
                if (lightsSound != null)
                {
                    audioSource.clip = lightsSound;
                    audioSource.loop = false;
                    audioSource.Play();
                }

                lightsActive = true;
                alarmChecked = true;
            }
        }
    }

    public bool CanInteract(Transform interactor)
    {
        // игрок взаимодействует с машиной только для проверки (по TaskManager)
        return false; // здесь взаимодействие не требуется, логика автоматическая
    }

    public void Interact(Transform interactor)
    {
        // не используется
    }

    // Метод для TaskManager
    public bool HasAlarmBeenChecked()
    {
        return alarmChecked;
    }
}
