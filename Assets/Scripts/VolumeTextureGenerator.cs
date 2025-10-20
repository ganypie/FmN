using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Генерирует 3D текстуру из обычной 2D текстуры шума
/// </summary>
public class Texture2DTo3D : MonoBehaviour
{
    [Header("Входная текстура")]
    public Texture2D sourceTexture;

    [Header("Настройки 3D текстуры")]
    public int size = 32; // размер куба
    public string assetPath = "Assets/Textures3D/VolumeFrom2D.asset";

    public Texture3D volumeTexture;

    void Start()
    {
        if (sourceTexture == null)
        {
            Debug.LogError("Не задана исходная Texture2D!");
            return;
        }

        volumeTexture = Generate3DTextureFrom2D(sourceTexture, size);
        SaveTextureAsset();
    }

    Texture3D Generate3DTextureFrom2D(Texture2D tex2D, int size)
    {
        Texture3D tex3D = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size * size];

        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Координаты в 2D (с повторением)
                    float u = (x / (float)size) * tex2D.width;
                    float v = (y / (float)size) * tex2D.height;

                    // Сдвиг по Z
                    int shiftX = (int)(z * tex2D.width / (float)size) % tex2D.width;

                    Color c = tex2D.GetPixel((int)u + shiftX, (int)v);
                    colors[x + y * size + z * size * size] = c;
                }
            }
        }

        tex3D.SetPixels(colors);
        tex3D.Apply();
        return tex3D;
    }

    void SaveTextureAsset()
    {
#if UNITY_EDITOR
        if (!System.IO.Directory.Exists("Assets/Textures3D"))
            System.IO.Directory.CreateDirectory("Assets/Textures3D");

        AssetDatabase.CreateAsset(volumeTexture, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("3D-текстура из 2D сохранена как Asset: " + assetPath);
#endif
    }
}
