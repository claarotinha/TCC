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
    [Header("Limites externos - editáveis na Hierarchy")]
    [SerializeField] private Collider2D limitesExternosRua, limitesExternosCampo;
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
    private bool PodeAtualizarCena => pronto && !encerrando && isActiveAndEnabled &&
        !GameSaveSystem.IsRestoring && GameProgress.Instance != null && GameProgress.Instance.Data != null;
    private bool pronto, ocupado, dialogando, aguardandoClique, menuAberto, encerrando;
    private int quadroBloqueado;
    private Quaternion rotacaoPai;
    private float ajustePivoIrma;
    private float zoomInicial;
    private Vector3 inicioMariNaRua;

    private void ConfigurarLimitesExternos()
    {
        Collider2D limites = campo != null && campo.activeInHierarchy
            ? limitesExternosCampo : limitesExternosRua;
        if (NegacaoEmCurso && limitesNegacao != null) limites = limitesNegacao;
        if (limites == null) return;
        // A negação usa temporariamente o espaço do abrigo; depois restaura o cenário completo.
        if (mari != null) mari.DefinirLimites(limites);
        if (seguirCamera != null) seguirCamera.DefinirLimites(limites, true);
    }

    [SerializeField] private GameObject fundoPretoSeparacao;

    private void Awake()
    {
        if (fundoPretoSeparacao != null) fundoPretoSeparacao.SetActive(true);
        ConfigurarLimitesExternos();
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
        PrepararFalasDasIrmas();
        if (!ReferenciasValidas())
        {
            Debug.LogError("EntradaCurrais: inicialização interrompida em " + name + ".\n" + DiagnosticarConfiguracaoEntrada(), this);
            FecharDialogo();
            if (fade != null) CorFade(0f);
            if (fundoPretoSeparacao != null) fundoPretoSeparacao.SetActive(false);
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
            if (fundoPretoSeparacao != null) fundoPretoSeparacao.SetActive(false);
            LiberarMari();
            yield break;
        }
        Vector3 posicaoRestaurada = mari.transform.position;
        GameProgress.Instance.NormalizeData();
        bool testePatio = PrepararTestePatio(restaurandoPartida);
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
        PrepararDespedidaEQuarto();
        pronto = true;
        if (testePatio)
        {
            MostrarAbrigo();
            AtualizarEstadoBusca();
            PosicionarInicioTestePatio();
            RetomarExploracao(mari.transform.position, false, false);
            RestaurarAcompanhante();
            yield break;
        }
        int etapa = Mathf.Clamp(Dados.curraisEtapaEntrada, 0, 4);
        if (fundoPretoSeparacao != null) fundoPretoSeparacao.SetActive(etapa == 0 && Dados.curraisSeparacaoFundoPreto);
        Dados.curraisEtapaEntrada = etapa;
        if (etapa == 0) Iniciar(Separar());
        else if (etapa == 2 && Dados.curraisRevistaApresentada)
        {
            MostrarEntrada();
            RetomarExploracao(posicaoRestaurada, restaurandoPartida, restaurandoPosicao);
            if (restaurandoPartida) RestaurarFamiliaNaCaminhada();
        }
        else if (etapa == 1 || etapa == 2) Iniciar(Entrar());
        else if (etapa == 3)
        {
            MostrarAbrigo();
            RetomarExploracao(posicaoRestaurada, restaurandoPartida, restaurandoPosicao);
            if (restaurandoPartida) RestaurarFamiliaNaCaminhada();
        }
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
        referenciasPresentes &= ConferirReferencia(fundoPretoSeparacao, nameof(fundoPretoSeparacao));
        referenciasPresentes &= FalasValidas(chegadaAlegreFamilia) & FalasValidas(chegadaAlegreFortaleza) & FalasValidas(irmasAntesSeparacao);
        if (quadrosEntrada == null || quadrosEntrada.Length != 12)
        {
            Debug.LogError("EntradaCurrais: configure os 12 quadrosEntrada na ordem da cutscene.", this);
            referenciasPresentes = false;
        }
        else for (int i = 0; i < quadrosEntrada.Length; i++)
            referenciasPresentes &= ConferirReferencia(quadrosEntrada[i], "quadrosEntrada[" + i + "]");
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

    private string DiagnosticarConfiguracaoEntrada()
    {
        var problemas = new List<string>();
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic;
        // Os validadores antigos podem retornar false sem emitir mensagem.
        // Executa cada setor separadamente para que um não esconda os demais.
        foreach (var metodo in typeof(EntradaCurrais).GetMethods(flags))
        {
            if (!metodo.Name.StartsWith("Referencias") || !metodo.Name.EndsWith("Validas") ||
                metodo.Name == nameof(ReferenciasValidas) || metodo.ReturnType != typeof(bool) ||
                metodo.GetParameters().Length != 0) continue;
            try
            {
                if (!(bool)metodo.Invoke(this, null)) problemas.Add("Setor inválido: " + metodo.Name);
            }
            catch (System.Reflection.TargetInvocationException erro)
            {
                problemas.Add(metodo.Name + ": " + (erro.InnerException ?? erro).Message);
            }
        }
        foreach (var campo in typeof(EntradaCurrais).GetFields(flags))
        {
            if (!Attribute.IsDefined(campo, typeof(SerializeField))) continue;
            object valor = campo.GetValue(this);
            if (campo.FieldType == typeof(Fala[]) && !FalasValidas((Fala[])valor))
                problemas.Add("Falas vazias ou incompletas: " + campo.Name);
            else if (campo.FieldType == typeof(Fala) && !FalaValida((Fala)valor))
                problemas.Add("Fala incompleta: " + campo.Name);
            else if (typeof(UnityEngine.Object).IsAssignableFrom(campo.FieldType) &&
                (UnityEngine.Object)valor == null)
                problemas.Add("Campo vazio: " + campo.Name);
            else if (valor is Array itens && typeof(UnityEngine.Object).IsAssignableFrom(campo.FieldType.GetElementType()))
            {
                for (int i = 0; i < itens.Length; i++)
                    if ((UnityEngine.Object)itens.GetValue(i) == null)
                        problemas.Add("Referência vazia: " + campo.Name + "[" + i + "]");
            }
        }
        if (quadrosEntrada == null || quadrosEntrada.Length != 12)
            problemas.Add("quadrosEntrada: são necessárias 12 imagens.");
        if (!PresencasValidas()) problemas.Add("pessoasPresentes: confira os personagens e desenhos indicados no Console.");
        if (problemas.Count == 0)
            problemas.Add("A configuração mudou durante a validação. Reabra a cena salva e confira as mensagens anteriores do Console.");
        return string.Join("\n", problemas);
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
        if (!PodeAtualizarCena || PauseHelper.BlockInput() || Time.timeScale <= 0f) return;
        // Closing Examine takes priority over UI controls and exploration.
        if (ExamineObject.IsShowing())
        {
            if (Input.GetMouseButtonDown(0)) ExamineObject.TryCloseCurrentPanelFromPointer();
            return;
        }
        if (AtualizarExamineNomes()) return;
        if (AtualizarExamineDistribuicao() || AtualizarExamineLuzia()) return;
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
        if (Dados.curraisEtapaEntrada == 4 && AtualizarTentativaDePassagem()) return;
        if (Dados.curraisEtapaEntrada == 4 && AtualizarChegadaAbrigoFinal()) return;
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
            else ConversarPresencaProxima();
            return;
        }
        if (ocupado || menuAberto || Time.frameCount <= quadroBloqueado ||
            !Input.GetMouseButtonDown(0) || SobreInterface()) return;
        Ray raio = cameraCena.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.forward, mari.transform.position);
        if (!plano.Raycast(raio, out float distanciaDoRaio)) return;
        Vector2 ponto = raio.GetPoint(distanciaDoRaio);
        if (Dados.curraisEtapaEntrada != 4) return;
        if (InteragirBusca(ponto) || InteragirPresenca(ponto)) return;
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
            return PonteiroSobreChegada(ponto) || SobrePresenca(ponto);
        if (Dados.curraisEtapaEntrada != 4) return false;
        foreach (Collider2D alvo in AlvosAtuais())
            if (alvo != null && alvo.enabled && alvo.gameObject.activeInHierarchy && alvo.OverlapPoint(ponto)) return true;
        if (SobrePresenca(ponto)) return true;
        foreach (Exame exame in exames)
            if (exame.area != null && exame.area.enabled && exame.area.gameObject.activeInHierarchy && exame.area.OverlapPoint(ponto)) return true;
        return false;
    }

    private static bool PainelAuxiliarAberto()
    {
        if (ExamineObject.IsShowing()) return true;
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
                    EncerrarLavagemELembranca();
                    EncerrarPatioDetalhado();
                    EncerrarDespedidaEQuarto();
                    EncerrarNoiteFamilia();
                    EncerrarDetalhesConsulta();
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

    [Header("Cutscene de entrada — ordem das imagens na Hierarchy")]
    [SerializeField] private GameObject[] quadrosEntrada;
    [SerializeField] private Fala[] chegadaAlegreFamilia, chegadaAlegreFortaleza;
    [SerializeField] private Fala[] irmasAntesSeparacao;

    private void PrepararFalasDasIrmas()
    {
        // Cenas salvas antes da inclusão deste campo podem carregá-lo vazio.
        // Restaura o diálogo aprovado antes de validar a entrada; textos válidos
        // editados na Hierarchy continuam sendo usados.
        if (FalasValidas(irmasAntesSeparacao)) return;
        irmasAntesSeparacao = new[]
        {
            new Fala { personagem = "Mariquinha", texto = "Fique comigo, Zefinha." },
            new Fala { personagem = "Zefinha", texto = "Tô aqui." }
        };
    }

    private void MostrarQuadroEntrada(int indice)
    {
        fundoPretoSeparacao.SetActive(true);
        for (int i = 0; i < quadrosEntrada.Length; i++)
            quadrosEntrada[i].SetActive(i == indice);
    }

    private IEnumerator DizerTrechoEntrada(Fala[] falas, int inicio, int quantidade)
    {
        Fala[] trecho = new Fala[quantidade];
        Array.Copy(falas, inicio, trecho, 0, quantidade);
        yield return Dizer(trecho);
    }

    private IEnumerator Separar()
    {
        // A chegada é contada pelas imagens; os atores do cenário não encenam
        // novamente a escolta por baixo da cutscene.
        rua.SetActive(true); campo.SetActive(false);
        luzia.SetActive(true); lacoChao.SetActive(false); lacoPanela.SetActive(false);
        CorFade(1f);
        MostrarQuadroEntrada(0);
        yield return Escurecer(0f);
        yield return Dizer(chegadaAlegreFamilia);
        MostrarQuadroEntrada(1);
        yield return Dizer(chegadaAlegreFortaleza);
        MostrarQuadroEntrada(2);
        yield return Dizer(antesSeparacao);
        MostrarQuadroEntrada(3);
        yield return DizerEntradaUmaVez("aviso-pertences", new[] { entrada[0] });
        MostrarQuadroEntrada(4);
        yield return Esperar(1.2f);
        MostrarQuadroEntrada(5);
        yield return Dizer(irmasAntesSeparacao);
        yield return DizerTrechoEntrada(ordemSeparar, 0, ordemSeparar.Length - 1);
        MostrarQuadroEntrada(6);
        yield return DizerTrechoEntrada(ordemSeparar, ordemSeparar.Length - 1, 1);
        MostrarQuadroEntrada(7);
        yield return Dizer(barreira);
        MostrarQuadroEntrada(8);
        yield return Esperar(1f);
        MostrarQuadroEntrada(9);
        yield return Dizer(protestoPai);
        MostrarQuadroEntrada(10);
        Dados.curraisPaiFerido = true;
        yield return Dizer(reacaoGolpe);
        MostrarQuadroEntrada(11);
        yield return Dizer(new[] { Dados.estradaSegundaEscolhaNoturna == 1 ? promessa : semPromessa, respostaZefinha });
        yield return Dizer(depoisSeparacao);
        yield return Escurecer(1f);
        fundoPretoSeparacao.SetActive(false);
        yield return ChapterCard.Show("josefa-06");
        Dados.curraisEtapaEntrada = 2;
        Dados.curraisSeparacaoFundoPreto = false;
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
            int inicio = ViuConversaEntrada("aviso-pertences") ? 1 : 0;
            Fala[] chegadaRevista = new Fala[entrada.Length - inicio];
            Array.Copy(entrada, inicio, chegadaRevista, 0, chegadaRevista.Length);
            if (chegadaRevista.Length > 0) yield return Dizer(chegadaRevista);
            RegistrarConversaEntrada("aviso-pertences");
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
        ConfigurarLimitesExternos();
        Dados.curraisPaiFerido = true;
        carroca.gameObject.SetActive(false);
        guardaRetaguarda.gameObject.SetActive(false);
        irma.gameObject.SetActive(false); outraFila.SetActive(false);
        luzia.SetActive(true); lacoChao.SetActive(false);
        lacoPanela.SetActive(false);
        Posicionar(mari.transform, mariEntrada); Posicionar(pai, paiEntrada);
        Posicionar(mae, maeEntrada); Posicionar(baleia, baleiaEntrada);
        FormarFilaDaFamilia(false);
        Transform enquadramento = Dados.curraisEtapaEntrada == 2 && cameraRevista != null ? cameraRevista : cameraCampo;
        cameraCena.transform.position = enquadramento.position;
    }

    private bool ReterPertences()
    {
        if (Dados.curraisPertencesRetidos) return true;
        if (dispensarPertencesParaTeste)
        {
            Dados.curraisPertencesRetidos = true;
            return true;
        }
        // As entregas individuais já registraram os itens retidos. Não remover
        // silenciosamente o que ainda não foi entregue pelo jogador.
        if (InventoryManager.Instance == null || InventoryManager.Instance.Items.Count > 0) return false;
        Dados.curraisPertencesRetidos = true;
        return true;
    }

    private IEnumerator ChegarAbrigo()
    {
        // Depois da revista, continuam de onde estão. Nada de salto ao abrigo.
        Dados.curraisEtapaEntrada = 3;
        Dados.curraisAbrigoApresentado = true;
        AtualizarVisuaisChegada();
        Salvar();
        yield break;
    }

    private void MostrarAbrigo()
    {
        MostrarEntrada();
        if (!Dados.curraisFamiliaParou) return;
        Posicionar(mari.transform, mariAbrigo); Posicionar(pai, paiAbrigo);
        Posicionar(mae, maeAbrigo); Posicionar(baleia, baleiaAbrigo);
        if (Dados.curraisParadaRegistrada && Dados.curraisEtapaBusca == 0)
        {
            RestaurarXParada(pai, Dados.curraisParadaPaiX);
            RestaurarXParada(mae, Dados.curraisParadaMaeX);
            RestaurarXParada(baleia, Dados.curraisParadaBaleiaX);
        }
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
        bool livre = !encerrando && !ocupado && !menuAberto && !dialogando && !ExamineObject.IsShowing() && !planoMemoriaAtivo;
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
            nomeTexto.text = NomeDaPessoa(fala.personagem);
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
        if (personagem == "Luzia" && !Dados.curraisLuziaApresentada)
        {
            SolicitarContatoLuzia(() => MostrarOpcoes(personagem, texto, rotulos, acoes));
            return;
        }
        if (rotulos.Length > 3)
        { MostrarPaginaAssuntos(personagem, texto, rotulos, acoes, 0); return; }
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        PausarExploracao();
        InvestigationGuard.BlockCurrentClick();
        dialogando = false;
        menuAberto = true;
        acoesMenu = acoes;
        AbrirPainelDialogo(); escolhas.SetActive(true);
        nomeTexto.text = NomeDaPessoa(personagem);
        falaTexto.text = string.IsNullOrWhiteSpace(texto) ? "..." : texto;
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
        if (SobreGuardaPorcoSaida(ponto)) { MenuPedidoSaida(); return true; }
        if (PodeRecolherCabacaMesa && AreaCabacaMesa != null && AreaCabacaMesa.gameObject.activeInHierarchy && AreaCabacaMesa.OverlapPoint(ponto))
        { Iniciar(RecolherCabacaMesa()); return true; }
        if (areaPanoAbrigo.enabled && areaPanoAbrigo.OverlapPoint(ponto))
        { Iniciar(RecolherPanoAbrigo()); return true; }
        if (InteragirCriancaERegistros(ponto)) return true;
        if (!Dados.curraisPanelaConcluida && Dados.curraisLotePanosRecolhido)
        {
            if (areaRaimundo.enabled && areaRaimundo.OverlapPoint(ponto))
            { Iniciar(RetomarCaminhoDoLote(true)); return true; }
            if ((areaLavadeira.enabled && areaLavadeira.OverlapPoint(ponto)) ||
                (areaRecebimentoLavagem.enabled && areaRecebimentoLavagem.OverlapPoint(ponto)))
            { Iniciar(RetomarCaminhoDoLote(false)); return true; }
        }
        if (Dados.curraisPanelaConcluida) return InteragirCampo(ponto);
        Collider2D[] areas = { areaLotePanos, areaLuzia, areaAgua, areaDistribuicaoAgua, areaMae, areaPaiAbrigo, areaMenino, areaRegistro, areaPanela };
        foreach (Collider2D area in areas)
        {
            if (area == null || !area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            if (area == areaLotePanos) Iniciar(RecolherLotePanos());
            else if (area == areaLuzia)
            {
                AbrirMenuLuzia();
            }
            else if (area == areaAgua || area == areaDistribuicaoAgua)
            {
                MenuFuncionarioAgua();
            }
            else if (area == areaMae)
            {
                MenuMaePrimeiroAbrigo();
            }
            else if (area == areaPaiAbrigo) MenuPaiPrimeiroAbrigo();
            else if (area == areaMenino) MenuCriancaNomes();
            else if (area == areaRegistro)
            {
                MenuPrimeiroRegistro();
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
        if (RetomarAguaPeloClique()) return;
        if (!Dados.curraisPanelaConcluida && !ConviteAguaConhecido)
        { MenuLuziaAbrigoLinear(); return; }
        if (!Dados.curraisPanelaConcluida) { MenuLuziaAbrigoLinear(); return; }
        if (Dados.curraisAguaRecebida && Dados.curraisEscolhaAgua == 0)
        {
            AbrirAssuntosEntrada("luzia-agua-recebida", "Luzia", "Você conseguiu água?",
                new[] { "familia-espera-agua", "luzia-irma" },
                new[] { "Como a senhora tá?", "Perguntar por Mariquinha" },
                new Func<IEnumerator>[] { () => Dizer(maeEsperaAgua), () => Dizer(luziaIrma) });
            return;
        }
        AbrirAssuntosEntrada("luzia-primeiro-abrigo", "Luzia", "Pode falar, Josefa.",
            new[] { Dados.curraisEscolhaAgua == 0 ? "luzia-buscar-agua" : "familia-depois-agua", "luzia-irma" },
            new[] { "Sobre a água", "Minha irmã" },
            new Func<IEnumerator>[] {
                () => Dados.curraisEscolhaAgua == 0 ? IniciarBuscaAguaAbrigo() : Dizer(familiaDepoisAgua),
                () => Dizer(luziaIrma) });
    }

    private Collider2D areaCabacaMesaCache;
    private Collider2D AreaCabacaMesa
    {
        get
        {
            if (areaCabacaMesaCache == null && cabacaMesa != null)
                areaCabacaMesaCache = cabacaMesa.GetComponent<Collider2D>();
            return areaCabacaMesaCache;
        }
    }

    private bool PodeRecolherCabacaMesa => Dados.curraisPertencesRetidos &&
        !Dados.curraisCabacaDevolvida && CabacaAutorizada && Dados.curraisEtapaEntrada == 4 &&
        Dados.curraisEtapaEntrada <= 4 && !Dados.curraisPanelaConcluida;

    private void AtualizarCabacaMesa()
    {
        if (cabacaMesa == null) return;
        cabacaMesa.SetActive(!Dados.curraisCabacaDevolvida);
        if (AreaCabacaMesa != null) AreaCabacaMesa.enabled = PodeRecolherCabacaMesa;
    }

    private IEnumerator RecolherCabacaMesa()
    {
        if (!PodeRecolherCabacaMesa) yield break;
        InventorySaveCatalog catalogo = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalogo == null || !catalogo.TryGetId(cabacaVaziaBusca, out string idVazia) ||
            !catalogo.TryGetId(cabacaCheiaBusca, out string idCheia))
        {
            yield return Dizer(cabacaIndisponivel);
            yield break;
        }
        yield return ExaminarOuColetar(cabacaVaziaBusca);
        if (!coletaDoCenarioConfirmada) yield break;
        // CollectPrompt já adicionou exatamente uma unidade ao confirmar.
        int indice = Dados.curraisItensRetidos.FindIndex(id => id == idVazia || id == idCheia);
        if (indice >= 0) Dados.curraisItensRetidos.RemoveAt(indice);
        Dados.curraisCabacaDevolvida = true;
        AtualizarVisuaisChegada(); AtualizarEstadoBusca(); Salvar();
        yield return ChamarVezDaAgua();
    }

    private bool TemItem(ItemData itemBusca)
    {
        foreach (ItemData atual in InventoryManager.Instance.Items) if (atual == itemBusca) return true;
        return false;
    }

    private IEnumerator ReceberAgua()
    {
        if (!PodeEncherCabaca || !TemItem(cabacaVaziaBusca)) yield break;
        yield return Dizer(apresentacaoPorcaoAgua);
        // A pausa permite mostrar o recipiente durante o enchimento.
        yield return Esperar(.7f);
        if (!TemItem(cabacaVaziaBusca)) yield break;
        InventoryManager.Instance.RemoveItem(cabacaVaziaBusca);
        InventoryManager.Instance.AddItem(cabacaCheiaBusca);
        Dados.curraisAguaOrientada = true;
        Dados.curraisAguaRecebida = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(porcaoRecebidaAgua);
    }

    private IEnumerator DestinarAgua(int destinatario = 0)
    {
        if (!PodeEscolherAgua || destinatario < 1 || destinatario > 2 || !TemItem(cabacaCheiaBusca)) yield break;
        if (destinatario == 1) yield return Dizer(ofertaAguaMae);
        else yield return FalarPosto("Zefinha", "Tome, dona Luzia.", "Luzia", "E tua mãe?",
            "Mãe", "Pode beber. Eu tô bem.");
        yield return BeberAguaAbrigo(destinatario);
        if (!TemItem(cabacaCheiaBusca)) yield break;
        InventoryManager.Instance.RemoveItem(cabacaCheiaBusca);
        InventoryManager.Instance.AddItem(cabacaVaziaBusca);
        Dados.curraisEscolhaAgua = destinatario;
        GameProgress.Instance.UnlockDiaryPage(destinatario == 1 ? "currais-agua-mae" : "currais-agua-luzia");
        AtualizarEstadoBusca(); Salvar();
        if (destinatario == 1) yield return Dizer(depoisAguaMae);
        else yield return FalarPosto("Luzia", "Obrigada, Josefa. Eu fico aqui com teu pai.",
            "Mãe", "Venha, minha filha. Nós vai perguntar pela tua irmã.");
        yield return Dizer(guardarCabacaAgua);
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

    private void AbrirMenuMenino() => MostrarAssuntosMenino();

    private IEnumerator ResponderMenino(int bit, Fala[] falas)
    {
        if ((Dados.curraisPerguntasMenino & bit) != 0) yield break;
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
                    ? ("Mãe") : falas[i].personagem,
                texto = falas[i].texto };
        return resultado;
    }

    private IEnumerator ConsultarRegistro()
    {
        if (Dados.curraisEscolhaAgua == 0 || Dados.curraisRegistroConcluido) yield break;
        yield return Dizer(pedidoNomeInicio);
        yield return Dizer(pedidoNomeMae);
        yield return ConsultarFolhaNaMesa();
        yield return Dizer(pedidoNomeServico);
        yield return Dizer(pedidoNomeProtestoMae);
        yield return Dizer(pedidoNomeFinal);
        Dados.curraisTarefaPanosIniciada = true;
        Dados.curraisRegistroConcluido = true;
        GameProgress.Instance.UnlockDiaryPage("currais-registro");
        AtualizarEstadoBusca(); Salvar(); MenuTarefaPanos();
    }

    private IEnumerator RelembrarRegistro()
    {
        yield return Dizer(ComAcompanhante(registroInicio));
        yield return Dizer(registroMae);
        yield return ConsultarFolhaNaMesa();
    }

    private IEnumerator EscolherPertence() { yield return DecidirCanhotoNomes(); }

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
        canhotoCenario.SetActive(panelaAcessivel || CanhotoPendenteDeColeta);
        AtualizarCabacaMesa();
        lacoPanela.SetActive(false);
        areaPanela.enabled = panelaAcessivel;
        meninoBusca.gameObject.SetActive(true);
        // Conversar não muda a posição editada da criança.
        if (Dados.curraisEscolhaAgua != 0 &&
            (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca == 0))
        {
            luzia.transform.position = luziaRepouso.position;
            luzia.transform.localRotation = Quaternion.identity;
        }
        AtualizarCampo();
        Physics2D.SyncTransforms();
    }

    private void RestaurarAcompanhante()
    {
        if (ConsultaDisponivel && Dados.curraisEtapaConsulta >= 1)
        { RestaurarFamiliaConsulta(); return; }
        if (PostoDisponivel) return;
        if (Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 1)
        { RestaurarMaeNaBusca(); return; }
        if (!Dados.curraisDistribuicaoIniciada) return;
        SeguidorFamilia seguidor = seguidoraMae;
        Vector3 posicao = seguidor.transform.position;
        posicao.x = mari.transform.position.x - seguidor.DistanciaAtras;
        seguidor.transform.position = posicao;
        Physics2D.SyncTransforms();
    }

    private void AtualizarCenaNoFimDoQuadro()
    {
        if (!PodeAtualizarCena) return;
        AtualizarCompanhiaSepultamento();
        if (sepultamentoNaTela) AjustarCompanhiaBentoNoPlano();
        ManterPessoasPresentes();
        meninoBusca.gameObject.SetActive(true);
        if (campo.activeInHierarchy) areaMenino.enabled = true;
        if (Dados.curraisEtapaEntrada == 2 || Dados.curraisEtapaEntrada == 3) AtualizarVisuaisChegada();
        // Marcas de câmera são aplicadas somente pela inicialização/transições.
        // Uma fala na revista ou no abrigo não reposiciona nem redefine o zoom.
        bool livre = !ocupado && !menuAberto && !dialogando && !ExamineObject.IsShowing() && !PauseHelper.BlockInput();
        if (AtualizarCaminhadaFamilia(livre)) return;
        if (AtualizarRetornoAgua(livre)) return;
        if (AtualizarAproximacaoRaimundo(livre)) return;
        if (AtualizarAproximacaoLavagem(livre)) return;
        if (AtualizarAproximacaoPatio(livre)) return;
        if (AtualizarFluxoBento(livre)) return;
        if (Dados.curraisEtapaEntrada != 4) return;
        if (planoMemoriaAtivo)
        {
            cameraCena.transform.position = cameraPlanoMemoria.position;
            seguidorPaiConsulta.enabled = false; seguidoraMae.enabled = false; seguidoraLuzia.enabled = false;
            return;
        }
        if (ConsultaDisponivel) { AtualizarAcompanhamentoConsulta(livre); return; }
        // A escolha da água continua valendo na lavagem, na passagem e na chegada ao pátio.
        if (!PostoDisponivel && Dados.curraisPanelaConcluida &&
            Dados.curraisEtapaBusca >= 1 && Dados.curraisEtapaBusca <= 5)
        {
            seguidoraMae.enabled = livre;
            seguidoraLuzia.enabled = false;
            return;
        }
        bool acompanharDistribuicao = !PostoDisponivel && Dados.curraisDistribuicaoIniciada;
        seguidoraLuzia.enabled = false;
        seguidoraMae.enabled = livre && acompanharDistribuicao && Dados.curraisEscolhaAgua != 0;
    }

    private void FecharMenu()
    {
        if (PauseHelper.BlockInput()) return;
        menuAberto = false; FecharDialogo(); LiberarMari();
    }

    private IEnumerator Examinar(Exame exame)
    {
        yield return ExaminarPosto(string.Empty, exameFolhasNomes, exame.texto);
    }

    private IEnumerator Mover(Transform alvo, Vector3 destino, float duracao)
    {
        Vector3 origem = alvo.position;
        if (alvo == mari.transform || alvo == pai || alvo == mae || alvo == irma ||
            alvo == baleia || alvo == cabo || alvo == guardaRetaguarda || alvo == bento || alvo == meninoBusca)
            destino.y = origem.y;
        float tempo = 0f;
        Animator animador = alvo.GetComponent<Animator>();
        try
        {
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
        }
        finally
        {
            // StopAllCoroutines também descarta o enumerador: não deixa walk preso.
            if (animador != null && animador.enabled) animador.SetBool("IsWalking", false);
            if (alvo != null)
            {
                Rigidbody2D corpo = alvo.GetComponent<Rigidbody2D>();
                if (corpo != null) { corpo.position = alvo.position; corpo.linearVelocity = Vector2.zero; }
            }
            Physics2D.SyncTransforms();
        }
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
        pronto = false;
        encerrando = true;
        if (puzzleRetalhos != null) puzzleRetalhos.Fechar();
        if (puzzleRegistro != null) puzzleRegistro.Fechar();
        if (examineNomesAtual != null && examineNomesAtual.PanelIsOpen) examineNomesAtual.HidePanel();
        examineNomesAtual = null;
        if (examePrimeiroCliqueLuzia != null && examePrimeiroCliqueLuzia.PanelIsOpen)
            examePrimeiroCliqueLuzia.HidePanel();
        exameLuziaEmCurso = false;
        if (exameDistribuicao != null && exameDistribuicao.PanelIsOpen) exameDistribuicao.HidePanel();
        exameDistribuicaoEmCurso = false;
        StopAllCoroutines();
        EncerrarGestoRevista();
        EncerrarLavagemELembranca();
        EncerrarPatioDetalhado();
        EncerrarDespedidaEQuarto();
        EncerrarNoiteFamilia();
        EncerrarDetalhesConsulta();
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
