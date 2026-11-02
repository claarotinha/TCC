
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

    private readonly string[] nomes =
    {
        "Pai"
    };

    private readonly string[] falas =
    {
        "Se a gente não parar agora para arrumar comida, não vamos longe, é melhor aproveitar que o sol já esfriou e ainda não anoiteceu. Zefinha, procure um galho mais flexível, uma pedra lisa e um elástico para a gente montar um estilingue e pegar aqueles pássaros."
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
            !terminou)
        {
            Iniciar();
        }
    }

    private void Iniciar()
    {
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
            bool falaPainho = nomes[linhaAtual] == "Painho";

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
