using System;
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

    private int etapa; // 0 agua, 1 travessia, 2 busca, 3 abrigo, 4 concluida
    private bool iniciou, conversando, movimentoAnterior, esperandoEscolha, dialogoCaminhada;
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
        if (baleia != null) posicaoBaleia = baleia.position;
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
        if (Time.timeScale == 0f) return;
        ControleJornada j = ControleJornada.Instance;
        if (!iniciou && j != null && j.ParadaAtiva && j.ParadaAtual == 2 && !dialogueUI.activeInHierarchy)
        {
            iniciou = true;
            Falar(new[] { "Mãe" },
                new[] { "Estou com muita sede, Zefinha, use a sua cabaça para coletar um pouco de água." },
                null);
        }
        if (!conversando || esperandoEscolha || Time.frameCount <= quadro ||
            !Input.GetMouseButtonDown(0)) return;
        indice++;
        if (indice < falas.Length) { Mostrar(); return; }
        Fechar();
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
        dialogoCaminhada = duranteCaminhada;
        movimentoAnterior = mari.enabled;
        if (!duranteCaminhada) mari.enabled = false;
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
        if (!dialogoCaminhada) mari.enabled = movimentoAnterior;
        dialogoCaminhada = false;
        Action acao = depois; depois = null;
        if (acao != null) acao();
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
        if (!iniciou || etapa != 0 || conversando || item != cabacaVazia ||
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
        Falar(new[] { "Mãe", "Pai" },
            new[] {
                "Ave minha Nossa Senhora! Essa cachorra ficou doida, correndo desenfreada assim!",
                "Severina, Jozefa, vão atrás de Baleia, ela deve ter visto alguma coisa ali na frente, eu e sua mãe vamo aproveitar pra ver se tem algum lugar por perto."
            }, IniciarBusca);
    }

    private void IniciarBusca()
    {
        etapa = 2;
        if (baleia != null) baleia.position = new Vector3(mari.transform.position.x + 3.8f, posicaoBaleia.y, posicaoBaleia.z);
        for (int i = 0; i < 3; i++)
            CriarMarcador("Pegada " + (i + 1),
                new Vector2(transform.position.x + 0.5f + i * 1.1f, -1.4f),
                new Color(0.39f, 0.28f, 0.18f), 0.35f, 0.22f, "pegada" + i);
        CriarMarcador("Baleia encontrada",
            new Vector2(transform.position.x + 3.8f, -1.3f),
            new Color(0.55f, 0.4f, 0.28f), 0.8f, 0.6f, "baleia");
    }

    public void Clicar(string alvo)
    {
        if (!iniciou || conversando || Time.timeScale == 0f) return;
        if (mari == null || Vector2.Distance(mari.transform.position, Posicao(alvo)) > 5f)
            return;
        if (alvo.StartsWith("pegada") && etapa == 2)
        {
            int n;
            if (!int.TryParse(alvo.Substring(6), out n) || n != pegadas) return;
            pegadas++;
            foreach (GameObject obj in marcadores)
                if (obj.name == "Pegada " + pegadas) obj.SetActive(false);
            return;
        }
        if (alvo == "baleia" && etapa == 2 && pegadas == 3)
        {
            etapa = 3;
            if (InventoryManager.Instance != null) InventoryManager.Instance.AddItem(prea);
            if (baleia != null) baleia.position = new Vector3(baleia.position.x, posicaoBaleia.y, posicaoBaleia.z);
            foreach (GameObject obj in marcadores)
                if (obj.name.StartsWith("Pegada") || obj.name == "Baleia encontrada")
                    obj.SetActive(false);
            Falar(new[] { "Zefinha", "Pai", "Mãe", "Mariquinha", "Pai" },
                new[] {
                    "achamos ela painho.",
                    "Já tá escurecendo, tem uma casinha ali mais a frente, vamo ve se da pra entrar e esticar as pernas, amanhã a gente continua.",
                    "A casa ta trancafiada, mas pela janelinha da pra ve que num tem ninguém. A gente pode arrombar a porta com um empurrão.",
                    "Acho que dá para encontrar uma pedra e tacar na janela.",
                    "O que tu acha Zefinha? A gente empurra a porta ou quebramos a janela?"
                }, () => { if (casa != null) casa.gameObject.SetActive(true); });
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
        if (choicesContainer == null) { Debug.LogError("ChoicesContainer ausente"); return; }
        conversando = true; esperandoEscolha = true;
        movimentoAnterior = mari.enabled; mari.enabled = false;
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel);
        characterNameText.text = "Escolha";
        dialogueText.text = "Como entrar na casa?";
        if (portraitImage != null) portraitImage.enabled = false;
        choicesContainer.SetActive(true);
        foreach (Transform child in choicesContainer.transform) child.gameObject.SetActive(false);
        CriarBotao("Arrombar a porta", 0);
        CriarBotao("Quebrar a janela", 1);
    }

    private void CriarBotao(string texto, int escolha)
    {
        GameObject go = new GameObject(texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(choicesContainer.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, escolha == 0 ? 30f : -30f);
        rt.sizeDelta = new Vector2(440f, 55f);
        Image img = go.GetComponent<Image>(); img.color = new Color(0.22f, 0.17f, 0.12f, 0.95f);
        Button botao = go.GetComponent<Button>(); botoes.Add(botao);
        GameObject label = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        label.transform.SetParent(go.transform, false);
        RectTransform lr = label.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = label.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = 22; tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; tmp.raycastTarget = false;
        botao.onClick.AddListener(() => EscolhaFeita(escolha));
    }

    private void EscolhaFeita(int escolha)
    {
        foreach (Button b in botoes) if (b != null) Destroy(b.gameObject);
        botoes.Clear();
        choicesContainer.SetActive(false);
        esperandoEscolha = false; conversando = false;
        dialogueUI.SetActive(false); mari.enabled = movimentoAnterior;
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
        sr.color = cor; sr.sortingOrder = 3; sr.enabled = false;
        go.transform.localScale = new Vector3(largura, altura, 1);
        BoxCollider2D col = go.GetComponent<BoxCollider2D>(); col.isTrigger = true;
        MarcadorSegundaParada marcador = go.GetComponent<MarcadorSegundaParada>();
        marcador.configurar(this, id);
        marcadores.Add(go);
    }

    private void OnDisable()
    {
        if (conversando && mari != null && !dialogoCaminhada) mari.enabled = movimentoAnterior;
        if (conversando && dialogueUI != null) dialogueUI.SetActive(false);
    }
}

public class MarcadorSegundaParada : MonoBehaviour
{
    private SegundaParada controle;
    public string id { get; private set; }
    public void configurar(SegundaParada c, string identificador) { controle = c; id = identificador; }
    private void OnMouseDown()
    {
        if (controle == null) return;
        if (id == "riacho")
        {
            InventoryManager inventario = InventoryManager.Instance;
            if (inventario != null && inventario.SelectedItem != null &&
                controle.UsarNoRiacho(inventario.SelectedItem))
                return;
        }
        controle.Clicar(id);
    }
}
