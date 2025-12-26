using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // === УПРАВЛЕНИЕ ДОСТУПНО ===
    public bool canMove = true;

    public void SetMovement(bool value)
    {
        canMove = value;
    }

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;

    [Header("Directional Speed Multipliers")]
    public float forwardMultiplier = 1.2f;
    public float backwardMultiplier = 0.7f;
    public float strafeMultiplier = 0.85f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public Transform playerCamera;
    public float crouchTransitionSpeed = 8f;
    [Range(0.2f, 1f)] public float crouchHeightFactor = 0.5f;
    public float crouchSpeedFactor = 0.5f;

    private CharacterController controller;
    private float verticalVelocity;

    private float originalHeight;
    private float crouchHeight;

    private Vector3 cameraOriginalLocalPos;
    private Vector3 cameraCrouchLocalPos;

    private bool isCrouching = false;

    // Cached per-frame vars
    private bool isGrounded;
    private bool isRunning;
    private float currentSpeed;
    private float moveX;
    private float moveZ;
    private float appliedSpeed;
    private Vector3 horizontalMove;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        originalHeight = controller.height;
        crouchHeight = originalHeight * crouchHeightFactor;

        cameraOriginalLocalPos = playerCamera.localPosition;
        cameraCrouchLocalPos = new Vector3(
            cameraOriginalLocalPos.x,
            cameraOriginalLocalPos.y * crouchHeightFactor,
            cameraOriginalLocalPos.z
        );
    }

    void Update()
    {
        // === ПАУЗА ===
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        // === БЛОКИРОВКА УПРАВЛЕНИЯ (ИНТРО / СЦЕНЫ) ===
        if (!canMove)
            return;

        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        // Скорость ходьбы / бега
        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isCrouching)
        {
            currentSpeed *= crouchSpeedFactor;
        }

        moveX = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        moveZ = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        appliedSpeed = currentSpeed;
        if (moveZ > 0) appliedSpeed *= forwardMultiplier;
        else if (moveZ < 0) appliedSpeed *= backwardMultiplier;
        if (moveX != 0) appliedSpeed *= strafeMultiplier;

        horizontalMove = (transform.right * moveX + transform.forward * moveZ).normalized * appliedSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        velocity = horizontalMove + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // === ПРИСЕДАНИЕ ===
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
        }

        if (isCrouching)
        {
            controller.height = Mathf.Lerp(controller.height, crouchHeight, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                cameraCrouchLocalPos,
                Time.deltaTime * crouchTransitionSpeed
            );
        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, originalHeight, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = Vector3.Lerp(
                playerCamera.localPosition,
                cameraOriginalLocalPos,
                Time.deltaTime * crouchTransitionSpeed
            );
        }
    }
}
