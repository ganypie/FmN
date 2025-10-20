using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backToPauseButton;

    [Header("Settings Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;

    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = PauseManager.Instance;
        
        // Setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        if (backToPauseButton != null)
            backToPauseButton.onClick.AddListener(OnBackToPauseClicked);

        // Setup settings controls
        SetupSettingsControls();
        
        // Hide settings panel initially
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetupSettingsControls()
    {
        // Volume sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        // Fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
        }

        // Resolution dropdown
        if (resolutionDropdown != null)
        {
            SetupResolutionDropdown();
        }
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        
        // Get available resolutions
        Resolution[] resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            resolutionDropdown.options.Add(new Dropdown.OptionData(option));
            
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    // Button event handlers
    private void OnContinueClicked()
    {
        if (pauseManager != null)
            pauseManager.ResumeGame();
    }

    private void OnSettingsClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
        if (pauseManager != null)
            pauseManager.QuitToMainMenu();
    }

    private void OnBackToPauseClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Settings event handlers
    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        AudioListener.volume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        // Apply to music audio sources here
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        // Apply to SFX audio sources here
    }

    private void OnFullscreenToggled(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    private void OnResolutionChanged(int resolutionIndex)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (resolutionIndex >= 0 && resolutionIndex < resolutions.Length)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            PlayerPrefs.SetInt("ResolutionWidth", resolution.width);
            PlayerPrefs.SetInt("ResolutionHeight", resolution.height);
        }
    }

    void OnDestroy()
    {
        // Save settings when menu is destroyed
        PlayerPrefs.Save();
    }
}
