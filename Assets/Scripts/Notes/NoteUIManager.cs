using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;

public class NoteUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject notePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private KeyCode closeKey = KeyCode.E;
    [SerializeField] private float fadeSpeed = 5f;
    
    [Header("Freeze Settings")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private bool disableCameraRotation = true; // Новая опция для отключения вращения камеры
    
    [Header("Camera Control")]
    [SerializeField] private string[] cameraRotationScriptNames = {
        "FirstPersonController",
        "MouseLook",
        "PlayerCamera",
        "CameraController"
    }; // Типичные имена скриптов управления камерой

    // Состояния UI
    private bool isOpen = false;
    private bool isTransitioning = false;
    private List<MonoBehaviour> temporarilyDisabledScripts = new List<MonoBehaviour>();
    
    // Сохраняем предыдущие состояния
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private float previousTimeScale;
    private bool previousAudioState;

    private void Awake()
    {
        ValidateComponents();
        InitializeUI();
    }

    private void ValidateComponents()
    {
        if (notePanel == null)
            Debug.LogError($"[{nameof(NoteUIManager)}] Note panel is not assigned!");
        if (titleText == null)
            Debug.LogError($"[{nameof(NoteUIManager)}] Title text is not assigned!");
        if (bodyText == null)
            Debug.LogError($"[{nameof(NoteUIManager)}] Body text is not assigned!");
        if (closeButton == null)
            Debug.LogError($"[{nameof(NoteUIManager)}] Close button is not assigned!");
        if (panelCanvasGroup == null && notePanel != null)
            panelCanvasGroup = notePanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            Debug.LogError($"[{nameof(NoteUIManager)}] CanvasGroup component is missing on the note panel!");
    }

    private void InitializeUI()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
        }
        
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void Update()
    {
        if (isTransitioning)
            UpdateTransition();
            
        if (isOpen && Input.GetKeyDown(closeKey))
            Close();
    }

    private void UpdateTransition()
    {
        if (panelCanvasGroup == null) return;

        float targetAlpha = isOpen ? 1f : 0f;
        panelCanvasGroup.alpha = Mathf.MoveTowards(panelCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        
        // Обновляем интерактивность UI вместе с прозрачностью
        panelCanvasGroup.interactable = isOpen;
        panelCanvasGroup.blocksRaycasts = isOpen;

        if (Mathf.Approximately(panelCanvasGroup.alpha, targetAlpha))
        {
            isTransitioning = false;
            if (!isOpen)
            {
                notePanel.SetActive(false);
                panelCanvasGroup.alpha = 0f; // Гарантируем сброс прозрачности
            }
        }
    }

    public void Open(NoteData note)
    {
        if (notePanel == null || note == null) return;
        if (isOpen || isTransitioning) return;

        // Сохраняем текущие состояния
        SaveCurrentStates();

        // Заполняем UI
        titleText.text = note.title;
        bodyText.text = note.text;
        notePanel.SetActive(true);
        
        // Запускаем анимацию появления
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            isTransitioning = true;
        }
        
        isOpen = true;

        // Применяем паузу
        ApplyPauseState(true);
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
            // Применяем паузу
            Time.timeScale = 0f;
            AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Отключаем скрипты
            temporarilyDisabledScripts.Clear();
            
            // Отключаем указанные скрипты
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

            // Дополнительно ищем и отключаем скрипты управления камерой
            if (disableCameraRotation)
            {
                var cameraScripts = FindCameraControlScripts();
                foreach (var script in cameraScripts)
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
            // Восстанавливаем предыдущие состояния
            Time.timeScale = previousTimeScale;
            AudioListener.pause = previousAudioState;
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;

            // Включаем обратно все временно отключенные скрипты
            foreach (var script in temporarilyDisabledScripts)
            {
                if (script != null)
                    script.enabled = true;
            }
            temporarilyDisabledScripts.Clear();
        }
    }

    private MonoBehaviour[] FindCameraControlScripts()
    {
        var results = new List<MonoBehaviour>();
        var allScripts = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var script in allScripts)
        {
            // Проверяем, содержит ли имя скрипта одно из известных названий скриптов камеры
            if (script != null && cameraRotationScriptNames.Any(name => 
                script.GetType().Name.Contains(name, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(script);
            }
        }

        return results.ToArray();
    }

    public void Close()
    {
        if (notePanel == null || !isOpen) return;
        if (isTransitioning) return;

        isOpen = false;
        isTransitioning = true;

        // Снимаем паузу (делаем это до начала анимации)
        ApplyPauseState(false);
        
        if (panelCanvasGroup == null)
        {
            // Если нет CanvasGroup, закрываем мгновенно
            notePanel.SetActive(false);
            isTransitioning = false;
        }
    }

    private void OnDisable()
    {
        // На всякий случай восстанавливаем состояние при отключении компонента
        if (isOpen)
            ApplyPauseState(false);
    }
}
