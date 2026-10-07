using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashManager : MonoBehaviour
{
    private const string IntroWatchedKey = "Currais.IntroWatched";

    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipIndicator;

    private bool canSkip;
    private bool leaving;

    private void Awake()
    {
        // Assistir à introdução é uma preferência; não depende dos checkpoints.
        canSkip = PlayerPrefs.GetInt(IntroWatchedKey, 0) == 1;
        if (skipIndicator != null)
            skipIndicator.SetActive(canSkip);

        if (videoPlayer == null)
        {
            Debug.LogError("SplashManager: VideoPlayer não configurado.", this);
            enabled = false;
            return;
        }

        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += FimDoVideo;
    }

    private void Update()
    {
        if (canSkip && !leaving && Input.GetKeyDown(KeyCode.Escape))
            PularVideo();
    }

    private void FimDoVideo(VideoPlayer vp)
    {
        if (leaving || vp != videoPlayer) return;

        // Só libera ESC para a próxima exibição depois de assistir até o fim.
        PlayerPrefs.SetInt(IntroWatchedKey, 1);
        PlayerPrefs.Save();
        GoToMenu();
    }

    private void PularVideo()
    {
        if (canSkip)
            GoToMenu();
    }

    private void GoToMenu()
    {
        if (leaving) return;
        leaving = true;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("MenuPrincipal");
        else
            SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= FimDoVideo;
    }
}
