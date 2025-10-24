using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotesInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button closeButton;

    [Header("List / Item")]
    [SerializeField] private Transform listContent;
    [SerializeField] private GameObject listItemPrefab;

    [Header("Viewer")]
    [SerializeField] private TMP_Text viewerTitleText;
    [SerializeField] private TMP_Text viewerBodyText;
    [SerializeField] private Transform previewSpawnRoot;

    [Header("Pause Settings")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    [SerializeField] private bool disableCameraRotation = true;
    [SerializeField] private string[] cameraRotationScriptNames = {
        "FirstPersonController",
        "MouseLook",
        "PlayerCamera",
        "CameraController"
    };

    [Header("Input")]
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;

    private bool isOpen = false;
    private List<GameObject> pooledListItems = new List<GameObject>();
    private List<MonoBehaviour> temporarilyDisabledScripts = new List<MonoBehaviour>();
    private GameObject currentPreviewInstance;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private float previousTimeScale;
    private bool previousAudioState;

    private void Awake()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(CloseInventory);

        // Подписка на обновления списка из NoteManager
        NoteManager.OnNotesUpdated += RebuildList;

        // Первичная сборка списка заметок
        RebuildList();
    }

    private void OnDestroy()
    {
        NoteManager.OnNotesUpdated -= RebuildList;
    }

    private void Update()
    {
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (isOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        if (isOpen || inventoryPanel == null) return;

        SaveCurrentStates();
        inventoryPanel.SetActive(true);
        isOpen = true;

        RebuildList();
        ApplyPauseState(true);
    }

    public void CloseInventory()
    {
        if (!isOpen || inventoryPanel == null) return;

        isOpen = false;
        ApplyPauseState(false);
        ClearPreview();

        inventoryPanel.SetActive(false);
    }

    private void RebuildList()
    {
        if (listContent == null || listItemPrefab == null || NoteManager.Instance == null)
            return;

        // 1. Отключаем все элементы списка и добавляем их в пул
        for (int i = 0; i < listContent.childCount; i++)
        {
            var child = listContent.GetChild(i).gameObject;
            if (!pooledListItems.Contains(child))
                pooledListItems.Add(child);
            child.SetActive(false);
        }

        // 2. Получаем текущие заметки
        var notes = NoteManager.Instance.GetAllNotes();

        // 3. Создаём или берём из пула элементы списка
        foreach (var note in notes)
        {
            GameObject go;

            if (pooledListItems.Count > 0)
            {
                go = pooledListItems[pooledListItems.Count - 1];
                pooledListItems.RemoveAt(pooledListItems.Count - 1);
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(listItemPrefab);
            }

            // Устанавливаем правильного родителя в Layout
            go.transform.SetParent(listContent, false);

            // Настраиваем элемент
            var itemUI = go.GetComponent<InventoryItemUI>();
            if (itemUI != null)
                itemUI.Setup(note, this);
            else
                Debug.LogWarning("ListItemPrefab не содержит InventoryItemUI!");
        }
    }

    public void OnNoteSelected(NoteData note)
    {
        if (note == null) return;

        // Обновляем текст
        if (viewerTitleText != null) viewerTitleText.text = note.title;
        if (viewerBodyText != null) viewerBodyText.text = note.text;

        // Обновляем превью модели
        ClearPreview();

        if (note.modelPrefab != null && previewSpawnRoot != null)
        {
            currentPreviewInstance = Instantiate(note.modelPrefab, previewSpawnRoot);
            currentPreviewInstance.transform.localPosition = Vector3.zero;
            currentPreviewInstance.transform.localRotation = Quaternion.identity;

            // Добавляем ModelRotator, если нет
            if (currentPreviewInstance.GetComponent<ModelRotator>() == null)
            {
                var rotator = currentPreviewInstance.AddComponent<ModelRotator>();
                rotator.SetTargetTransform(currentPreviewInstance.transform);
            }
        }
    }

    private void ClearPreview()
    {
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }
    }

    private void SaveCurrentStates()
    {
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        previousTimeScale = Time.timeScale;
        previousAudioState = AudioListener.pause;
    }

    private void ApplyPauseState(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            temporarilyDisabledScripts.Clear();

            if (scriptsToDisable != null)
            {
                foreach (var script in scriptsToDisable)
                {
                    if (script != null && script.enabled)
                    {
                        script.enabled = false;
                        temporarilyDisabledScripts.Add(script);
                    }
                }
            }

            if (disableCameraRotation)
            {
                foreach (var script in FindCameraControlScripts())
                {
                    if (script != null && script.enabled && !temporarilyDisabledScripts.Contains(script))
                    {
                        script.enabled = false;
                        temporarilyDisabledScripts.Add(script);
                    }
                }
            }
        }
        else
        {
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioState;
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;

            foreach (var script in temporarilyDisabledScripts)
            {
                if (script != null) script.enabled = true;
            }
            temporarilyDisabledScripts.Clear();
        }
    }

    private MonoBehaviour[] FindCameraControlScripts()
    {
        var results = new List<MonoBehaviour>();
        foreach (var script in FindObjectsOfType<MonoBehaviour>())
        {
            foreach (var name in cameraRotationScriptNames)
            {
                if (script.GetType().Name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    results.Add(script);
                    break;
                }
            }
        }
        return results.ToArray();
    }
}
