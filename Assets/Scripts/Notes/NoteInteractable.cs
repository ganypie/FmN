using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoteInteractable : MonoBehaviour, IInteractable
{
    public static event System.Action<NoteData> OnNoteInteracted;

    [Header("Data")]
    [SerializeField] private NoteData noteData;

    [Header("Behaviour")]
    [SerializeField] private bool openOnInteract = true;
    [SerializeField] private bool removeOnPickup;
    [SerializeField] private float interactionDistance = 2f;

    private NoteUIManager uiManager;

    private void Start()
    {
        uiManager = Object.FindFirstObjectByType<NoteUIManager>();
        if (uiManager == null)
            Debug.LogWarning($"NoteUIManager не найден на сцене для {gameObject.name}!");
    }

    private void Reset()
    {
        if (noteData != null) removeOnPickup = noteData.removeOnPickup;
    }

    // IInteractable
    public void Interact(Transform interactor)
    {
        OnInteract();
    }

    public bool CanInteract(Transform interactor)
    {
        if (interactor == null) return false;
        return Vector3.Distance(transform.position, interactor.position) <= interactionDistance;
    }

    // Основной метод взаимодействия
    public void OnInteract()
    {
        if (noteData == null)
        {
            Debug.LogWarning($"NoteInteractable на объекте {gameObject.name} не содержит NoteData.");
            return;
        }

        if (openOnInteract && uiManager != null)
        {
            uiManager.Open(noteData);
        }

        // Вызываем событие взаимодействия с запиской
        OnNoteInteracted?.Invoke(noteData);

        // Добавляем запись в менеджер собранных заметок
        NoteManager.NotifyCollected(noteData);

        // Удаляем объект со сцены при необходимости
        bool shouldRemove = removeOnPickup || noteData.removeOnPickup;
        if (shouldRemove)
        {
            // Используем Destroy вместо SetActive(false)
            Destroy(gameObject);
        }
    }
}
