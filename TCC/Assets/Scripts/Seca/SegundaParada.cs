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

    private int etapa; // 0 agua, 1 travessia, 2 busca, 3 abrigo, 4 concluida, 5 retorno
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
        JanelaQuebrada = PlayerPrefs.GetInt("EstradaMariquinhaFerida", 0) == 1;
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
        if (casa != null) casa.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PauseHelper.BlockInput() || Time.timeScale == 0f) return;
        ControleJornada j = ControleJornada.Instance;
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
        if (!conversando && !preparandoBusca && Time.frameCount > quadro &&
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
            if (Mathf.Abs(diferenca.x) > metade.x ||
                Mathf.Abs(diferenca.y) > metade.y) continue;
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
        if (movimentoBloqueado) PararMari();
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
        etapa = 3;
        RetomarFamilia();
        Falar(new[] { "Zefinha", "Pai" },
            new[] {
                "achamos ela painho.",
                "Já tá escurecendo, tem uma casinha ali mais a frente, vamo ve se da pra entrar e esticar as pernas, amanhã a gente continua."
            }, ConversarNaCasa);
    }

    private void ConversarNaCasa()
    {
        Falar(new[] { "Mãe", "Mariquinha", "Pai" },
            new[] {
                "A casa ta trancafiada, mas pela janelinha da pra ve que num tem ninguém. A gente pode arrombar a porta com um empurrão.",
                "Acho que dá para encontrar uma pedra e tacar na janela.",
                "O que tu acha Zefinha? A gente empurra a porta ou quebramos a janela?"
            }, () => { if (casa != null) casa.gameObject.SetActive(true); });
    }

    public void Clicar(string alvo)
    {
        if (!iniciou || conversando || Time.frameCount <= quadro ||
            PauseHelper.BlockInput() || Time.timeScale == 0f) return;
        if (mari == null || Vector2.Distance(mari.transform.position, Posicao(alvo)) > 5f)
            return;
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
        if (alvo == "baleia" && etapa == 2 && pegadas == 3 &&
            Vector2.Distance(mari.transform.position, Posicao(alvo)) <= 1.5f)
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
        else if (alvo == "casa" && etapa == 3) EscolherEntrada();
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
        dialogueUI.SetActive(false); LiberarMovimento();
        // A escolha fica registrada no controlador da parada para a terceira parada.
        JanelaQuebrada = escolha == 1;
        PlayerPrefs.SetInt("EstradaMariquinhaFerida", JanelaQuebrada ? 1 : 0);
        PlayerPrefs.Save();
        Falar(new[] { "Cena", "Cena" },
            escolha == 0
                ? new[] { "A família consegue abrir a porta.", "Ninguém se machuca." }
                : new[] { "A família quebra a janela para conseguir entrar.", "Durante a ação, Mariquinha machuca a mão em um pedaço de vidro." },
            Descansar);
    }

    public static bool JanelaQuebrada { get; private set; }

    private void Descansar()
    {
        Falar(new[] { "Cena", "Cena" },
            new[] {
                "A família entra na casa e encontra um lugar para dormir.",
                "A tela escurece, encerrando o dia."
            }, () => {
                etapa = 4;
                ControleJornada.Instance?.ConcluirParada(2);
                Debug.Log("Segunda parada concluída.", this);
            });
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

    private void OnDisable()
    {
        StopAllCoroutines();
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
