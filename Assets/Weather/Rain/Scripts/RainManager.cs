using UnityEngine;

public class RainController : MonoBehaviour
{
    [Header("Основные настройки дождя")]
    public bool rainEnabled = false;
    public GameObject rainRoot; // ссылка на RainGrid или на весь RainManagerScene
    public float emissionRate = 200f; // скорость "интенсивности" дождя
    public float fadeDuration = 2f;   // плавное включение/выключение

    private ParticleSystem[] rainSystems;
    private float[] baseEmissionRates;

    void Awake()
    {
        if (rainRoot == null)
            rainRoot = gameObject;

        // Собираем все ParticleSystem внутри rainRoot
        rainSystems = rainRoot.GetComponentsInChildren<ParticleSystem>(true);
        baseEmissionRates = new float[rainSystems.Length];

        for (int i = 0; i < rainSystems.Length; i++)
        {
            var emission = rainSystems[i].emission;
            baseEmissionRates[i] = emission.rateOverTime.constant;
        }

        ApplyRainSettings();
    }

    /// <summary>
    /// Мгновенно включить/выключить дождь
    /// </summary>
    public void SetRainActive(bool active)
    {
        rainEnabled = active;

        foreach (var system in rainSystems)
        {
            var emission = system.emission;
            emission.enabled = active;
        }
    }

    /// <summary>
    /// Плавное включение/выключение дождя
    /// </summary>
    public void FadeRain(bool active)
    {
        StopAllCoroutines();
        StartCoroutine(FadeRainRoutine(active));
    }

    private System.Collections.IEnumerator FadeRainRoutine(bool active)
    {
        float elapsed = 0f;

        // Сохраняем стартовые значения
        float[] startRates = new float[rainSystems.Length];
        for (int i = 0; i < rainSystems.Length; i++)
            startRates[i] = rainSystems[i].emission.rateOverTime.constant;

        float[] targetRates = new float[rainSystems.Length];
        for (int i = 0; i < rainSystems.Length; i++)
            targetRates[i] = active ? baseEmissionRates[i] : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            for (int i = 0; i < rainSystems.Length; i++)
            {
                var emission = rainSystems[i].emission;
                float newRate = Mathf.Lerp(startRates[i], targetRates[i], t);
                emission.rateOverTime = newRate;
            }

            yield return null;
        }

        // Полное выключение emission
        if (!active)
        {
            foreach (var system in rainSystems)
            {
                var emission = system.emission;
                emission.enabled = false;
            }
        }

        rainEnabled = active;
    }

    /// <summary>
    /// Применить текущие настройки
    /// </summary>
    private void ApplyRainSettings()
    {
        SetRainActive(rainEnabled);
    }
}
