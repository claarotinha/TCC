using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UniversalPauseManager : MonoBehaviour
{
    public static UniversalPauseManager Instance { get; private set; }
    public static bool IsPaused { get; private set; }

    [Header("Menu de pausa")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseWindow;

    [Header("Botões")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button diaryButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Subpainéis")]
    [SerializeField] private GameObject diaryPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Cena do menu principal")]
    [SerializeField] private string mainMenuScene = "MenuPrincipal";

    private readonly List<Button> backButtons = new List<Button>();
    private GameObject currentSubpanel;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this &&
            Instance.gameObject.scene == gameObject.scene)
        {
            Debug.LogError(
                "Existe mais de um UniversalPauseManager ativo.",
                this
            );

            enabled = false;
            return;
        }

        // Uma cena nova assume a pausa; o controlador da cena anterior não
        // pode desabilitar este nem deixar Time.timeScale em zero.
        if (Instance != null && Instance != this) Instance.ResumeGame();
        Instance = this;
        IsPaused = false;
    }

    private void Start()
    {
        if (pausePanel == null || pauseWindow == null)
        {
            Debug.LogError(
                "UniversalPauseManager: preencha Pause Panel " +
                "e Pause Window.",
                this
            );

            enabled = false;
            return;
        }

        IsPaused = false;
        pausePanel.SetActive(false);
        HideSubpanels();
        pauseWindow.SetActive(true);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (diaryButton != null)
            diaryButton.onClick.AddListener(OpenDiary);

        if (soundButton != null)
            soundButton.onClick.AddListener(OpenSound);

        if (controlsButton != null)
            controlsButton.onClick.AddListener(OpenControls);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        BindBackButtons();
        RefreshButtons();
    }

    private void BindBackButtons()
    {
        Canvas canvas = pausePanel.GetComponentInParent<Canvas>(true);
        if (canvas == null) return;
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button.name != "BackDiaryButton" && button.name != "BackSoundButton" &&
                button.name != "BackControlsButton") continue;
            button.onClick.AddListener(BackToPause);
            backButtons.Add(button);
        }
    }

    private void Update()
    {
        if (SleepBed.IsSleeping || HouseEntrance.IsChangingScene)
            return;

        RefreshButtons();

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (IsPaused && currentSubpanel != null)
            BackToPause();
        else
            TogglePause();
    }

    private void RefreshButtons()
    {
        if (diaryButton != null)
        {
            diaryButton.interactable =
                diaryPanel != null &&
                GameProgress.Instance != null &&
                GameProgress.Instance.Data.diaryCollected;
        }

        if (soundButton != null)
            soundButton.interactable = soundPanel != null;

        if (controlsButton != null)
            controlsButton.interactable = controlsPanel != null;
    }

    public void TogglePause()
    {
        if (SleepBed.IsSleeping || HouseEntrance.IsChangingScene)
            return;

        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (SleepBed.IsSleeping || HouseEntrance.IsChangingScene ||
            IsPaused ||
            pausePanel == null ||
            pauseWindow == null)
            return;

        previousTimeScale = Time.timeScale;
        IsPaused = true;

        HideSubpanels();
        pauseWindow.SetActive(true);
        pausePanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    public void ResumeGame()
    {
        bool wasPaused = IsPaused;
        IsPaused = false;

        HideSubpanels();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (pauseWindow != null)
            pauseWindow.SetActive(true);

        if (wasPaused)
            Time.timeScale = previousTimeScale;

        InvestigationGuard.BlockCurrentClick();
    }

    private void OpenDiary()
    {
        if (GameProgress.Instance == null ||
            !GameProgress.Instance.Data.diaryCollected)
            return;

        OpenSubpanel(diaryPanel);
    }

    private void OpenSound()
    {
        OpenSubpanel(soundPanel);
    }

    private void OpenControls()
    {
        OpenSubpanel(controlsPanel);
    }

    private void OpenSubpanel(GameObject panel)
    {
        if (!IsPaused || panel == null || SleepBed.IsSleeping)
            return;

        HideSubpanels();
        pauseWindow.SetActive(false);

        currentSubpanel = panel;
        panel.SetActive(true);
    }

    public void BackToPause()
    {
        HideSubpanels();

        if (pauseWindow != null)
            pauseWindow.SetActive(true);
    }

    private void HideSubpanels()
    {
        if (diaryPanel != null)
            diaryPanel.SetActive(false);

        if (soundPanel != null)
            soundPanel.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        currentSubpanel = null;
    }

    public void GoToMainMenu()
    {
        if (SleepBed.IsSleeping || HouseEntrance.IsChangingScene)
            return;

        if (!Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            Debug.LogError(
                "A cena '" + mainMenuScene +
                "' não foi encontrada na lista de cenas da build.",
                this
            );
            return;
        }

        ResumeGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnDisable()
    {
        if (Instance != this)
            return;

        ResumeGame();
        Instance = null;
    }

    private void OnDestroy()
    {
        foreach (Button button in backButtons)
            if (button != null) button.onClick.RemoveListener(BackToPause);
        backButtons.Clear();
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);

        if (diaryButton != null)
            diaryButton.onClick.RemoveListener(OpenDiary);

        if (soundButton != null)
            soundButton.onClick.RemoveListener(OpenSound);

        if (controlsButton != null)
            controlsButton.onClick.RemoveListener(OpenControls);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);
    }
}