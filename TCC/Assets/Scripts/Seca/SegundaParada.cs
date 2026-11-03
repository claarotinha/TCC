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
        AplicarFerimento();
        PrepararAguaTerceira();
        if (GameProgress.Instance != null && GameProgress.Instance.Data.estradaAbrigoConcluido)
        {
            iniciou = true;
            etapa = 4;
            StartCoroutine(RestaurarAbrigoCarregado());
            return;
        }
        if (baleia != null)
        {
            posicaoBaleia = baleia.position;
            seguidorBaleia = baleia.GetComponent<SeguidorFamilia>();
            desenhoBaleia = baleia.GetComponent<SpriteRenderer>();
            escalaBaleia = baleia.localScale;
            if (desenhoBaleia != null) spriteBaleia = desenhoBaleia.sprite;
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
        Sprite sprite = JanelaQuebrada && mariquinhaFerida != null ? mariquinhaFerida : spriteMariquinhaNormal;
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
