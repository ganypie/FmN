using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public GameObject rainSystems;
    public GameObject fogSystem;

    public enum WeatherType { Clear, Rain, Fog }
    public WeatherType currentWeather = WeatherType.Clear;

    void Start()
    {
        // Ensure visual and audio states are initialized on scene load
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
}
