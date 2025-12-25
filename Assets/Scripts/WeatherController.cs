using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    public GameObject rainSystems;
    public GameObject fogSystem;

    public enum WeatherType { Clear, Rain, Fog }
    public WeatherType currentWeather = WeatherType.Clear;

    private Coroutine rainRoutine;

    void Start()
    {
        // Инициализируем погодные системы
        SetWeather(currentWeather);
    }

    void UpdateWeather()
    {
        // Выключаем всё
        if (rainSystems != null) rainSystems.SetActive(false);
        if (fogSystem != null) fogSystem.SetActive(false);

        // Включаем выбранное
        switch (currentWeather)
        {
            case WeatherType.Rain:
                if (rainSystems != null) rainSystems.SetActive(true);
                break;
            case WeatherType.Fog:
                if (fogSystem != null) fogSystem.SetActive(true);
                break;
            case WeatherType.Clear:
            default:
                // ничего не включаем
                break;
        }
    }

    public void SetWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;
        UpdateWeather();
    }

    /// <summary>
    /// Плавно включает систему дождя, включая адаптер звука через WeatherAudioAdapter.
    /// </summary>
    /// <param name="duration">Время на fade-in дождя</param>
    public IEnumerator EnableRainSystemSmooth(float duration = 2f)
    {
        if (rainSystems == null) yield break;

        // Включаем объект дождя, но интенсивность постепенно растёт
        rainSystems.SetActive(true);

        float elapsed = 0f;

        // Если есть адаптер, плавно меняем интенсивность от 0 до текущей
        float startIntensity = 0f;
        float targetIntensity = 1f; // можно брать из адаптера rainIntensity
        if (FindObjectOfType<Game.Audio.WeatherAudioAdapter>() != null)
        {
            var adapter = FindObjectOfType<Game.Audio.WeatherAudioAdapter>();
            startIntensity = 0f;
            targetIntensity = adapter.rainIntensity;
            adapter.weatherController.currentWeather = WeatherType.Rain; // ставим состояние
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Плавная подача громкости через адаптер
            if (FindObjectOfType<Game.Audio.WeatherAudioAdapter>() != null)
            {
                var adapter = FindObjectOfType<Game.Audio.WeatherAudioAdapter>();
                adapter.ApplyRainIntensity(Mathf.Lerp(startIntensity, targetIntensity, t));
            }

            yield return null;
        }
    }

    /// <summary>
    /// Плавно выключает дождь
    /// </summary>
    /// <param name="duration"></param>
    public IEnumerator DisableRainSystemSmooth(float duration = 2f)
    {
        if (rainSystems == null) yield break;

        float elapsed = 0f;

        float startIntensity = 1f;
        float targetIntensity = 0f;

        if (FindObjectOfType<Game.Audio.WeatherAudioAdapter>() != null)
        {
            var adapter = FindObjectOfType<Game.Audio.WeatherAudioAdapter>();
            startIntensity = adapter.rainIntensity;
            targetIntensity = 0f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (FindObjectOfType<Game.Audio.WeatherAudioAdapter>() != null)
            {
                var adapter = FindObjectOfType<Game.Audio.WeatherAudioAdapter>();
                adapter.ApplyRainIntensity(Mathf.Lerp(startIntensity, targetIntensity, t));
            }

            yield return null;
        }

        rainSystems.SetActive(false);
    }
}
