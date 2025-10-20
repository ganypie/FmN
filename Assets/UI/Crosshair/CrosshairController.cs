using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("Player & Camera")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    [Header("Crosshair")]
    [SerializeField] private CrosshairAnimator crosshairAnimator;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;    // Оставляем для обратной совместимости
    [SerializeField] private LayerMask flashlightLayer;  // Слой для фонарика
    
    private LayerMask combinedLayers; // Комбинированная маска для обоих слоев

    private IInteractable currentInteractable;
    private bool isVisible = false;
    private Ray cachedRay;
    private RaycastHit cachedHit;

    void Update()
    {
        if (playerCamera == null || player == null) return;
        
        // Не обрабатываем взаимодействие во время паузы
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        isVisible = false;
        currentInteractable = null;

        // Луч из центра камеры
        cachedRay.origin = playerCamera.transform.position;
        cachedRay.direction = playerCamera.transform.forward;

        combinedLayers = interactLayer | flashlightLayer;  // Объединяем маски слоев
        if (Physics.Raycast(cachedRay, out cachedHit, interactDistance, combinedLayers))
        {
            IInteractable interactable = cachedHit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract(player))
            {
                isVisible = true;
                currentInteractable = interactable;
            }
        }

        // Управляем кроссхейром
        if (crosshairAnimator != null)
        {
            if (isVisible)
                crosshairAnimator.Show();
            else
                crosshairAnimator.Hide();
        }

        // Если наведен и нажата E → вызвать Interact
        if (isVisible && Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(player);
        }
    }
}
