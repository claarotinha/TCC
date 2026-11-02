using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogoMariquinha : MonoBehaviour
{
    [Header("Personagens")]
    [SerializeField] private Transform mari;
    [SerializeField] private PlayerMovement movimentoMari;
    [SerializeField] private DialogoInicialEstrada abertura;
    [SerializeField] private DialogoPrimeiraParada primeiraParada;
    [SerializeField] private float distanciaInteracao = 2.3f;

    [Header("Dialogo existente")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite retratoMariquinha;
    [SerializeField] private Sprite retratoMari;

    [Header("Item")]
    [SerializeField] private ItemData elastico;

    private static readonly string[] nomes = { "Zefinha", "Mariquinha" };
    private static readonly string[] falas =
    {
        "Encontrei uma boa pedra e bom galho painho, mas não sei onde encontrar um elástico.",
        "Eu tenho um zefinha, pegue, pode usar."
    };

    private bool conversando;
    private bool entregou;
    private bool movimentoAnterior;
    private int linha;
    private int quadro;
    private Rigidbody2D corpo;

    private void Awake()
    {
        if (movimentoMari != null)
            corpo = movimentoMari.GetComponent<Rigidbody2D>();
        entregou = GameProgress.Instance != null && GameProgress.Instance.Data.estradaElasticoEntregue;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || mari == null || movimentoMari == null)
            return;

        if (conversando)
        {
            if (Time.frameCount > quadro && Input.GetMouseButtonDown(0))
            {
                linha++;
                if (linha >= falas.Length || entregou)
                    Encerrar();
                else
                    Mostrar();
            }
            return;
        }


    }

    private void OnMouseDown()
    {
        if (conversando || Time.timeScale == 0f || mari == null || movimentoMari == null)
            return;
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        if (PodeConversar())
            Iniciar();
    }

    private bool PodeConversar()
    {
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada == null || !jornada.ParadaAtiva || jornada.ParadaAtual != 1)
            return false;
        if (abertura != null && !abertura.Finished)
            return false;
        if (primeiraParada != null && !primeiraParada.Finished)
            return false;
        return Vector2.Distance(mari.position, transform.position) <= distanciaInteracao;
    }

    private void FixedUpdate()
    {
        if (conversando && corpo != null)
        {
            Vector2 velocidade = corpo.linearVelocity;
            velocidade.x = 0f;
            corpo.linearVelocity = velocidade;
        }
    }

    private void Iniciar()
    {
        if (dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null || elastico == null)
        {
            Debug.LogError("DialogoMariquinha: faltam referencias no Inspector.", this);
            return;
        }
        conversando = true;
        linha = 0;
        movimentoAnterior = movimentoMari.enabled;
        movimentoMari.enabled = false;
        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        MostrarGrupo(dialogueUI);
        MostrarGrupo(dialoguePanel);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        Mostrar();
    }

    private void Mostrar()
    {
        if (entregou)
        {
            characterNameText.text = "Mariquinha";
            dialogueText.text = "Ja te dei meu elastico. Tomara que de certo.";
        }
        else
        {
            characterNameText.text = nomes[linha];
            dialogueText.text = falas[linha];
        }
        if (portraitImage != null)
        {
            Sprite sprite = (entregou || nomes[linha] == "Mariquinha") ? retratoMariquinha : retratoMari;
            portraitImage.sprite = sprite;
            portraitImage.enabled = sprite != null;
        }
        quadro = Time.frameCount;
    }

    private void Encerrar()
    {
        if (!entregou)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(elastico);
                entregou = true;
                if (GameProgress.Instance != null)
                    GameProgress.Instance.Data.estradaElasticoEntregue = true;
            }
            else
                Debug.LogError("DialogoMariquinha: InventoryManager ausente.", this);
        }
        conversando = false;
        dialogueUI.SetActive(false);
        movimentoMari.enabled = movimentoAnterior;
    }

    private static void MostrarGrupo(GameObject alvo)
    {
        CanvasGroup grupo = alvo.GetComponent<CanvasGroup>();
        if (grupo == null) return;
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        if (!conversando) return;
        conversando = false;
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (movimentoMari != null) movimentoMari.enabled = movimentoAnterior;
    }
}
