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

    [Header("Subpainéis — preencher nos próximos passos")]
    [SerializeField] private GameObject diaryPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Cena do menu principal")]
    [SerializeField] private string mainMenuScene = "MenuPrincipal";

    private GameObject currentSubpanel;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Existe mais de um UniversalPauseManager ativo.",
                this
            );
            enabled = false;
            return;
        }

        Instance = this;
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

        RefreshButtons();
    }

    private void Update()
    {
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
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused || pausePanel == null || pauseWindow == null)
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
        if (!IsPaused || panel == null)
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