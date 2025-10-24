using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "Notes/NoteData")]
public class NoteData : ScriptableObject
{
    [Header("Basic Info")]
    public string title;
    [TextArea(3, 10)]
    public string text;

    [Header("Visuals (optional)")]
    public Sprite icon; // для списка (опционально)
    public GameObject modelPrefab; // 3D модель для превью (опционально)

    [Header("Settings")]
    public bool removeOnPickup = true;
}
