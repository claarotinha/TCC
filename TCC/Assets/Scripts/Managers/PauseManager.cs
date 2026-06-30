using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Variável global que os outros scripts vão consultar
    public static bool IsPaused { get; private set; } = false;

    [Header("UI")]
    public GameObject pauseMenu;

    [Header("Confirmation")]
    public GameObject confirmPanel;

    private bool isPaused = false;

    void Start()
    {
        pauseMenu.SetActive(false);

        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Se a confirmação estiver aberta,
            // ESC fecha apenas a confirmação.
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
        }
    }

    // ===========================
    // BOTÃO MENU PRINCIPAL
    // ===========================

    public void OpenConfirmation()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    public void CloseConfirmation()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
}