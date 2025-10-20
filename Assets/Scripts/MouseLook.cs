using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f; // стандартная чувствительность
    public Transform playerBody;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 2f;
    public float bobHorizontalAmplitude = 0.04f;
    public float bobVerticalAmplitude = 0.025f;
    public float speedForFullBob = 5f;
    public CharacterController controller;

    private float xRotation = 0f;
    private Vector3 initialCameraLocalPos;
    private float bobTimer = 0f;
    private float bobWeight = 0f;
    private float bobWeightVel;
    private Vector3 bobVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        initialCameraLocalPos = transform.localPosition;
    }

    void Update()
    {
        // Не обрабатываем ввод мыши во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
            
        HandleMouseLook();
        if (enableHeadBob) HandleHeadBob();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadBob()
    {
        if (controller == null) return;

        Vector3 vel = controller.velocity;
        vel.y = 0f;
        float speed = vel.magnitude;
        float targetWeight = Mathf.Clamp01(speed / Mathf.Max(0.01f, speedForFullBob));
        bobWeight = Mathf.SmoothDamp(bobWeight, targetWeight, ref bobWeightVel, 0.1f);

        float phase = bobTimer;
        if (bobWeight > 0.01f)
        {
            bobTimer += Time.deltaTime * bobFrequency * bobWeight;
            if (bobTimer > Mathf.PI * 2f) bobTimer -= Mathf.PI * 2f;

            phase = bobTimer;
        }

        float hor = Mathf.Sin(phase) * bobHorizontalAmplitude * bobWeight;
        float vert = Mathf.Abs(Mathf.Cos(phase)) * bobVerticalAmplitude * bobWeight;

        Vector3 targetLocal = initialCameraLocalPos + new Vector3(hor, vert, 0);
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetLocal, ref bobVelocity, 0.1f);
    }
}
