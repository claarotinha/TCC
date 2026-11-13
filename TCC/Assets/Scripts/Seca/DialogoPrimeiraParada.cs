
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogoPrimeiraParada : MonoBehaviour
{
    [Header("Personagem")]
    [SerializeField] private PlayerMovement mari;

    [Header("Canvas existente")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite retratoPainho;

    [Header("Dica editável na Hierarchy")]
    [SerializeField] private TMP_Text dicaCombinacao;
    [SerializeField] private CacaPassarinho cacaPassarinho;

    [SerializeField] private ItemData[] materiaisCombinacao;
    [SerializeField] private ItemData estilingueParcial, estilinguePronto;
    private bool inventarioDicaAnterior;
    private float proximaChecagemDica;

    private readonly string[] nomes =
    {
        "Pai", "Pai"
    };

    private readonly string[] falas =
    {
        "Se a gente não parar agora para arrumar comida, não vamos longe, é melhor aproveitar que o sol já esfriou e ainda não anoiteceu.",
        "Zefinha, procure um galho mais flexível, uma pedra lisa e um elástico para a gente montar um estilingue e pegar aqueles pássaros."
    };

    public bool Finished => terminou;

    private int linhaAtual;
    private int quadroDaFala;
    private bool conversando;
    private bool terminou;
    private bool movimentoAnterior;
    private Rigidbody2D corpoMari;

    private void Start()
    {
        if (mari != null)
            corpoMari = mari.GetComponent<Rigidbody2D>();

    }

    private void Update()
    {
        AtualizarDicaCombinacao();
        ControleJornada jornada = ControleJornada.Instance;
        if (!terminou && !conversando && jornada != null && jornada.ParadaAtiva && jornada.ParadaAtual == 1 &&
            dialogueUI != null && !dialogueUI.activeInHierarchy) Iniciar();
        if (!conversando || Time.timeScale == 0f)
            return;

        if (Time.frameCount > quadroDaFala &&
            Input.GetMouseButtonDown(0))
        {
            linhaAtual++;

            if (linhaAtual >= falas.Length)
                Encerrar();
            else
                MostrarFala();
        }
    }

    private void AtualizarDicaCombinacao()
    {
        bool aberto = InventoryTabController.Instance != null && InventoryTabController.Instance.IsOpen;
        InventoryManager inventario = InventoryManager.Instance;
        bool materiais = inventario != null && materiaisCombinacao != null && materiaisCombinacao.Length == 3 &&
            (materiaisCombinacao.All(item => item != null && inventario.Items.Contains(item)) ||
             (estilingueParcial != null && inventario.Items.Contains(estilingueParcial) && inventario.Items.Contains(materiaisCombinacao[1])));
        bool mostrar = terminou && aberto && materiais && cacaPassarinho != null && cacaPassarinho.CanInteract &&
            (estilinguePronto == null || !inventario.Items.Contains(estilinguePronto));
        if (dicaCombinacao != null)
        {
            dicaCombinacao.gameObject.SetActive(mostrar);
            dicaCombinacao.enabled = true;
        }
        // Keep just the authored Hierarchy text if an older persistent canvas carries the same hint.
        if (aberto != inventarioDicaAnterior || Time.unscaledTime >= proximaChecagemDica)
        {
            foreach (TMP_Text texto in FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (texto != dicaCombinacao && texto.text != null &&
                    texto.text.Contains("Arraste um item sobre o outro") && texto.text.Contains("Confirme"))
                    texto.enabled = false;
            proximaChecagemDica = Time.unscaledTime + .5f;
        }
        inventarioDicaAnterior = aberto;
    }

    private void FixedUpdate()
    {
        if (conversando && corpoMari != null)
        {
            Vector2 velocidade = corpoMari.linearVelocity;
            velocidade.x = 0f;
            corpoMari.linearVelocity = velocidade;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (terminou || conversando)
            return;

        if (other.GetComponent<PlayerMovement>() != mari)
            return;

        ControleJornada jornada = ControleJornada.Instance;

        if (jornada == null)
            return;

        // O GatilhoParada tambem inicia a parada.
        // A conversa espera esse estado estar ativo.
        StartCoroutine(IniciarQuandoDisponivel());
    }

    private System.Collections.IEnumerator IniciarQuandoDisponivel()
    {
        yield return null;

        ControleJornada jornada = ControleJornada.Instance;

        if (jornada != null &&
            jornada.ParadaAtiva &&
            jornada.ParadaAtual == 1 &&
            !terminou && !conversando)
        {
            Iniciar();
        }
    }

    private void Iniciar()
    {
        // O gatilho e o Update podem tentar iniciar a mesma conversa no mesmo quadro.
        // Preserve o estado capturado antes do primeiro bloqueio de movimento.
        if (conversando || terminou)
            return;

        if (mari == null || dialogueUI == null ||
            dialoguePanel == null ||
            characterNameText == null || dialogueText == null)
        {
            Debug.LogError(
                "Configure as referencias do dialogo da primeira parada.",
                this
            );
            return;
        }

        conversando = true;
        linhaAtual = 0;

        movimentoAnterior = mari.enabled;
        mari.enabled = false;

        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);

        AtivarCanvasGroup(dialogueUI);
        AtivarCanvasGroup(dialoguePanel);

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        MostrarFala();
    }

    private void MostrarFala()
    {
        characterNameText.text = nomes[linhaAtual];
        dialogueText.text = falas[linhaAtual];

        if (portraitImage != null)
        {
            bool falaPainho = nomes[linhaAtual] == "Pai";

            portraitImage.sprite =
                falaPainho ? retratoPainho : null;

            portraitImage.enabled =
                falaPainho && retratoPainho != null;
        }

        quadroDaFala = Time.frameCount;
    }

    private void Encerrar()
    {
        conversando = false;
        terminou = true;

        dialogueUI.SetActive(false);
        mari.enabled = movimentoAnterior;

        // Nao conclui a parada aqui.
        // Ela sera concluida depois do puzzle da comida.
    }

    private static void AtivarCanvasGroup(GameObject objeto)
    {
        CanvasGroup grupo = objeto.GetComponent<CanvasGroup>();

        if (grupo == null)
            return;

        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        if (!conversando)
            return;

        conversando = false;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (mari != null)
            mari.enabled = movimentoAnterior;
    }
}
