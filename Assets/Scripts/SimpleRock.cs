using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleRock : MonoBehaviour, IInteractable
{
    [Header("Player & Rock Settings")]
    public Transform player;
    public float rotateAngle = 90f;     // угол поворота камня
    public float rotateSpeed = 180f;    // скорость поворота
    public float interactDistance = 3f; // радиус взаимодействия

    [Header("Easing Curve")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Quaternion closedRot;
    private Quaternion targetRot;
    private bool isRotated = false;
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
    }

    void Start()
    {
        closedRot = transform.rotation;
        targetRot = closedRot;
    }

    void Update()
    {
        // Плавный поворот с кривой скорости
        angleCache = Quaternion.Angle(transform.rotation, targetRot);
        if (angleCache > 0.01f)
        {
            float t = Mathf.Clamp01(angleCache / rotateAngle);
            float speedFactor = speedCurve.Evaluate(t);
            float step = rotateSpeed * speedFactor * Time.deltaTime;
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
        ToggleRock();
    }

    private void ToggleRock()
    {
        isRotated = !isRotated;

        if (isRotated)
            targetRot = closedRot * Quaternion.Euler(0f, rotateAngle, 0f);
        else
            targetRot = closedRot;
    }
}
