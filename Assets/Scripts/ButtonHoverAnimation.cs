using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private bool animateColor = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioSource audioSource;

    private Vector3 originalScale;
    private Button button;
    private Text buttonText;
    private Image buttonImage;
    private Coroutine currentAnimation;

    void Awake()
    {
        // Cache components
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<Text>();
        buttonImage = GetComponent<Image>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Set initial color
        if (animateColor && buttonText != null)
            buttonText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        // Stop any existing animation
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // Start hover animation
        currentAnimation = StartCoroutine(AnimateToScale(originalScale * hoverScale, hoverColor));

        // Play hover sound
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop any existing animation
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // Start exit animation
        currentAnimation = StartCoroutine(AnimateToScale(originalScale, normalColor));
    }

    private IEnumerator AnimateToScale(Vector3 targetScale, Color targetColor)
    {
        Vector3 startScale = transform.localScale;
        Color startColor = animateColor && buttonText != null ? buttonText.color : normalColor;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work during pause
            float t = elapsed / animationDuration;
            float easedT = easeCurve.Evaluate(t);
            
            // Animate scale
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            
            // Animate color
            if (animateColor && buttonText != null)
            {
                buttonText.color = Color.Lerp(startColor, targetColor, easedT);
            }
            
            yield return null;
        }
        
        // Ensure final values are set
        transform.localScale = targetScale;
        if (animateColor && buttonText != null)
            buttonText.color = targetColor;
        
        currentAnimation = null;
    }

    // Method to play click sound (call this from button's OnClick event)
    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    // Method to reset button state (useful when button becomes disabled)
    public void ResetButtonState()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        transform.localScale = originalScale;
        
        if (animateColor && buttonText != null)
            buttonText.color = normalColor;
    }

    void OnDestroy()
    {
        // Clean up coroutines
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
    }
}