using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance { get; private set; }

    [Header("Debug / Info")]
    [SerializeField] private List<NoteData> collectedNotes = new List<NoteData>();

    // Событие для UI (чтобы обновить список)
    public delegate void NotesUpdated();
    public static event NotesUpdated OnNotesUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Добавляет новую записку, если её ещё нет в списке.
    /// </summary>
    public void AddNote(NoteData note)
    {
        if (note == null) return;

        if (!collectedNotes.Contains(note))
        {
            collectedNotes.Add(note);
            OnNotesUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Проверяет, найдена ли уже записка.
    /// </summary>
    public bool HasNote(NoteData note)
    {
        return collectedNotes.Contains(note);
    }

    /// <summary>
    /// Возвращает копию списка найденных записок.
    /// </summary>
    public List<NoteData> GetAllNotes()
    {
        return new List<NoteData>(collectedNotes);
    }

    /// <summary>
    /// Очистка списка (для тестов или новой игры).
    /// </summary>
    public void ClearAll()
    {
        collectedNotes.Clear();
        OnNotesUpdated?.Invoke();
    }

    // --- Для интеграции со старым NoteInteractable ---
    public static void NotifyCollected(NoteData note)
    {
        if (Instance == null)
        {
            Debug.LogWarning("NoteManager instance not found in scene!");
            return;
        }

        Instance.AddNote(note);
    }
}
