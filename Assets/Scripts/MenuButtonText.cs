using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class MenuButtonText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text target (auto-find if empty)")]
    public TMP_Text textComponent;

    [Header("Glitch fonts (index 0 is not required but will be used as fallback/original)")]
    public TMP_FontAsset[] glitchFonts;

    [Header("Glitch settings")]
    [Tooltip("How strong letters shake (in local units, ~0.5 - 3 recommended)")]
    public float shakeIntensity = 1.5f;

    [Tooltip("How often to change font / shake (seconds). Lower = more frantic")]
    public float fontChangeInterval = 0.08f;

    private TMP_FontAsset originalFont;
    private Coroutine glitchRoutine;

    void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponentInChildren<TMP_Text>();

        if (textComponent == null)
        {
            Debug.LogError("[MenuButtonText] TMP_Text not found on or inside this GameObject.", this);
            enabled = false;
            return;
        }

        originalFont = textComponent.font;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        glitchRoutine = StartCoroutine(GlitchLoop());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);
        glitchRoutine = null;
        ResetTextImmediately();
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            // случайный шрифт (если есть)
            if (glitchFonts != null && glitchFonts.Length > 0)
            {
                int idx = Random.Range(0, glitchFonts.Length);
                // Присваиваем новый TMP_FontAsset
                textComponent.font = glitchFonts[idx];
            }

            // форсируем обновление текста и достаём текстовую инфо
            textComponent.ForceMeshUpdate();
            TMP_TextInfo textInfo = textComponent.textInfo;

            // Копируем исходные вершины для каждого меша (чтобы не наслаивать смещения)
            int meshCount = textInfo.meshInfo.Length;
            Vector3[][] sourceVertices = new Vector3[meshCount][];
            for (int m = 0; m < meshCount; m++)
            {
                var verts = textInfo.meshInfo[m].vertices;
                sourceVertices[m] = (Vector3[])verts.Clone(); // клонируем текущие вершины как "чистый" базовый
            }

            // Применяем случайные смещения к каждой видимой букве, записывая поверх клонированного массива
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var cInfo = textInfo.characterInfo[i];
                if (!cInfo.isVisible) continue;

                int matIndex = cInfo.materialReferenceIndex;
                int vertIndex = cInfo.vertexIndex;

                // смещение для буквы (рандом в квадрате интенсивности)
                Vector3 offset = new Vector3(
                    Random.Range(-shakeIntensity, shakeIntensity),
                    Random.Range(-shakeIntensity, shakeIntensity),
                    0f
                );

                // Берём оригинальные вершины для этого меша (sourceVertices[matIndex]) и записываем изменённые значения в textInfo.meshInfo[matIndex].vertices
                for (int j = 0; j < 4; j++)
                {
                    // sourceVertices уже содержит значения, соответствующие vertexIndex + j
                    textInfo.meshInfo[matIndex].vertices[vertIndex + j] = sourceVertices[matIndex][vertIndex + j] + offset;
                }
            }

            // Обновляем меши в текстовом объекте
            for (int m = 0; m < meshCount; m++)
            {
                var meshInfo = textInfo.meshInfo[m];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textComponent.UpdateGeometry(meshInfo.mesh, m);
            }

            yield return new WaitForSeconds(fontChangeInterval);
        }
    }

    private void ResetTextImmediately()
    {
        // Возвращаем оригинальный шрифт
        if (originalFont != null)
            textComponent.font = originalFont;

        // Форсируем обновление — это восстановит вершины в исходное состояние
        textComponent.ForceMeshUpdate();
    }
}
