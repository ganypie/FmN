using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private bool isPaused = false;

    public static PauseManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure pause menu is hidden at start
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Ensure cursor is locked at start (for FPS gameplay)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the world completely
        AudioListener.pause = true; // Pause audio DSP for non-OneShot sustain
        Cursor.lockState = CursorLockMode.None; // Unlock cursor for menu interaction
        Cursor.visible = true;

        if (pauseMenuUI != null)
        {
            Debug.Log("Showing pause menu UI: " + pauseMenuUI.name);
            pauseMenuUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PauseMenuUI is null! Cannot show pause menu.");
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume normal time
        AudioListener.pause = false; // Resume audio
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor back to center
        Cursor.visible = false;

        // Hide pause menu and any settings panels
        if (pauseMenuUI != null)
        {
            Debug.Log("Hiding pause menu UI: " + pauseMenuUI.name);
            pauseMenuUI.SetActive(false);

            // Also hide settings panel if it exists
            var pauseMenuUIComponent = pauseMenuUI.GetComponent<PauseMenuUI>();
            if (pauseMenuUIComponent != null)
            {
                pauseMenuUIComponent.HideSettingsPanel();
            }
        }
        else
        {
            Debug.LogWarning("PauseMenuUI is null! Cannot hide pause menu.");
        }
    }

    public void OpenSettings()
    {
        // This will be handled by PauseMenuUI
        Debug.Log("Opening Settings...");
    }

    public void QuitToMainMenu()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // Public getter for other scripts to check pause state
    public bool IsPaused => isPaused;
}



