using UnityEngine;

// Скрипт для передачи параметров выбранного спотлайта в материал тумана
// Гарантирует синхронизацию с активным материалом
public class FogFlashlight : MonoBehaviour
{
    [Header("Fog Material")]
    public Renderer fogRenderer; // Renderer, на котором материал тумана
    public Material fogMaterial; // Если используется sharedMaterial, fogRenderer будет приоритетнее

    [Header("Spotlight (фонарик)")]
    public Light spotlight; // Можно указать вручную или найти автоматически

    [Header("Автоматический поиск фонарика игрока")]
    public string flashlightTag = "PlayerFlashlight"; // Тег для поиска фонарика

    void Start()
    {
        // Если фонарик не указан вручную, ищем по тегу
        if (spotlight == null && !string.IsNullOrEmpty(flashlightTag))
        {
            var go = GameObject.FindWithTag(flashlightTag);
            if (go != null)
            {
                spotlight = go.GetComponent<Light>();
            }
        }
    }

    void Update()
    {
        Material mat = fogMaterial;
        if (fogRenderer != null)
        {
            mat = fogRenderer.material; // Используем экземпляр материала, а не sharedMaterial
        }
        if (mat == null || spotlight == null) return;
        // Передаём параметры фонарика в материал тумана
        mat.SetVector("_SpotLightPos", spotlight.transform.position);
        mat.SetVector("_SpotLightDir", spotlight.transform.forward);
        mat.SetColor("_SpotLightColor", spotlight.color);
        mat.SetFloat("_SpotLightIntensity", spotlight.intensity);
        mat.SetFloat("_SpotLightAngle", Mathf.Cos(spotlight.spotAngle * 0.5f * Mathf.Deg2Rad));
    }
}