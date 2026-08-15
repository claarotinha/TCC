using UnityEngine;

public class UniversalPauseManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;

    private void Start()
    {
        pausePanel.SetActive(false);
        ResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;

        pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        IsPaused = false;

        pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }
}