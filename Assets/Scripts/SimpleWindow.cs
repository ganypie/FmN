using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleWindow : MonoBehaviour, IInteractable
{
    [Header("Player & Window Settings")]
    public Transform player;
    public float openHeight = 0.6f;            // насколько поднимается окно
    public float moveSpeed = 2f;               // базовая скорость движения
    public float interactDistance = 2f;        // радиус взаимодействия

    [Header("Easing Curve (кривая скорости)")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    private Vector3 closedLocalPos;
    private Vector3 targetLocalPos;
    private bool isOpen = false;
    private Collider cachedCollider;
    private float moveProgress = 0f;

    void Awake()
    {
        // Автоматический поиск игрока, если не задан
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        cachedCollider = GetComponent<Collider>();
    }

    void Start()
    {
        closedLocalPos = transform.localPosition;
        targetLocalPos = closedLocalPos;
    }

    void Update()
    {
        // Плавное движение с использованием кривой скорости
        float distance = Vector3.Distance(transform.localPosition, targetLocalPos);
        if (distance > 0.001f)
        {
            moveProgress = Mathf.Clamp01(moveProgress + Time.deltaTime * moveSpeed);
            float speedFactor = speedCurve.Evaluate(moveProgress);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, speedFactor);
        }
        else
        {
            moveProgress = 0f; // сброс при завершении
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
        ToggleWindow(interactor);
    }

    private void ToggleWindow(Transform interactor)
    {
        isOpen = !isOpen;
        moveProgress = 0f;

        if (isOpen)
        {
            targetLocalPos = closedLocalPos + Vector3.up * openHeight;

            if (audioSource && openSound)
                audioSource.PlayOneShot(openSound);
        }
        else
        {
            targetLocalPos = closedLocalPos;

            if (audioSource && closeSound)
                audioSource.PlayOneShot(closeSound);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
