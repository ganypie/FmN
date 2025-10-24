using System.Collections.Generic;
using UnityEngine;

public static class NoteManager
{
    private static List<NoteData> collectedNotes = new List<NoteData>();

    public static void NotifyCollected(NoteData note)
    {
        if (!collectedNotes.Contains(note))
        {
            collectedNotes.Add(note);
            Debug.Log($"Записка добавлена в инвентарь: {note.title}");
        }
    }

    public static List<NoteData> GetCollectedNotes()
    {
        return new List<NoteData>(collectedNotes);
    }
}
