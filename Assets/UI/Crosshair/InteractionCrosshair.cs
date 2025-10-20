using UnityEngine;

public class InteractionCrosshair : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;           // если оставить пустым, попробуем Camera.main
    public CanvasGroup crosshairGroup;    // перетащи сюда CanvasGroup от твоего UI-Image

    [Header("Interaction")]
    public string interactTag = "Interactable";
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0;   // по умолчанию "Everything" — можно сузить для скорости

    [Header("Appearance")]
    public float fadeSpeed = 8f;          // чем выше — тем быстрее появление/исчезновение

    // внутренний
    float targetAlpha = 0f;
    private Ray cachedRay;
    private RaycastHit cachedHit;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (crosshairGroup == null)
            Debug.LogError("[InteractionCrosshair] Assign crosshairGroup in inspector!");

        // Начинаем скрытым
        if (crosshairGroup != null)
        {
            crosshairGroup.alpha = 0f;
            crosshairGroup.interactable = false;
            crosshairGroup.blocksRaycasts = false;
        }
    }

    void Start()
    {
        // no-op, left for future initialization if needed

    }

    void Update()
    {
        bool show = false;

        if (playerCamera != null)
        {
            Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            cachedRay = playerCamera.ScreenPointToRay(screenCenter);

            // Используем маску слоёв для оптимизации (если нужно)
            if (Physics.Raycast(cachedRay, out cachedHit, interactDistance, interactMask))
            {
                Collider c = cachedHit.collider;
                if (c != null)
                {
                    // проверка по тегу (и на случай, если tag на родительском объекте)
                    if (c.CompareTag(interactTag) || c.transform.root.CompareTag(interactTag))
                    {
                        show = true;
                    }
                    // опционально: можно также проверить наличие компонента Interactable:
                    // else if (c.GetComponent<Interactable>() != null) show = true;
                }
            }
        }

        targetAlpha = show ? 1f : 0f;

        // Плавное появление/исчезновение (линейно)
        if (crosshairGroup != null)
        {
            crosshairGroup.alpha = Mathf.MoveTowards(crosshairGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }
}
