using UnityEngine;

public class CrosshairAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;       // CanvasGroup на объекте
    [SerializeField] private RectTransform rectTransform;   // RectTransform объекта
    [SerializeField] private float animationSpeed = 5f;     // скорость появления/исчезновения

    private bool visible = false;

    // Вызвать, когда нужно показать кроссхейр
    public void Show() => visible = true;

    // Вызвать, когда нужно скрыть кроссхейр
    public void Hide() => visible = false;

   void Update()
    {
        float targetScale = visible ? 1f : 0f;
        float currentScale = rectTransform.localScale.x;
        currentScale = Mathf.MoveTowards(currentScale, targetScale, Time.deltaTime * animationSpeed);
        rectTransform.localScale = new Vector3(currentScale, currentScale, 1f);

        float targetAlpha = visible ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * animationSpeed);
    }
}
