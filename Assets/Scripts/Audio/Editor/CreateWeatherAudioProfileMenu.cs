using UnityEditor;
using UnityEngine;

namespace Game.Audio.Editor
{
    /// <summary>
    /// Fallback menu to create WeatherAudioProfile via Assets/Create.
    /// </summary>
    public static class CreateWeatherAudioProfileMenu
    {
        [MenuItem("Assets/Create/Audio/Weather Audio Profile", false, 2000)]
        public static void CreateProfile()
        {
            var asset = ScriptableObject.CreateInstance<Game.Audio.WeatherAudioProfile>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) path = "Assets";
            if (!AssetDatabase.IsValidFolder(path)) path = System.IO.Path.GetDirectoryName(path).Replace('\\','/');
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/WeatherAudioProfile.asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}


