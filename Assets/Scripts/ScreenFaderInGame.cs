using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFaderInGame : MonoBehaviour
{
    public Image fadeImage;

    [Header("Fade Settings")]
    public float fadeDuration = 5f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private void Start()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / fadeDuration);

            float alpha = fadeCurve.Evaluate(normalizedTime);
            fadeImage.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);
    }

    // Публичный корутин для плавного фейда на чёрный экран
    public IEnumerator FadeOutCoroutine()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / fadeDuration);

            // Используем инвертированную кривую, т.к. FadeIn использует кривую, убывающую от 1 до 0
            float alpha = 1f - fadeCurve.Evaluate(normalizedTime);
            fadeImage.color = new Color(0, 0, 0, alpha);

            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
    }
}
