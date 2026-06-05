using UnityEngine;
using UnityEngine.Video;

public class SplashManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += FimDoVideo;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PularVideo();
        }
    }

    void FimDoVideo(VideoPlayer vp)
    {
        GameManager.Instance.LoadScene("MenuPrincipal");
    }

    void PularVideo()
    {
        GameManager.Instance.LoadScene("MenuPrincipal");
    }
}