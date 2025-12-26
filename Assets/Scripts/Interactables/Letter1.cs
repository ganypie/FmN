// LetterInteractable.cs
using UnityEngine;
using System.Collections;
using System.Reflection;

[RequireComponent(typeof(Collider))]
public class LetterInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public Transform playerCamera;
    public float moveDuration = 0.6f;
    public Vector3 localOffset = new Vector3(0f, -0.05f, 0.4f);
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
    public float interactDistance = 2f;

    private bool isReading = false;
    private Vector3 startPos;
    private Quaternion startRot;
    private Collider cachedCollider;

    private MouseLook mouseLook;
    private PlayerMovement playerMovement;

    private bool isCameraLocked = false;

    void Awake()
    {
        cachedCollider = GetComponent<Collider>();
        cachedCollider.isTrigger = true;

        if (playerCamera == null)
            playerCamera = Camera.main.transform;

        mouseLook = playerCamera.GetComponent<MouseLook>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
    }

    void LateUpdate()
    {
        if (isCameraLocked && playerCamera != null)
        {
            playerCamera.LookAt(transform.position);
        }
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null) return false;
        return Vector3.Distance(transform.position, interactor.position) <= interactDistance && !isReading;
    }

    public void Interact(Transform interactor)
    {
        if (!isReading)
            StartCoroutine(MoveToCameraAndRead());
    }

    private IEnumerator MoveToCameraAndRead()
    {
        isReading = true;

        if (mouseLook != null) mouseLook.enabled = false;
        if (playerMovement != null) playerMovement.SetMovement(false);

        startPos = transform.position;
        startRot = transform.rotation;

        Vector3 targetPos =
            playerCamera.position +
            playerCamera.forward * localOffset.z +
            playerCamera.up * localOffset.y +
            playerCamera.right * localOffset.x;

        Quaternion targetRot =
            Quaternion.LookRotation(-playerCamera.up) *
            Quaternion.Euler(rotationOffset);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / moveDuration);

            transform.position = Vector3.Lerp(startPos, targetPos, k);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, k);

            playerCamera.LookAt(transform.position);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        isCameraLocked = true;

        while (!Input.GetKeyDown(KeyCode.E))
            yield return null;

        // --- ВАЖНО: сначала отпускаем lock ---
        isCameraLocked = false;

        // --- синхронизируем MouseLook с текущим положением камеры ---
        SyncMouseLookRotation();

        if (mouseLook != null) mouseLook.enabled = true;
        if (playerMovement != null) playerMovement.SetMovement(true);

        DemoTaskSetup setup = FindObjectOfType<DemoTaskSetup>();
        if (setup != null)
            setup.letter = null;

        Destroy(gameObject);
    }

    private void SyncMouseLookRotation()
    {
        if (mouseLook == null || playerCamera == null) return;

        float x = playerCamera.localEulerAngles.x;
        if (x > 180f) x -= 360f;

        FieldInfo field = typeof(MouseLook).GetField(
            "xRotation",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (field != null)
            field.SetValue(mouseLook, -x);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}
