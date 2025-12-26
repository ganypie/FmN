using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleDoor : MonoBehaviour, IInteractable
{
    [Header("Player & Door Settings")]
    public Transform player;
    public float openAngle = 90f;       // угол открытия
    public float openSpeed = 180f;      // максимальная скорость вращения
    public float interactDistance = 3f; // радиус взаимодействия

    [Header("Easing Curve")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound Settings")]
    public AudioSource audioSource;        // Источник звука
    public AudioClip openSound;            // Звук открытия
    public AudioClip closeSound;           // Звук закрытия
    [Range(0f, 1f)] public float soundVolume = 1f; // Громкость
    [Range(0.5f, 2f)] public float soundPitch = 1f; // Скорость (высота) звука

    private Quaternion closedRot;
    private Quaternion targetRot;
    private bool isOpen = false;
    private Collider cachedCollider;
    private float angleCache;

    void Awake()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }
        cachedCollider = GetComponent<Collider>();

        // Проверяем, есть ли источник звука, если нет — создаём
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        closedRot = transform.rotation;
        targetRot = closedRot;
    }

    void Update()
    {
        // Плавное вращение с кривой скорости
        angleCache = Quaternion.Angle(transform.rotation, targetRot);
        if (angleCache > 0.01f)
        {
            float t = Mathf.Clamp01(angleCache / openAngle);
            float speedFactor = speedCurve.Evaluate(t);
            float step = openSpeed * speedFactor * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, step);
        }
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null) return false;
        if (cachedCollider == null) return false;

        float dist = InteractionUtils.ComputeDistanceToInteractor(cachedCollider, interactor);
        return dist <= interactDistance;
    }

    public void Interact(Transform interactor)
    {
        ToggleDoor(interactor);
    }

    private void ToggleDoor(Transform interactor)
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            if (interactor.position.x < transform.position.x)
                targetRot = closedRot * Quaternion.Euler(0f, 0f, -openAngle);
            else
                targetRot = closedRot * Quaternion.Euler(0f, 0f, openAngle);

            PlayDoorSound(openSound);

            // --- добавляем сигнал для задачи ---
            if (DoorWatcher.Instance != null)
                DoorWatcher.Instance.DoorOpened();
        }
        else
        {
            targetRot = closedRot;
            PlayDoorSound(closeSound);
        }
    }


    private void PlayDoorSound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.pitch = soundPitch;
        audioSource.Play();
    }
}
