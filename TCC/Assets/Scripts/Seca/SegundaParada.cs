using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Segunda parada: riacho, busca por Baleia e escolha do abrigo.
public class SegundaParada : MonoBehaviour
{
    [SerializeField] private PlayerMovement mari;
    [SerializeField] private ItemData cabacaVazia;
    [SerializeField] private ItemData cabacaCheia;
    [SerializeField] private ItemData prea;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Transform baleia;
    [SerializeField] private SeguidorFamilia mariquinha;
    [SerializeField] private SeguidorFamilia mae;
    [SerializeField] private SeguidorFamilia pai;
    [SerializeField] private Transform limiteBusca;
    [SerializeField] private Sprite[] corridaBaleia;
    [SerializeField] private Sprite baleiaComPrea;
    [SerializeField, Min(0.1f)] private float velocidadeFuga = 6.5f;
    [SerializeField, Min(0.02f)] private float intervaloCorrida = 0.1f;

    [Header("Casa abandonada")]
    [SerializeField] private Sprite casaFechada;
    [SerializeField] private Sprite casaPortaAberta;
    [SerializeField] private Sprite casaJanelaQuebrada;
    [SerializeField] private Sprite descansoFamilia;
    [SerializeField] private Sprite mariquinhaFerida;
    [SerializeField] private Transform terceiraParada;
    [SerializeField, Min(0.1f)] private float duracaoTransicao = 0.7f;

    [Header("Agua da terceira parada")]
    [SerializeField] private ItemData facao;
    [SerializeField] private Sprite barrigudaInteira;
    [SerializeField] private Sprite barrigudaAberta;

    [Header("Curativo e encontro da terceira parada")]
    [SerializeField] private ItemData plantaMedicinal;
    [SerializeField] private ItemData curativo;
    [SerializeField] private Sprite mariquinhaComCurativo;
    [SerializeField] private Sprite mulherRetirante;
    [SerializeField] private Sprite meninoRetirante;
    [SerializeField] private Sprite pessoaDeitada;
    private GameObject plantaCenario, mulherCenario, meninoCenario, pessoaCenario;
    private bool plantaColetada, curativoPreparado, curativoAplicado;
    private bool encontroVisitado, encontroConcluido, perguntasAbertas;
    private int perguntasAntonio;

    [Header("Quarta parada - juazeiro")]
    [SerializeField] private Transform quartaParada;
    [SerializeField] private Sprite juazeiroSprite;
    [SerializeField] private Sprite placaFortaleza;
    [SerializeField] private Sprite familiaJuazeiro;
    [SerializeField] private Sprite familiaJuazeiroCurativo;
    private GameObject juazeiroCenario, familiaDescanso;
    private bool juazeiroIniciado, placaJuazeiroLida, letrasJuazeiroConcluidas, juazeiroConcluido;
    private bool sequenciaJuazeiroIniciada, restaurandoFamilia;
    private float fracaoPainelJuazeiro, zoomJuazeiro;
    private readonly Vector3[] cantosPainelJuazeiro = new Vector3[4];

    [Header("Quinta parada - ultima noite")]
    [SerializeField] private Transform quintaParada;
    [SerializeField] private Collider2D limitesEstrada;
    [SerializeField] private ItemData galhosSecos, folhasSecas, galhosAlecrim, passaro;
    [SerializeField] private Sprite fogueiraMontada, fogueiraAcesa, fogueiraAssando, fogueiraBrasas;
    [SerializeField] private Sprite irmasAcordadas, irmasAcordadasCurativo;
    [SerializeField] private Sprite irmasDormindo;
    [SerializeField] private Sprite irmasDormindoCurativo;
    [SerializeField] private Sprite irmasPromessa;
    [SerializeField] private Sprite irmasSilencio;
    [SerializeField] private Sprite irmasSilencioCurativo;
    [SerializeField] private Sprite paisAcordados;
    private GameObject arvoreAcampamento, fogueiraCenario, irmasCenario, paisCenario;
    private readonly GameObject[] materiaisCenario = new GameObject[3];
    private SpriteRenderer[] cenarioHorario;
    private Color[] coresHorario;
    private bool acampamentoIniciado, introAcampamentoConcluida, fogueiraPreparada, pedidoAlecrimConcluido;
    private bool refeicaoPreparada, conversaFogueiraConcluida, conversaIrmasConcluida, conversaPaisConcluida;
    private bool acampamentoConcluido, noiteEmAndamento, retomouAcampamento, escolhaNoturnaAberta;
    private bool segundaEscolhaNoturnaAberta;
    private int materiaisAcampamento, primeiraEscolhaNoturna, segundaEscolhaNoturna;
    private float fracaoPainelNoite;
    private int focoNoite;

    [Header("Fortaleza - fim do ato 1")]
    [SerializeField] private Sprite cenarioFortaleza;
    [SerializeField] private Sprite policialParado, policialPassoA, policialPassoB, policialBloqueio;
    [SerializeField] private Sprite familiaSurpresa, familiaSurpresaCurativo;
    private GameObject cidadeCenario, familiaFinal;
    private readonly GameObject[] policiais = new GameObject[3];
    private bool fortalezaIniciada, dialogoFortalezaConcluido, abordagemIniciada, atoConcluido;
    private bool finalEmAndamento, encerramentoMostrado;
    private SpriteRenderer[] desenhosFundoFim;
    private bool[] fundoFimAtivo;
    private CameraClearFlags fundoCameraOriginal;
    private Color corCameraOriginal;

    private Transform barriguda;
    private Vector3 posicaoBarriguda;
    private bool aguaTerceiraIniciada, dialogoTerceiraConcluido, aguaTerceiraConcluida;
    private bool recolhendoAgua, avisouProblemaAgua;

    private bool sequenciaAbrigo, deslocandoFamilia, grupoCapturado, fadePreparado;
    private Transform[] pessoasAbrigo;
    private SeguidorFamilia[] seguidoresAbrigo;
    private bool[] seguidoresAtivos, desenhosAtivos;
    private SpriteRenderer[] desenhosAbrigo;
    private SpriteRenderer visualMariquinha;
    private Sprite spriteMariquinhaNormal;
    private Vector3 escalaMariquinhaNormal;
    private InventoryTabController inventarioAbrigo;
    private bool inventarioEstavaAtivo;
    private Camera cameraAbrigo;
    private CameraFollow seguirCamera;
    private bool cameraSeguia;
    private float zoomOriginal;
    private Vector3 cameraOriginal;
    private Image imagemFade;
    private RectTransform retanguloFade;
    private Sprite spriteFadeOriginal;
    private Image.Type tipoFadeOriginal;
    private Color corFadeOriginal;
    private bool raycastFadeOriginal;
    private bool imagemFadeAtivaOriginal;
    private Vector2 ancoraMinOriginal, ancoraMaxOriginal, posicaoFadeOriginal, tamanhoFadeOriginal;
    private readonly List<bool> filhosFadeAtivos = new List<bool>();
    private Vector3 posicaoCasaExterior, escalaCasaExterior;
    private Sprite spriteCasaExterior;
    private int ordemCasaExterior;
    private static bool ferimentoSemProgresso;

    private int etapa; // 0 agua, 1 travessia, 2 busca, 3 escolha, 4 concluida, 5 retorno, 6 casa, 7 cutscene
    private bool iniciou, conversando, movimentoAnterior, esperandoEscolha, movimentoBloqueado;
    private SeguidorFamilia seguidorBaleia;
    private bool baleiaSeguia;
    private bool maeSeguia, paiSeguia, familiaEsperando, fugindo, preparandoBusca;
    private SpriteRenderer desenhoBaleia;
    private Sprite spriteBaleia;
    private Vector3 escalaBaleia, destinoBaleia;
    private float inicioPegadas, retornoX;
    private int indice, quadro, pegadas;
    private readonly List<GameObject> marcadores = new List<GameObject>();
    private readonly List<Button> botoes = new List<Button>();
    private string[] nomes, falas;
    private Action depois;
    private Vector3 posicaoBaleia;
    private Transform riacho, casa;

    private void Start()
    {
        ferimentoSemProgresso = false;
        if (mariquinha != null)
        {
            visualMariquinha = mariquinha.GetComponent<SpriteRenderer>();
            if (visualMariquinha != null) spriteMariquinhaNormal = visualMariquinha.sprite;
            escalaMariquinhaNormal = mariquinha.transform.localScale;
        }
        if (baleia != null)
        {
            posicaoBaleia = baleia.position;
            seguidorBaleia = baleia.GetComponent<SeguidorFamilia>();
            desenhoBaleia = baleia.GetComponent<SpriteRenderer>();
            escalaBaleia = baleia.localScale;
            if (desenhoBaleia != null) spriteBaleia = desenhoBaleia.sprite;
        }
        PrepararAguaTerceira();
        PrepararEncontroTerceira();
        PrepararJuazeiro();
        PrepararFortaleza();
        PrepararAcampamento();
        AplicarFerimento();
        if (GameProgress.Instance != null && GameProgress.Instance.Data.estradaAbrigoConcluido)
        {
            iniciou = true;
            etapa = 4;
            restaurandoFamilia = true;
            StartCoroutine(RestaurarAbrigoCarregado());
            return;
        }
        CriarMarcador("Riacho", new Vector2(transform.position.x + 1.8f, -0.6f),
            new Color(0.25f, 0.52f, 0.68f), 3.2f, 0.55f, "riacho");
        CriarMarcador("Casa abandonada", new Vector2(transform.position.x + 5f, 0f),
            new Color(0.48f, 0.34f, 0.22f), 2f, 2.4f, "casa");
        foreach (GameObject obj in marcadores)
            if (obj.name == "Riacho") riacho = obj.transform;
            else if (obj.name == "Casa abandonada") casa = obj.transform;
        if (casa != null)
        {
            float x = limiteBusca != null ? limiteBusca.position.x - 2.8f : transform.position.x + 5f;
            casa.position = new Vector3(x, -2.4f, 0f);
            TrocarCasa(casaFechada);
            casa.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (PauseHelper.BlockInput() || Time.timeScale == 0f || sequenciaAbrigo) return;
        ControleJornada j = ControleJornada.Instance;
        AtualizarFortaleza();
        if (sequenciaAbrigo || encerramentoMostrado) return;
        AtualizarAcampamento();
        if (sequenciaAbrigo) return;
        if (sequenciaJuazeiroIniciada && grupoCapturado)
        {
            PrepararCameraJuazeiro();
            cameraAbrigo.orthographicSize = zoomJuazeiro;
            cameraAbrigo.transform.position = CentroCameraJuazeiro();
        }
        AtualizarJuazeiro();
        if (sequenciaAbrigo) return;
        AtualizarEncontroTerceira();
        if (j != null && j.ParadaAtiva && j.ParadaAtual == 3 &&
            !aguaTerceiraConcluida && !dialogoTerceiraConcluido &&
            !conversando && dialogueUI != null && !dialogueUI.activeInHierarchy)
            IniciarConversaAguaTerceira();
        if (!iniciou && j != null && j.ParadaAtiva && j.ParadaAtual == 2 && dialogueUI != null && !dialogueUI.activeInHierarchy)
        {
            iniciou = true;
            Falar(new[] { "Mãe" },
                new[] { "Estou com muita sede, Zefinha, use a sua cabaça para coletar um pouco de água." },
                null);
        }
        if (etapa == 5 && !conversando && mari != null &&
            Mathf.Abs(mari.transform.position.x - retornoX) <= 1f)
        {
            VoltarParaFamilia();
        }
        if (etapa == 6 && !conversando && casa != null && mari != null &&
            Mathf.Abs(mari.transform.position.x - casa.position.x) <= 3.5f)
            IniciarChegadaCasa();
        if (!conversando && !preparandoBusca && !sequenciaAbrigo && Time.frameCount > quadro &&
            Input.GetMouseButtonDown(0))
            VerificarCliqueCenario();

        if (!conversando || esperandoEscolha || Time.frameCount <= quadro ||
            !Input.GetMouseButtonDown(0)) return;
        indice++;
        if (indice < falas.Length) { Mostrar(); return; }
        Fechar();
    }

    private bool PonteiroEmControleUI()
    {
        UnityEngine.EventSystems.EventSystem eventos =
            UnityEngine.EventSystems.EventSystem.current;
        if (eventos == null) return false;
        var ponteiro = new UnityEngine.EventSystems.PointerEventData(eventos);
        ponteiro.position = Input.mousePosition;
        var resultados = new List<UnityEngine.EventSystems.RaycastResult>();
        eventos.RaycastAll(ponteiro, resultados);
        foreach (var resultado in resultados)
        {
            // Colliders de cenario e fundos vazios de Canvas nao sao controles.
            if (!(resultado.module is GraphicRaycaster) || resultado.gameObject == null) continue;
            if (resultado.gameObject.GetComponentInParent<Selectable>() != null ||
                resultado.gameObject.GetComponentInParent<InventorySlot>() != null)
                return true;
        }
        return false;
    }

    private void VerificarCliqueCenario()
    {
        Camera camera = Camera.main;
        if (camera == null || PonteiroEmControleUI()) return;
        Ray raio = camera.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        float distancia;
        if (!plano.Raycast(raio, out distancia)) return;
        Vector3 ponto = raio.GetPoint(distancia);
        if (AcampamentoEmAndamento() && !noiteEmAndamento &&
            AcertouMaeAcampamento(ponto)) { InteragirAcampamento(); return; }
        InventoryManager inventarioSelecionado = InventoryManager.Instance;
        if (inventarioSelecionado != null && inventarioSelecionado.SelectedItem == curativo &&
            AcertouMariquinha(ponto) && UsarCurativo(curativo)) return;
        MarcadorSegundaParada escolhido = null;
        float menorDistancia = float.PositiveInfinity;

        // Consulta os proprios alvos, sem deixar o collider da estrada cobri-los.
        foreach (GameObject obj in marcadores)
        {
            if (obj == null || !obj.activeInHierarchy) continue;
            BoxCollider2D area = obj.GetComponent<BoxCollider2D>();
            MarcadorSegundaParada marcador = obj.GetComponent<MarcadorSegundaParada>();
            if (area == null || !area.enabled || marcador == null) continue;
            Vector2 local = area.transform.InverseTransformPoint(ponto);
            Vector2 metade = area.size * 0.5f;
            Vector2 diferenca = local - area.offset;
            bool acertou = Mathf.Abs(diferenca.x) <= metade.x &&
                Mathf.Abs(diferenca.y) <= metade.y;
            if (marcador.id == "baleia" && desenhoBaleia != null &&
                desenhoBaleia.enabled && desenhoBaleia.gameObject.activeInHierarchy)
            {
                // O clique acompanha o desenho real, inclusive o prea e mudancas de escala.
                Bounds desenho = desenhoBaleia.bounds;
                desenho.Expand(new Vector3(0.2f, 0.2f, 0f));
                acertou |= ponto.x >= desenho.min.x && ponto.x <= desenho.max.x &&
                    ponto.y >= desenho.min.y && ponto.y <= desenho.max.y;
            }
            if (!acertou) continue;
            float proximidade = ((Vector2)(ponto - obj.transform.position)).sqrMagnitude;
            if (proximidade < menorDistancia)
            {
                menorDistancia = proximidade;
                escolhido = marcador;
            }
        }
        if (escolhido == null) return;
        if (escolhido.id == "riacho")
        {
            InventoryManager inventario = InventoryManager.Instance;
            if (inventario != null && inventario.SelectedItem != null)
                UsarNoRiacho(inventario.SelectedItem);
            return;
        }
        Clicar(escolhido.id);
    }

    private void Falar(string[] personagens, string[] textos, Action fim, bool duranteCaminhada = false)
    {
        if (dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null || mari == null)
        {
            Debug.LogError("SegundaParada: referencias do dialogo ausentes.", this);
            return;
        }
        nomes = personagens; falas = textos; depois = fim; indice = 0;
        conversando = true; esperandoEscolha = false;
        if (duranteCaminhada) LiberarMovimento();
        else BloquearMovimento();
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        Mostrar();
    }

    private void Mostrar()
    {
        characterNameText.text = nomes[indice];
        dialogueText.text = falas[indice];
        if (portraitImage != null) portraitImage.enabled = false;
        quadro = Time.frameCount;
    }

    private static void Grupo(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) return;
        cg.alpha = 1f; cg.interactable = true; cg.blocksRaycasts = true;
    }

    private void Fechar()
    {
        conversando = false;
        dialogueUI.SetActive(false);
        LiberarMovimento();
        Action acao = depois; depois = null;
        if (acao != null) acao();
    }

    private void BloquearMovimento()
    {
        if (mari == null || movimentoBloqueado) return;
        movimentoAnterior = mari.enabled;
        movimentoBloqueado = true;
        mari.enabled = false;
        PararMari();
    }

    private void LiberarMovimento()
    {
        if (!movimentoBloqueado) return;
        if (mari != null) mari.enabled = movimentoAnterior;
        movimentoBloqueado = false;
    }

    private void PararMari()
    {
        if (mari == null) return;
        Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
        if (corpo != null)
            corpo.linearVelocity = new Vector2(0f, corpo.linearVelocity.y);
        Animator animacao = mari.GetComponent<Animator>();
        if (animacao != null)
        {
            animacao.SetBool("IsWalking", false);
            animacao.SetBool("IsRunning", false);
        }
    }

    private void FixedUpdate()
    {
        if (movimentoBloqueado)
        {
            if (!deslocandoFamilia) PararMari();
            else if (mari != null)
            {
                Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
                if (corpo != null) corpo.linearVelocity = Vector2.zero;
            }
        }
    }

    public bool SoltarNoRiacho(ItemData item, Vector2 posicaoTela)
    {
        if (riacho == null || item != cabacaVazia || Camera.main == null)
            return false;
        Vector3 ponto = Camera.main.ScreenToWorldPoint(
            new Vector3(posicaoTela.x, posicaoTela.y,
                Mathf.Abs(Camera.main.transform.position.z - riacho.position.z)));
        Collider2D area = riacho.GetComponent<Collider2D>();
        if (area == null) return false;
        Vector2 posicao = new Vector2(ponto.x, ponto.y);
        if (area.OverlapPoint(posicao) ||
            Vector2.Distance(posicao, riacho.position) < 1.6f)
            return UsarNoRiacho(item);
        return false;
    }

    public bool UsarNoRiacho(ItemData item)
    {
        if (!iniciou || etapa != 0 || conversando || PauseHelper.BlockInput() ||
            Time.timeScale == 0f || cabacaCheia == null || item != cabacaVazia ||
            InventoryManager.Instance == null ||
            !Possui(item)) return false;
        InventoryManager.Instance.RemoveItem(cabacaVazia);
        InventoryManager.Instance.AddItem(cabacaCheia);
        etapa = 1;
        Falar(new[] { "Cena" },
            new[] { "O grupo atravessa por uma parte rasa do riacho." },
            DepoisDaTravessia);
        return true;
    }

    private bool Possui(ItemData item)
    {
        if (item == null || InventoryManager.Instance == null) return false;
        foreach (ItemData atual in InventoryManager.Instance.Items)
            if (atual == item) return true;
        return false;
    }

    private void DepoisDaTravessia()
    {
        if (riacho != null) riacho.gameObject.SetActive(false);
        // Estas conversas usam o painel compartilhado, mas nao interrompem Mari.
        Falar(
            new[] { "Mariquinha", "Zefinha", "Mariquinha", "Zefinha (em pensamentos)", "Zefinha", "Mariquinha", "Zefinha" },
            new[] {
                "Zefinha…", "hum?", "Tu acha que na cidade tem sorvete?",
                "Claro que tem! Eu mesma vivo indo comer com mainha, ela nunca provou antes?",
                "Eu acho que tem sim…", "Eu nunca vi. Nos pode comer quando chegar lá?",
                "A gente pode pedir a painho e mainha sim."
            }, ConversaSertao, true);
    }

    private void ConversaSertao()
    {
        Falar(new[] { "Pai", "Mainha" },
            new[] {
                "Meu pai dizia que o sertão escuta. Quando o vento muda… É porque ele tá conversando. Quando num venta… É porque tá zangado. E quando chove… É porque perdoou nóis.",
                "Que Deus nos conceda a graça da chuva."
            }, ConversaBaleia, true);
    }

    private void ConversaBaleia()
    {
        PrepararFuga();
        StartCoroutine(CorrerBaleia());
        Falar(new[] { "Mãe", "Pai" },
            new[] {
                "Ave minha Nossa Senhora! Essa cachorra ficou doida, correndo desenfreada assim!",
                "Severina, Jozefa, vão atrás de Baleia, ela deve ter visto alguma coisa ali na frente, eu e sua mãe vamo aproveitar pra ver se tem algum lugar por perto."
            }, IniciarBusca);
    }

    private void PrepararFuga()
    {
        familiaEsperando = true;
        maeSeguia = mae != null && mae.enabled;
        paiSeguia = pai != null && pai.enabled;
        if (mae != null) mae.enabled = false;
        if (pai != null) pai.enabled = false;
        if (mariquinha != null) mariquinha.enabled = true;
        baleiaSeguia = seguidorBaleia != null && seguidorBaleia.enabled;
        if (seguidorBaleia != null) seguidorBaleia.enabled = false;

        // A busca cabe no trecho atual mesmo se Mari caminhou durante as falas.
        inicioPegadas = Mathf.Max(transform.position.x + 0.5f, mari.transform.position.x + 0.5f);
        if (limiteBusca != null)
            inicioPegadas = Mathf.Min(inicioPegadas, limiteBusca.position.x - 4.3f);
        retornoX = mae != null ? mae.transform.position.x + 1.2f : mari.transform.position.x;
        destinoBaleia = new Vector3(inicioPegadas + 3.3f, posicaoBaleia.y, posicaoBaleia.z);
    }

    private IEnumerator CorrerBaleia()
    {
        fugindo = true;
        float tempo = 0f;
        while (baleia != null && Mathf.Abs(baleia.position.x - destinoBaleia.x) > 0.01f)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                float direcao = Mathf.Sign(destinoBaleia.x - baleia.position.x);
                Vector3 posicao = baleia.position;
                posicao.x = Mathf.MoveTowards(posicao.x, destinoBaleia.x, velocidadeFuga * Time.deltaTime);
                baleia.position = posicao;
                tempo += Time.deltaTime;
                if (corridaBaleia != null && corridaBaleia.Length > 0)
                {
                    int frame = Mathf.FloorToInt(tempo / Mathf.Max(0.02f, intervaloCorrida)) % corridaBaleia.Length;
                    MostrarBaleia(corridaBaleia[frame]);
                }
                if (desenhoBaleia != null) desenhoBaleia.flipX = direcao < 0f;
            }
            yield return null;
        }
        MostrarBaleia(baleiaComPrea);
        fugindo = false;
    }

    private void MostrarBaleia(Sprite sprite)
    {
        if (desenhoBaleia == null || sprite == null) return;
        desenhoBaleia.sprite = sprite;
        // Os quadros mantem os pes na mesma altura, sem saltos de escala.
        float proporcao = spriteBaleia != null && sprite.bounds.size.y > 0f
            ? spriteBaleia.bounds.size.y / sprite.bounds.size.y : 1f;
        baleia.localScale = escalaBaleia * proporcao;
    }

    private void IniciarBusca()
    {
        if (preparandoBusca || etapa != 1) return;
        preparandoBusca = true;
        StartCoroutine(PrepararBusca());
    }

    private IEnumerator PrepararBusca()
    {
        BloquearMovimento();
        while (fugindo) yield return null;
        etapa = 2;
        for (int i = 0; i < 3; i++)
            CriarMarcador("Pegada " + (i + 1),
                new Vector2(inicioPegadas + i * 1.1f, -1.4f),
                new Color(0.39f, 0.28f, 0.18f), 0.35f, 0.22f, "pegada" + i);
        CriarMarcador("Baleia encontrada",
            new Vector2(destinoBaleia.x, baleia != null ? baleia.position.y + 0.35f : -1.3f),
            new Color(0.55f, 0.4f, 0.28f), 1.2f, 0.9f, "baleia");
        AtualizarPegadas();
        preparandoBusca = false;
        LiberarMovimento();
    }

    private void AtualizarPegadas()
    {
        foreach (GameObject obj in marcadores)
        {
            MarcadorSegundaParada pista = obj.GetComponent<MarcadorSegundaParada>();
            if (pista == null || !pista.id.StartsWith("pegada")) continue;
            int numero;
            if (!int.TryParse(pista.id.Substring(6), out numero)) continue;
            SpriteRenderer desenho = obj.GetComponent<SpriteRenderer>();
            if (desenho != null)
                desenho.color = numero == pegadas
                    ? new Color(0.55f, 0.4f, 0.25f)
                    : new Color(0.39f, 0.28f, 0.18f, 0.6f);
        }
    }

    private void RetomarFamilia()
    {
        if (!familiaEsperando) return;
        if (mae != null) mae.enabled = maeSeguia;
        if (pai != null) pai.enabled = paiSeguia;
        if (seguidorBaleia != null) seguidorBaleia.enabled = baleiaSeguia;
        familiaEsperando = false;
    }

    private void VoltarParaFamilia()
    {
        etapa = 6;
        RetomarFamilia();
        Falar(new[] { "Zefinha", "Pai" },
            new[] {
                "achamos ela painho.",
                "Já tá escurecendo, tem uma casinha ali mais a frente, vamo ve se da pra entrar e esticar as pernas, amanhã a gente continua."
            }, RevelarCasa);
    }

    private void ConversarNaCasa()
    {
        Falar(new[] { "Mãe", "Mariquinha", "Pai" },
            new[] {
                "A casa ta trancafiada, mas pela janelinha da pra ve que num tem ninguém. A gente pode arrombar a porta com um empurrão.",
                "Acho que dá para encontrar uma pedra e tacar na janela.",
                "O que tu acha Zefinha? A gente empurra a porta ou quebramos a janela?"
            }, EscolherEntrada);
    }

    public void Clicar(string alvo)
    {
        if ((!iniciou && !aguaTerceiraIniciada) || conversando || Time.frameCount <= quadro ||
            PauseHelper.BlockInput() || Time.timeScale == 0f || sequenciaAbrigo) return;
        if (mari == null || Vector2.Distance(mari.transform.position, Posicao(alvo)) > 5f)
            return;
        if (alvo == "fogueira") { InteragirAcampamento(); return; }
        if (alvo.StartsWith("materialAcampamento"))
        {
            int material;
            if (int.TryParse(alvo.Substring("materialAcampamento".Length), out material))
                ColetarMaterialAcampamento(material);
            return;
        }
        if (alvo == "plantaMedicinal" || alvo == "mulherRetirante" || alvo == "meninoRetirante")
        {
            InteragirEncontroTerceira(alvo);
            return;
        }
        if (alvo == "barriguda")
        {
            ControleJornada jornada = ControleJornada.Instance;
            if (jornada == null || !jornada.ParadaAtiva || jornada.ParadaAtual != 3 ||
                !dialogoTerceiraConcluido || aguaTerceiraConcluida) return;
            ItemData selecionado = InventoryManager.Instance != null ? InventoryManager.Instance.SelectedItem : null;
            if (!UsarNaBarriguda(selecionado))
                Falar(new[] { "Mãe" },
                    new[] { "Apois usa ele naquela barriguda ali que tá gorda de tanta água." }, null);
            return;
        }
        if (alvo.StartsWith("pegada") && etapa == 2)
        {
            int n;
            if (!int.TryParse(alvo.Substring(6), out n) || n != pegadas) return;
            pegadas++;
            foreach (GameObject obj in marcadores)
                if (obj.name == "Pegada " + pegadas) obj.SetActive(false);
            AtualizarPegadas();
            return;
        }
        if (alvo == "baleia" && etapa == 2 && pegadas == 3)
        {
            if (prea == null || InventoryManager.Instance == null) return;
            etapa = 5;
            InventoryManager.Instance.AddItem(prea);
            MostrarBaleia(spriteBaleia);
            if (seguidorBaleia != null) seguidorBaleia.enabled = baleiaSeguia;
            foreach (GameObject obj in marcadores)
                if (obj.name.StartsWith("Pegada") || obj.name == "Baleia encontrada")
                    obj.SetActive(false);
        }
        else if (alvo == "casa" && etapa == 6) IniciarChegadaCasa();
    }

    private Vector2 Posicao(string alvo)
    {
        foreach (GameObject obj in marcadores)
            if (obj.GetComponent<MarcadorSegundaParada>()?.id == alvo) return obj.transform.position;
        return transform.position;
    }

    private void EscolherEntrada()
    {
        if (choicesContainer == null) { Debug.LogError("ChoicesContainer ausente", this); return; }
        botoes.Clear();
        foreach (Transform child in choicesContainer.transform)
        {
            Button botao = child.GetComponent<Button>();
            if (botao != null) botoes.Add(botao);
        }
        if (botoes.Count < 2)
        {
            Debug.LogError("SegundaParada: faltam os botoes existentes da escolha.", this);
            botoes.Clear();
            return;
        }
        conversando = true; esperandoEscolha = true;
        BloquearMovimento();
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        characterNameText.text = "Escolha";
        dialogueText.text = "Como entrar na casa?";
        if (portraitImage != null) portraitImage.enabled = false;
        choicesContainer.SetActive(true);
        Grupo(choicesContainer);
        string[] opcoes = { "Arrombar a porta", "Quebrar a janela" };
        for (int i = 0; i < botoes.Count; i++)
        {
            Button botao = botoes[i];
            botao.gameObject.SetActive(i < opcoes.Length);
            if (i >= opcoes.Length) continue;
            TMP_Text rotulo = botao.GetComponentInChildren<TMP_Text>(true);
            if (rotulo != null) rotulo.text = opcoes[i];
            int escolha = i;
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => EscolhaFeita(escolha));
            botao.interactable = true;
        }
    }

    private void LimparEscolhas()
    {
        foreach (Button botao in botoes)
        {
            if (botao == null) continue;
            botao.onClick.RemoveAllListeners();
            botao.gameObject.SetActive(false);
        }
        botoes.Clear();
    }

    private void EscolhaFeita(int escolha)
    {
        if (!esperandoEscolha || PauseHelper.BlockInput() || Time.timeScale == 0f ||
            (escolha != 0 && escolha != 1)) return;
        LimparEscolhas();
        choicesContainer.SetActive(false);
        esperandoEscolha = false; conversando = false;
        dialogueUI.SetActive(false);
        ferimentoSemProgresso = escolha == 1;
        if (GameProgress.Instance != null)
        {
            GameProgressData dados = GameProgress.Instance.Data;
            dados.estradaEntradaAbrigo = escolha + 1;
            dados.estradaMariquinhaFerida = escolha == 1;
        }
        sequenciaAbrigo = true;
        etapa = 7;
        BloquearMovimento();
        StartCoroutine(MostrarEntrada(escolha));
    }

    public static bool JanelaQuebrada => GameProgress.Instance != null
        ? GameProgress.Instance.Data.estradaMariquinhaFerida : ferimentoSemProgresso;

    private void RevelarCasa()
    {
        if (casa != null) casa.gameObject.SetActive(true);
    }

    private bool AbrigoConfigurado()
    {
        bool valido = casa != null && casaFechada != null && casaPortaAberta != null &&
            casaJanelaQuebrada != null && descansoFamilia != null && mariquinhaFerida != null &&
            mari != null && mariquinha != null && mae != null && pai != null && baleia != null &&
            terceiraParada != null && dialoguePanel != null && dialoguePanel.GetComponent<Image>() != null &&
            Camera.main != null && ControleJornada.Instance != null;
        if (!valido) Debug.LogError("SegundaParada: confira as referencias da casa abandonada.", this);
        return valido;
    }

    private void IniciarChegadaCasa()
    {
        if (etapa != 6 || sequenciaAbrigo || !AbrigoConfigurado()) return;
        sequenciaAbrigo = true;
        etapa = 7;
        BloquearMovimento();
        CapturarGrupoAbrigo();
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        StartCoroutine(ChegarCasa());
    }

    private void CapturarGrupoAbrigo()
    {
        if (grupoCapturado) return;
        pessoasAbrigo = new[] { mari.transform, mariquinha.transform, mae.transform, pai.transform, baleia };
        seguidoresAbrigo = new[] { mariquinha, mae, pai, seguidorBaleia };
        seguidoresAtivos = new bool[seguidoresAbrigo.Length];
        for (int i = 0; i < seguidoresAbrigo.Length; i++)
        {
            if (seguidoresAbrigo[i] == null) continue;
            seguidoresAtivos[i] = seguidoresAbrigo[i].enabled;
            seguidoresAbrigo[i].enabled = false;
        }
        desenhosAbrigo = new SpriteRenderer[pessoasAbrigo.Length];
        desenhosAtivos = new bool[pessoasAbrigo.Length];
        for (int i = 0; i < pessoasAbrigo.Length; i++)
        {
            desenhosAbrigo[i] = pessoasAbrigo[i].GetComponent<SpriteRenderer>();
            desenhosAtivos[i] = desenhosAbrigo[i] != null && desenhosAbrigo[i].enabled;
        }
        inventarioAbrigo = InventoryTabController.Instance;
        if (inventarioAbrigo != null)
        {
            inventarioEstavaAtivo = inventarioAbrigo.enabled;
            inventarioAbrigo.Close();
            inventarioAbrigo.enabled = false;
        }
        cameraAbrigo = Camera.main;
        seguirCamera = cameraAbrigo.GetComponent<CameraFollow>();
        cameraSeguia = seguirCamera != null && seguirCamera.enabled;
        zoomOriginal = cameraAbrigo.orthographicSize;
        cameraOriginal = cameraAbrigo.transform.position;
        grupoCapturado = true;
    }

    private IEnumerator ChegarCasa()
    {
        Vector3[] origens = new Vector3[pessoasAbrigo.Length];
        for (int i = 0; i < origens.Length; i++) origens[i] = pessoasAbrigo[i].position;
        float tempo = 0f;
        deslocandoFamilia = true;
        while (tempo < 1.2f)
        {
            tempo += Time.deltaTime;
            for (int i = 0; i < pessoasAbrigo.Length; i++)
            {
                Vector3 destino = origens[i];
                destino.x = casa.position.x - 2.2f - i * 0.9f;
                Posicionar(pessoasAbrigo[i], Vector3.Lerp(origens[i], destino, Mathf.Clamp01(tempo / 1.2f)));
            }
            Animator animacao = mari.GetComponent<Animator>();
            if (animacao != null) animacao.SetBool("IsWalking", true);
            yield return null;
        }
        deslocandoFamilia = false;
        PararMari();
        sequenciaAbrigo = false;
        etapa = 3;
        ConversarNaCasa();
    }

    private void TrocarCasa(Sprite sprite)
    {
        if (casa == null || sprite == null) return;
        SpriteRenderer visual = casa.GetComponent<SpriteRenderer>();
        visual.sprite = sprite;
        visual.color = Color.white;
        visual.sortingOrder = 1;
        float escala = 5.2f / Mathf.Max(0.01f, sprite.bounds.size.x);
        casa.localScale = Vector3.one * escala;
        BoxCollider2D area = casa.GetComponent<BoxCollider2D>();
        if (area != null)
        {
            area.size = sprite.bounds.size;
            area.offset = sprite.bounds.center;
        }
    }

    private IEnumerator MoverPessoa(Transform pessoa, float x, float duracao)
    {
        Vector3 inicio = pessoa.position;
        Vector3 destino = inicio; destino.x = x;
        float tempo = 0f;
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            Posicionar(pessoa, Vector3.Lerp(inicio, destino, Mathf.Clamp01(tempo / duracao)));
            yield return null;
        }
        Posicionar(pessoa, destino);
    }

    private IEnumerator MostrarEntrada(int escolha)
    {
        if (escolha == 0)
        {
            float x = pai.transform.position.x;
            yield return MoverPessoa(pai.transform, casa.position.x - 1.1f, 0.7f);
            TrocarCasa(casaPortaAberta);
            yield return new WaitForSeconds(0.35f);
            yield return MoverPessoa(pai.transform, x, 0.5f);
        }
        else
        {
            float x = mariquinha.transform.position.x;
            yield return MoverPessoa(mariquinha.transform, casa.position.x + 1.1f, 0.7f);
            TrocarCasa(casaJanelaQuebrada);
            AplicarFerimento();
            if (seguirCamera != null) seguirCamera.enabled = false;
            cameraAbrigo.orthographicSize = Mathf.Min(zoomOriginal, 2.6f);
            Vector3 foco = visualMariquinha.bounds.center;
            cameraAbrigo.transform.position = new Vector3(foco.x, foco.y, cameraOriginal.z);
            yield return new WaitForSeconds(1.2f);
            RestaurarCamera();
            yield return MoverPessoa(mariquinha.transform, x, 0.6f);
        }
        sequenciaAbrigo = false;
        Falar(new[] { "Cena", "Cena" },
            escolha == 0
                ? new[] { "A família consegue abrir a porta.", "Ninguém se machuca." }
                : new[] { "A família quebra a janela para conseguir entrar.", "Durante a ação, Mariquinha machuca a mão em um pedaço de vidro." },
            Descansar);
    }

    private void AplicarFerimento()
    {
        if (visualMariquinha == null || spriteMariquinhaNormal == null) return;
        float pe = visualMariquinha.bounds.min.y;
        Sprite sprite = spriteMariquinhaNormal;
        if (JanelaQuebrada)
            sprite = curativoAplicado && mariquinhaComCurativo != null ? mariquinhaComCurativo
                : (mariquinhaFerida != null ? mariquinhaFerida : spriteMariquinhaNormal);
        visualMariquinha.sprite = sprite;
        float escala = spriteMariquinhaNormal.bounds.size.y / Mathf.Max(0.01f, sprite.bounds.size.y);
        mariquinha.transform.localScale = escalaMariquinhaNormal * escala;
        Vector3 posicao = mariquinha.transform.position;
        posicao.y += pe - visualMariquinha.bounds.min.y;
        mariquinha.transform.position = posicao;
        if (JanelaQuebrada) mariquinha.DistanciaAtras = 1.8f;
    }

    private void Descansar()
    {
        if (sequenciaAbrigo) return;
        sequenciaAbrigo = true;
        etapa = 7;
        BloquearMovimento();
        StartCoroutine(MostrarDescanso());
    }

    private IEnumerator MostrarDescanso()
    {
        dialogueUI.SetActive(false);
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        yield return EntrarNoAbrigo();
        PrepararFade();
        yield return Escurecer(1f);
        SpriteRenderer visualCasa = casa.GetComponent<SpriteRenderer>();
        posicaoCasaExterior = casa.position;
        escalaCasaExterior = casa.localScale;
        spriteCasaExterior = visualCasa.sprite;
        ordemCasaExterior = visualCasa.sortingOrder;
        visualCasa.sprite = descansoFamilia;
        visualCasa.sortingOrder = 30;
        Vector3 centro = cameraAbrigo.transform.position;
        casa.position = new Vector3(centro.x, centro.y, 0f);
        float escala = Mathf.Max(2f * cameraAbrigo.orthographicSize / descansoFamilia.bounds.size.y,
            2f * cameraAbrigo.orthographicSize * cameraAbrigo.aspect / descansoFamilia.bounds.size.x);
        casa.localScale = Vector3.one * escala;
        if (seguirCamera != null) seguirCamera.enabled = false;
        for (int i = 0; i < desenhosAbrigo.Length; i++)
            if (desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = false;
        yield return Escurecer(0f);
        yield return new WaitForSeconds(3f);
        yield return Escurecer(1f);
        yield return new WaitForSeconds(0.8f);
        RestaurarExterior();
        PosicionarGrupoManha();
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada != null && jornada.ParadaAtiva && jornada.ParadaAtual == 2)
        {
            jornada.ConcluirParada(2);
            if (GameProgress.Instance != null)
            {
                GameProgress.Instance.Data.estradaAbrigoConcluido = true;
                GameProgress.Instance.Data.estradaOpeningCompleted = true;
            }
        }
        RestaurarCamera();
        RestaurarGrupoAbrigo();
        AplicarFerimento();
        yield return new WaitForSeconds(0.2f);
        yield return Escurecer(0f);
        RestaurarFade();
        etapa = 4;
        sequenciaAbrigo = false;
        LiberarMovimento();
        RestaurarInventarioAbrigo();
        quadro = Time.frameCount;
        if (GameProgress.Instance != null && GameProgress.Instance.Data.CanSave &&
            GameProgress.Instance.Data.estradaAbrigoConcluido && !GameSaveSystem.IsRestoring)
        {
            GameSaveSystem salvar = GameProgress.Instance.GetComponent<GameSaveSystem>();
            if (salvar != null) salvar.TrySaveCheckpoint("Estrada");
        }
        Debug.Log("Segunda parada concluída.", this);
    }

    private IEnumerator EntrarNoAbrigo()
    {
        Vector3[] origens = new Vector3[pessoasAbrigo.Length];
        for (int i = 0; i < origens.Length; i++) origens[i] = pessoasAbrigo[i].position;
        float entradaX = casa.position.x + (JanelaQuebrada ? 1.1f : -1.1f);
        float tempo = 0f;
        deslocandoFamilia = true;
        while (tempo < 1.4f)
        {
            tempo += Time.deltaTime;
            for (int i = 0; i < pessoasAbrigo.Length; i++)
            {
                float andamento = Mathf.Clamp01((tempo - i * 0.14f) / 0.7f);
                Vector3 destino = origens[i]; destino.x = entradaX;
                Posicionar(pessoasAbrigo[i], Vector3.Lerp(origens[i], destino, andamento));
                if (andamento >= 1f && desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = false;
            }
            Animator animacao = mari.GetComponent<Animator>();
            if (animacao != null) animacao.SetBool("IsWalking", true);
            yield return null;
        }
        deslocandoFamilia = false;
        PararMari();
    }

    private void PrepararFade()
    {
        imagemFade = dialoguePanel.GetComponent<Image>();
        retanguloFade = dialoguePanel.GetComponent<RectTransform>();
        spriteFadeOriginal = imagemFade.sprite;
        tipoFadeOriginal = imagemFade.type;
        corFadeOriginal = imagemFade.color;
        raycastFadeOriginal = imagemFade.raycastTarget;
        imagemFadeAtivaOriginal = imagemFade.enabled;
        ancoraMinOriginal = retanguloFade.anchorMin; ancoraMaxOriginal = retanguloFade.anchorMax;
        posicaoFadeOriginal = retanguloFade.anchoredPosition; tamanhoFadeOriginal = retanguloFade.sizeDelta;
        filhosFadeAtivos.Clear();
        foreach (Transform filho in dialoguePanel.transform)
        {
            filhosFadeAtivos.Add(filho.gameObject.activeSelf);
            filho.gameObject.SetActive(false);
        }
        retanguloFade.anchorMin = Vector2.zero; retanguloFade.anchorMax = Vector2.one;
        retanguloFade.anchoredPosition = Vector2.zero; retanguloFade.sizeDelta = Vector2.zero;
        imagemFade.sprite = null; imagemFade.type = Image.Type.Simple;
        imagemFade.enabled = true;
        imagemFade.color = new Color(0f, 0f, 0f, 0f); imagemFade.raycastTarget = true;
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        fadePreparado = true;
    }

    private IEnumerator Escurecer(float alpha)
    {
        float inicio = imagemFade.color.a;
        float tempo = 0f;
        float duracao = Mathf.Max(0.1f, duracaoTransicao);
        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            imagemFade.color = new Color(0f, 0f, 0f, Mathf.Lerp(inicio, alpha, Mathf.Clamp01(tempo / duracao)));
            yield return null;
        }
        imagemFade.color = new Color(0f, 0f, 0f, alpha);
    }

    private void RestaurarFade()
    {
        if (!fadePreparado) return;
        if (imagemFade != null)
        {
            imagemFade.sprite = spriteFadeOriginal; imagemFade.type = tipoFadeOriginal;
            imagemFade.color = corFadeOriginal; imagemFade.raycastTarget = raycastFadeOriginal;
            imagemFade.enabled = imagemFadeAtivaOriginal;
            retanguloFade.anchorMin = ancoraMinOriginal; retanguloFade.anchorMax = ancoraMaxOriginal;
            retanguloFade.anchoredPosition = posicaoFadeOriginal; retanguloFade.sizeDelta = tamanhoFadeOriginal;
            int i = 0;
            foreach (Transform filho in dialoguePanel.transform)
            {
                if (i < filhosFadeAtivos.Count) filho.gameObject.SetActive(filhosFadeAtivos[i]);
                i++;
            }
        }
        if (dialogueUI != null) dialogueUI.SetActive(false);
        filhosFadeAtivos.Clear();
        fadePreparado = false;
    }

    private void RestaurarExterior()
    {
        if (spriteCasaExterior == null || casa == null) return;
        SpriteRenderer visual = casa.GetComponent<SpriteRenderer>();
        casa.position = posicaoCasaExterior; casa.localScale = escalaCasaExterior;
        visual.sprite = spriteCasaExterior; visual.sortingOrder = ordemCasaExterior;
        BoxCollider2D area = casa.GetComponent<BoxCollider2D>();
        if (area != null) area.enabled = false;
        spriteCasaExterior = null;
    }

    private void RestaurarCamera()
    {
        if (cameraAbrigo == null || mari == null) return;
        cameraAbrigo.orthographicSize = zoomOriginal;
        cameraAbrigo.transform.position = new Vector3(mari.transform.position.x, cameraOriginal.y, cameraOriginal.z);
        if (seguirCamera != null) seguirCamera.enabled = cameraSeguia;
    }

    private void RestaurarGrupoAbrigo()
    {
        if (!grupoCapturado) return;
        for (int i = 0; i < seguidoresAbrigo.Length; i++)
            if (seguidoresAbrigo[i] != null) seguidoresAbrigo[i].enabled = seguidoresAtivos[i];
        for (int i = 0; i < desenhosAbrigo.Length; i++)
            if (desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = desenhosAtivos[i];
        grupoCapturado = false;
    }

    private void RestaurarInventarioAbrigo()
    {
        if (inventarioAbrigo == null) return;
        inventarioAbrigo.enabled = inventarioEstavaAtivo;
        inventarioAbrigo = null;
    }

    private static void Posicionar(Transform pessoa, Vector3 destino)
    {
        if (pessoa == null) return;
        float deslocamento = destino.x - pessoa.position.x;
        SpriteRenderer desenho = pessoa.GetComponent<SpriteRenderer>();
        if (desenho != null && Mathf.Abs(deslocamento) > 0.001f) desenho.flipX = deslocamento < 0f;
        pessoa.position = destino;
        Rigidbody2D corpo = pessoa.GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.position = new Vector2(destino.x, destino.y);
            corpo.linearVelocity = Vector2.zero;
        }
    }

    private void PosicionarGrupoManha()
    {
        if (mari == null || terceiraParada == null) return;
        float x = terceiraParada.position.x - 1.8f;
        Vector3 posicao = mari.transform.position; posicao.x = x;
        Posicionar(mari.transform, posicao);
        SeguidorFamilia[] familiares = { mariquinha, mae, pai, seguidorBaleia };
        for (int i = 0; i < familiares.Length; i++)
        {
            if (familiares[i] == null) continue;
            Vector3 lugar = familiares[i].transform.position;
            lugar.x = x - familiares[i].DistanciaAtras;
            Posicionar(familiares[i].transform, lugar);
        }
        if (pessoasAbrigo != null)
            foreach (Transform pessoa in pessoasAbrigo)
            {
                SpriteRenderer desenho = pessoa.GetComponent<SpriteRenderer>();
                if (desenho != null) desenho.flipX = false;
            }
        Physics2D.SyncTransforms();
    }

    private IEnumerator RestaurarAbrigoCarregado()
    {
        while (GameSaveSystem.IsRestoring) yield return null;
        // Aguarda os Start dos seguidores antes de restaurar a formacao.
        yield return null;
        if (mari != null)
        {
            SeguidorFamilia[] familiares = { mariquinha, mae, pai, seguidorBaleia };
            // O seguidor de Baleia ainda nao foi lido quando pulamos a criacao dos marcadores.
            if (baleia != null) familiares[3] = baleia.GetComponent<SeguidorFamilia>();
            foreach (SeguidorFamilia familiar in familiares)
            {
                if (familiar == null) continue;
                Vector3 lugar = familiar.transform.position;
                lugar.x = mari.transform.position.x - familiar.DistanciaAtras;
                Posicionar(familiar.transform, lugar);
            }
        }
        AplicarFerimento();
        restaurandoFamilia = false;
    }

    private void CriarMarcador(string nome, Vector2 pos, Color cor, float largura, float altura, string id)
    {
        GameObject go = new GameObject(nome, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(MarcadorSegundaParada));
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0);
        Texture2D tex = new Texture2D(1, 1);
        tex.filterMode = FilterMode.Point; tex.SetPixel(0, 0, Color.white); tex.Apply();
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        sr.color = cor; sr.sortingOrder = 3;
        // Mantem visiveis os alvos ja usados pelo puzzle; Baleia usa seu proprio desenho.
        sr.enabled = id != "baleia" || baleia == null;
        go.transform.localScale = new Vector3(largura, altura, 1);
        BoxCollider2D col = go.GetComponent<BoxCollider2D>(); col.isTrigger = true;
        if (id.StartsWith("pegada")) col.size = new Vector2(1.8f, 2f);
        MarcadorSegundaParada marcador = go.GetComponent<MarcadorSegundaParada>();
        marcador.configurar(this, id);
        marcadores.Add(go);
    }

    private void PrepararAguaTerceira()
    {
        if (terceiraParada == null || barrigudaInteira == null || barrigudaAberta == null || facao == null)
        {
            Debug.LogError("SegundaParada: faltam as referencias da barriguda.", this);
            return;
        }
        if (GameProgress.Instance != null)
        {
            GameProgressData dados = GameProgress.Instance.Data;
            aguaTerceiraIniciada = dados.estradaTerceiraAguaIniciada;
            dialogoTerceiraConcluido = dados.estradaDialogoBarrigudaConcluido;
            aguaTerceiraConcluida = dados.estradaBarrigudaConcluida;
        }
        CriarMarcador("Barriguda", new Vector2(terceiraParada.position.x + 2.4f, -2.4f),
            Color.white, 1f, 1f, "barriguda");
        barriguda = marcadores[marcadores.Count - 1].transform;
        posicaoBarriguda = barriguda.position;
        MostrarBarriguda(aguaTerceiraConcluida ? barrigudaAberta : barrigudaInteira);
    }

    private void MostrarBarriguda(Sprite sprite)
    {
        if (barriguda == null || sprite == null) return;
        SpriteRenderer desenho = barriguda.GetComponent<SpriteRenderer>();
        desenho.sprite = sprite;
        desenho.color = Color.white;
        desenho.sortingOrder = 2;
        float escala = 4.8f / Mathf.Max(0.01f, sprite.bounds.size.y);
        barriguda.localScale = Vector3.one * escala;
        BoxCollider2D area = barriguda.GetComponent<BoxCollider2D>();
        area.size = sprite.bounds.size;
        area.offset = sprite.bounds.center;
    }

    private bool TrocarCabaca(ItemData origem, ItemData destino)
    {
        InventoryManager inventario = InventoryManager.Instance;
        if (inventario == null || origem == null || destino == null) return false;
        var itens = new List<ItemData>(inventario.Items);
        int indiceCabaca = itens.IndexOf(origem);
        if (indiceCabaca < 0) return itens.Contains(destino);
        itens[indiceCabaca] = destino;
        // Uma unica atualizacao evita reconstruir os slots no meio da troca.
        inventario.ReplaceItems(itens);
        return true;
    }

    private void IniciarConversaAguaTerceira()
    {
        if (barriguda == null || mari == null || cabacaVazia == null || cabacaCheia == null ||
            InventoryManager.Instance == null || (!Possui(cabacaVazia) && !Possui(cabacaCheia)))
        {
            if (!avisouProblemaAgua)
                Debug.LogError("Terceira parada: confira a cabaca no inventario e as referencias.", this);
            avisouProblemaAgua = true;
            return;
        }
        if (!aguaTerceiraIniciada)
        {
            // A primeira fala informa que a agua acabou: a troca ocorre neste momento.
            if (!TrocarCabaca(cabacaCheia, cabacaVazia)) return;
            aguaTerceiraIniciada = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaTerceiraAguaIniciada = true;
        }
        Falar(new[] { "Mariquinha", "Mãe", "Pai", "Mãe", "Zefinha", "Mãe" },
            new[] {
                "Tô com sede mainha, a água da cabaça já acabou.",
                "Mas eu num vejo nenhum rio por perto minha menina.",
                "Num tem rio, mas tem barriguda, dentro tem água, nos quebra e toma.",
                "Tais com o facão zefinha?",
                "Tô…",
                "Apois usa ele naquela barriguda ali que tá gorda de tanta água."
            }, () => {
                dialogoTerceiraConcluido = true;
                if (GameProgress.Instance != null)
                    GameProgress.Instance.Data.estradaDialogoBarrigudaConcluido = true;
                SalvarAguaTerceira();
            });
    }

    public bool SoltarNaBarriguda(ItemData item, Vector2 posicaoTela)
    {
        Camera camera = Camera.main;
        if (barriguda == null || camera == null || item != facao) return false;
        Ray raio = camera.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        float distancia;
        if (!plano.Raycast(raio, out distancia)) return false;
        BoxCollider2D area = barriguda.GetComponent<BoxCollider2D>();
        Vector2 ponto = barriguda.InverseTransformPoint(raio.GetPoint(distancia));
        Vector2 diferenca = ponto - area.offset;
        if (Mathf.Abs(diferenca.x) > area.size.x * 0.5f ||
            Mathf.Abs(diferenca.y) > area.size.y * 0.5f) return false;
        return UsarNaBarriguda(item);
    }

    private bool UsarNaBarriguda(ItemData item)
    {
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada == null || !jornada.ParadaAtiva || jornada.ParadaAtual != 3 ||
            !aguaTerceiraIniciada || !dialogoTerceiraConcluido || aguaTerceiraConcluida ||
            recolhendoAgua || conversando || sequenciaAbrigo || PauseHelper.BlockInput() ||
            Time.timeScale == 0f || item != facao || !Possui(facao) || !Possui(cabacaVazia) ||
            barriguda == null || mari == null ||
            Vector2.Distance(mari.transform.position, barriguda.position) > 5f)
            return false;
        recolhendoAgua = true;
        sequenciaAbrigo = true;
        BloquearMovimento();
        inventarioAbrigo = InventoryTabController.Instance;
        if (inventarioAbrigo != null)
        {
            inventarioEstavaAtivo = inventarioAbrigo.enabled;
            inventarioAbrigo.Close();
            inventarioAbrigo.enabled = false;
        }
        StartCoroutine(RecolherAguaBarriguda());
        return true;
    }

    private IEnumerator RecolherAguaBarriguda()
    {
        float tempo = 0f;
        while (tempo < 0.6f)
        {
            tempo += Time.deltaTime;
            float deslocamento = Mathf.Sin(tempo * 45f) * 0.04f;
            barriguda.position = posicaoBarriguda + Vector3.right * deslocamento;
            yield return null;
        }
        barriguda.position = posicaoBarriguda;
        MostrarBarriguda(barrigudaAberta);
        if (TrocarCabaca(cabacaVazia, cabacaCheia))
        {
            aguaTerceiraConcluida = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaBarrigudaConcluida = true;
            ControleJornada.Instance?.LiberarTrechoTerceiraParada();
        }
        else
        {
            MostrarBarriguda(barrigudaInteira);
            Debug.LogError("Terceira parada: nao foi possivel encher a cabaca.", this);
        }
        yield return new WaitForSeconds(0.5f);
        recolhendoAgua = false;
        sequenciaAbrigo = false;
        LiberarMovimento();
        RestaurarInventarioAbrigo();
        quadro = Time.frameCount;
        if (aguaTerceiraConcluida) SalvarAguaTerceira();
    }

    private void PrepararEncontroTerceira()
    {
        if (GameProgress.Instance != null)
        {
            GameProgressData dados = GameProgress.Instance.Data;
            plantaColetada = dados.estradaPlantaMedicinalColetada;
            curativoPreparado = dados.estradaCurativoPreparado;
            curativoAplicado = dados.estradaCurativoAplicado;
            encontroVisitado = dados.estradaEncontroVisitado;
            encontroConcluido = dados.estradaEncontroConcluido;
            perguntasAntonio = dados.estradaPerguntasAntonio;
        }
        if (terceiraParada == null || plantaMedicinal == null || curativo == null ||
            plantaMedicinal.icon == null || mariquinhaComCurativo == null ||
            mulherRetirante == null || meninoRetirante == null || pessoaDeitada == null)
        {
            Debug.LogError("SegundaParada: faltam referencias do curativo/encontro.", this);
            return;
        }
        float x = terceiraParada.position.x;
        float alturaMeninas = spriteMariquinhaNormal != null
            ? spriteMariquinhaNormal.bounds.size.y * Mathf.Abs(escalaMariquinhaNormal.y) : 2f;
        plantaCenario = CriarAlvoEncontro("Planta medicinal", "plantaMedicinal",
            plantaMedicinal.icon, x + 1.1f, 0.65f, 3);
        pessoaCenario = CriarAlvoEncontro("Pessoa deitada", "pessoaDeitada",
            pessoaDeitada, x + 4.8f, alturaMeninas * 0.3f, 2);
        mulherCenario = CriarAlvoEncontro("Mulher retirante", "mulherRetirante",
            mulherRetirante, x + 5.15f, alturaMeninas * 1.3f, 3);
        meninoCenario = CriarAlvoEncontro("Menino retirante", "meninoRetirante",
            meninoRetirante, x + 6.4f, alturaMeninas * 1.08f, 3);
        pessoaCenario.GetComponent<BoxCollider2D>().enabled = false;
        AtualizarVisuaisEncontro();
    }

    private GameObject CriarAlvoEncontro(string nome, string id, Sprite sprite, float x, float altura, int ordem)
    {
        CriarMarcador(nome, new Vector2(x, -2.4f), Color.white, 1f, 1f, id);
        GameObject alvo = marcadores[marcadores.Count - 1];
        SpriteRenderer desenho = alvo.GetComponent<SpriteRenderer>();
        desenho.sprite = sprite;
        desenho.sortingOrder = ordem;
        float escala = altura / Mathf.Max(0.01f, sprite.bounds.size.y);
        alvo.transform.localScale = Vector3.one * escala;
        BoxCollider2D area = alvo.GetComponent<BoxCollider2D>();
        area.size = sprite.bounds.size;
        area.offset = sprite.bounds.center;
        // Os icones usam pivô central; os personagens ja usam pivô nos pes.
        Vector3 posicao = alvo.transform.position;
        posicao.y += -2.4f - desenho.bounds.min.y;
        alvo.transform.position = posicao;
        alvo.SetActive(false);
        return alvo;
    }

    private bool TerceiraEmAndamento()
    {
        ControleJornada jornada = ControleJornada.Instance;
        return jornada != null && jornada.ParadaAtiva && jornada.ParadaAtual == 3 &&
            aguaTerceiraConcluida && !encontroConcluido;
    }

    private void AtualizarVisuaisEncontro()
    {
        bool ativa = TerceiraEmAndamento();
        bool precisaCurativo = JanelaQuebrada && !curativoAplicado;
        if (plantaCenario != null) plantaCenario.SetActive(ativa && precisaCurativo && !plantaColetada);
        // O grupo permanece no cenario depois da despedida, sem repetir a conversa.
        bool mostrarGrupo = (ativa || encontroConcluido) && !precisaCurativo;
        if (mulherCenario != null) mulherCenario.SetActive(mostrarGrupo);
        if (meninoCenario != null) meninoCenario.SetActive(mostrarGrupo);
        if (pessoaCenario != null) pessoaCenario.SetActive(mostrarGrupo);
    }

    private void AtualizarEncontroTerceira()
    {
        if (GameSaveSystem.IsRestoring) return;
        AtualizarVisuaisEncontro();
        if (!TerceiraEmAndamento()) return;
        if (!curativoPreparado && Possui(curativo))
        {
            curativoPreparado = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaCurativoPreparado = true;
            SalvarAguaTerceira();
        }
        if (JanelaQuebrada && !curativoAplicado) return;
        ControleJornada.Instance.LiberarEncontroTerceiraParada();
        if (mari == null || meninoCenario == null || dialogueUI == null ||
            InvestigationGuard.Blocked) return;
        // As interacoes sao opcionais. A aproximacao do grupo permite seguir adiante.
        float x = mari.transform.position.x;
        float referencia = terceiraParada.position.x;
        if (!encontroVisitado && Mathf.Abs(x - (referencia + 5.7f)) <= 1.6f)
        {
            encontroVisitado = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaEncontroVisitado = true;
        }
        if (encontroVisitado && x >= referencia + 7.7f &&
            (!conversando || perguntasAbertas) &&
            (perguntasAbertas || !dialogueUI.activeInHierarchy))
            EncerrarEncontroTerceira();
    }

    private void InteragirEncontroTerceira(string alvo)
    {
        if (!TerceiraEmAndamento() || InvestigationGuard.Blocked ||
            dialogueUI == null || dialogueUI.activeInHierarchy) return;
        if (alvo == "plantaMedicinal")
        {
            if (!JanelaQuebrada || curativoAplicado || plantaColetada || plantaCenario == null ||
                !plantaCenario.activeInHierarchy || InventoryManager.Instance == null) return;
            CollectPrompt coleta = FindFirstObjectByType<CollectPrompt>();
            if (coleta != null) coleta.Show(plantaMedicinal, () =>
            {
                plantaColetada = true;
                if (GameProgress.Instance != null)
                    GameProgress.Instance.Data.estradaPlantaMedicinalColetada = true;
                AtualizarVisuaisEncontro();
                quadro = Time.frameCount;
                SalvarAguaTerceira();
            });
            return;
        }
        if (JanelaQuebrada && !curativoAplicado) return;
        encontroVisitado = true;
        if (GameProgress.Instance != null)
            GameProgress.Instance.Data.estradaEncontroVisitado = true;
        if (alvo == "mulherRetirante")
            Falar(new[] { "Mulher" }, new[] {
                "Num acorda mais… Dormiu ontem. Hoje tá frio. Sempre dizia que ia morrê de velho. Mentiroso, morreu na paz, dormindo…"
            }, SalvarAguaTerceira);
        else if (alvo == "meninoRetirante")
            Falar(new[] { "Menino" }, new[] {
                "Ela ja vinha reclamando de uma dor forte no coração, dormiu ontem e não acordou mais."
            }, MostrarPerguntasAntonio);
    }

    private bool AcertouMariquinha(Vector3 ponto)
    {
        if (visualMariquinha == null || !visualMariquinha.enabled ||
            !visualMariquinha.gameObject.activeInHierarchy) return false;
        Bounds area = visualMariquinha.bounds;
        area.Expand(new Vector3(0.25f, 0.25f, 0f));
        return ponto.x >= area.min.x && ponto.x <= area.max.x &&
            ponto.y >= area.min.y && ponto.y <= area.max.y;
    }

    public bool SoltarCurativo(ItemData item, Vector2 posicaoTela)
    {
        Camera camera = Camera.main;
        if (camera == null || item != curativo) return false;
        Ray raio = camera.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        float distancia;
        return plano.Raycast(raio, out distancia) && AcertouMariquinha(raio.GetPoint(distancia)) &&
            UsarCurativo(item);
    }

    private bool UsarCurativo(ItemData item)
    {
        if (!TerceiraEmAndamento() || !JanelaQuebrada || curativoAplicado ||
            item == null || item != curativo || !Possui(curativo) || mariquinha == null ||
            mari == null || visualMariquinha == null || conversando || sequenciaAbrigo ||
            PauseHelper.BlockInput() || Time.timeScale == 0f || GameSaveSystem.IsRestoring ||
            (dialogueUI != null && dialogueUI.activeInHierarchy) ||
            Vector2.Distance(mari.transform.position, mariquinha.transform.position) > 5f) return false;
        curativoAplicado = true;
        curativoPreparado = true;
        if (GameProgress.Instance != null)
        {
            GameProgress.Instance.Data.estradaCurativoAplicado = true;
            GameProgress.Instance.Data.estradaCurativoPreparado = true;
        }
        InventoryManager.Instance.RemoveItem(curativo);
        // O tratamento muda o desenho, mas nunca apaga a escolha/ferimento da janela.
        AplicarFerimento();
        ControleJornada.Instance.LiberarEncontroTerceiraParada();
        AtualizarVisuaisEncontro();
        quadro = Time.frameCount;
        InvestigationGuard.BlockCurrentClick();
        SalvarAguaTerceira();
        return true;
    }

    private void MostrarPerguntasAntonio()
    {
        if (choicesContainer == null) return;
        LimparEscolhas();
        foreach (Transform child in choicesContainer.transform)
        {
            Button botao = child.GetComponent<Button>();
            if (botao != null) botoes.Add(botao);
        }
        if (botoes.Count < 4)
        {
            Debug.LogError("SegundaParada: faltam as tres perguntas e o botao Sair de Antonio.", this);
            LimparEscolhas();
            return;
        }
        conversando = true;
        esperandoEscolha = true;
        perguntasAbertas = true;
        depois = null;
        // O botao Sair fecha a conversa e devolve o controle da caminhada.
        BloquearMovimento();
        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        characterNameText.text = "Escolha";
        dialogueText.text = "Escolha uma pergunta ou selecione Sair.";
        if (portraitImage != null) portraitImage.enabled = false;
        choicesContainer.SetActive(true);
        Grupo(choicesContainer);
        string[] opcoes = { "Perguntar o nome", "Perguntar de onde ele vem", "Desejar os pêsames", "Sair" };
        for (int i = 0; i < botoes.Count; i++)
        {
            Button botao = botoes[i];
            botao.gameObject.SetActive(i < opcoes.Length);
            if (i >= opcoes.Length) continue;
            TMP_Text rotulo = botao.GetComponentInChildren<TMP_Text>(true);
            if (rotulo != null) rotulo.text = opcoes[i];
            int escolha = i;
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => ResponderAntonio(escolha));
            botao.interactable = true;
        }
        quadro = Time.frameCount;
        SalvarAguaTerceira();
    }

    private void SairConversaAntonio()
    {
        perguntasAbertas = false;
        esperandoEscolha = false;
        LimparEscolhas();
        choicesContainer.SetActive(false);
        conversando = false;
        depois = null;
        dialogueUI.SetActive(false);
        LiberarMovimento();
        quadro = Time.frameCount;
        InvestigationGuard.BlockCurrentClick();
        SalvarAguaTerceira();
    }

    private void ResponderAntonio(int escolha)
    {
        if (!perguntasAbertas || !esperandoEscolha || escolha < 0 || escolha > 3 ||
            PauseHelper.BlockInput() || Time.timeScale == 0f) return;
        if (escolha == 3) { SairConversaAntonio(); return; }
        perguntasAbertas = false;
        esperandoEscolha = false;
        LimparEscolhas();
        choicesContainer.SetActive(false);
        string[][] personagens = {
            new[] { "Zefinha", "Menino", "Zefinha", "Menino", "Zefinha" },
            new[] { "Zefinha", "Menino", "Zefinha", "Menino", "Zefinha" },
            new[] { "Zefinha", "Menino", "Menino", "Zefinha", "Menino" }
        };
        string[][] textos = {
            new[] { "Como é teu nome?", "Antônio.", "Eu sou Zefinha. Aquela ali é minha irmã, Mariquinha.", "Prazer…", "Prazer, Antônio." },
            new[] { "Ocês vieram de onde?", "De uma fazenda lá pro interior. A seca acabou com tudo que nós tinha.", "E tão indo pra Fortaleza também?", "Tava indo… Agora num sei mais.", "Sinto muito…" },
            new[] { "Sinto muito pelo que aconteceu.", "Obrigado…", "A gente saiu de casa achando que na cidade ia ser diferente.", "Talvez ainda seja…", "Tomara." }
        };
        Falar(personagens[escolha], textos[escolha], () =>
        {
            perguntasAntonio |= 1 << escolha;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaPerguntasAntonio = perguntasAntonio;
            MostrarPerguntasAntonio();
        });
    }

    private void EncerrarEncontroTerceira()
    {
        perguntasAbertas = false;
        esperandoEscolha = false;
        LimparEscolhas();
        if (choicesContainer != null) choicesContainer.SetActive(false);
        Falar(new[] { "Zefinha (em pensamentos)" }, new[] {
            "Eu queria dizer alguma coisa que ajudasse… Mas num sei o que dizer."
        }, () =>
        {
            encontroConcluido = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaEncontroConcluido = true;
            ControleJornada.Instance.ConcluirParada(3);
            AtualizarVisuaisEncontro();
            quadro = Time.frameCount;
            SalvarAguaTerceira();
        });
    }

    private void PrepararJuazeiro()
    {
        if (GameProgress.Instance != null)
        {
            GameProgressData dados = GameProgress.Instance.Data;
            juazeiroIniciado = dados.estradaJuazeiroIniciado;
            placaJuazeiroLida = dados.estradaPlacaJuazeiroLida;
            letrasJuazeiroConcluidas = dados.estradaLetrasJuazeiroConcluidas;
            juazeiroConcluido = dados.estradaJuazeiroConcluido;
        }
        if (quartaParada == null || juazeiroSprite == null || placaFortaleza == null ||
            familiaJuazeiro == null || familiaJuazeiroCurativo == null ||
            mari == null || mariquinha == null || mae == null || pai == null || baleia == null ||
            dialogueText == null || dialoguePanel == null || dialoguePanel.GetComponent<Image>() == null)
        {
            Debug.LogError("SegundaParada: faltam referencias do juazeiro e da familia descansando.", this);
            return;
        }
        juazeiroCenario = CriarAlvoEncontro("Juazeiro", "juazeiro", juazeiroSprite,
            quartaParada.position.x + 2f, 5.6f, -1);
        juazeiroCenario.GetComponent<BoxCollider2D>().enabled = false;
        juazeiroCenario.SetActive(true);
        GameObject placa = CriarAlvoEncontro("Placa Fortaleza", "placaFortaleza", placaFortaleza,
            quartaParada.position.x - 0.6f, 1.8f, 1);
        placa.GetComponent<BoxCollider2D>().enabled = false;
        placa.SetActive(true);

        // O texto e composto pela fonte existente, para preservar acentos e a seta.
        GameObject rotulo = new GameObject("Texto da placa", typeof(TextMeshPro));
        rotulo.transform.SetParent(transform, false);
        Bounds desenhoPlaca = placa.GetComponent<SpriteRenderer>().bounds;
        rotulo.transform.position = new Vector3(desenhoPlaca.center.x,
            desenhoPlaca.min.y + desenhoPlaca.size.y * 0.66f, -0.1f);
        TextMeshPro texto = rotulo.GetComponent<TextMeshPro>();
        texto.font = dialogueText.font;
        texto.text = "FORTALEZA\n— 12 LÉGUAS →";
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(0.24f, 0.12f, 0.07f);
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 2f;
        texto.fontSizeMax = 3.2f;
        texto.fontSize = 3.2f;
        texto.textWrappingMode = TextWrappingModes.Normal;
        texto.rectTransform.sizeDelta = new Vector2(desenhoPlaca.size.x * 0.82f, desenhoPlaca.size.y * 0.4f);
        texto.GetComponent<MeshRenderer>().sortingOrder = 2;

        float altura = 7.2f * familiaJuazeiro.bounds.size.y / familiaJuazeiro.bounds.size.x;
        familiaDescanso = CriarAlvoEncontro("Familia no juazeiro", "descansoJuazeiro", familiaJuazeiro,
            juazeiroCenario.transform.position.x - 0.6f, altura, 11);
        familiaDescanso.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void AtualizarJuazeiro()
    {
        ControleJornada jornada = ControleJornada.Instance;
        if (restaurandoFamilia || juazeiroConcluido || sequenciaJuazeiroIniciada ||
            conversando || juazeiroCenario == null || familiaDescanso == null || Camera.main == null ||
            jornada == null || !jornada.ParadaAtiva || jornada.ParadaAtual != 4 ||
            dialogueUI == null || dialogueUI.activeInHierarchy || InvestigationGuard.Blocked) return;
        sequenciaJuazeiroIniciada = true;
        if (!juazeiroIniciado)
        {
            juazeiroIniciado = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaJuazeiroIniciado = true;
            SalvarAguaTerceira();
        }
        if (placaJuazeiroLida) IniciarDescansoJuazeiro();
        else Falar(new[] { "Mariquinha" }, new[] {
            "To cansada painho, será que a gente num pode senta um pouco?"
        }, () => StartCoroutine(AproximarJuazeiro()));
    }

    private Vector3 CentroCameraJuazeiro()
    {
        float chao = juazeiroCenario.transform.position.y;
        float limiteY = chao - 0.25f + zoomJuazeiro * (1f - 2f * fracaoPainelJuazeiro);
        return new Vector3(juazeiroCenario.transform.position.x - 0.5f,
            Mathf.Min(chao + 1.2f, limiteY), cameraOriginal.z);
    }

    private void PrepararCameraJuazeiro()
    {
        RectTransform painel = dialoguePanel.GetComponent<RectTransform>();
        painel.GetWorldCorners(cantosPainelJuazeiro);
        Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
        Camera cameraUI = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        float topo = RectTransformUtility.WorldToScreenPoint(cameraUI, cantosPainelJuazeiro[1]).y;
        fracaoPainelJuazeiro = Mathf.Clamp(topo / Mathf.Max(1f, Screen.height), 0f, 0.85f);
        float altura = juazeiroCenario.GetComponent<SpriteRenderer>().bounds.size.y + 0.85f;
        zoomJuazeiro = Mathf.Max(5f, zoomOriginal, altura / (2f * (1f - fracaoPainelJuazeiro)));
    }

    private IEnumerator AproximarJuazeiro()
    {
        sequenciaAbrigo = true;
        BloquearMovimento();
        CapturarGrupoAbrigo();
        if (seguirCamera != null) seguirCamera.enabled = false;
        PrepararCameraJuazeiro();
        Vector3[] origens = new Vector3[pessoasAbrigo.Length];
        Vector3[] destinos = new Vector3[pessoasAbrigo.Length];
        float destinoMari = juazeiroCenario.transform.position.x - 0.2f;
        for (int i = 0; i < pessoasAbrigo.Length; i++)
        {
            origens[i] = pessoasAbrigo[i].position;
            destinos[i] = origens[i];
            float distancia = i > 0 && seguidoresAbrigo[i - 1] != null
                ? seguidoresAbrigo[i - 1].DistanciaAtras : i * 0.9f;
            destinos[i].x = destinoMari - distancia;
        }
        float tempo = 0f;
        deslocandoFamilia = true;
        while (tempo < 1.2f)
        {
            tempo += Time.deltaTime;
            float andamento = Mathf.Clamp01(tempo / 1.2f);
            for (int i = 0; i < pessoasAbrigo.Length; i++)
                Posicionar(pessoasAbrigo[i], Vector3.Lerp(origens[i], destinos[i], andamento));
            cameraAbrigo.transform.position = Vector3.Lerp(cameraOriginal, CentroCameraJuazeiro(), andamento);
            cameraAbrigo.orthographicSize = Mathf.Lerp(zoomOriginal, zoomJuazeiro, andamento);
            Animator animacao = mari.GetComponent<Animator>();
            if (animacao != null) animacao.SetBool("IsWalking", true);
            yield return null;
        }
        deslocandoFamilia = false;
        PararMari();
        sequenciaAbrigo = false;
        Falar(new[] { "Zefinha", "Pai", "Mariquinha", "Pai" }, new[] {
            "A placa ta dizendo que ainda falta umas doze léguas pra Fortaleza.",
            "Vamos sentar um pouco debaixo daquele Juazeiro, mas pouquinho mesmo, ainda tem muita estrada pela frente.",
            "Doze légua ainda? Ave Maria…",
            "Uma légua de cada vez, minha filha. Uma de cada vez."
        }, () =>
        {
            placaJuazeiroLida = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaPlacaJuazeiroLida = true;
            SalvarAguaTerceira();
            IniciarDescansoJuazeiro();
        });
    }

    private void IniciarDescansoJuazeiro()
    {
        sequenciaAbrigo = true;
        BloquearMovimento();
        CapturarGrupoAbrigo();
        if (seguirCamera != null) seguirCamera.enabled = false;
        PrepararCameraJuazeiro();
        StartCoroutine(SentarNoJuazeiro());
    }

    private IEnumerator SentarNoJuazeiro()
    {
        PrepararFade();
        yield return Escurecer(1f);
        cameraAbrigo.transform.position = CentroCameraJuazeiro();
        cameraAbrigo.orthographicSize = zoomJuazeiro;
        Sprite sprite = JanelaQuebrada && curativoAplicado ? familiaJuazeiroCurativo : familiaJuazeiro;
        SpriteRenderer descanso = familiaDescanso.GetComponent<SpriteRenderer>();
        descanso.sprite = sprite;
        familiaDescanso.transform.localScale = Vector3.one * (7.2f / sprite.bounds.size.x);
        familiaDescanso.SetActive(true);
        for (int i = 0; i < desenhosAbrigo.Length; i++)
            if (desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = false;
        yield return Escurecer(0f);
        RestaurarFade();
        sequenciaAbrigo = false;
        if (letrasJuazeiroConcluidas) IniciarCochiloJuazeiro();
        else Falar(new[] {
            "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha",
            "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha"
        }, new[] {
            "Como tu fez isso?",
            "Isso o quê?",
            "Tu olhou e já sabia o que estava escrito.",
            "Ah… Foi a Sinhazinha da fazenda, a filha do patrão, ela me ensinou escondida.",
            "As letra é bonita. Parece desenho.",
            "Parece mesmo.",
            "Tu me ensina?",
            "Ensino.",
            "Quem fez as letras?",
            "Eu não sei.",
            "Deve ser alguém muito inteligente, porque mesmo depois de morto, num deixa de falar.",
            "É… Acho que sim. Vamos tirar um cochilo também, antes que painho e mainha acorde."
        }, IniciarCochiloJuazeiro);
    }

    private void IniciarCochiloJuazeiro()
    {
        letrasJuazeiroConcluidas = true;
        if (GameProgress.Instance != null)
            GameProgress.Instance.Data.estradaLetrasJuazeiroConcluidas = true;
        SalvarAguaTerceira();
        sequenciaAbrigo = true;
        BloquearMovimento();
        StartCoroutine(CochilarNoJuazeiro());
    }

    private IEnumerator CochilarNoJuazeiro()
    {
        PrepararFade();
        yield return Escurecer(1f);
        yield return new WaitForSeconds(1.4f);
        familiaDescanso.SetActive(false);
        // Reconstroi a formacao anterior sem apagar o curativo nem aproximar as irmas feridas.
        for (int i = 0; i < seguidoresAbrigo.Length; i++)
        {
            SeguidorFamilia familiar = seguidoresAbrigo[i];
            if (familiar == null) continue;
            Vector3 lugar = familiar.transform.position;
            lugar.x = mari.transform.position.x - familiar.DistanciaAtras;
            Posicionar(familiar.transform, lugar);
        }
        foreach (SpriteRenderer desenho in desenhosAbrigo)
            if (desenho != null) desenho.flipX = false;
        RestaurarGrupoAbrigo();
        AplicarFerimento();
        Physics2D.SyncTransforms();
        RestaurarCamera();
        yield return Escurecer(0f);
        RestaurarFade();
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada != null && jornada.ParadaAtiva && jornada.ParadaAtual == 4)
        {
            juazeiroConcluido = true;
            if (GameProgress.Instance != null)
                GameProgress.Instance.Data.estradaJuazeiroConcluido = true;
            jornada.ConcluirParada(4);
        }
        sequenciaJuazeiroIniciada = false;
        sequenciaAbrigo = false;
        LiberarMovimento();
        RestaurarInventarioAbrigo();
        quadro = Time.frameCount;
        SalvarAguaTerceira();
    }

    private void PrepararAcampamento()
    {
        if (GameProgress.Instance != null)
        {
            GameProgressData d = GameProgress.Instance.Data;
            acampamentoIniciado = d.estradaAcampamentoIniciado;
            introAcampamentoConcluida = d.estradaIntroAcampamentoConcluida;
            materiaisAcampamento = d.estradaMateriaisAcampamento;
            fogueiraPreparada = d.estradaFogueiraPreparada;
            pedidoAlecrimConcluido = d.estradaPedidoAlecrimConcluido;
            refeicaoPreparada = d.estradaRefeicaoPreparada;
            conversaFogueiraConcluida = d.estradaConversaFogueiraConcluida;
            primeiraEscolhaNoturna = d.estradaPrimeiraEscolhaNoturna;
            segundaEscolhaNoturna = d.estradaSegundaEscolhaNoturna;
            conversaIrmasConcluida = d.estradaConversaIrmasConcluida;
            conversaPaisConcluida = d.estradaConversaPaisConcluida;
            acampamentoConcluido = d.estradaAcampamentoConcluido;
        }
        if (quintaParada == null || juazeiroSprite == null || familiaDescanso == null ||
            mari == null || mae == null || pai == null || mariquinha == null || baleia == null ||
            galhosSecos == null || folhasSecas == null || galhosAlecrim == null || passaro == null || prea == null ||
            galhosSecos.icon == null || folhasSecas.icon == null || galhosAlecrim.icon == null ||
            fogueiraMontada == null || fogueiraAcesa == null || fogueiraAssando == null || fogueiraBrasas == null ||
            irmasAcordadas == null || irmasAcordadasCurativo == null || irmasDormindo == null ||
            irmasDormindoCurativo == null || irmasPromessa == null || irmasSilencio == null ||
            irmasSilencioCurativo == null || paisAcordados == null)
        {
            Debug.LogError("SegundaParada: faltam referencias dos materiais, fogueira ou poses da ultima noite.", this);
            return;
        }
        float x = quintaParada.position.x;
        arvoreAcampamento = CriarAlvoEncontro("Juazeiro do acampamento", "arvoreAcampamento", juazeiroSprite, x + 0.8f, 5.6f, -1);
        arvoreAcampamento.GetComponent<BoxCollider2D>().enabled = false;
        arvoreAcampamento.SetActive(true);
        fogueiraCenario = CriarAlvoEncontro("Fogueira", "fogueira", fogueiraMontada, x + 1.9f, 1f, 12);
        ItemData[] materiais = { galhosSecos, folhasSecas, galhosAlecrim };
        float[] locais = { 3.5f, 5.5f, 7.3f };
        for (int i = 0; i < materiais.Length; i++)
            materiaisCenario[i] = CriarAlvoEncontro(materiais[i].itemName, "materialAcampamento" + i,
                materiais[i].icon, x + locais[i], 0.7f, 3);
        irmasCenario = CriarAlvoEncontro("Irmas na ultima noite", "irmasNoite", irmasAcordadas, x + 2f, 1f, 11);
        paisCenario = CriarAlvoEncontro("Pais na ultima noite", "paisNoite", paisAcordados, x - 1.4f, 1f, 11);
        irmasCenario.GetComponent<BoxCollider2D>().enabled = false;
        paisCenario.GetComponent<BoxCollider2D>().enabled = false;
        var desenhos = new List<SpriteRenderer>();
        foreach (SpriteRenderer desenho in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Transform pessoa = desenho.transform;
            if (pessoa == mari.transform || pessoa == mariquinha.transform || pessoa == mae.transform ||
                pessoa == pai.transform || pessoa == baleia || desenho.gameObject == familiaDescanso ||
                desenho.gameObject == irmasCenario || desenho.gameObject == paisCenario || desenho.gameObject == fogueiraCenario) continue;
            desenhos.Add(desenho);
        }
        cenarioHorario = desenhos.ToArray();
        coresHorario = new Color[cenarioHorario.Length];
        for (int i = 0; i < cenarioHorario.Length; i++) coresHorario[i] = cenarioHorario[i].color;
        AtualizarMateriaisAcampamento();
        AplicarHorarioAcampamento();
    }

    private bool AcampamentoEmAndamento()
    {
        ControleJornada j = ControleJornada.Instance;
        return j != null && j.ParadaAtiva && j.ParadaAtual == 5 && !acampamentoConcluido;
    }

    private void AtualizarMateriaisAcampamento()
    {
        for (int i = 0; i < materiaisCenario.Length; i++)
            if (materiaisCenario[i] != null)
                materiaisCenario[i].SetActive(introAcampamentoConcluida && !acampamentoConcluido &&
                    (materiaisAcampamento & (1 << i)) == 0);
        if (fogueiraCenario != null)
        {
            fogueiraCenario.SetActive(acampamentoIniciado);
            Sprite sprite = acampamentoConcluido || conversaFogueiraConcluida ? fogueiraBrasas :
                refeicaoPreparada && !conversaFogueiraConcluida ? fogueiraAssando :
                fogueiraPreparada ? fogueiraAcesa : fogueiraMontada;
            MostrarFogueira(sprite);
        }
    }

    private void MostrarFogueira(Sprite sprite)
    {
        if (fogueiraCenario == null || sprite == null) return;
        SpriteRenderer desenho = fogueiraCenario.GetComponent<SpriteRenderer>();
        desenho.sprite = sprite;
        fogueiraCenario.transform.localScale = Vector3.one * (1.6f / sprite.bounds.size.x);
        BoxCollider2D area = fogueiraCenario.GetComponent<BoxCollider2D>();
        area.size = sprite.bounds.size; area.offset = sprite.bounds.center;
    }

    private void AplicarHorarioAcampamento()
    {
        if (cenarioHorario == null) return;
        Color luz = !acampamentoIniciado || acampamentoConcluido ? Color.white :
            refeicaoPreparada ? new Color(0.23f, 0.28f, 0.45f) : new Color(0.86f, 0.68f, 0.63f);
        for (int i = 0; i < cenarioHorario.Length; i++)
            if (cenarioHorario[i] != null) cenarioHorario[i].color = coresHorario[i] * luz;
    }

    private void RestaurarCoresAcampamento()
    {
        if (cenarioHorario == null) return;
        for (int i = 0; i < cenarioHorario.Length; i++)
            if (cenarioHorario[i] != null) cenarioHorario[i].color = coresHorario[i];
    }

    private void AguardarFamiliaAcampamento()
    {
        if (familiaEsperando) return;
        maeSeguia = mae.enabled; paiSeguia = pai.enabled;
        baleiaSeguia = seguidorBaleia != null && seguidorBaleia.enabled;
        mae.enabled = false; pai.enabled = false;
        if (seguidorBaleia != null) seguidorBaleia.enabled = false;
        familiaEsperando = true;
    }

    private void PosicionarPaisAcampamento()
    {
        float x = quintaParada.position.x;
        Vector3 posicao = mae.transform.position; posicao.x = x; Posicionar(mae.transform, posicao);
        posicao = pai.transform.position; posicao.x = x - 1.4f; Posicionar(pai.transform, posicao);
        posicao = baleia.position; posicao.x = x - 2.7f; Posicionar(baleia, posicao);
    }

    private void AtualizarAcampamento()
    {
        if (restaurandoFamilia || !AcampamentoEmAndamento() || fogueiraCenario == null) return;
        if (noiteEmAndamento)
        {
            if (grupoCapturado)
            {
                LerPainelNoite();
                EnquadrarNoite();
            }
            return;
        }
        if (conversando || dialogueUI == null || dialogueUI.activeInHierarchy || InvestigationGuard.Blocked) return;
        if (!retomouAcampamento)
        {
            retomouAcampamento = true;
            AguardarFamiliaAcampamento();
            if (introAcampamentoConcluida) PosicionarPaisAcampamento();
        }
        if (!acampamentoIniciado)
        {
            acampamentoIniciado = true;
            AtualizarMateriaisAcampamento(); AplicarHorarioAcampamento(); SalvarAcampamento();
        }
        if (!introAcampamentoConcluida)
            Falar(new[] { "Pai", "Mãe" }, new[] {
                "Já tamo quase na entrada em Fortaleza, vamo fazer uma pausa, descansar uma última noite.",
                "Zefinha, vá junto de tua irmã procurar uns galhos secos e folhas secas, vamo montar uma fogueira debaixo daquele juazeiro."
            }, () => StartCoroutine(MontarAcampamento()));
        else if (refeicaoPreparada) IniciarNoiteAcampamento();
    }

    private IEnumerator MontarAcampamento()
    {
        sequenciaAbrigo = true; BloquearMovimento(); CapturarGrupoAbrigo();
        PrepararFade(); yield return Escurecer(1f);
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 3f; Posicionar(mari.transform, lugar);
        lugar = mariquinha.transform.position; lugar.x = mari.transform.position.x - mariquinha.DistanciaAtras; Posicionar(mariquinha.transform, lugar);
        PosicionarPaisAcampamento();
        introAcampamentoConcluida = true; AtualizarMateriaisAcampamento();
        RestaurarCamera();
        yield return Escurecer(0f); RestaurarFade();
        RestaurarGrupoAbrigo(); RestaurarInventarioAbrigo(); LiberarMovimento();
        sequenciaAbrigo = false; quadro = Time.frameCount;
        SalvarAcampamento();
    }

    private void ColetarMaterialAcampamento(int indiceMaterial)
    {
        if (indiceMaterial < 0 || indiceMaterial > 2 || !AcampamentoEmAndamento() ||
            !introAcampamentoConcluida || noiteEmAndamento || InvestigationGuard.Blocked ||
            (materiaisAcampamento & (1 << indiceMaterial)) != 0 || InventoryManager.Instance == null) return;
        ItemData[] itens = { galhosSecos, folhasSecas, galhosAlecrim };
        CollectPrompt coleta = FindFirstObjectByType<CollectPrompt>();
        if (coleta != null) coleta.Show(itens[indiceMaterial], () =>
        {
            materiaisAcampamento |= 1 << indiceMaterial;
            AtualizarMateriaisAcampamento(); quadro = Time.frameCount; SalvarAcampamento();
        });
    }

    private bool AcertouMaeAcampamento(Vector3 ponto)
    {
        if (mae == null) return false;
        SpriteRenderer desenho = mae.GetComponent<SpriteRenderer>();
        if (desenho == null || !desenho.enabled || !desenho.gameObject.activeInHierarchy) return false;
        Bounds area = desenho.bounds; area.Expand(new Vector3(0.25f, 0.25f, 0f));
        return ponto.x >= area.min.x && ponto.x <= area.max.x && ponto.y >= area.min.y && ponto.y <= area.max.y;
    }

    public bool SoltarNoAcampamento(ItemData item, Vector2 posicaoTela)
    {
        if (item == null || !AcampamentoEmAndamento() || noiteEmAndamento || Camera.main == null ||
            (item != galhosSecos && item != folhasSecas && item != galhosAlecrim && item != passaro && item != prea)) return false;
        Ray raio = Camera.main.ScreenPointToRay(posicaoTela); float distancia;
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        if (!plano.Raycast(raio, out distancia)) return false;
        Vector3 ponto = raio.GetPoint(distancia);
        Collider2D area = fogueiraCenario != null ? fogueiraCenario.GetComponent<Collider2D>() : null;
        if (!AcertouMaeAcampamento(ponto) && (area == null || !area.OverlapPoint(ponto))) return false;
        return InteragirAcampamento(true);
    }

    private bool ConsumirItensAcampamento(params ItemData[] usados)
    {
        InventoryManager inventario = InventoryManager.Instance;
        if (inventario == null) return false;
        var itens = new List<ItemData>(inventario.Items);
        foreach (ItemData item in usados)
            if (item == null || !itens.Remove(item)) return false;
        inventario.ReplaceItems(itens);
        return true;
    }

    private bool InteragirAcampamento(bool peloInventario = false)
    {
        if (!AcampamentoEmAndamento() || !introAcampamentoConcluida || noiteEmAndamento ||
            conversando || sequenciaAbrigo || PauseHelper.BlockInput() || Time.timeScale == 0f ||
            Time.frameCount <= quadro || (!peloInventario && InvestigationGuard.Blocked) ||
            mari == null || mae == null || Vector2.Distance(mari.transform.position, mae.transform.position) > 5f) return false;
        InventoryTabController.Instance?.Close();
        if (!fogueiraPreparada)
        {
            if (!Possui(galhosSecos) || !Possui(folhasSecas))
            {
                Falar(new[] { "Mãe" }, new[] {
                    "Zefinha, vá junto de tua irmã procurar uns galhos secos e folhas secas, vamo montar uma fogueira debaixo daquele juazeiro."
                }, null);
                return true;
            }
            if (!ConsumirItensAcampamento(galhosSecos, folhasSecas)) return false;
            fogueiraPreparada = true; AtualizarMateriaisAcampamento(); SalvarAcampamento();
        }
        if (!pedidoAlecrimConcluido || !Possui(galhosAlecrim))
        {
            Falar(new[] { "Mãe" }, new[] {
                "Agora vá atrás de alguns galhos de alecrim, pra gente conseguir colocar os bicho no espeto, vamo assar na fogueira."
            }, () =>
            {
                pedidoAlecrimConcluido = true; SalvarAcampamento();
                if (Possui(galhosAlecrim)) PrepararRefeicaoAcampamento();
            });
        }
        else PrepararRefeicaoAcampamento();
        return true;
    }

    private void PrepararRefeicaoAcampamento()
    {
        if (refeicaoPreparada || !fogueiraPreparada || !pedidoAlecrimConcluido) return;
        if (!ConsumirItensAcampamento(galhosAlecrim, passaro, prea))
        {
            Falar(new[] { "Objetivo" }, new[] { "Para preparar a refeição, reúna os galhos de alecrim, o pássaro e o preá no inventário." }, null);
            return;
        }
        refeicaoPreparada = true; AtualizarMateriaisAcampamento(); SalvarAcampamento();
        IniciarNoiteAcampamento();
    }

    private void LerPainelNoite()
    {
        dialoguePanel.GetComponent<RectTransform>().GetWorldCorners(cantosPainelJuazeiro);
        Canvas canvas = dialoguePanel.GetComponentInParent<Canvas>();
        Camera cameraUI = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        float topo = RectTransformUtility.WorldToScreenPoint(cameraUI, cantosPainelJuazeiro[1]).y;
        fracaoPainelNoite = Mathf.Clamp(topo / Mathf.Max(1f, Screen.height), 0f, 0.85f);
    }

    private Bounds AreaFocoNoite()
    {
        GameObject alvo = focoNoite == 1 ? irmasCenario : focoNoite == 2 ? paisCenario : familiaDescanso;
        Bounds area = alvo.GetComponent<SpriteRenderer>().bounds;
        if (focoNoite == 0)
        {
            area.Encapsulate(arvoreAcampamento.GetComponent<SpriteRenderer>().bounds);
            area.Encapsulate(fogueiraCenario.GetComponent<SpriteRenderer>().bounds);
        }
        return area;
    }

    private void EnquadrarNoite()
    {
        Bounds area = AreaFocoNoite();
        float zoom = Mathf.Max(2.5f, (area.size.y + 0.9f) / (2f * (1f - fracaoPainelNoite)),
            (area.size.x + 1f) / (2f * cameraAbrigo.aspect));
        cameraAbrigo.orthographicSize = zoom;
        cameraAbrigo.transform.position = new Vector3(LimitarCameraNoiteX(area.center.x, zoom),
            area.min.y - 0.25f + zoom * (1f - 2f * fracaoPainelNoite), cameraOriginal.z);
    }

    private float LimitarCameraNoiteX(float x, float zoom)
    {
        if (limitesEstrada == null) return x;
        Bounds area = limitesEstrada.bounds;
        float metade = zoom * cameraAbrigo.aspect;
        float minimo = area.min.x + metade, maximo = area.max.x - metade;
        return minimo <= maximo ? Mathf.Clamp(x, minimo, maximo) : area.center.x;
    }

    private void MostrarPoseNoite(GameObject alvo, Sprite sprite, float largura)
    {
        alvo.GetComponent<SpriteRenderer>().sprite = sprite;
        alvo.transform.localScale = Vector3.one * (largura / sprite.bounds.size.x);
        alvo.GetComponent<SpriteRenderer>().color = new Color(0.82f, 0.84f, 0.94f);
        alvo.SetActive(true);
    }

    private void IniciarNoiteAcampamento()
    {
        if (noiteEmAndamento || !refeicaoPreparada || acampamentoConcluido) return;
        noiteEmAndamento = true; sequenciaAbrigo = true;
        BloquearMovimento(); CapturarGrupoAbrigo();
        if (seguirCamera != null) seguirCamera.enabled = false;
        LerPainelNoite();
        StartCoroutine(MostrarNoiteAcampamento());
    }

    private IEnumerator MostrarNoiteAcampamento()
    {
        PrepararFade(); yield return Escurecer(1f);
        AplicarHorarioAcampamento();
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 2f; Posicionar(mari.transform, lugar);
        for (int i = 0; i < desenhosAbrigo.Length; i++)
            if (desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = false;
        familiaDescanso.transform.position = new Vector3(quintaParada.position.x + 1f, -1.8f, 0f);
        MostrarPoseNoite(familiaDescanso, JanelaQuebrada && curativoAplicado ? familiaJuazeiroCurativo : familiaJuazeiro, 7.2f);
        focoNoite = 0;
        if (conversaFogueiraConcluida)
        {
            familiaDescanso.SetActive(false);
            MostrarFogueira(fogueiraBrasas);
            MostrarPoseNoite(paisCenario, paisAcordados, 4.8f);
            MostrarPoseNoite(irmasCenario, conversaIrmasConcluida ? SpriteIrmasDormindo() :
                JanelaQuebrada ? irmasAcordadasCurativo : irmasAcordadas, 4.8f);
            focoNoite = conversaIrmasConcluida || conversaPaisConcluida ? 2 : 1;
        }
        EnquadrarNoite();
        yield return Escurecer(0f); RestaurarFade();
        if (!conversaFogueiraConcluida)
        {
            yield return new WaitForSeconds(2.5f);
            sequenciaAbrigo = false;
            Falar(new[] { "Mariquinha", "Mãe", "Mariquinha", "Mãe", "Mariquinha", "Mãe", "Zefinha", "Mãe", "Mariquinha", "Pai" }, new[] {
                "Será que dueu ou foi num sopro? igual voinho?",
                "Tá falando de que Severina?",
                "Do homem na estrada mainha.",
                "Num sei minha filha, esqueça isso, guardar tanta desgraça faz mal para o coração.",
                "Num guarda é pio, num quero esquecer as coisa que a gente passou, vou escrever um livro mainha, tu vai vê, Zefinha vai me ensinar a ler e escrever.",
                "E desde quando tu sabe ler e escrever Maria Jozefa?",
                "A sinhazinha da fazenda que painho trabalhava me ensinou às escondidas mainha, sei um pouco.",
                "Hora essa! Livro num enche bucho não Severina, tu devia é aprender a bordar, isso sim pode te ajudar no futuro…",
                "Num quero, quero mesmo ser inteligente igual gente que escreve coisa, num é painho? isso num da dinheiro?",
                "Sabe… Tem hora que eu acho… Que Deus fez o sertanejo diferente. Gente quebra. Gente desiste. Mas nóis… Nóis é bicho. Bicho apanha. Bicho passa fome. Bicho sangra. Mas continua andando. Enquanto eu respirá… Ocês num morre. Bicho trabaia Mariquinha, trabaia para não passar fome, num tem tempo pra escrever não."
            }, () =>
            {
                conversaFogueiraConcluida = true; MostrarFogueira(fogueiraBrasas); SalvarAcampamento();
                IniciarCenaIrmas();
            });
        }
        else
        {
            sequenciaAbrigo = false;
            if (conversaPaisConcluida) IniciarFimNoite();
            else if (conversaIrmasConcluida) ConversarPaisNoite();
            else RetomarConversaIrmas();
        }
    }

    private void IniciarCenaIrmas()
    {
        sequenciaAbrigo = true; BloquearMovimento();
        LerPainelNoite(); StartCoroutine(MostrarCenaIrmas());
    }

    private IEnumerator MostrarCenaIrmas()
    {
        PrepararFade(); yield return Escurecer(1f);
        familiaDescanso.SetActive(false); MostrarFogueira(fogueiraBrasas);
        MostrarPoseNoite(irmasCenario, JanelaQuebrada ? irmasAcordadasCurativo : irmasAcordadas, 4.8f);
        MostrarPoseNoite(paisCenario, paisAcordados, 4.8f);
        focoNoite = 1; EnquadrarNoite();
        yield return Escurecer(0f); RestaurarFade(); sequenciaAbrigo = false;
        RetomarConversaIrmas();
    }

    private void RetomarConversaIrmas()
    {
        if (primeiraEscolhaNoturna == 0)
            Falar(new[] { "Mariquinha" }, new[] { "Zefinha… Tu tá acordada?" }, () => MostrarEscolhaNoturna(false));
        else ContinuarPrimeiraEscolhaNoturna();
    }

    private void MostrarEscolhaNoturna(bool segunda)
    {
        LimparEscolhas();
        foreach (Transform filho in choicesContainer.transform)
        {
            Button botao = filho.GetComponent<Button>();
            if (botao != null) botoes.Add(botao);
        }
        string[] opcoes = segunda ? new[] { "Eu prometo.", "Vai dar tudo certo.", "Ficar em silêncio." }
            : new[] { "Ficar em silêncio.", "Responder." };
        if (botoes.Count < opcoes.Length)
        {
            Debug.LogError("SegundaParada: faltam os botoes existentes para as escolhas noturnas.", this);
            LimparEscolhas(); LiberarMovimento(); return;
        }
        escolhaNoturnaAberta = true; segundaEscolhaNoturnaAberta = segunda;
        conversando = true; esperandoEscolha = true; depois = null; BloquearMovimento();
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true); choicesContainer.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel); Grupo(choicesContainer);
        characterNameText.text = "Escolha";
        dialogueText.text = segunda ? "Promete que nós nunca vai se largá?" : "Zefinha… Tu tá acordada?";
        if (portraitImage != null) portraitImage.enabled = false;
        for (int i = 0; i < botoes.Count; i++)
        {
            Button botao = botoes[i]; botao.gameObject.SetActive(i < opcoes.Length);
            if (i >= opcoes.Length) continue;
            TMP_Text texto = botao.GetComponentInChildren<TMP_Text>(true);
            if (texto != null) texto.text = opcoes[i];
            int opcao = i + 1;
            botao.onClick.RemoveAllListeners(); botao.onClick.AddListener(() => EscolherNoite(opcao));
            botao.interactable = true;
        }
        quadro = Time.frameCount;
    }

    private void EscolherNoite(int opcao)
    {
        if (!escolhaNoturnaAberta || !esperandoEscolha || PauseHelper.BlockInput() || Time.timeScale == 0f ||
            opcao < 1 || opcao > (segundaEscolhaNoturnaAberta ? 3 : 2)) return;
        bool segunda = segundaEscolhaNoturnaAberta;
        escolhaNoturnaAberta = false; esperandoEscolha = false; conversando = false;
        LimparEscolhas(); choicesContainer.SetActive(false); dialogueUI.SetActive(false);
        InvestigationGuard.BlockCurrentClick();
        if (segunda) segundaEscolhaNoturna = opcao;
        else primeiraEscolhaNoturna = opcao;
        SalvarAcampamento();
        if (segunda) ContinuarSegundaEscolhaNoturna();
        else ContinuarPrimeiraEscolhaNoturna();
    }

    private void ContinuarPrimeiraEscolhaNoturna()
    {
        if (primeiraEscolhaNoturna == 1)
        {
            Falar(new[] { "Mariquinha" }, new[] { "Acho que tu já dormiu…" }, () =>
                StartCoroutine(PausarConversaNoite(0.8f, () => Falar(new[] { "Mariquinha", "Mariquinha" }, new[] {
                    "Eu queria tanto que a gente já tivesse chegado…", "Boa noite, irmã."
                }, () => StartCoroutine(EncerrarConversaIrmas())))));
            return;
        }
        if (segundaEscolhaNoturna != 0) { ContinuarSegundaEscolhaNoturna(); return; }
        Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha" }, new[] {
            "Tô.", "Posso conta um segredo?", "Pode.",
            "Eu num quero sê bicho não… Quero sê gente. Quero brincá. Quero estudá. Quero comê até encher a barriga. Quero vê mainha rindo de novo. Quero ser inteligente igual tu. Os porco pode sê bicho. As galinha. Os boi. Mas nós não. Promete que nós nunca vai se largá?"
        }, () => MostrarEscolhaNoturna(true));
    }

    private IEnumerator PausarConversaNoite(float duracao, Action continuacao)
    {
        sequenciaAbrigo = true; BloquearMovimento();
        yield return new WaitForSeconds(duracao);
        sequenciaAbrigo = false;
        continuacao?.Invoke();
    }

    private void ContinuarSegundaEscolhaNoturna()
    {
        if (segundaEscolhaNoturna == 1)
            Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha" }, new[] {
                "Eu prometo.",
                "E tu promete vai me ensina? Pra eu fica famosa com meu livro, igual as moça de vestido bunito que anda na cidade.",
                "Prometo.", "Até eu aprender igualzinho a tu.", "Igualzinho a mim não… Melhor.",
                "Melhor que tu?", "Muito melhor.", "Promessa?", "Promessa.",
                "Você é a melhor Zefinha, uma irma boa, sabe que te amo num sabe?", "Eu sei."
            }, () => StartCoroutine(EncerrarConversaIrmas()));
        else if (segundaEscolhaNoturna == 2)
            Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Mariquinha", "Zefinha" }, new[] {
                "Vai dar tudo certo, Mariquinha.", "Tu acha mesmo?",
                "Acho. A gente vai chegar na cidade, painho vai arrumar trabalho… As coisa vão melhorar.",
                "Eu queria acreditar assim também…", "Então acredita comigo.", "Tá bom… Vou tentar.",
                "Boa noite, Zefinha.", "Boa noite, irmã."
            }, () => StartCoroutine(EncerrarConversaIrmas()));
        else if (segundaEscolhaNoturna == 3)
        {
            Falar(new[] { "Mariquinha" }, new[] { "Tu num precisa prometer se num quiser…" }, () =>
                StartCoroutine(PausarConversaNoite(0.8f, () => Falar(new[] {
                    "Mariquinha", "Zefinha", "Mariquinha", "Mariquinha"
                }, new[] {
                    "Eu só tenho medo de perder vocês.", "…", "Desculpa… Num queria te aperrear.", "Boa noite, Zefinha."
                }, ConcluirSilencioNoturno))));
        }
    }

    private void ConcluirSilencioNoturno()
    {
        MostrarPoseNoite(irmasCenario, JanelaQuebrada ? irmasSilencioCurativo : irmasSilencio, 4.8f);
        Falar(new[] { "Zefinha (em pensamentos)" }, new[] {
            "Eu queria prometer… Mas nem sei o que vai acontecer amanhã."
        }, () => StartCoroutine(EncerrarConversaIrmas()));
    }

    private IEnumerator EncerrarConversaIrmas()
    {
        sequenciaAbrigo = true; BloquearMovimento();
        if (segundaEscolhaNoturna == 3) yield return new WaitForSeconds(1.8f);
        // Somente a promessa sem ferimento mostra as maos entrelacadas.
        MostrarPoseNoite(irmasCenario, SpriteIrmasDormindo(), 4.8f);
        yield return new WaitForSeconds(2f);
        conversaIrmasConcluida = true; SalvarAcampamento();
        sequenciaAbrigo = false; IniciarCenaPais();
    }

    private Sprite SpriteIrmasDormindo()
    {
        return JanelaQuebrada ? irmasDormindoCurativo :
            primeiraEscolhaNoturna == 2 && segundaEscolhaNoturna == 1 ? irmasPromessa : irmasDormindo;
    }

    private void IniciarCenaPais()
    {
        sequenciaAbrigo = true; BloquearMovimento(); LerPainelNoite();
        StartCoroutine(MostrarCenaPais());
    }

    private IEnumerator MostrarCenaPais()
    {
        PrepararFade(); yield return Escurecer(1f);
        familiaDescanso.SetActive(false);
        MostrarPoseNoite(paisCenario, paisAcordados, 4.8f);
        MostrarPoseNoite(irmasCenario, SpriteIrmasDormindo(), 4.8f);
        focoNoite = 2; EnquadrarNoite();
        yield return Escurecer(0f); RestaurarFade(); sequenciaAbrigo = false;
        ConversarPaisNoite();
    }

    private void ConversarPaisNoite()
    {
        Falar(new[] { "Pai", "Mãe", "Pai", "Mãe" }, new[] {
            "Elas num merece isso.", "Eu sei.", "Queria te deixado alguma coisa.", "Mas tu deixou. Tu ficou."
        }, () =>
        {
            conversaPaisConcluida = true; SalvarAcampamento(); IniciarFimNoite();
        });
    }

    private void IniciarFimNoite()
    {
        sequenciaAbrigo = true; BloquearMovimento();
        StartCoroutine(AmanhecerAcampamento());
    }

    private IEnumerator AmanhecerAcampamento()
    {
        familiaDescanso.SetActive(false);
        MostrarPoseNoite(paisCenario, paisAcordados, 4.8f);
        focoNoite = 2; EnquadrarNoite();
        float zoom = cameraAbrigo.orthographicSize;
        Bounds area = paisCenario.GetComponent<SpriteRenderer>().bounds;
        float tempo = 0f;
        while (tempo < 2.5f)
        {
            tempo += Time.deltaTime;
            float tamanho = Mathf.Lerp(zoom, zoom + 1.2f, Mathf.Clamp01(tempo / 2.5f));
            cameraAbrigo.orthographicSize = tamanho;
            cameraAbrigo.transform.position = new Vector3(LimitarCameraNoiteX(area.center.x, tamanho),
                area.min.y - 0.25f + tamanho * (1f - 2f * fracaoPainelNoite), cameraOriginal.z);
            yield return null;
        }
        PrepararFade(); yield return Escurecer(1f);
        yield return new WaitForSeconds(1.4f);
        irmasCenario.SetActive(false); paisCenario.SetActive(false);
        RestaurarCoresAcampamento(); MostrarFogueira(fogueiraBrasas);
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 2.8f; Posicionar(mari.transform, lugar);
        foreach (SeguidorFamilia familiar in seguidoresAbrigo)
        {
            if (familiar == null) continue;
            lugar = familiar.transform.position; lugar.x = mari.transform.position.x - familiar.DistanciaAtras;
            Posicionar(familiar.transform, lugar);
        }
        foreach (SpriteRenderer desenho in desenhosAbrigo) if (desenho != null) desenho.flipX = false;
        RestaurarGrupoAbrigo(); RetomarFamilia(); AplicarFerimento();
        Physics2D.SyncTransforms(); RestaurarCamera();
        yield return Escurecer(0f); RestaurarFade();
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada != null && jornada.ParadaAtiva && jornada.ParadaAtual == 5)
        {
            acampamentoConcluido = true; jornada.ConcluirParada(5);
        }
        noiteEmAndamento = false; sequenciaAbrigo = false;
        RestaurarInventarioAbrigo(); LiberarMovimento();
        quadro = Time.frameCount; SalvarAcampamento();
    }

    private void SalvarAcampamento()
    {
        if (GameProgress.Instance == null || GameSaveSystem.IsRestoring) return;
        GameProgressData d = GameProgress.Instance.Data;
        d.estradaAcampamentoIniciado = acampamentoIniciado;
        d.estradaIntroAcampamentoConcluida = introAcampamentoConcluida;
        d.estradaMateriaisAcampamento = materiaisAcampamento;
        d.estradaFogueiraPreparada = fogueiraPreparada;
        d.estradaPedidoAlecrimConcluido = pedidoAlecrimConcluido;
        d.estradaRefeicaoPreparada = refeicaoPreparada;
        d.estradaConversaFogueiraConcluida = conversaFogueiraConcluida;
        d.estradaPrimeiraEscolhaNoturna = primeiraEscolhaNoturna;
        d.estradaSegundaEscolhaNoturna = segundaEscolhaNoturna;
        d.estradaConversaIrmasConcluida = conversaIrmasConcluida;
        d.estradaConversaPaisConcluida = conversaPaisConcluida;
        d.estradaAcampamentoConcluido = acampamentoConcluido;
        SalvarAguaTerceira();
    }

    private void PrepararFortaleza()
    {
        if (GameProgress.Instance != null)
        {
            GameProgressData d = GameProgress.Instance.Data;
            fortalezaIniciada = d.estradaFortalezaIniciada;
            dialogoFortalezaConcluido = d.estradaDialogoFortalezaConcluido;
            abordagemIniciada = d.estradaAbordagemIniciada;
            atoConcluido = d.ato1Concluido;
        }
        if (quintaParada == null || cenarioFortaleza == null || placaFortaleza == null ||
            policialParado == null || policialPassoA == null || policialPassoB == null ||
            policialBloqueio == null || familiaSurpresa == null || familiaSurpresaCurativo == null ||
            mari == null || mariquinha == null || mae == null || pai == null || baleia == null ||
            Camera.main == null || dialogueUI == null || dialoguePanel == null ||
            dialogueText == null || characterNameText == null || choicesContainer == null)
        {
            Debug.LogError("SegundaParada: faltam referencias da chegada a Fortaleza.", this);
            return;
        }
        float x = quintaParada.position.x;
        float largura = 12.8f * cenarioFortaleza.bounds.size.x / cenarioFortaleza.bounds.size.y;
        cidadeCenario = CriarAlvoEncontro("Fortaleza", "cidade", cenarioFortaleza,
            x + 7f + largura * 0.5f, 12.8f, -18);
        cidadeCenario.transform.position += Vector3.down * 4f;
        cidadeCenario.GetComponent<BoxCollider2D>().enabled = false;
        cidadeCenario.SetActive(true);
        GameObject placa = CriarAlvoEncontro("Entrada de Fortaleza", "entradaFortaleza", placaFortaleza,
            x + 12f, 1.8f, 1);
        placa.GetComponent<BoxCollider2D>().enabled = false;
        placa.SetActive(true);
        Bounds b = placa.GetComponent<SpriteRenderer>().bounds;
        GameObject rotulo = new GameObject("Texto Fortaleza", typeof(TextMeshPro));
        rotulo.transform.SetParent(transform, false);
        rotulo.transform.position = new Vector3(b.center.x, b.min.y + b.size.y * 0.66f, -0.1f);
        TextMeshPro texto = rotulo.GetComponent<TextMeshPro>();
        texto.font = dialogueText.font;
        texto.text = "FORTALEZA";
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(0.24f, 0.12f, 0.07f);
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 2f; texto.fontSizeMax = 3.2f; texto.fontSize = 3.2f;
        texto.rectTransform.sizeDelta = new Vector2(b.size.x * 0.82f, b.size.y * 0.4f);
        texto.GetComponent<MeshRenderer>().sortingOrder = 2;
        for (int i = 0; i < policiais.Length; i++)
        {
            policiais[i] = CriarAlvoEncontro("Policial " + (i + 1), "policial", i == 0 ? policialBloqueio : policialParado,
                x + 24f + i * 0.8f, 2.9f, 13);
            policiais[i].GetComponent<BoxCollider2D>().enabled = false;
            policiais[i].SetActive(true);
        }
        float altura = 6.8f * familiaSurpresa.bounds.size.y / familiaSurpresa.bounds.size.x;
        familiaFinal = CriarAlvoEncontro("Familia na abordagem", "familiaFinal", familiaSurpresa,
            x + 20f, altura, 11);
        familiaFinal.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void AtualizarFortaleza()
    {
        if (cidadeCenario == null || restaurandoFamilia || finalEmAndamento || encerramentoMostrado ||
            GameSaveSystem.IsRestoring || !acampamentoConcluido) return;
        if (atoConcluido)
        {
            BloquearMovimento(); CapturarGrupoAbrigo();
            if (seguirCamera != null) seguirCamera.enabled = false;
            MostrarFimAto();
            return;
        }
        if (conversando || (dialogueUI != null && dialogueUI.activeInHierarchy)) return;
        if (abordagemIniciada || (dialogoFortalezaConcluido && mari.transform.position.x >= quintaParada.position.x + 21.8f))
        {
            IniciarAbordagem(); return;
        }
        if (!dialogoFortalezaConcluido && (fortalezaIniciada || mari.transform.position.x >= quintaParada.position.x + 12f))
        {
            fortalezaIniciada = true; SalvarFortaleza();
            Falar(new[] { "Mariquinha", "Mãe", "Mariquinha", "Pai" }, new[] {
                "Zefinha… Olha! Quanta casa! Mainha! Painho! Nos chegamu foi? Num vejo a hora de provar sorvete.",
                "É cidade bonita…", "La ninguém passa fome?", "Num vai passar mais."
            }, () => { dialogoFortalezaConcluido = true; SalvarFortaleza(); }, true);
        }
    }

    private void IniciarAbordagem()
    {
        finalEmAndamento = true; abordagemIniciada = true;
        BloquearMovimento(); CapturarGrupoAbrigo();
        if (seguirCamera != null) seguirCamera.enabled = false;
        SalvarFortaleza();
        Falar(new[] { "Policial" }, new[] {
            "Parados! Onde pensam que estão indo? Larguem essas trouxas."
        }, () => { BloquearMovimento(); sequenciaAbrigo = true; StartCoroutine(EncerrarJornada()); });
    }

    private void PosePolicial(GameObject policial, Sprite pose, bool olharDireita)
    {
        SpriteRenderer desenho = policial.GetComponent<SpriteRenderer>();
        float pe = desenho.bounds.min.y;
        desenho.sprite = pose; desenho.flipX = olharDireita;
        policial.transform.localScale = Vector3.one * (2.9f / Mathf.Max(0.01f, pose.bounds.size.y));
        policial.transform.position += Vector3.up * (pe - desenho.bounds.min.y);
    }

    private void EnquadrarAbordagem(float zoom)
    {
        Bounds b = cidadeCenario.GetComponent<SpriteRenderer>().bounds;
        cameraAbrigo.orthographicSize = Mathf.Min(zoom, b.size.x / (2f * cameraAbrigo.aspect), b.size.y * 0.5f - 0.2f);
        float meiaLargura = cameraAbrigo.orthographicSize * cameraAbrigo.aspect;
        Vector3 p = cameraOriginal;
        p.x = Mathf.Clamp(familiaFinal.transform.position.x + 0.4f, b.min.x + meiaLargura, b.max.x - meiaLargura);
        p.y = Mathf.Clamp(p.y, b.min.y + cameraAbrigo.orthographicSize, b.max.y - cameraAbrigo.orthographicSize);
        cameraAbrigo.transform.position = p;
    }

    private IEnumerator EncerrarJornada()
    {
        PrepararFade(); yield return Escurecer(1f);
        foreach (SpriteRenderer desenho in desenhosAbrigo) if (desenho != null) desenho.enabled = false;
        Sprite pose = JanelaQuebrada && curativoAplicado ? familiaSurpresaCurativo : familiaSurpresa;
        familiaFinal.GetComponent<SpriteRenderer>().sprite = pose;
        familiaFinal.transform.localScale = Vector3.one * (6.8f / pose.bounds.size.x);
        familiaFinal.transform.position = new Vector3(mari.transform.position.x - 1.4f, -2.4f, 0f);
        familiaFinal.SetActive(true);
        EnquadrarAbordagem(5f);
        yield return Escurecer(0f); RestaurarFade();
        Bounds familia = familiaFinal.GetComponent<SpriteRenderer>().bounds;
        Vector3 centro = familiaFinal.transform.position;
        Vector3[] origens = new Vector3[3];
        Vector3[] destinos = {
            new Vector3(familia.max.x + 0.8f, -2.4f, 0f),
            new Vector3(familia.min.x - 1.2f, -2.4f, 0f),
            new Vector3(familia.center.x, -1.7f, 0f)
        };
        for (int i = 0; i < policiais.Length; i++)
        {
            origens[i] = policiais[i].transform.position;
            policiais[i].GetComponent<SpriteRenderer>().sortingOrder = i == 0 ? 13 : 10;
        }
        float tempo = 0f;
        while (tempo < 4f)
        {
            tempo += Time.deltaTime;
            float t = Mathf.Clamp01(tempo / 2.2f);
            for (int i = 0; i < policiais.Length; i++)
            {
                Vector3 p = Vector3.Lerp(origens[i], destinos[i], t);
                if (i > 0) p.y += Mathf.Sin(t * Mathf.PI) * 1.2f;
                policiais[i].transform.position = p;
                Sprite passo = ((int)(tempo / 0.18f) % 2 == 0) ? policialPassoA : policialPassoB;
                PosePolicial(policiais[i], t < 1f ? passo : (i == 0 ? policialBloqueio : policialParado), i == 1);
                if (i == 1 && t >= 1f) policiais[i].GetComponent<SpriteRenderer>().sortingOrder = 13;
            }
            familiaFinal.transform.position = centro + Vector3.right * (Mathf.Sin(tempo * 15f) * 0.035f);
            EnquadrarAbordagem(Mathf.Lerp(5f, 6.1f, tempo / 4f));
            yield return null;
        }
        familiaFinal.transform.position = centro;
        PrepararFade(); yield return Escurecer(1f);
        yield return new WaitForSeconds(1f);
        atoConcluido = true; SalvarFortaleza();
        MostrarFimAto();
        sequenciaAbrigo = false;
    }

    private void MostrarFimAto()
    {
        // O painel existente apresenta o encerramento sobre a cena escurecida.
        desenhosFundoFim = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        fundoFimAtivo = new bool[desenhosFundoFim.Length];
        for (int i = 0; i < desenhosFundoFim.Length; i++)
        {
            fundoFimAtivo[i] = desenhosFundoFim[i].enabled;
            desenhosFundoFim[i].enabled = false;
        }
        fundoCameraOriginal = cameraAbrigo.clearFlags; corCameraOriginal = cameraAbrigo.backgroundColor;
        cameraAbrigo.clearFlags = CameraClearFlags.SolidColor; cameraAbrigo.backgroundColor = Color.black;
        // Textos de placas usam MeshRenderer, fora do conjunto de sprites acima.
        cidadeCenario.SetActive(false);
        foreach (TextMeshPro placa in GetComponentsInChildren<TextMeshPro>(true)) placa.gameObject.SetActive(false);
        RestaurarFade(); LimparEscolhas();
        encerramentoMostrado = true; conversando = true; esperandoEscolha = true;
        depois = null;
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        characterNameText.text = "Currais";
        dialogueText.text = "FIM DO ATO 1 — A JORNADA DOS RETIRANTES";
        if (portraitImage != null) portraitImage.enabled = false;
        choicesContainer.SetActive(true); Grupo(choicesContainer);
        Button[] opcoes = choicesContainer.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < opcoes.Length; i++)
        {
            Button botao = opcoes[i]; botoes.Add(botao);
            botao.onClick.RemoveAllListeners(); botao.gameObject.SetActive(i == 0);
            if (i != 0) continue;
            TMP_Text texto = botao.GetComponentInChildren<TMP_Text>(true);
            if (texto != null) texto.text = "Menu principal";
            botao.interactable = true; botao.onClick.AddListener(VoltarMenuFim);
        }
    }

    private void RestaurarFundoFim()
    {
        if (desenhosFundoFim == null) return;
        for (int i = 0; i < desenhosFundoFim.Length; i++)
            if (desenhosFundoFim[i] != null) desenhosFundoFim[i].enabled = fundoFimAtivo[i];
        if (cameraAbrigo != null)
        {
            cameraAbrigo.clearFlags = fundoCameraOriginal; cameraAbrigo.backgroundColor = corCameraOriginal;
        }
        desenhosFundoFim = null; fundoFimAtivo = null;
    }

    private void VoltarMenuFim()
    {
        if (!encerramentoMostrado || PauseHelper.BlockInput() || !Application.CanStreamedLevelBeLoaded("MenuPrincipal")) return;
        LimparEscolhas(); dialogueUI.SetActive(false);
        RestaurarFundoFim(); RestaurarFade(); RestaurarCamera(); RestaurarGrupoAbrigo();
        RestaurarInventarioAbrigo(); LiberarMovimento();
        if (UniversalPauseManager.Instance != null) UniversalPauseManager.Instance.ResumeGame();
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.LoadScene("MenuPrincipal");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("MenuPrincipal");
    }

    private void SalvarFortaleza()
    {
        if (GameProgress.Instance == null || GameSaveSystem.IsRestoring) return;
        GameProgressData d = GameProgress.Instance.Data;
        d.estradaFortalezaIniciada = fortalezaIniciada;
        d.estradaDialogoFortalezaConcluido = dialogoFortalezaConcluido;
        d.estradaAbordagemIniciada = abordagemIniciada;
        d.ato1Concluido = atoConcluido;
        SalvarAguaTerceira();
    }

    private void SalvarAguaTerceira()
    {
        if (GameProgress.Instance == null || !GameProgress.Instance.Data.CanSave || GameSaveSystem.IsRestoring)
            return;
        GameSaveSystem salvar = GameProgress.Instance.GetComponent<GameSaveSystem>();
        if (salvar != null) salvar.TrySaveCheckpoint("Estrada");
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        RestaurarFundoFim();
        if (familiaFinal != null) familiaFinal.SetActive(false);
        finalEmAndamento = false; encerramentoMostrado = false;
        if (irmasCenario != null) irmasCenario.SetActive(false);
        if (paisCenario != null) paisCenario.SetActive(false);
        RestaurarCoresAcampamento();
        noiteEmAndamento = false; retomouAcampamento = false;
        escolhaNoturnaAberta = false;
        if (familiaDescanso != null) familiaDescanso.SetActive(false);
        sequenciaJuazeiroIniciada = false;
        restaurandoFamilia = false;
        recolhendoAgua = false;
        if (barriguda != null) barriguda.position = posicaoBarriguda;
        RestaurarInventarioAbrigo();
        RestaurarFade();
        RestaurarExterior();
        RestaurarCamera();
        RestaurarGrupoAbrigo();
        sequenciaAbrigo = false;
        deslocandoFamilia = false;
        fugindo = false;
        preparandoBusca = false;
        perguntasAbertas = false;
        LiberarMovimento();
        RetomarFamilia();
        MostrarBaleia(spriteBaleia);
        if (conversando && dialogueUI != null) dialogueUI.SetActive(false);
        LimparEscolhas();
        conversando = false;
        esperandoEscolha = false;
        depois = null;
    }
}

public class MarcadorSegundaParada : MonoBehaviour
{
    public string id { get; private set; }
    public void configurar(SegundaParada c, string identificador) { id = identificador; }
}





