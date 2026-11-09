using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EpilogoSceneController : MonoBehaviour
{
    [Serializable]
    private struct Fala
    {
        public string personagem;
        [TextArea(2, 6)] public string texto;
    }

    [Header("Ambientes e enquadramento da Casa Manha")]
    [SerializeField] private GameObject[] ambientes;
    [SerializeField] private SpriteRenderer[] fundos;
    [SerializeField] private Transform[] pontosCamera;
    [SerializeField] private Camera cameraCena;
    [SerializeField, Min(1f)] private float tamanhoCamera = 5f;

    [Header("Interface de diálogo existente")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Etapa 2 - volta e pesquisa")]
    [SerializeField] private Fala[] falasDespertar;
    [SerializeField] private Fala[] falasCozinha;
    [SerializeField] private Fala[] cartelasPesquisa;
    [SerializeField] private Fala passagemSemanas;
    [SerializeField] private Image imagemPesquisa;
    [SerializeField] private Sprite[] imagensPesquisa;
    [SerializeField] private UnityEvent aoEncontrarCarta = new UnityEvent();
    [SerializeField] private UnityEvent aoFicarSemNoticias = new UnityEvent();

    [Header("Transição")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.7f;
    private bool transitionRunning;
    private bool narrativaEmCurso;
    private bool finalEncaminhado;
    private int ambienteAtual;
    private float ultimoAspecto;

    public int AmbienteAtual => ambienteAtual;
    public bool EmTransicao => transitionRunning;
    public bool PesquisaConcluida { get; private set; }
    public bool FinalComCarta => GameProgress.Instance != null &&
        GameProgress.Instance.Data.curraisDestinoConfirmado &&
        GameProgress.Instance.Data.curraisIdentidadeConfirmada;

    private IEnumerator Start()
    {
        if (!ReferenciasValidas())
        {
            Debug.LogError("Epilogo: confira ambientes, cameras, dialogo e pesquisa no Inspector.", this);
            enabled = false;
            yield break;
        }
        FecharDialogo();
        imagemPesquisa.gameObject.SetActive(false);
        InventoryTabController.Instance?.Close();
        narrativaEmCurso = true;
        transitionRunning = true;
        DefinirFade(1f);
        while (GameSaveSystem.IsRestoring) yield return null;
        int etapa = GameProgress.Instance != null ? GameProgress.Instance.Data.epilogoEtapa : 0;
        AtivarAmbiente(etapa == 0 ? 0 : 1);
        yield return FadePara(0f);
        transitionRunning = false;

        if (etapa == 0)
        {
            foreach (Fala fala in falasDespertar) yield return Dizer(fala);
            yield return TrocarAmbiente(1);
            SalvarEtapa(1);
        }
        if (etapa < 2)
        {
            foreach (Fala fala in falasCozinha) yield return Dizer(fala);
            for (int i = 0; i < cartelasPesquisa.Length; i++)
            {
                imagemPesquisa.sprite = imagensPesquisa[i];
                imagemPesquisa.gameObject.SetActive(true);
                yield return Dizer(cartelasPesquisa[i]);
            }
            imagemPesquisa.gameObject.SetActive(false);
            yield return FadePara(1f);
            fadeImage.transform.SetAsFirstSibling();
            yield return Dizer(passagemSemanas);
            FecharDialogo();
            fadeImage.transform.SetAsLastSibling();
            yield return FadePara(0f);
            SalvarEtapa(2);
        }
        FecharDialogo();
        narrativaEmCurso = false;
        PesquisaConcluida = true;
        EncaminharFinal();
    }

    private bool ReferenciasValidas()
    {
        if (ambientes == null || fundos == null || pontosCamera == null ||
            ambientes.Length != 3 || fundos.Length != 3 || pontosCamera.Length != 3 ||
            cameraCena == null || fadeImage == null || dialogueUI == null || dialoguePanel == null ||
            choicesContainer == null || characterNameText == null || dialogueText == null || portraitImage == null ||
            imagemPesquisa == null || imagensPesquisa == null || cartelasPesquisa == null ||
            imagensPesquisa.Length != cartelasPesquisa.Length || cartelasPesquisa.Length == 0 ||
            falasDespertar == null || falasDespertar.Length == 0 || falasCozinha == null || falasCozinha.Length == 0)
            return false;
        for (int i = 0; i < 3; i++)
            if (ambientes[i] == null || fundos[i] == null || fundos[i].sprite == null || pontosCamera[i] == null) return false;
        for (int i = 0; i < imagensPesquisa.Length; i++) if (imagensPesquisa[i] == null) return false;
        return true;
    }

    private IEnumerator Dizer(Fala fala)
    {
        // Canvas e painéis são os mesmos usados nas outras cenas da casa.
        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        choicesContainer.SetActive(false);
        portraitImage.enabled = false;
        characterNameText.text = fala.personagem;
        dialogueText.text = fala.texto;
        int quadro = Time.frameCount;
        while (true)
        {
            yield return null;
            if (Time.frameCount > quadro && !PauseHelper.BlockInput() && Time.timeScale > 0f &&
                (InventoryTabController.Instance == null || !InventoryTabController.Instance.IsOpen) &&
                Input.GetMouseButtonDown(0)) break;
        }
        InvestigationGuard.BlockCurrentClick();
    }

    private void SalvarEtapa(int etapa)
    {
        GameProgress progress = GameProgress.Instance;
        if (progress == null) return;
        progress.Data.epilogoEtapa = etapa;
        if (!GameSaveSystem.IsRestoring && progress.Data.CanSave)
            progress.GetComponent<GameSaveSystem>()?.TrySaveCheckpoint(gameObject.scene.name);
    }

    private void EncaminharFinal()
    {
        if (finalEncaminhado) return;
        finalEncaminhado = true;
        // Apenas identidade E destino confirmados permitem localizar a carta.
        // As próximas etapas apresentam o conteúdo de cada final.
        if (FinalComCarta) aoEncontrarCarta.Invoke();
        else aoFicarSemNoticias.Invoke();
    }

    public void MostrarAmbiente(int indice)
    {
        if (!isActiveAndEnabled || transitionRunning || narrativaEmCurso ||
            indice < 0 || indice >= ambientes.Length) return;
        StartCoroutine(TrocarAmbiente(indice));
    }

    private IEnumerator TrocarAmbiente(int indice)
    {
        transitionRunning = true;
        InventoryTabController.Instance?.Close();
        FecharDialogo();
        yield return FadePara(1f);
        AtivarAmbiente(indice);
        yield return FadePara(0f);
        transitionRunning = false;
    }

    private void AtivarAmbiente(int indice)
    {
        ambienteAtual = indice;
        for (int i = 0; i < ambientes.Length; i++) ambientes[i].SetActive(i == indice);
        EnquadrarAmbiente();
    }

    private void LateUpdate()
    {
        if (narrativaEmCurso) InventoryTabController.Instance?.Close();
        if (cameraCena != null && !Mathf.Approximately(ultimoAspecto, cameraCena.aspect)) EnquadrarAmbiente();
    }

    private void EnquadrarAmbiente()
    {
        if (pontosCamera == null || ambienteAtual >= pontosCamera.Length || pontosCamera[ambienteAtual] == null) return;
        ultimoAspecto = cameraCena.aspect;
        cameraCena.orthographicSize = tamanhoCamera;
        Vector3 ponto = pontosCamera[ambienteAtual].position;
        cameraCena.transform.position = new Vector3(ponto.x, ponto.y, -10f);
    }

    private IEnumerator FadePara(float destino)
    {
        float origem = fadeImage.color.a;
        float tempo = 0f;
        while (tempo < fadeDuration)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                DefinirFade(Mathf.Lerp(origem, destino, Mathf.Clamp01(tempo / fadeDuration)));
            }
            yield return null;
        }
        DefinirFade(destino);
    }

    private void DefinirFade(float alpha)
    {
        fadeImage.color = new Color(0f, 0f, 0f, alpha);
        fadeImage.raycastTarget = false;
    }

    private void FecharDialogo()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        narrativaEmCurso = false;
        transitionRunning = false;
        FecharDialogo();
        if (imagemPesquisa != null) imagemPesquisa.gameObject.SetActive(false);
        if (fadeImage != null) DefinirFade(0f);
    }
}
