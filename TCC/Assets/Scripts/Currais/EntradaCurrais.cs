using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Ato 2, etapa 1. As artes, falas e posições são referências editáveis da cena.
public partial class EntradaCurrais : MonoBehaviour
{
    [Serializable]
    public class Fala
    {
        public string personagem;
        [TextArea(2, 6)] public string texto;
    }

    [Serializable]
    public class Exame
    {
        public Collider2D area;
        public string titulo;
        [TextArea(2, 5)] public string texto;
    }

    [Header("Sistemas existentes (também permitem testar esta cena diretamente)")]
    [SerializeField] private GameObject progressoPrefab;
    [SerializeField] private GameObject inventarioPrefab;
    [SerializeField] private PlayerMovement mari;
    [SerializeField] private Camera cameraCena;
    [SerializeField] private CameraFollow seguirCamera;

    [Header("Canvas reutilizado da Estrada")]
    [SerializeField] private GameObject dialogoUI, painelDialogo, escolhas;
    [SerializeField] private TMP_Text nomeTexto, falaTexto, objetivoTexto;
    [SerializeField] private Image retrato, fade;
    [SerializeField] private Button conversarBotao, sairBotao;

    [Header("Cenários separados na Hierarchy")]
    [SerializeField] private GameObject rua, campo;
    [SerializeField] private GameObject lacoChao, lacoPanela, outraFila;
    [SerializeField] private Collider2D areaLaco, areaLuzia;
    [SerializeField] private Transform pai, mae, baleia, irma, cabo, carroca;
    [SerializeField] private GameObject luzia;
    [SerializeField] private SeguidorFamilia[] seguidores;
    [SerializeField] private SpriteRenderer visualIrma;
    [SerializeField] private Sprite irmaFerida, irmaCurativo;

    [Header("Marcas de posição editáveis, sem coordenadas no código")]
    [SerializeField] private Transform cameraRua, cameraCampo;
    [SerializeField] private Transform irmaSeparada, irmaSaida, caboBarreira, carrocaSaida;
    [SerializeField] private Transform pontoColeta, mariEntrada, paiEntrada, maeEntrada, baleiaEntrada;
    [SerializeField] private Transform mariAbrigo, paiAbrigo, maeAbrigo, baleiaAbrigo;

    [Header("Tempos das cutscenes")]
    [SerializeField, Min(0.1f)] private float tempoFade = 0.7f;
    [SerializeField, Min(0.1f)] private float tempoPlano = 1.2f;
    [SerializeField, Min(0.1f)] private float tempoSeparacao = 2.5f;
    [SerializeField, Min(0.1f)] private float tempoCarroca = 2.8f;

    [Header("Falas do roteiro - editáveis no Inspector")]
    [SerializeField] private Fala[] antesSeparacao, barreira, depoisSeparacao;
    [SerializeField] private Fala promessa, semPromessa, respostaZefinha;
    [SerializeField] private Fala[] entrada, revista, abrigo;
    [SerializeField] private Exame[] exames;
    [SerializeField] private string objetivoBusca = "Descobrir para onde levaram Mariquinha.";
    [SerializeField] private string objetivoLaco = "Clique no pedaço do laço que caiu no chão.";

    [Header("Etapa 2 - exploração e escolhas")]
    [SerializeField] private Button[] botoesEtapa2;
    [SerializeField] private Collider2D areaAgua, areaMae, areaMenino, areaRegistro, areaPanela;
    [SerializeField] private Transform meninoBusca, meninoFimPasso, panelaBusca, panelaNoChao, panelaRecolhida, lacoNoChao;
    [SerializeField] private Transform luziaRepouso;
    [SerializeField] private SeguidorFamilia seguidoraLuzia, seguidoraMae;
    [SerializeField] private GameObject canhotoCenario, cabacaMesa;
    [SerializeField] private SpriteRenderer guardaRegistro;
    [SerializeField] private Sprite guardaRegistroParado, guardaRegistroBloqueio;
    [SerializeField] private ItemData cabacaVaziaBusca, cabacaCheiaBusca, itemLaco, itemCanhoto;
    [SerializeField, Min(0.5f)] private float alcanceInteracao = 3f;
    [SerializeField] private Fala[] aguaFuncionario, aguaAntesEscolha, aguaMae, aguaLuzia;
    [SerializeField] private Fala[] meninoApresentacao, meninoEspera, meninoNome, meninoSaida;
    [SerializeField] private Fala[] registroInicio, registroMae, registroLuzia;
    [SerializeField] private Fala[] panelaInicio, panelaLaco, panelaCanhoto;
    [SerializeField] private string[] opcoesLuziaInicio, opcoesAgua, opcoesMenino, opcoesRegistro, opcoesPanela, objetivosBusca;
    [SerializeField, TextArea(2, 4)] private string perguntaAgua, perguntaPanela;
    private Action[] acoesMenu;
    private int escolhaRecebida = -1;

    private GameProgressData Dados => GameProgress.Instance.Data;
    private bool pronto, ocupado, dialogando, aguardandoClique, menuAberto, encerrando;
    private int quadroBloqueado;
    private Quaternion rotacaoPai;
    private float ajustePivoIrma;
    private float zoomInicial;
    private Vector3 inicioMariNaRua;

    private void Awake()
    {
        if (mari != null) mari.LiberarLimites();
        if (seguirCamera != null) seguirCamera.LiberarLimites();
        if (objetivoTexto != null) objetivoTexto.gameObject.SetActive(false);
        if (mari != null) inicioMariNaRua = mari.transform.position;
        if (cameraCena != null) zoomInicial = cameraCena.orthographicSize;
        FixarCameraDaSequencia();
        if (seguirCamera != null) seguirCamera.enabled = false;
        if (mari != null) mari.enabled = false;
        if (seguidores != null)
            foreach (SeguidorFamilia seguidor in seguidores)
                if (seguidor != null) seguidor.enabled = false;
    }

    private IEnumerator Start()
    {
        if (!ReferenciasValidas())
        {
            Debug.LogError("EntradaCurrais: inicialização interrompida. Confira os campos identificados acima no objeto " + name + ".", this);
            FecharDialogo();
            if (fade != null) CorFade(0f);
            LiberarMari();
            yield break;
        }
        if (GameProgress.Instance == null) Instantiate(progressoPrefab);
        if (InventoryManager.Instance == null) Instantiate(inventarioPrefab);
        bool restaurandoPartida = GameSaveSystem.IsRestoring;
        bool restaurandoPosicao = GameSaveSystem.HasPendingPlayerPosition;
        yield return null;
        while (GameSaveSystem.IsRestoring) yield return null;
        if (GameProgress.Instance == null || InventoryManager.Instance == null)
        {
            Debug.LogError("EntradaCurrais: confira os prefabs existentes de progresso e inventário.", this);
            FecharDialogo();
            if (fade != null) CorFade(0f);
            LiberarMari();
            yield break;
        }
        Vector3 posicaoRestaurada = mari.transform.position;
        GameProgress.Instance.NormalizeData();
        GameProgress.Instance.UnlockDiaryPage("josefa-06");
        rotacaoPai = pai.localRotation;
        if (Dados.estradaMariquinhaFerida && visualIrma != null)
        {
            Sprite pose = Dados.estradaCurativoAplicado ? irmaCurativo : irmaFerida;
            if (pose != null)
            {
                Animator animador = visualIrma.GetComponent<Animator>();
                if (animador != null) animador.enabled = false;
                float pe = visualIrma.bounds.min.y;
                float altura = visualIrma.sprite != null ? visualIrma.sprite.bounds.size.y : pose.bounds.size.y;
                float yAnterior = irma.position.y;
                visualIrma.sprite = pose;
                if (pose.bounds.size.y > 0f)
                    visualIrma.transform.localScale *= altura / pose.bounds.size.y;
                irma.position += Vector3.up * (pe - visualIrma.bounds.min.y);
                ajustePivoIrma = irma.position.y - yAnterior;
            }
        }
        FixarCameraDaSequencia();
        FecharDialogo();
        PrepararOpcoes();
        PrepararChegadaAlternativa();
        PrepararRevistaNoCheckpoint();
        PrepararEncenacaoEntrada();
        PrepararTarefaPanos();
        PrepararEtapaLavagem();
        PrepararSocorro();
        PrepararFamiliaAlternativa();
        pronto = true;
        int etapa = Mathf.Clamp(Dados.curraisEtapaEntrada, 0, 4);
        Dados.curraisEtapaEntrada = etapa;
        if (etapa == 0) Iniciar(Separar());
        else if (etapa == 2 && Dados.curraisRevistaApresentada)
        {
            MostrarEntrada();
            RetomarExploracao(posicaoRestaurada, restaurandoPartida, restaurandoPosicao);
        }
        else if (etapa == 1 || etapa == 2) Iniciar(Entrar());
        else if (etapa == 3 && Dados.curraisAbrigoApresentado && !Dados.curraisPaiAcomodado)
        {
            MostrarAbrigo();
            if (Dados.curraisPanoAbrigoRecolhido && !TemItem(itemPanoAbrigo))
                InventoryManager.Instance.AddItem(itemPanoAbrigo);
            RetomarExploracao(posicaoRestaurada, restaurandoPartida, restaurandoPosicao);
        }
        else if (etapa == 3) Iniciar(ChegarAbrigo());
        else
        {
            MostrarAbrigo();
            RetomarExploracao(posicaoRestaurada, restaurandoPartida, restaurandoPosicao);
            if (MemoriaDisponivel && Dados.curraisMemoriaIniciada) Iniciar(RetomarMemoria());
        }
    }

    private void RetomarExploracao(Vector3 posicaoRestaurada, bool restaurandoPartida, bool restaurandoPosicao)
    {
        if (restaurandoPosicao)
        {
            if (!float.IsNaN(posicaoRestaurada.x) && !float.IsInfinity(posicaoRestaurada.x) &&
                !float.IsNaN(posicaoRestaurada.y) && !float.IsInfinity(posicaoRestaurada.y) &&
                !float.IsNaN(posicaoRestaurada.z) && !float.IsInfinity(posicaoRestaurada.z))
            {
                mari.transform.position = posicaoRestaurada;
                Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
                if (corpo != null)
                {
                    corpo.position = posicaoRestaurada;
                    corpo.linearVelocity = Vector2.zero;
                    corpo.angularVelocity = 0f;
                }
                Physics2D.SyncTransforms();
            }
            else Debug.LogWarning("Currais: posição inválida no checkpoint; usando a entrada da área.", this);
        }
        CorFade(0f);
        AtualizarVisuaisChegada();
        AtualizarEstadoBusca();
        if (restaurandoPartida)
        {
            if (Dados.curraisEtapaEntrada == 4) RestaurarAcompanhante();
            if (seguirCamera != null) seguirCamera.RestaurarEnquadramentoDoAlvo();
        }
        quadroBloqueado = Time.frameCount;
        InvestigationGuard.BlockCurrentClick();
        LiberarMari();
    }

    private bool ReferenciasValidas()
    {
        bool referenciasPresentes = true;
        referenciasPresentes &= ConferirReferencia(progressoPrefab, nameof(progressoPrefab));
        referenciasPresentes &= ConferirReferencia(inventarioPrefab, nameof(inventarioPrefab));
        referenciasPresentes &= ConferirReferencia(mari, nameof(mari));
        referenciasPresentes &= ConferirReferencia(cameraCena, nameof(cameraCena));
        referenciasPresentes &= ConferirReferencia(seguirCamera, nameof(seguirCamera));
        referenciasPresentes &= ConferirReferencia(dialogoUI, nameof(dialogoUI));
        referenciasPresentes &= ConferirReferencia(painelDialogo, nameof(painelDialogo));
        referenciasPresentes &= ConferirReferencia(escolhas, nameof(escolhas));
        referenciasPresentes &= ConferirReferencia(nomeTexto, nameof(nomeTexto));
        referenciasPresentes &= ConferirReferencia(falaTexto, nameof(falaTexto));
        referenciasPresentes &= ConferirReferencia(objetivoTexto, nameof(objetivoTexto));
        referenciasPresentes &= ConferirReferencia(fade, nameof(fade));
        referenciasPresentes &= ConferirReferencia(conversarBotao, nameof(conversarBotao));
        referenciasPresentes &= ConferirReferencia(sairBotao, nameof(sairBotao));
        referenciasPresentes &= ConferirReferencia(rua, nameof(rua));
        referenciasPresentes &= ConferirReferencia(campo, nameof(campo));
        referenciasPresentes &= ConferirReferencia(lacoChao, nameof(lacoChao));
        referenciasPresentes &= ConferirReferencia(lacoPanela, nameof(lacoPanela));
        referenciasPresentes &= ConferirReferencia(outraFila, nameof(outraFila));
        referenciasPresentes &= ConferirReferencia(areaLaco, nameof(areaLaco));
        referenciasPresentes &= ConferirReferencia(areaLuzia, nameof(areaLuzia));
        referenciasPresentes &= ConferirReferencia(pai, nameof(pai));
        referenciasPresentes &= ConferirReferencia(mae, nameof(mae));
        referenciasPresentes &= ConferirReferencia(baleia, nameof(baleia));
        referenciasPresentes &= ConferirReferencia(irma, nameof(irma));
        referenciasPresentes &= ConferirReferencia(cabo, nameof(cabo));
        referenciasPresentes &= ConferirReferencia(carroca, nameof(carroca));
        referenciasPresentes &= ConferirReferencia(luzia, nameof(luzia));
        referenciasPresentes &= ConferirReferencia(cameraRua, nameof(cameraRua));
        referenciasPresentes &= ConferirReferencia(cameraCampo, nameof(cameraCampo));
        referenciasPresentes &= ConferirReferencia(irmaSeparada, nameof(irmaSeparada));
        referenciasPresentes &= ConferirReferencia(irmaSaida, nameof(irmaSaida));
        referenciasPresentes &= ConferirReferencia(caboBarreira, nameof(caboBarreira));
        referenciasPresentes &= ConferirReferencia(carrocaSaida, nameof(carrocaSaida));
        referenciasPresentes &= ConferirReferencia(pontoColeta, nameof(pontoColeta));
        referenciasPresentes &= ConferirReferencia(mariEntrada, nameof(mariEntrada));
        referenciasPresentes &= ConferirReferencia(paiEntrada, nameof(paiEntrada));
        referenciasPresentes &= ConferirReferencia(maeEntrada, nameof(maeEntrada));
        referenciasPresentes &= ConferirReferencia(baleiaEntrada, nameof(baleiaEntrada));
        referenciasPresentes &= ConferirReferencia(mariAbrigo, nameof(mariAbrigo));
        referenciasPresentes &= ConferirReferencia(paiAbrigo, nameof(paiAbrigo));
        referenciasPresentes &= ConferirReferencia(maeAbrigo, nameof(maeAbrigo));
        referenciasPresentes &= ConferirReferencia(baleiaAbrigo, nameof(baleiaAbrigo));
        if (!referenciasPresentes || !ReferenciasChegadaValidas() || !ReferenciasAbrigoValidas() || !ReferenciasTarefaValidas() || !ReferenciasLavagemValidas() || !ReferenciasSocorroValidas() || !ReferenciasFamiliaAlternativaValidas() || !ReferenciasEncerramentoValidas() || !ReferenciasEncenacaoValidas() || !ReferenciasRevistaValidas()) return false;
        return FalasValidas(antesSeparacao) && FalasValidas(barreira) &&
            FalasValidas(depoisSeparacao) && FalasValidas(entrada) &&
            FalasValidas(revista) && FalasValidas(abrigo) && ReferenciasBuscaValidas() && ReferenciasCampoValidas() && ReferenciasPostoValidas() && ReferenciasConsultaValidas() && ReferenciasMemoriaValidas() &&
            FalaValida(promessa) && FalaValida(semPromessa) && FalaValida(respostaZefinha);
    }

    private bool ReferenciasBuscaValidas()
    {
        bool referenciasPresentes = true;
        referenciasPresentes &= ConferirReferencia(areaAgua, nameof(areaAgua));
        referenciasPresentes &= ConferirReferencia(areaMae, nameof(areaMae));
        referenciasPresentes &= ConferirReferencia(areaMenino, nameof(areaMenino));
        referenciasPresentes &= ConferirReferencia(areaRegistro, nameof(areaRegistro));
        referenciasPresentes &= ConferirReferencia(areaPanela, nameof(areaPanela));
        referenciasPresentes &= ConferirReferencia(meninoBusca, nameof(meninoBusca));
        referenciasPresentes &= ConferirReferencia(meninoFimPasso, nameof(meninoFimPasso));
        referenciasPresentes &= ConferirReferencia(panelaBusca, nameof(panelaBusca));
        referenciasPresentes &= ConferirReferencia(panelaNoChao, nameof(panelaNoChao));
        referenciasPresentes &= ConferirReferencia(panelaRecolhida, nameof(panelaRecolhida));
        referenciasPresentes &= ConferirReferencia(lacoNoChao, nameof(lacoNoChao));
        referenciasPresentes &= ConferirReferencia(luziaRepouso, nameof(luziaRepouso));
        referenciasPresentes &= ConferirReferencia(seguidoraLuzia, nameof(seguidoraLuzia));
        referenciasPresentes &= ConferirReferencia(seguidoraMae, nameof(seguidoraMae));
        referenciasPresentes &= ConferirReferencia(canhotoCenario, nameof(canhotoCenario));
        referenciasPresentes &= ConferirReferencia(cabacaMesa, nameof(cabacaMesa));
        referenciasPresentes &= ConferirReferencia(guardaRegistro, nameof(guardaRegistro));
        referenciasPresentes &= ConferirReferencia(guardaRegistroParado, nameof(guardaRegistroParado));
        referenciasPresentes &= ConferirReferencia(guardaRegistroBloqueio, nameof(guardaRegistroBloqueio));
        referenciasPresentes &= ConferirReferencia(cabacaVaziaBusca, nameof(cabacaVaziaBusca));
        referenciasPresentes &= ConferirReferencia(cabacaCheiaBusca, nameof(cabacaCheiaBusca));
        referenciasPresentes &= ConferirReferencia(itemLaco, nameof(itemLaco));
        referenciasPresentes &= ConferirReferencia(itemCanhoto, nameof(itemCanhoto));
        if (!referenciasPresentes) return false;
        if (botoesEtapa2 == null || botoesEtapa2.Length != 4 ||
            objetivosBusca == null || objetivosBusca.Length != 6)
        {
            Debug.LogError("EntradaCurrais: confira os quatro botoesEtapa2 e os seis objetivosBusca.", this);
            return false;
        }
        foreach (Button botao in botoesEtapa2) if (botao == null) return false;
        Fala[][] sequencias = { aguaFuncionario, aguaAntesEscolha, aguaMae, aguaLuzia,
            meninoApresentacao, meninoEspera, meninoNome, meninoSaida,
            registroInicio, registroMae, registroLuzia, panelaInicio, panelaLaco, panelaCanhoto };
        foreach (Fala[] sequencia in sequencias) if (!FalasValidas(sequencia)) return false;
        return opcoesLuziaInicio != null && opcoesLuziaInicio.Length == 3 &&
            opcoesAgua != null && opcoesAgua.Length == 2 && opcoesMenino != null && opcoesMenino.Length == 3 &&
            opcoesRegistro != null && opcoesRegistro.Length == 2 && opcoesPanela != null && opcoesPanela.Length == 3;
    }

    private bool ConferirReferencia(UnityEngine.Object referencia, string campo)
    {
        if (referencia != null) return true;
        Debug.LogError("EntradaCurrais: referência ausente: " + campo +
            ". Objeto: " + name + "; cena: " + gameObject.scene.name + ".", this);
        return false;
    }

    private static bool FalaValida(Fala fala) => fala != null &&
        !string.IsNullOrWhiteSpace(fala.personagem) && !string.IsNullOrWhiteSpace(fala.texto);

    private static bool FalasValidas(Fala[] falas)
    {
        if (falas == null || falas.Length == 0) return false;
        foreach (Fala fala in falas) if (!FalaValida(fala)) return false;
        return true;
    }

    private void Update()
    {
        if (!pronto || PauseHelper.BlockInput() || Time.timeScale <= 0f) return;
        AtualizarPatrulha();
        if (dialogando)
        {
            if (Time.frameCount > quadroBloqueado && Input.GetMouseButtonDown(0) &&
                !PainelAuxiliarAberto() && !InventarioAbertoMemoria() && !SobreControle() &&
                (!leituraPaginaMemoria || (paginaMemoriaPodeAvancar && !InventarioAbertoMemoria())))
            {
                aguardandoClique = false;
                quadroBloqueado = Time.frameCount;
                InvestigationGuard.BlockCurrentClick();
            }
            return;
        }
        if (PainelAuxiliarAberto() || Time.frameCount <= quadroBloqueado) return;
        if (Dados.curraisEtapaEntrada == 2 || Dados.curraisEtapaEntrada == 3)
        {
            AtualizarInteracaoChegada();
            return;
        }
        if (!ocupado && !menuAberto && Dados.curraisEtapaEntrada == 4 && !InventarioAbertoMemoria() && Input.GetKeyDown(KeyCode.E))
        {
            Collider2D[] alvos = AlvosAtuais();
            Collider2D maisPerto = null;
            float menor = alcanceInteracao;
            foreach (Collider2D alvo in alvos)
            {
                if (alvo == null || !alvo.enabled || !alvo.gameObject.activeInHierarchy) continue;
                float distanciaAoAlvo = Mathf.Abs(mari.transform.position.x - alvo.bounds.center.x);
                if (distanciaAoAlvo < menor) { menor = distanciaAoAlvo; maisPerto = alvo; }
            }
            if (maisPerto != null) InteragirBusca(maisPerto.bounds.center);
            return;
        }
        if (ocupado || menuAberto || Time.frameCount <= quadroBloqueado ||
            !Input.GetMouseButtonDown(0) || SobreInterface()) return;
        Ray raio = cameraCena.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.forward, mari.transform.position);
        if (!plano.Raycast(raio, out float distanciaDoRaio)) return;
        Vector2 ponto = raio.GetPoint(distanciaDoRaio);
        if (Dados.curraisEtapaEntrada != 4) return;
        if (InteragirBusca(ponto)) return;
        foreach (Exame exame in exames)
        {
            if (exame.area != null && exame.area.enabled && exame.area.OverlapPoint(ponto))
            {
                Iniciar(Examinar(exame));
                return;
            }
        }
    }

    public bool BloqueiaCursor => !pronto || ocupado || menuAberto || dialogando || encerrando;

    public bool PonteiroSobreInteracao(Vector2 ponto)
    {
        if (!isActiveAndEnabled || BloqueiaCursor) return false;
        if (Dados.curraisEtapaEntrada == 2 || Dados.curraisEtapaEntrada == 3)
            return PonteiroSobreChegada(ponto);
        if (Dados.curraisEtapaEntrada != 4) return false;
        foreach (Collider2D alvo in AlvosAtuais())
            if (alvo != null && alvo.enabled && alvo.gameObject.activeInHierarchy && alvo.OverlapPoint(ponto)) return true;
        foreach (Exame exame in exames)
            if (exame.area != null && exame.area.enabled && exame.area.gameObject.activeInHierarchy && exame.area.OverlapPoint(ponto)) return true;
        return false;
    }

    private static bool PainelAuxiliarAberto()
    {
        InventoryTabController inventario = InventoryTabController.Instance;
        if (inventario == null) return false;
        CollectPrompt coleta = inventario.GetComponent<CollectPrompt>();
        CombinationPrompt combinacao = inventario.GetComponent<CombinationPrompt>();
        return (coleta != null && coleta.IsOpen) || (combinacao != null && combinacao.IsOpen);
    }

    private bool SobreInterface(Vector2? posicaoTela = null)
    {
        if (SobreControle(posicaoTela)) return true;
        InventoryTabController inventario = InventoryTabController.Instance;
        if (inventario != null && inventario.PonteiroSobrePainel(posicaoTela ?? (Vector2)Input.mousePosition))
            return true;
        EventSystem sistema = EventSystem.current;
        if (sistema == null || dialogoUI == null || !dialogoUI.activeInHierarchy) return false;
        PointerEventData ponteiro = new PointerEventData(sistema)
            { position = posicaoTela ?? (Vector2)Input.mousePosition };
        List<RaycastResult> resultados = new List<RaycastResult>();
        sistema.RaycastAll(ponteiro, resultados);
        foreach (RaycastResult resultado in resultados)
            if (resultado.gameObject != null && resultado.module is GraphicRaycaster &&
                resultado.gameObject.transform.IsChildOf(dialogoUI.transform)) return true;
        return false;
    }

    private static bool SobreControle(Vector2? posicaoTela = null)
    {
        EventSystem sistema = EventSystem.current;
        if (sistema == null) return false;
        PointerEventData ponteiro = new PointerEventData(sistema) { position = posicaoTela ?? (Vector2)Input.mousePosition };
        List<RaycastResult> resultados = new List<RaycastResult>();
        sistema.RaycastAll(ponteiro, resultados);
        foreach (RaycastResult resultado in resultados)
            if (resultado.gameObject != null && resultado.module is GraphicRaycaster &&
                (resultado.gameObject.GetComponentInParent<Button>() != null ||
                 resultado.gameObject.GetComponentInParent<Slider>() != null ||
                 resultado.gameObject.GetComponentInParent<Scrollbar>() != null ||
                 resultado.gameObject.GetComponentInParent<TMP_InputField>() != null ||
                 resultado.gameObject.GetComponentInParent<InventorySlot>() != null)) return true;
        return false;
    }

    private void Iniciar(IEnumerator sequencia)
    {
        if (ocupado || encerrando || sequencia == null) return;
        ocupado = true;
        PausarExploracao();
        InvestigationGuard.BlockCurrentClick();
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        StartCoroutine(Executar(sequencia));
    }

    private IEnumerator Executar(IEnumerator sequencia)
    {
        // Dirige também os enumeradores filhos: uma falha neles não deixa o
        // controlador ocupado para sempre nem depende da ordem das corrotinas do Unity.
        Stack<IEnumerator> pilha = new Stack<IEnumerator>();
        pilha.Push(sequencia);
        Exception falha = null;
        try
        {
            while (pilha.Count > 0)
            {
                IEnumerator atual = pilha.Peek();
                bool continuar = false;
                object espera = null;
                try
                {
                    continuar = atual.MoveNext();
                    if (continuar) espera = atual.Current;
                }
                catch (Exception erro)
                {
                    falha = erro;
                    Debug.LogException(erro, this);
                }
                if (falha != null) yield break;
                if (!continuar)
                {
                    pilha.Pop();
                    DescartarSequencia(atual);
                    continue;
                }
                if (espera is IEnumerator filha) pilha.Push(filha);
                else yield return espera;
            }
        }
        finally
        {
            while (pilha.Count > 0) DescartarSequencia(pilha.Pop());
            ocupado = false;
            if (!encerrando)
            {
                if (falha != null)
                {
                    menuAberto = false;
                    acoesMenu = null;
                    EncerrarGestoRevista();
                    RestaurarPlanoSepultamento();
                    RestaurarDesenhoSaida();
                    RestaurarApresentacaoMemoria();
                    if (fade != null) CorFade(0f);
                }
                if (!menuAberto)
                {
                    FecharDialogo();
                    LiberarMari();
                }
            }
        }
    }

    private void DescartarSequencia(IEnumerator sequencia)
    {
        try { (sequencia as IDisposable)?.Dispose(); }
        catch (Exception erro) { Debug.LogException(erro, this); }
    }

    private IEnumerator Separar()
    {
        // Um checkpoint de chegada repete a separação a partir da posição da própria cena.
        // Não reaproveita uma posição da Estrada ou de uma inicialização interrompida.
        mari.transform.position = inicioMariNaRua;
        Rigidbody2D corpoInicial = mari.GetComponent<Rigidbody2D>();
        if (corpoInicial != null)
        {
            corpoInicial.position = inicioMariNaRua;
            corpoInicial.linearVelocity = Vector2.zero;
        }
        Physics2D.SyncTransforms();
        rua.SetActive(true); campo.SetActive(false);
        irma.gameObject.SetActive(true);
        carroca.gameObject.SetActive(true);
        outraFila.SetActive(true);
        guardaRetaguarda.gameObject.SetActive(true);
        luzia.SetActive(false); lacoChao.SetActive(false); lacoPanela.SetActive(false);
        cameraCena.transform.position = cameraRua.position;
        CorFade(1f);
        yield return Esperar(tempoPlano);
        yield return Escurecer(0f);
        yield return Dizer(chegadaFamilia);
        yield return Dizer(antesSeparacao);
        yield return MarcharEscolta();
        yield return Dizer(ordemSeparar);
        yield return Mover(guardaRetaguarda, guardaSeparacao.position, tempoPlano);
        yield return Mover(irma, irmaSeparada.position + Vector3.up * ajustePivoIrma, tempoSeparacao);
        yield return Mover(cabo, caboBarreira.position, tempoPlano);
        yield return Dizer(barreira);
        yield return Mover(pai, paiReacao.position, tempoPlano);
        yield return Dizer(protestoPai);
        // O avanço da guarda, o corte e as falas mostram a origem da lesão.
        yield return Mover(guardaRetaguarda, guardaGolpe.position, tempoPlano * .5f);
        CorFade(1f);
        pai.localRotation = rotacaoPai;
        yield return Esperar(.15f);
        CorFade(0f);
        Dados.curraisPaiFerido = true;
        yield return AmpararPaiNaEntrada();
        yield return Dizer(reacaoGolpe);
        yield return Dizer(new[] { Dados.estradaSegundaEscolhaNoturna == 1 ? promessa : semPromessa, respostaZefinha });
        yield return PartirOutraFila();
        yield return Dizer(depoisSeparacao);
        yield return ChapterCard.Show("josefa-06");
        Dados.curraisEtapaEntrada = 2;
        Salvar();
        yield return Entrar();
    }

    private IEnumerator PartirOutraFila()
    {
        Vector3 origemIrma = irma.position, origemCarroca = carroca.position;
        Vector3 origemFila = outraFila.transform.position;
        Vector3 origemGuarda = guardaRetaguarda.position;
        Vector3 destinoIrma = irmaSaida.position + Vector3.up * ajustePivoIrma;
        destinoIrma.y = origemIrma.y;
        Vector3 deslocamento = destinoIrma - origemIrma;
        float tempo = 0f;
        AnimarSaidaDaFila(true);
        while (tempo < tempoCarroca)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                float t = Mathf.Clamp01(tempo / tempoCarroca);
                irma.position = Vector3.Lerp(origemIrma, destinoIrma, t);
                carroca.position = Vector3.Lerp(origemCarroca, carrocaSaida.position, t);
                outraFila.transform.position = origemFila + deslocamento * t;
                guardaRetaguarda.position = origemGuarda + deslocamento * t;
            }
            AnimarSaidaDaFila(!PauseHelper.BlockInput() && Time.timeScale > 0f);
            yield return null;
        }
        AnimarSaidaDaFila(false);
        carroca.gameObject.SetActive(false);
        irma.gameObject.SetActive(false);
        outraFila.SetActive(false);
        guardaRetaguarda.gameObject.SetActive(false);
    }

    private IEnumerator Entrar()
    {
        yield return Escurecer(1f);
        MostrarEntrada();
        yield return Escurecer(0f);
        if (!Dados.curraisRevistaApresentada)
        {
            yield return Dizer(entrada);
            Dados.curraisRevistaApresentada = true;
        }
        Dados.curraisEtapaEntrada = 2;
        AtualizarVisuaisChegada();
        objetivoTexto.text = string.Empty;
        Salvar();
    }

    private void MostrarEntrada()
    {
        rua.SetActive(false); campo.SetActive(true);
        Dados.curraisPaiFerido = true;
        carroca.gameObject.SetActive(false);
        guardaRetaguarda.gameObject.SetActive(false);
        irma.gameObject.SetActive(false); outraFila.SetActive(false);
        luzia.SetActive(true); lacoChao.SetActive(false);
        lacoPanela.SetActive(false);
        Posicionar(mari.transform, mariEntrada); Posicionar(pai, paiEntrada);
        Posicionar(mae, maeEntrada); Posicionar(baleia, baleiaEntrada);
        Transform enquadramento = Dados.curraisEtapaEntrada == 2 && cameraRevista != null ? cameraRevista : cameraCampo;
        cameraCena.transform.position = enquadramento.position;
    }

    private bool ReterPertences()
    {
        if (Dados.curraisPertencesRetidos) return true;
        // As entregas individuais já registraram os itens retidos. Não remover
        // silenciosamente o que ainda não foi entregue pelo jogador.
        if (InventoryManager.Instance == null || InventoryManager.Instance.Items.Count > 0) return false;
        Dados.curraisPertencesRetidos = true;
        return true;
    }

    private IEnumerator ChegarAbrigo()
    {
        yield return Escurecer(1f);
        MostrarAbrigo();
        yield return Escurecer(0f);
        if (!Dados.curraisAbrigoApresentado)
        {
            yield return Dizer(abrigo);
            Dados.curraisAbrigoApresentado = true;
        }
        if (Dados.curraisPaiAcomodado)
        {
            Dados.curraisEtapaEntrada = 4;
            AtualizarEstadoBusca();
            Salvar();
            yield break;
        }
        if (Dados.curraisPanoAbrigoRecolhido && !TemItem(itemPanoAbrigo))
            InventoryManager.Instance.AddItem(itemPanoAbrigo);
        Dados.curraisEtapaEntrada = 3;
        AtualizarVisuaisChegada();
        objetivoTexto.text = string.Empty;
        Salvar();
    }

    private void MostrarAbrigo()
    {
        MostrarEntrada();
        Posicionar(mari.transform, mariAbrigo); Posicionar(pai, paiAbrigo);
        Posicionar(mae, Dados.curraisPaiAcomodado ? maeCuidadoAbrigo : maeAbrigo); Posicionar(baleia, baleiaAbrigo);
        lacoPanela.SetActive(false);
        pai.localRotation = rotacaoPai;
    }

    private static void Posicionar(Transform alvo, Transform marca)
    {
        alvo.position = marca.position;
        Rigidbody2D corpo = alvo.GetComponent<Rigidbody2D>();
        if (corpo != null) { corpo.position = alvo.position; corpo.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private void PausarExploracao()
    {
        if (mari != null) mari.enabled = false;
        // Conversas comuns congelam o enquadramento atual, sem buscar marcas da cena.
        if (seguirCamera != null) seguirCamera.enabled = false;
        if (seguidores != null)
            foreach (SeguidorFamilia seguidor in seguidores)
                if (seguidor != null) seguidor.enabled = false;
        if (seguidoraLuzia != null) seguidoraLuzia.enabled = false;
        if (seguidoraMae != null) seguidoraMae.enabled = false;
        if (seguidorPaiConsulta != null) seguidorPaiConsulta.enabled = false;
    }

    private void LiberarMari()
    {
        bool livre = !encerrando && !ocupado && !menuAberto && !dialogando && !planoMemoriaAtivo;
        if (mari != null) mari.enabled = livre;
        if (seguirCamera != null)
        {
            if (livre) seguirCamera.RetomarSemReposicionar();
            else seguirCamera.enabled = false;
        }
    }

    private void FixarCameraDaSequencia()
    {
        if (cameraCena == null || cameraCena.gameObject.scene != gameObject.scene) return;
        if (seguirCamera != null) seguirCamera.enabled = false;
        Transform marca = campo != null && campo.activeInHierarchy
            ? (GameProgress.Instance != null && Dados.curraisEtapaEntrada == 2 && cameraRevista != null ? cameraRevista : cameraCampo)
            : cameraRua;
        if (marca != null) cameraCena.transform.position = marca.position;
        if (zoomInicial > 0f) cameraCena.orthographicSize = zoomInicial;
    }

    private IEnumerator Dizer(Fala[] falas)
    {
        dialogando = true;
        escolhas.SetActive(false);
        AbrirPainelDialogo();
        if (retrato != null) retrato.enabled = false;
        foreach (Fala fala in falas)
        {
            nomeTexto.text = fala.personagem;
            falaTexto.text = fala.texto;
            aguardandoClique = true;
            quadroBloqueado = Time.frameCount;
            while (aguardandoClique) yield return null;
        }
        FecharDialogo();
    }

    private void AbrirPainelDialogo()
    {
        dialogoUI.SetActive(true);
        painelDialogo.SetActive(true);
        // SetActive não restaura um CanvasGroup ocultado pelo modelo de diálogo.
        RestaurarGrupo(dialogoUI);
        RestaurarGrupo(painelDialogo);
        RestaurarGrupo(escolhas);
    }

    private static void RestaurarGrupo(GameObject painel)
    {
        CanvasGroup grupo = painel.GetComponent<CanvasGroup>();
        if (grupo == null) return;
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void FecharDialogo()
    {
        dialogando = false; aguardandoClique = false;
        if (escolhas != null) escolhas.SetActive(false);
        if (dialogoUI != null) dialogoUI.SetActive(false);
        quadroBloqueado = Time.frameCount;
        InvestigationGuard.BlockCurrentClick();
    }

    private void PrepararOpcoes()
    {
        for (int i = 0; i < botoesEtapa2.Length; i++)
        {
            int indice = i;
            botoesEtapa2[i].onClick.RemoveAllListeners();
            botoesEtapa2[i].onClick.AddListener(() => SelecionarOpcao(indice));
        }
    }

    private void SelecionarOpcao(int indice)
    {
        if (!menuAberto || Time.frameCount <= quadroBloqueado || PauseHelper.BlockInput() || Time.timeScale <= 0f ||
            acoesMenu == null || indice < 0 || indice >= acoesMenu.Length) return;
        Action acao = acoesMenu[indice];
        acoesMenu = null;
        menuAberto = false;
        FecharDialogo();
        acao?.Invoke();
        if (!ocupado && Dados.curraisEtapaEntrada >= 2) LiberarMari();
    }

    private void MostrarOpcoes(string personagem, string texto, string[] rotulos, Action[] acoes)
    {
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        PausarExploracao();
        InvestigationGuard.BlockCurrentClick();
        dialogando = false;
        menuAberto = true;
        acoesMenu = acoes;
        AbrirPainelDialogo(); escolhas.SetActive(true);
        nomeTexto.text = personagem; falaTexto.text = texto;
        if (retrato != null) retrato.enabled = false;
        for (int i = 0; i < botoesEtapa2.Length; i++)
        {
            Button botao = botoesEtapa2[i];
            bool mostrar = i < rotulos.Length;
            botao.gameObject.SetActive(mostrar);
            botao.interactable = mostrar;
            TMP_Text textoBotao = botao.GetComponentInChildren<TMP_Text>(true);
            if (mostrar && textoBotao != null) textoBotao.text = rotulos[i];
        }
        quadroBloqueado = Time.frameCount;
    }

    private IEnumerator Escolher(string personagem, string texto, string[] rotulos)
    {
        escolhaRecebida = -1;
        Action[] acoes = new Action[rotulos.Length];
        for (int i = 0; i < acoes.Length; i++)
        {
            int opcao = i;
            acoes[i] = () => escolhaRecebida = opcao;
        }
        MostrarOpcoes(personagem, texto, rotulos, acoes);
        while (escolhaRecebida < 0) yield return null;
    }

    private bool InteragirBusca(Vector2 ponto)
    {
        if (InteragirCriancaERegistros(ponto)) return true;
        if (Dados.curraisPanelaConcluida) return InteragirCampo(ponto);
        Collider2D[] areas = { areaLotePanos, areaLuzia, areaAgua, areaMae, areaPaiAbrigo, areaMenino, areaRegistro, areaPanela };
        foreach (Collider2D area in areas)
        {
            if (area == null || !area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            if (area == areaLotePanos) Iniciar(RecolherLotePanos());
            else if (area == areaLuzia)
            {
                if (Dados.curraisAguaRecebida && Dados.curraisEscolhaAgua == 0) Iniciar(DestinarAgua());
                else AbrirMenuLuzia();
            }
            else if (area == areaAgua)
            {
                if (!Dados.curraisDistribuicaoIniciada) Iniciar(Dizer(aguaAguardar));
                else if (!Dados.curraisAguaRecebida) Iniciar(ReceberAgua());
                else Iniciar(Dizer(aguaJaRecebida));
            }
            else if (area == areaMae)
            {
                MenuMaePrimeiroAbrigo();
            }
            else if (area == areaPaiAbrigo) Iniciar(ConversarPaiPrimeiroAbrigo());
            else if (area == areaMenino)
            {
                if (Dados.curraisEscolhaAgua == 0) objetivoTexto.text = string.Empty;
                else if (!Dados.curraisMeninoVisitado) Iniciar(ConhecerMenino());
                else Iniciar(ReencontrarMenino());
            }
            else if (area == areaRegistro)
            {
                if (Dados.curraisEscolhaAgua == 0) Iniciar(Dizer(mesaAntesAgua));
                else if (!Dados.curraisRegistroConcluido) Iniciar(ConsultarRegistro());
                else MenuTarefaPanos();
            }
            else if (area == areaPanela)
            {
                if (Dados.curraisRegistroConcluido && !Dados.curraisPanelaConcluida) Iniciar(EscolherPertence());
                else objetivoTexto.text = string.Empty;
            }
            return true;
        }
        return false;
    }

    private void AbrirMenuLuzia()
    {
        if (!Dados.curraisDistribuicaoIniciada)
        {
            MostrarOpcoes("Luzia", luziaBuscarAgua[0].texto, opcoesLuziaInicio, new Action[] {
                () => Iniciar(IniciarBuscaAguaAbrigo()),
                () => Iniciar(Dizer(abrigo)), FecharMenu });
        }
        else
            MostrarOpcoes("Luzia", Dados.curraisEscolhaAgua == 0 ? luziaBuscarAgua[0].texto : familiaDepoisAgua[0].texto,
                new[] { "Sobre a água", "Minha irmã", "Sair" },
                new Action[] { () => Iniciar(Dizer(Dados.curraisEscolhaAgua == 0 ? luziaBuscarAgua : familiaDepoisAgua)),
                    () => Iniciar(Dizer(luziaIrma)), FecharMenu });
    }

    private bool DevolverCabaca()
    {
        if (Dados.curraisCabacaDevolvida) return true;
        InventorySaveCatalog catalogo = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalogo == null || !catalogo.TryGetId(cabacaVaziaBusca, out string idVazia) ||
            !catalogo.TryGetId(cabacaCheiaBusca, out string idCheia)) return false;
        int indice = Dados.curraisItensRetidos.FindIndex(id => id == idVazia || id == idCheia);
        if (indice >= 0) Dados.curraisItensRetidos.RemoveAt(indice);
        // A cabaça visível na distribuição permite recuperar saves sem registro do recipiente.
        // Somente esta unidade é devolvida; os outros pertences permanecem retidos.
        if (!TemItem(cabacaVaziaBusca) && !TemItem(cabacaCheiaBusca))
            InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        if (!TemItem(cabacaVaziaBusca) && TemItem(cabacaCheiaBusca))
        {
            InventoryManager.Instance.RemoveItem(cabacaCheiaBusca);
            InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        }
        Dados.curraisCabacaDevolvida = true;
        return true;
    }

    private bool TemItem(ItemData itemBusca)
    {
        foreach (ItemData atual in InventoryManager.Instance.Items) if (atual == itemBusca) return true;
        return false;
    }

    private IEnumerator ReceberAgua()
    {
        if (!DevolverCabaca())
        {
            yield return Dizer(cabacaIndisponivel);
            yield break;
        }
        if (!Dados.curraisAguaOrientada)
        {
            yield return Dizer(orientacaoAgua);
            Dados.curraisAguaOrientada = true;
            AtualizarEstadoBusca(); Salvar();
            yield break;
        }
        if (!TemItem(cabacaVaziaBusca) || InventoryManager.Instance.SelectedItem != cabacaVaziaBusca)
        {
            yield return Dizer(aguaApresentar);
            yield break;
        }
        InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
        InventoryManager.Instance.AddItem(cabacaCheiaBusca);
        Dados.curraisAguaRecebida = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(aguaFuncionario);
    }

    private IEnumerator DestinarAgua()
    {
        if (Dados.curraisEscolhaAgua != 0) yield break;

        if (!TemItem(cabacaCheiaBusca))
        {
            yield return Dizer(maeEsperaAgua);
            yield break;
        }
        yield return Dizer(aguaAntesEscolha);
        yield return Escolher("Zefinha", perguntaAgua, opcoesAgua);
        int opcao = escolhaRecebida;
        if (opcao < 0 || opcao > 1 || !TemItem(cabacaCheiaBusca)) yield break;
        yield return BeberAguaAbrigo(opcao + 1);
        if (!TemItem(cabacaCheiaBusca)) yield break;
        InventoryManager.Instance.RemoveItem(cabacaCheiaBusca);
        InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        Dados.curraisEscolhaAgua = opcao + 1;
        GameProgress.Instance.UnlockDiaryPage(opcao == 0 ? "currais-agua-mae" : "currais-agua-luzia");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(opcao == 0 ? aguaMae : aguaLuzia);
    }

    private IEnumerator ConhecerMenino()
    {
        // A arte existente possui idle; a criança permanece no lugar enquanto responde.
        OlharParaMari(meninoBusca);
        yield return Dizer(meninoApresentacao);
        Dados.curraisMeninoVisitado = true;
        Salvar();
        AbrirMenuMenino();
    }

    private void AbrirMenuMenino()
    {
        OlharParaMari(meninoBusca);
        MostrarOpcoes("Criança", (Dados.curraisPerguntasMenino & 2) != 0
                ? meninoNomeLembrado[meninoNomeLembrado.Length - 1].texto
                : meninoApresentacao[meninoApresentacao.Length - 1].texto,
            opcoesMenino, new Action[] {
                () => Iniciar(ResponderMenino(1, meninoEspera)),
                () => Iniciar(ResponderMenino(2, meninoNome)),
                () => Iniciar(Dizer(meninoSaida)) });
    }

    private IEnumerator ResponderMenino(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisPerguntasMenino |= bit;
        GameProgress.Instance.UnlockDiaryPage(bit == 1 ? "currais-menino-espera" : "currais-menino-nome");
        Salvar();
        AbrirMenuMenino();
    }

    private Fala[] ComAcompanhante(Fala[] falas)
    {
        Fala[] resultado = new Fala[falas.Length];
        for (int i = 0; i < falas.Length; i++)
            resultado[i] = new Fala {
                personagem = falas[i].personagem == "ACOMPANHANTE"
                    ? (Dados.curraisEscolhaAgua == 1 ? "Mãe" : "Luzia") : falas[i].personagem,
                texto = falas[i].texto };
        return resultado;
    }

    private IEnumerator ConsultarRegistro()
    {
        yield return ConsultarFolhaNaMesa();
        yield return Dizer(ComAcompanhante(pedidoPanos));
        Dados.curraisTarefaPanosIniciada = true;
        Dados.curraisRegistroConcluido = true;
        GameProgress.Instance.UnlockDiaryPage("currais-registro");
        AtualizarEstadoBusca(); Salvar();
        MenuTarefaPanos();
    }

    private IEnumerator RelembrarRegistro()
    {
        yield return Dizer(ComAcompanhante(registroInicio));
        yield return Dizer(Dados.curraisEscolhaAgua == 1 ? registroMae : registroLuzia);
        yield return ConsultarFolhaNaMesa();
    }

    private IEnumerator EscolherPertence()
    {
        if (Dados.curraisPanelaConcluida || !Dados.curraisLotePanosRecolhido) yield break;
        yield return Dizer(panelaInicio);
        yield return Escolher("Zefinha", perguntaPanela, opcoesPanela);
        if (escolhaRecebida < 0 || escolhaRecebida > 1) yield break;
        bool perguntar = escolhaRecebida == 0;
        if (perguntar)
        {
            yield return ExaminarOuColetar(itemCanhoto);
            if (!coletaDoCenarioConfirmada) yield break;
            if (!TemItem(itemCanhoto)) InventoryManager.Instance.AddItem(itemCanhoto);
            Dados.curraisDestinoConfirmado = true;
        }
        // O laço foi retirado da chegada. Mantém-se apenas o estado de compatibilidade.
        Dados.curraisEscolhaPertence = perguntar ? 2 : 0;
        Dados.curraisLacoGuardado = false;
        Dados.curraisLacoNaPanela = false;
        Dados.curraisPanelaConcluida = true;
        if (perguntar) GameProgress.Instance.UnlockDiaryPage("currais-canhoto");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(perguntar ? panelaCanhoto : seguirPanos);
    }

    private string ObjetivoAtual() => string.Empty;

    private void AtualizarEstadoBusca()
    {
        if (Dados.curraisEtapaEntrada != 4) return;
        AtualizarVisuaisChegada();
        AtualizarTarefaPanos();
        objetivoTexto.text = string.Empty;
        bool panelaAcessivel = Dados.curraisLotePanosRecolhido && !Dados.curraisPanelaConcluida;
        if (panelaAcessivel)
        {
            panelaBusca.position = panelaNoChao.position;
            lacoPanela.transform.position = lacoNoChao.position;
        }
        else if (Dados.curraisPanelaConcluida) panelaBusca.position = panelaRecolhida.position;
        canhotoCenario.SetActive(panelaAcessivel);
        cabacaMesa.SetActive(!Dados.curraisCabacaDevolvida);
        lacoPanela.SetActive(false);
        areaPanela.enabled = panelaAcessivel;
        meninoBusca.gameObject.SetActive(Dados.curraisEscolhaAgua != 0);
        // Conversar não muda a posição editada da criança.
        if (Dados.curraisEscolhaAgua == 1 &&
            (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca == 0))
        {
            luzia.transform.position = luziaRepouso.position;
            luzia.transform.localRotation = Quaternion.identity;
        }
        else if (Dados.curraisEscolhaAgua == 2 &&
            (!ConsultaDisponivel || Dados.curraisEtapaConsulta == 0))
        {
            Posicionar(mae, maeCuidadoAbrigo);
        }
        AtualizarCampo();
        Physics2D.SyncTransforms();
    }

    private void RestaurarAcompanhante()
    {
        if (ConsultaDisponivel && Dados.curraisEtapaConsulta >= 1)
        { RestaurarFamiliaConsulta(); return; }
        if (PostoDisponivel && Dados.curraisEtapaPosto >= 2)
        { Posicionar(luzia.transform, Dados.curraisEtapaPosto == 4 ? luziaNoite : luziaRepouso); return; }
        if (Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 1)
        { RestaurarLuziaNaBusca(); return; }
        if (!Dados.curraisDistribuicaoIniciada) return;
        SeguidorFamilia seguidor = Dados.curraisEscolhaAgua == 1 ? seguidoraMae : seguidoraLuzia;
        Vector3 posicao = seguidor.transform.position;
        posicao.x = mari.transform.position.x - seguidor.DistanciaAtras;
        seguidor.transform.position = posicao;
        Physics2D.SyncTransforms();
    }

    private void LateUpdate()
    {
        if (!pronto) return;
        if (Dados.curraisEtapaEntrada == 2 || Dados.curraisEtapaEntrada == 3) AtualizarVisuaisChegada();
        // Marcas de câmera são aplicadas somente pela inicialização/transições.
        // Uma fala na revista ou no abrigo não reposiciona nem redefine o zoom.
        if (Dados.curraisEtapaEntrada != 4) return;
        bool livre = !ocupado && !menuAberto && !dialogando && !PauseHelper.BlockInput();
        if (planoMemoriaAtivo)
        {
            cameraCena.transform.position = cameraPlanoMemoria.position;
            seguidorPaiConsulta.enabled = false; seguidoraMae.enabled = false; seguidoraLuzia.enabled = false;
            return;
        }
        if (ConsultaDisponivel) { AtualizarAcompanhamentoConsulta(livre); return; }
        bool buscaComLuzia = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 1 &&
            (!PostoDisponivel || Dados.curraisEtapaPosto < 2);
        bool luziaAcompanha = buscaComLuzia || (!PostoDisponivel && Dados.curraisDistribuicaoIniciada && Dados.curraisEscolhaAgua != 1);
        if (seguidoraLuzia.enabled != (livre && luziaAcompanha)) seguidoraLuzia.enabled = livre && luziaAcompanha;
        bool maeAcompanha = livre && !PostoDisponivel && !buscaComLuzia && Dados.curraisEscolhaAgua == 1;
        if (seguidoraMae.enabled != maeAcompanha) seguidoraMae.enabled = maeAcompanha;
    }

    private void FecharMenu()
    {
        if (PauseHelper.BlockInput()) return;
        menuAberto = false; FecharDialogo(); LiberarMari();
    }

    private IEnumerator Examinar(Exame exame)
    {
        yield return Dizer(new[] { new Fala { personagem = exame.titulo, texto = exame.texto } });
    }

    private IEnumerator Mover(Transform alvo, Vector3 destino, float duracao)
    {
        Vector3 origem = alvo.position;
        if (alvo == mari.transform || alvo == pai || alvo == mae || alvo == irma ||
            alvo == baleia || alvo == cabo || alvo == guardaRetaguarda || alvo == bento || alvo == meninoBusca)
            destino.y = origem.y;
        float tempo = 0f;
        Animator animador = alvo.GetComponent<Animator>();
        if (animador != null && animador.enabled) animador.SetBool("IsWalking", true);
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                alvo.position = Vector3.Lerp(origem, destino, Mathf.Clamp01(tempo / duracao));
                Rigidbody2D corpoEmMovimento = alvo.GetComponent<Rigidbody2D>();
                if (corpoEmMovimento != null)
                { corpoEmMovimento.position = alvo.position; corpoEmMovimento.linearVelocity = Vector2.zero; }
                if (animador != null && animador.enabled) animador.SetBool("IsWalking", true);
                SpriteRenderer visual = alvo.GetComponent<SpriteRenderer>();
                if (visual != null && Mathf.Abs(destino.x - origem.x) > .01f)
                    visual.flipX = (destino.x - origem.x) * alvo.lossyScale.x < 0f;
            }
            else if (animador != null && animador.enabled) animador.SetBool("IsWalking", false);
            yield return null;
        }
        if (animador != null && animador.enabled) animador.SetBool("IsWalking", false);
        Rigidbody2D corpo = alvo.GetComponent<Rigidbody2D>();
        if (corpo != null) { corpo.position = alvo.position; corpo.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private IEnumerator Esperar(float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Escurecer(float alpha)
    {
        float origem = fade.color.a, tempo = 0f;
        fade.gameObject.SetActive(true);
        fade.raycastTarget = false;
        while (tempo < tempoFade)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                CorFade(Mathf.Lerp(origem, alpha, Mathf.Clamp01(tempo / tempoFade)));
            }
            yield return null;
        }
        CorFade(alpha);
    }

    private void CorFade(float alpha)
    {
        fade.gameObject.SetActive(true);
        fade.color = new Color(0f, 0f, 0f, alpha);
        fade.raycastTarget = false;
    }

    private void Salvar()
    {
        if (GameSaveSystem.IsRestoring || !Dados.CanSave) return;
        GameSaveSystem sistema = GameProgress.Instance.GetComponent<GameSaveSystem>();
        if (sistema != null) sistema.TrySaveCheckpoint(gameObject.scene.name);
    }

    private void OnDisable()
    {
        encerrando = true;
        StopAllCoroutines();
        EncerrarGestoRevista();
        RestaurarPlanoSepultamento();
        RestaurarDesenhoSaida();
        RestaurarApresentacaoMemoria();
        menuAberto = false;
        ocupado = false;
        acoesMenu = null;
        FecharDialogo();
        if (mari != null) mari.enabled = true;
        if (seguirCamera != null) seguirCamera.enabled = true;
        if (botoesEtapa2 != null)
            foreach (Button botao in botoesEtapa2)
                if (botao != null) botao.onClick.RemoveAllListeners();
        if (seguidoraLuzia != null) seguidoraLuzia.enabled = false;
        if (seguidoraMae != null) seguidoraMae.enabled = false;
        if (seguidorPaiConsulta != null) seguidorPaiConsulta.enabled = false;
    }
}
