using UnityEngine;

public class FogManager : MonoBehaviour
{
    [Header("Основные настройки тумана")]
    public bool fogEnabled = false;
    public Color fogColor = Color.gray;
    public FogMode fogMode = FogMode.ExponentialSquared;
    [Range(0.01f, 1f)] public float fogDensity = 0.02f;
    public float fogStartDistance = 0f;
    public float fogEndDistance = 300f;

    void Start()
    {
        ApplyFogSettings();
    }

    /// <summary>
    /// Применить текущие настройки к RenderSettings
    /// </summary>
    public void ApplyFogSettings()
    {
        RenderSettings.fog = fogEnabled;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = fogMode;

        if (fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
        else
        {
            RenderSettings.fogDensity = fogDensity;
        }
    }

    /// <summary>
    /// Включить или выключить туман
    /// </summary>
    public void SetFogActive(bool active)
    {
        fogEnabled = active;
        ApplyFogSettings();
    }

    /// <summary>
    /// Плавное включение/выключение тумана (корутина)
    /// </summary>
    public void FadeFog(bool active, float duration = 2f)
    {
        StopAllCoroutines();
        StartCoroutine(FadeFogRoutine(active, duration));
    }

    private System.Collections.IEnumerator FadeFogRoutine(bool active, float duration)
    {
        float startDensity = RenderSettings.fogDensity;
        float targetDensity = active ? fogDensity : 0f;
        float elapsed = 0f;

        RenderSettings.fog = true; // оставляем включённым, пока идёт анимация

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            RenderSettings.fogDensity = Mathf.Lerp(startDensity, targetDensity, t);
            yield return null;
        }

        if (!active) RenderSettings.fog = false;
    }
}
