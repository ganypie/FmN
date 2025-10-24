using UnityEngine;

[CreateAssetMenu(fileName = "NewNote", menuName = "Notes/NoteData")]
public class NoteData : ScriptableObject
{
    [Header("Basic Info")]
    public string title;
    [TextArea(3, 10)]
    public string text;

    [Header("Settings")]
    public bool removeOnPickup = true; // удалять объект со сцены после подбора
}
