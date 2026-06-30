using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    // Variável global que os outros scripts vão consultar
    public static bool IsPaused { get; private set; } = false;

    [Header("UI")]
    public GameObject pauseMenu;

    [Header("Confirmation")]
    public GameObject confirmPanel;

    [Header("Controls")]
    public GameObject controlsPanel;

    [Header("Settings")]
    public GameObject settingsPanel;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fullscreenToggle;

    private bool isPaused = false;

    void Start()
    {
        pauseMenu.SetActive(false);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Valores iniciais
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioListener.volume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 1f;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = 1f;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (controlsPanel != null && controlsPanel.activeSelf)
            {
                CloseControls();
                return;
            }

            if (confirmPanel != null && confirmPanel.activeSelf)
            {
                CloseConfirmation();
                return;
            }

            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        IsPaused = isPaused;

        pauseMenu.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (confirmPanel != null)
                confirmPanel.SetActive(false);

            if (controlsPanel != null)
                controlsPanel.SetActive(false);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
    }

    // ===========================
    // MENU PRINCIPAL
    // ===========================

    public void OpenConfirmation()
    {
        confirmPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        confirmPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene("MenuPrincipal");
    }

    // ===========================
    // CONTROLES
    // ===========================

    public void OpenControls()
    {
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);
    }

    // ===========================
    // CONFIGURAÇÕES
    // ===========================

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void SetMusicVolume(float value)
    {
        Debug.Log("Volume da música: " + value);
    }

    public void SetSFXVolume(float value)
    {
        Debug.Log("Volume dos efeitos: " + value);
    }

    public void ToggleFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}