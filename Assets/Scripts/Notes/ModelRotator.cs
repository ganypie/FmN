using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 200f; // скорость вращения (градусы/сек)
    public bool invertY = false;

    private Transform target;

    public void SetTargetTransform(Transform t)
    {
        target = t;
    }

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        if (target == null) return;

        // Левая кнопка мыши — вращение
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float x = delta.x;
            float y = delta.y * (invertY ? -1f : 1f);

            // Применяем вращение в локальных координатах
            target.Rotate(Vector3.up, -x * rotationSpeed * Time.unscaledDeltaTime, Space.World);
            target.Rotate(Vector3.right, y * rotationSpeed * Time.unscaledDeltaTime, Space.Self);

            lastMousePosition = Input.mousePosition;
        }
    }
}
