using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backToPauseButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio / Display Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown resolutionDropdown;

    private PauseManager pauseManager;

    void Start()
    {
        pauseManager = PauseManager.Instance;

        if (continueButton != null) continueButton.onClick.AddListener(() => pauseManager.ResumeGame());
        if (settingsButton != null) settingsButton.onClick.AddListener(() => ShowSettings());
        if (quitButton != null) quitButton.onClick.AddListener(() => pauseManager.QuitToMainMenu());
        if (backToPauseButton != null) backToPauseButton.onClick.AddListener(() => HideSettingsPanel());

        SetupSettings();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void SetupSettings()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(val =>
            {
                PlayerPrefs.SetFloat("MasterVolume", val);
                AudioListener.volume = val;
            });
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("MusicVolume", val));
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat("SFXVolume", val));
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(val =>
            {
                Screen.fullScreen = val;
                PlayerPrefs.SetInt("Fullscreen", val ? 1 : 0);
            });
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            int currentIndex = 0;
            var resolutions = Screen.resolutions;
            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add(resolutions[i].width + " x " + resolutions[i].height);
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currentIndex = i;
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentIndex;
            resolutionDropdown.RefreshShownValue();
            resolutionDropdown.onValueChanged.AddListener(i =>
            {
                var r = resolutions[i];
                Screen.SetResolution(r.width, r.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionWidth", r.width);
                PlayerPrefs.SetInt("ResolutionHeight", r.height);
            });
        }
    }

    public void ShowSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
