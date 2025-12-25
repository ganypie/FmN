using UnityEngine;
using System.Collections;
using Game.Audio;

public class StartSequenceController : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup blackScreen;

    [Header("Audio")]
    public AudioSource knockSource;

    [Header("Weather")]
    public WeatherController weatherController;
    public WeatherAudioAdapter weatherAudioAdapter;

    [Header("Timings")]
    public float rainDelay = 3f;
    public float knockInitialDelay = 3f;
    public float knockInterval = 4f;
    public float blackScreenFadeTime = 1.5f;
    public float openEyesDelay = 0.5f;

    [Header("Player")]
    public GameObject playerController;

    private bool doorOpened = false;

    private void Start()
    {
        // ВКЛЮЧАЕМ ОБЪЕКТ ЧЁРНОГО ЭКРАНА, ЕСЛИ ОН БЫЛ ВЫКЛЮЧЕН
        if (blackScreen != null && !blackScreen.gameObject.activeSelf)
            blackScreen.gameObject.SetActive(true);

        // ГАРАНТИЯ: чёрный экран непрозрачен при старте
        if (blackScreen != null)
            blackScreen.alpha = 1f;

        // Гарантия: дождь НЕ звучит с самого начала
        if (weatherAudioAdapter != null)
            weatherAudioAdapter.ApplyRainIntensity(0f);

        // Гарантия: дождя визуально тоже нет
        if (weatherController != null)
            weatherController.SetWeather(WeatherController.WeatherType.Clear);

        if (GameState.StartedNewGame)
        {
            StartCoroutine(GameIntroSequence());
            GameState.StartedNewGame = false;
        }
    }


    private IEnumerator GameIntroSequence()
    {
        // Чёрный экран уже включён в сцене → ничего не трогаем

        // --- 1. ЖДЁМ ДО ДЕШДЯ ---
        yield return new WaitForSeconds(rainDelay);

        // Включаем визуальный дождь
        if (weatherController != null)
            weatherController.SetWeather(WeatherController.WeatherType.Rain);

        // Включаем звук дождя (плавный рост интенсивности от 0 до 1 за 2 сек)
        if (weatherAudioAdapter != null)
            yield return StartCoroutine(FadeInRainSound(2f));


        // --- 2. ЖДЁМ ДО СТУКА ---
        yield return new WaitForSeconds(knockInitialDelay);

        // Стук начинается раньше открытия глаз
        StartCoroutine(PlayKnockUntilDoorOpens());


        // --- 3. ОТКРЫВАЕМ ГЛАЗА ---
        yield return StartCoroutine(FadeOutBlackScreen());
        yield return new WaitForSeconds(openEyesDelay);

        // Активируем контроллер игрока
        if (playerController != null)
            playerController.SetActive(true);
    }

    private IEnumerator FadeInRainSound(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Clamp01(t / duration);

            if (weatherAudioAdapter != null)
                weatherAudioAdapter.ApplyRainIntensity(v);

            yield return null;
        }

        if (weatherAudioAdapter != null)
            weatherAudioAdapter.ApplyRainIntensity(1f);
    }

    private IEnumerator PlayKnockUntilDoorOpens()
    {
        while (!doorOpened)
        {
            if (knockSource != null)
                knockSource.Play();

            yield return new WaitForSeconds(knockInterval);
        }

        if (knockSource != null)
            knockSource.Stop();
    }

    public void OnDoorOpened()
    {
        doorOpened = true;
    }

    private IEnumerator FadeOutBlackScreen()
    {
        float t = 0f;
        float startAlpha = blackScreen.alpha;

        while (t < blackScreenFadeTime)
        {
            t += Time.deltaTime;
            blackScreen.alpha = Mathf.Lerp(startAlpha, 0f, t / blackScreenFadeTime);
            yield return null;
        }

        blackScreen.alpha = 0f;
    }
}
