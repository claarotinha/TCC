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

    [Header("Etapa 3 - a carta que chegou tarde")]
    [SerializeField] private Fala[] descobertaCarta;
    [SerializeField] private Fala perguntaNaCozinha;
    [SerializeField] private Fala pedidoLeitura;
    [SerializeField] private Fala confirmacaoMae;
    [SerializeField] private Fala gestoZefinha;
    [SerializeField] private Fala gestoFinal;
    [SerializeField] private Fala[] depoisDaCarta;
    [SerializeField] private Fala[] encerramentoCarta;
    [SerializeField, TextArea(4, 12)] private string[] paginasCarta;
    [SerializeField] private GameObject painelCarta;
    [SerializeField] private TMP_Text textoCarta;
    [SerializeField, Min(0f)] private float silencioFinal = 3f;
    [SerializeField] private UnityEvent aoConcluirFinalCarta = new UnityEvent();
    public bool FinalCartaConcluido { get; private set; }

    [Header("Gestos e imagens do epílogo")]
    [SerializeField] private Transform maeCozinha;
    [SerializeField] private Transform zefinhaIdosa;
    [SerializeField] private SpriteRenderer mariLeitura;
    [SerializeField] private Sprite[] passosMari;
    [SerializeField] private Image retratoSeverina;
    [SerializeField] private CanvasGroup grupoCarta;

    [Header("Etapa 4 - sem notícias")]
    [SerializeField] private Fala[] conversaSemNoticias;
    [SerializeField] private Fala[] quartoSemNoticias;
    [SerializeField] private Fala[] encerramentoSemNoticias;
    [SerializeField] private UnityEvent aoConcluirFinalSemNoticias = new UnityEvent();
    public bool FinalSemNoticiasConcluido { get; private set; }

    [Header("Etapa 5 - encerramento comum")]
    [SerializeField] private GameObject painelEncerramento;
    [SerializeField] private TMP_Text textoEncerramento;
    [SerializeField] private Button botaoMenuFinal;
    [SerializeField, TextArea(3, 10)] private string[] textosHistoricos;
    [SerializeField, TextArea(3, 12)] private string[] paginasCreditos;
    private bool voltandoAoMenu;

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
        RecuperarReferenciasDaCena();
        if (!ReferenciasValidas())
        {
            enabled = false;
            yield break;
        }
        FecharDialogo();
        painelEncerramento.SetActive(false);
        botaoMenuFinal.onClick.AddListener(VoltarAoMenuFinal);
        imagemPesquisa.gameObject.SetActive(false);
        if (painelCarta != null) painelCarta.SetActive(false);
        if (retratoSeverina != null) retratoSeverina.gameObject.SetActive(false);
        InventoryTabController.Instance?.Close();
        narrativaEmCurso = true;
        transitionRunning = true;
        DefinirFade(1f);
        while (GameSaveSystem.IsRestoring) yield return null;
        GameProgress.Instance?.UnlockDiaryPage("josefa-14");
        GameProgressData dados = GameProgress.Instance != null ? GameProgress.Instance.Data : null;
        if (dados != null && dados.epilogoConcluido && dados.epilogoFinalVisto != 0)
        {
            AtivarAmbiente(2);
            PesquisaConcluida = true;
            FinalCartaConcluido = dados.epilogoFinalVisto == 1;
            FinalSemNoticiasConcluido = dados.epilogoFinalVisto == 2;
            yield return EncerramentoComum(false);
            yield break;
        }
        int etapa = dados != null ? dados.epilogoEtapa : 0;
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
        if (FinalComCarta) yield return ApresentarFinalCarta();
        else yield return ApresentarFinalSemNoticias();
        if (FinalCartaConcluido || FinalSemNoticiasConcluido)
            yield return EncerramentoComum(true);
    }

    private T BuscarComponente<T>(string nome) where T : Component
    {
        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
            foreach (Transform candidato in raiz.GetComponentsInChildren<Transform>(true))
                if (candidato.name == nome) return candidato.GetComponent<T>();
        return null;
    }

    private GameObject BuscarObjeto(string nome)
    {
        Transform encontrado = BuscarComponente<Transform>(nome);
        return encontrado != null ? encontrado.gameObject : null;
    }

    private void RecuperarReferenciasDaCena()
    {
        // Recupera apenas os objetos existentes nesta cena, inclusive os inativos.
        // Não instancia Canvas, personagens ou cenários durante o jogo.
        if (cameraCena == null) cameraCena = BuscarComponente<Camera>("Main Camera");
        if (dialogueUI == null) dialogueUI = BuscarObjeto("DialogueUI");
        if (dialoguePanel == null) dialoguePanel = BuscarObjeto("DialoguePanel");
        if (choicesContainer == null) choicesContainer = BuscarObjeto("ChoicesContainer");
        if (characterNameText == null) characterNameText = BuscarComponente<TMP_Text>("CharacterName");
        if (dialogueText == null) dialogueText = BuscarComponente<TMP_Text>("DialogueText");
        if (portraitImage == null) portraitImage = BuscarComponente<Image>("Portrait");
        if (fadeImage == null) fadeImage = BuscarComponente<Image>("Escurecimento do epilogo");
        if (imagemPesquisa == null) imagemPesquisa = BuscarComponente<Image>("Pesquisa - fotografias e papeis");
        if (painelCarta == null) painelCarta = BuscarObjeto("Carta de Maria Severina");
        if (textoCarta == null) textoCarta = BuscarComponente<TMP_Text>("Texto da carta");
        if (painelEncerramento == null) painelEncerramento = BuscarObjeto("Encerramento e créditos");
        if (textoEncerramento == null) textoEncerramento = BuscarComponente<TMP_Text>("Texto do encerramento");
        if (botaoMenuFinal == null) botaoMenuFinal = BuscarComponente<Button>("Voltar ao menu - final");
        string[] nomes = { "01 Quarto de Mari", "02 Cozinha - pesquisa", "03 Quarto de Zefinha" };
        if (ambientes == null || ambientes.Length != nomes.Length) ambientes = new GameObject[nomes.Length];
        if (pontosCamera == null || pontosCamera.Length != nomes.Length) pontosCamera = new Transform[nomes.Length];
        for (int i = 0; i < nomes.Length; i++)
        {
            if (ambientes[i] == null) ambientes[i] = BuscarObjeto(nomes[i]);
            if (pontosCamera[i] == null && ambientes[i] != null)
                pontosCamera[i] = ambientes[i].transform.Find("Enquadramento - camera da Casa Manha");
        }
    }

    private bool Conferir(bool valido, string campo)
    {
        if (!valido) Debug.LogError("Epilogo: referência ausente ou inválida em " + campo + ".", this);
        return valido;
    }

    private bool ReferenciasValidas()
    {
        bool valido = true;
        valido &= Conferir(cameraCena != null, nameof(cameraCena));
        valido &= Conferir(fadeImage != null, nameof(fadeImage));
        valido &= Conferir(dialogueUI != null, nameof(dialogueUI));
        valido &= Conferir(dialoguePanel != null, nameof(dialoguePanel));
        valido &= Conferir(choicesContainer != null, nameof(choicesContainer));
        valido &= Conferir(characterNameText != null, nameof(characterNameText));
        valido &= Conferir(dialogueText != null, nameof(dialogueText));
        valido &= Conferir(portraitImage != null, nameof(portraitImage));
        valido &= Conferir(imagemPesquisa != null, nameof(imagemPesquisa));
        for (int i = 0; i < 3; i++)
        {
            valido &= Conferir(ambientes[i] != null, $"ambientes[{i}]");
            valido &= Conferir(pontosCamera[i] != null, $"pontosCamera[{i}]");
        }
        valido &= Conferir(falasDespertar != null && falasDespertar.Length > 0, nameof(falasDespertar));
        valido &= Conferir(falasCozinha != null && falasCozinha.Length > 0, nameof(falasCozinha));
        bool pesquisaValida = imagensPesquisa != null && cartelasPesquisa != null &&
            cartelasPesquisa.Length > 0 && imagensPesquisa.Length == cartelasPesquisa.Length;
        valido &= Conferir(pesquisaValida, "imagensPesquisa / cartelasPesquisa");
        if (pesquisaValida)
            for (int i = 0; i < imagensPesquisa.Length; i++)
                valido &= Conferir(imagensPesquisa[i] != null, $"imagensPesquisa[{i}]");
        valido &= Conferir(maeCozinha != null, nameof(maeCozinha));
        valido &= Conferir(zefinhaIdosa != null, nameof(zefinhaIdosa));
        valido &= Conferir(mariLeitura != null, nameof(mariLeitura));
        valido &= Conferir(passosMari != null && passosMari.Length > 0, nameof(passosMari));
        valido &= Conferir(retratoSeverina != null && retratoSeverina.sprite != null, nameof(retratoSeverina));
        valido &= Conferir(grupoCarta != null, nameof(grupoCarta));
        valido &= Conferir(painelEncerramento != null, nameof(painelEncerramento));
        valido &= Conferir(textoEncerramento != null, nameof(textoEncerramento));
        valido &= Conferir(botaoMenuFinal != null, nameof(botaoMenuFinal));
        valido &= Conferir(textosHistoricos != null && textosHistoricos.Length > 0, nameof(textosHistoricos));
        valido &= Conferir(paginasCreditos != null && paginasCreditos.Length > 0, nameof(paginasCreditos));
        return valido;
    }

    private static void MostrarPainelDialogo(GameObject painel)
    {
        painel.SetActive(true);
        // O modelo de diálogo da casa vem com CanvasGroup oculto.
        // Ativar o GameObject sozinho mantém todas as falas invisíveis.
        CanvasGroup grupo = painel.GetComponent<CanvasGroup>();
        if (grupo == null) return;
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private IEnumerator Dizer(Fala fala)
    {
        // Canvas e painéis são os mesmos usados nas outras cenas da casa.
        MostrarPainelDialogo(dialogueUI);
        MostrarPainelDialogo(dialoguePanel);
        choicesContainer.SetActive(false);
        portraitImage.enabled = false;
        characterNameText.text = fala.personagem;
        dialogueText.text = fala.texto;
        yield return AguardarAvanco();
    }

    private IEnumerator AguardarAvanco()
    {
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
        // Cada caminho mantém os mesmos fatos e pistas do salvamento.
        if (FinalComCarta) aoEncontrarCarta.Invoke();
        else aoFicarSemNoticias.Invoke();
    }

    private IEnumerator ApresentarFinalCarta()
    {
        // Nunca entra neste final com apenas uma das pistas confirmadas.
        if (!FinalComCarta) yield break;
        bool valido = Conferir(painelCarta != null, nameof(painelCarta));
        valido &= Conferir(textoCarta != null, nameof(textoCarta));
        valido &= Conferir(paginasCarta != null && paginasCarta.Length > 0, nameof(paginasCarta));
        valido &= Conferir(descobertaCarta != null && descobertaCarta.Length > 0, nameof(descobertaCarta));
        valido &= Conferir(depoisDaCarta != null && depoisDaCarta.Length > 0, nameof(depoisDaCarta));
        valido &= Conferir(encerramentoCarta != null && encerramentoCarta.Length > 0, nameof(encerramentoCarta));
        if (!valido) yield break;

        narrativaEmCurso = true;
        InventoryTabController.Instance?.Close();
        imagemPesquisa.sprite = imagensPesquisa[0];
        Vector2 ancoraMinFoto = imagemPesquisa.rectTransform.anchorMin;
        Vector2 ancoraMaxFoto = imagemPesquisa.rectTransform.anchorMax;
        imagemPesquisa.rectTransform.anchorMin = new Vector2(0.24f, 0.48f);
        imagemPesquisa.rectTransform.anchorMax = new Vector2(0.49f, 0.90f);
        imagemPesquisa.gameObject.SetActive(true);
        if (retratoSeverina != null) retratoSeverina.gameObject.SetActive(true);
        foreach (Fala fala in descobertaCarta) yield return Dizer(fala);
        imagemPesquisa.gameObject.SetActive(false);
        imagemPesquisa.rectTransform.anchorMin = ancoraMinFoto;
        imagemPesquisa.rectTransform.anchorMax = ancoraMaxFoto;
        if (retratoSeverina != null) retratoSeverina.gameObject.SetActive(false);
        yield return Dizer(perguntaNaCozinha);
        yield return AnimarGesto(maeCozinha, 0.7f, 2f);
        yield return Dizer(confirmacaoMae);
        yield return TrocarAmbiente(2);
        yield return Dizer(pedidoLeitura);
        FecharDialogo();
        painelCarta.SetActive(true);
        yield return AnimarCarta(true);
        foreach (string pagina in paginasCarta)
        {
            textoCarta.text = pagina;
            yield return AguardarAvanco();
        }
        yield return AnimarCarta(false);
        painelCarta.SetActive(false);
        yield return AnimarGesto(zefinhaIdosa, 1.2f, 0.6f);
        yield return Dizer(gestoZefinha);
        foreach (Fala fala in depoisDaCarta) yield return Dizer(fala);
        yield return AproximarMari();
        yield return Dizer(gestoFinal);
        FecharDialogo();
        float tempo = 0f;
        while (tempo < silencioFinal)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
            yield return null;
        }
        yield return FadePara(1f);
        fadeImage.transform.SetAsFirstSibling();
        foreach (Fala fala in encerramentoCarta) yield return Dizer(fala);
        FecharDialogo();
        fadeImage.transform.SetAsLastSibling();
        FinalCartaConcluido = true;
        narrativaEmCurso = false;
        aoConcluirFinalCarta.Invoke();
    }

    private IEnumerator ApresentarFinalSemNoticias()
    {
        if (FinalComCarta) yield break;
        bool valido = Conferir(conversaSemNoticias != null && conversaSemNoticias.Length > 0, nameof(conversaSemNoticias));
        valido &= Conferir(quartoSemNoticias != null && quartoSemNoticias.Length > 0, nameof(quartoSemNoticias));
        valido &= Conferir(encerramentoSemNoticias != null && encerramentoSemNoticias.Length > 0, nameof(encerramentoSemNoticias));
        if (!valido) yield break;
        narrativaEmCurso = true;
        InventoryTabController.Instance?.Close();
        // Só a fotografia das meninas: a pesquisa não revela destino nem carta.
        imagemPesquisa.sprite = imagensPesquisa[0];
        imagemPesquisa.gameObject.SetActive(true);
        for (int i = 0; i < conversaSemNoticias.Length; i++)
        {
            if (i == 2) yield return AguardarTempo(1.5f);
            yield return Dizer(conversaSemNoticias[i]);
        }
        imagemPesquisa.gameObject.SetActive(false);
        yield return TrocarAmbiente(2);
        yield return AnimarGesto(zefinhaIdosa, 1f, 0.6f);
        foreach (Fala fala in quartoSemNoticias) yield return Dizer(fala);
        yield return AproximarMari();
        FecharDialogo();
        imagemPesquisa.gameObject.SetActive(true);
        yield return AguardarTempo(silencioFinal);
        // O fundo escurece primeiro; a fotografia continua acima do preto.
        fadeImage.transform.SetAsFirstSibling();
        yield return FadePara(1f);
        yield return AguardarTempo(3f);
        yield return DesaparecerFotografia();
        imagemPesquisa.gameObject.SetActive(false);
        foreach (Fala fala in encerramentoSemNoticias) yield return Dizer(fala);
        FecharDialogo();
        fadeImage.transform.SetAsLastSibling();
        FinalSemNoticiasConcluido = true;
        narrativaEmCurso = false;
        aoConcluirFinalSemNoticias.Invoke();
    }

    private IEnumerator AguardarTempo(float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator AnimarGesto(Transform alvo, float duracao, float angulo)
    {
        if (alvo == null) yield break;
        Quaternion origem = alvo.localRotation;
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                alvo.localRotation = origem * Quaternion.Euler(0f, 0f,
                    Mathf.Sin(tempo / duracao * Mathf.PI * 4f) * angulo);
            }
            yield return null;
        }
        alvo.localRotation = origem;
    }

    private IEnumerator AproximarMari()
    {
        if (mariLeitura == null) yield break;
        Vector3 inicio = mariLeitura.transform.localPosition;
        Vector3 destino = inicio + Vector3.right * 1.2f;
        Sprite repouso = mariLeitura.sprite;
        float tempo = 0f;
        while (tempo < 1.2f)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                mariLeitura.transform.localPosition = Vector3.Lerp(inicio, destino, Mathf.SmoothStep(0f, 1f, tempo / 1.2f));
                if (passosMari != null && passosMari.Length > 0)
                    mariLeitura.sprite = passosMari[Mathf.FloorToInt(tempo * 8f) % passosMari.Length];
            }
            yield return null;
        }
        mariLeitura.sprite = repouso;
    }

    private IEnumerator AnimarCarta(bool mostrar)
    {
        if (grupoCarta == null) yield break;
        float tempo = 0f;
        grupoCarta.alpha = mostrar ? 0f : 1f;
        while (tempo < 0.5f)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                float fracao = Mathf.Clamp01(tempo / 0.5f);
                grupoCarta.alpha = mostrar ? fracao : 1f - fracao;
            }
            yield return null;
        }
        grupoCarta.alpha = mostrar ? 1f : 0f;
    }

    private IEnumerator DesaparecerFotografia()
    {
        Color cor = imagemPesquisa.color;
        float tempo = 0f;
        while (tempo < 1.5f)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                imagemPesquisa.color = new Color(cor.r, cor.g, cor.b, Mathf.Lerp(cor.a, 0f, tempo / 1.5f));
            }
            yield return null;
        }
        imagemPesquisa.color = cor;
    }

    private IEnumerator EncerramentoComum(bool registrarFinal)
    {
        narrativaEmCurso = true;
        transitionRunning = false;
        FecharDialogo();
        painelCarta.SetActive(false);
        imagemPesquisa.gameObject.SetActive(false);
        retratoSeverina.gameObject.SetActive(false);
        InventoryTabController.Instance?.Close();
        if (registrarFinal && GameProgress.Instance != null)
        {
            GameProgress progress = GameProgress.Instance;
            progress.Data.epilogoFinalVisto = FinalCartaConcluido ? 1 : 2;
            progress.Data.epilogoConcluido = true;
            progress.Data.epilogoEtapa = 2;
            // Reutiliza o checkpoint e o inventário existentes. Não desbloqueia
            // salvamento nem sobrescreve a partida ao testar uma cena isolada.
            if (progress.Data.CanSave && !GameSaveSystem.IsRestoring)
                progress.GetComponent<GameSaveSystem>()?.TrySaveCheckpoint(gameObject.scene.name);
        }
        DefinirFade(1f);
        fadeImage.transform.SetAsFirstSibling();
        painelEncerramento.SetActive(true);
        botaoMenuFinal.gameObject.SetActive(false);
        foreach (string texto in textosHistoricos)
        {
            textoEncerramento.text = texto + "\n\nClique para continuar.";
            yield return AguardarAvanco();
        }
        foreach (string pagina in paginasCreditos)
        {
            textoEncerramento.text = pagina + "\n\nClique para continuar.";
            yield return AguardarAvanco();
        }
        int final = GameProgress.Instance != null ? GameProgress.Instance.Data.epilogoFinalVisto :
            (FinalCartaConcluido ? 1 : 2);
        textoEncerramento.text = final == 1 ? "CURRAIS\n\nA carta que chegou tarde" : "CURRAIS\n\nSem notícias";
        botaoMenuFinal.gameObject.SetActive(true);
    }

    public void VoltarAoMenuFinal()
    {
        if (voltandoAoMenu || !painelEncerramento.activeInHierarchy ||
            !botaoMenuFinal.gameObject.activeSelf || PauseHelper.BlockInput()) return;
        UniversalPauseManager pause = UniversalPauseManager.Instance;
        if (pause == null)
        {
            Debug.LogError("Epilogo: gerenciador de pausa/menu não encontrado.", this);
            return;
        }
        voltandoAoMenu = true;
        pause.GoToMainMenu();
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
        if (botaoMenuFinal != null) botaoMenuFinal.onClick.RemoveListener(VoltarAoMenuFinal);
        if (painelEncerramento != null) painelEncerramento.SetActive(false);
        narrativaEmCurso = false;
        transitionRunning = false;
        FecharDialogo();
        if (imagemPesquisa != null) imagemPesquisa.gameObject.SetActive(false);
        if (retratoSeverina != null) retratoSeverina.gameObject.SetActive(false);
        if (painelCarta != null) painelCarta.SetActive(false);
        if (fadeImage != null) DefinirFade(0f);
    }
}
