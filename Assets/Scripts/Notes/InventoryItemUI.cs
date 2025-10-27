using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI References (can assign any prefab or child)")]
    [SerializeField] private Button button;         // кнопка, визуал можно менять в инспекторе
    [SerializeField] private TMP_Text titleText;    // текст заголовка, можно любой TMP_Text
    [SerializeField] private Image iconImage;       // иконка, можно пропустить

    private NoteData noteData;
    private NotesInventoryUI parentUI;

    private void Awake()
    {
        // Автоматически ищем компоненты, если не назначены
        if (button == null)
            button = GetComponentInChildren<Button>();

        if (titleText == null)
            titleText = GetComponentInChildren<TMP_Text>();

        if (iconImage == null)
            iconImage = GetComponentInChildren<Image>();

        // Предупреждаем, если чего-то нет
        if (button == null)
            Debug.LogWarning("Button не найден на InventoryItemUI: " + gameObject.name);
        if (titleText == null)
            Debug.LogWarning("TMP_Text не найден на InventoryItemUI: " + gameObject.name);
    }

    public void Setup(NoteData note, NotesInventoryUI parent)
    {
        noteData = note;
        parentUI = parent;

        // Устанавливаем текст заголовка
        if (titleText != null)
            titleText.text = note.title;

        // Устанавливаем иконку
        if (iconImage != null)
        {
            if (note.icon != null)
            {
                iconImage.sprite = note.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // Настраиваем кнопку
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (parentUI != null)
            parentUI.OnNoteSelected(noteData);
    }
}
