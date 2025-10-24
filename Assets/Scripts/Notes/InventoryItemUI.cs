using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image iconImage; // optional

    private NoteData noteData;
    private NotesInventoryUI parentUI;

    public void Setup(NoteData note, NotesInventoryUI parent)
    {
        noteData = note;
        parentUI = parent;

        if (titleText != null)
            titleText.text = note.title;

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
