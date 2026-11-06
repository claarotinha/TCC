using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashManager : MonoBehaviour
{
    private const string IntroWatchedKey = "Currais.IntroWatched";

    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipIndicator;

    [Serializable]
    private class CreditoAbertura
    {
        public CanvasGroup painel;
        [Range(0f, 1f)] public float inicio;
        [Range(0f, 1f)] public float fim;
    }

    [Header("Vídeo e créditos editáveis na Hierarchy")]
    [SerializeField] private RawImage imagemVideo;
    [SerializeField] private CreditoAbertura[] creditos;
    [SerializeField, Min(0f)] private float tempoFadeCreditos = 0.6f;

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

        OcultarCreditos();
        videoPlayer.isLooping = false;
        // Usa diretamente a textura decodificada. Evita cópia/formato da RenderTexture.
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.targetTexture = null;
        videoPlayer.sendFrameReadyEvents = true;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        if (imagemVideo != null)
        {
            imagemVideo.texture = null;
            imagemVideo.enabled = false;
        }
        videoPlayer.frameReady += QuadroDisponivel;
        videoPlayer.prepareCompleted += VideoPreparado;
        videoPlayer.errorReceived += ErroVideo;
        videoPlayer.loopPointReached += FimDoVideo;
    }

    private void Start()
    {
        if (videoPlayer == null) return;
        if (videoPlayer.isPrepared) VideoPreparado(videoPlayer);
        else videoPlayer.Prepare();
    }

    private void VideoPreparado(VideoPlayer player)
    {
        if (leaving || player != videoPlayer) return;
        player.Play();
    }

    private void QuadroDisponivel(VideoPlayer player, long quadro)
    {
        if (leaving || player != videoPlayer || imagemVideo == null || player.texture == null) return;
        if (imagemVideo.texture != player.texture) imagemVideo.texture = player.texture;
        imagemVideo.enabled = true;
    }

    private void LateUpdate()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
            QuadroDisponivel(videoPlayer, videoPlayer.frame);
    }

    private void ErroVideo(VideoPlayer player, string mensagem)
    {
        if (leaving || player != videoPlayer) return;
        Debug.LogWarning("Não foi possível reproduzir a abertura: " + mensagem, this);
        GoToMenu();
    }

    private void OcultarCreditos()
    {
        if (creditos == null) return;
        foreach (CreditoAbertura credito in creditos)
        {
            if (credito == null || credito.painel == null) continue;
            credito.painel.alpha = 0f;
            credito.painel.interactable = false;
            credito.painel.blocksRaycasts = false;
        }
    }

    private void AtualizarCreditos()
    {
        if (videoPlayer == null || creditos == null || !videoPlayer.isPrepared ||
            videoPlayer.length <= 0d) return;
        float tempo = (float)videoPlayer.time;
        float duracao = (float)videoPlayer.length;
        foreach (CreditoAbertura credito in creditos)
        {
            if (credito == null || credito.painel == null) continue;
            float inicio = Mathf.Clamp01(credito.inicio) * duracao;
            float fim = Mathf.Clamp01(credito.fim) * duracao;
            float alpha = 0f;
            if (fim > inicio && tempo >= inicio && tempo <= fim)
            {
                float fade = Mathf.Min(tempoFadeCreditos, (fim - inicio) * 0.5f);
                alpha = fade <= 0f ? 1f : Mathf.Min(
                    Mathf.Clamp01((tempo - inicio) / fade),
                    Mathf.Clamp01((fim - tempo) / fade));
            }
            credito.painel.alpha = alpha;
        }
    }

    private void Update()
    {
        if (!leaving) AtualizarCreditos();
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
        OcultarCreditos();

        if (GameManager.Instance != null)
            GameManager.Instance.LoadScene("MenuPrincipal");
        else
            SceneManager.LoadScene("MenuPrincipal");
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.frameReady -= QuadroDisponivel;
            videoPlayer.loopPointReached -= FimDoVideo;
            videoPlayer.prepareCompleted -= VideoPreparado;
            videoPlayer.errorReceived -= ErroVideo;
        }
    }
}
