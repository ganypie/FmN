using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
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
    public Transform playerCamera;            // Камера игрока
    public float crouchTransitionSpeed = 8f;  // Скорость перехода
    [Range(0.2f, 1f)] public float crouchHeightFactor = 0.5f; // На сколько уменьшается рост (0.5 = наполовину)
    public float crouchSpeedFactor = 0.5f;    // Во сколько раз замедляется скорость при приседе

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
        // Запоминаем исходные параметры
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
        // Не обрабатываем движение во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
            
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        // Скорость ходьбы / бега
        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Уменьшаем скорость при приседе
        if (isCrouching)
        {
            currentSpeed *= crouchSpeedFactor;
        }

        moveX = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        moveZ = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        // Множители направления
        appliedSpeed = currentSpeed;
        if (moveZ > 0) appliedSpeed *= forwardMultiplier;
        else if (moveZ < 0) appliedSpeed *= backwardMultiplier;
        if (moveX != 0) appliedSpeed *= strafeMultiplier;

        // Горизонтальное движение
        horizontalMove = (transform.right * moveX + transform.forward * moveZ).normalized * appliedSpeed;

        // Прыжок
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Гравитация
        verticalVelocity += gravity * Time.deltaTime;

        // Итоговый вектор скорости
        velocity = horizontalMove + Vector3.up * verticalVelocity;

        // Перемещаем персонажа
        controller.Move(velocity * Time.deltaTime);

        // === ПРИСЕДАНИЕ ПО НАЖАТИЮ ===
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
