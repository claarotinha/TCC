using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class AvisoInicial : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textoAviso;
    [SerializeField] private TMP_Text textoFones;
    [SerializeField] private CanvasGroup orientacoes;
    [SerializeField] private RectTransform solAviso;

    [Header("Som")]
    public AudioSource typingSound;

    [Header("Configuração")]
    public float tempoEntreLetras = 0.08f;
    public float tempoDepoisDoTexto = 2f;
    [SerializeField, Min(0f)] private float tempoOrientacoes = 10f;
    [SerializeField, Min(0f)] private float tempoAvisoFones = 7f;
    [SerializeField, Min(0f)] private float tempoFade = 0.6f;
    [SerializeField] private float velocidadeSol = 150f;

    private string mensagem;

    private void Start()
    {
        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
            foreach (TMP_Text texto in raiz.GetComponentsInChildren<TMP_Text>(true))
            {
                if (textoAviso == null && texto.name == "TextoAviso") textoAviso = texto;
                if (textoFones == null && texto.name.ToLowerInvariant().Contains("fones")) textoFones = texto;
            }
        if (textoAviso == null || textoFones == null)
        {
            Debug.LogError("AvisoInicial: configure os textos da data e do aviso de fones na Hierarchy.", this);
            enabled = false;
            return;
        }
        // A data e os avisos são escritos nos próprios textos da Hierarchy.
        textoAviso.enabled = true;
        textoAviso.gameObject.SetActive(true);
        textoFones.enabled = true;
        mensagem = textoAviso.text;
        textoAviso.text = string.Empty;
        textoFones.gameObject.SetActive(false);
        StartCoroutine(Apresentar());
    }

    private void Update()
    {
        if (solAviso != null && solAviso.gameObject.activeInHierarchy &&
            orientacoes != null && orientacoes.gameObject.activeInHierarchy)
            solAviso.Rotate(0f, 0f, -velocidadeSol * Time.unscaledDeltaTime);
    }

    private IEnumerator Apresentar()
    {
        // O conteúdo é editado no TMP da Hierarchy, não neste código.
        textoFones.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(tempoAvisoFones);
        textoFones.gameObject.SetActive(false);

        if (orientacoes != null)
        {
            orientacoes.gameObject.SetActive(true);
            orientacoes.interactable = false;
            orientacoes.blocksRaycasts = false;
            yield return FadeOrientacoes(0f, 1f);
            yield return new WaitForSecondsRealtime(tempoOrientacoes);
            yield return FadeOrientacoes(1f, 0f);
            orientacoes.gameObject.SetActive(false);
        }

        if (typingSound != null) typingSound.Play();
        foreach (char letra in mensagem)
        {
            textoAviso.text += letra;
            yield return new WaitForSecondsRealtime(Mathf.Max(0f, tempoEntreLetras));
        }
        if (typingSound != null) typingSound.Stop();
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, tempoDepoisDoTexto));
        SceneManager.LoadScene("Escola");
    }

    private IEnumerator FadeOrientacoes(float de, float para)
    {
        float tempo = 0f;
        orientacoes.alpha = de;
        while (tempo < tempoFade)
        {
            tempo += Time.unscaledDeltaTime;
            orientacoes.alpha = Mathf.Lerp(de, para, tempo / tempoFade);
            yield return null;
        }
        orientacoes.alpha = para;
    }

    private void OnDisable()
    {
        if (typingSound != null) typingSound.Stop();
    }
}
