using UnityEngine;

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(1000)]
public class HeadBob : MonoBehaviour
{
    [Header("Настройки покачивания головы")]
    public float bobbingSpeed = 10f;
    public float bobbingAmount = 0.05f;
    public float runAmplitudeMultiplier = 1.5f;
    public float runSpeedMultiplier = 1.5f;
    public float walkAmplitudeMultiplier = 1.0f;
    public float walkSpeedMultiplier = 1.0f;
    public float speedThreshold = 0.1f;

    [Header("Ссылки")]
    [Tooltip("Ссылка на CharacterController игрока")]
    public CharacterController controller;

    private Transform camTransform;
    private Vector3 originalLocalPos;
    private float timer = 0f;

    void Start()
    {
        camTransform = transform;
        originalLocalPos = camTransform.localPosition;

        // Если ссылка не задана вручную — ищем у родителей
        if (controller == null)
        {
            controller = GetComponentInParent<CharacterController>();
        }

        if (controller == null)
        {
            Debug.LogError("HeadBob: Не найден CharacterController! Укажи ссылку вручную в инспекторе.");
        }
    }

    void Update()
    {
        if (controller == null) return; // предотвращаем ошибку

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0;
        float speed = horizontalVelocity.magnitude;

        if (speed > speedThreshold)
        {
            bool isRunning = speed > 5.5f; // можно откорректировать порог
            float amplitudeMul = isRunning ? runAmplitudeMultiplier : walkAmplitudeMultiplier;
            float speedMul = isRunning ? runSpeedMultiplier : walkSpeedMultiplier;

            timer += Time.deltaTime * bobbingSpeed * speed * speedMul;

            float sinOffset = Mathf.Sin(timer) * bobbingAmount * amplitudeMul;
            camTransform.localPosition = new Vector3(
                originalLocalPos.x,
                originalLocalPos.y + sinOffset,
                originalLocalPos.z
            );
        }
        else
        {
            timer = 0f;
            Vector3 currentPos = camTransform.localPosition;
            camTransform.localPosition = new Vector3(
                currentPos.x,
                Mathf.Lerp(currentPos.y, originalLocalPos.y, Time.deltaTime * bobbingSpeed),
                currentPos.z
            );
        }
    }
}
