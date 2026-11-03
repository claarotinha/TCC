using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Primeira parada: usar o estilingue no passarinho, com pedra no inventario.
public class CacaPassarinho : MonoBehaviour
{
    [SerializeField] private ItemData estilingue;
    [SerializeField] private ItemData pedra;
    [SerializeField] private ItemData estilingueSemPedra;
    [SerializeField] private ItemData passaro;
    [SerializeField] private PlayerMovement mari;
    [SerializeField] private DialogoPrimeiraParada dialogoInicial;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite retratoPainho;

    private readonly string[] nomes = { "Mari", "Painho", "Mari", "Painho" };
    private readonly string[] falas =
    {
        "Pronto painho, consegui montar o estilingue!",
        "Obrigado, minha filha.",
        "A pedra acerta o pássaro, que cai no chão.",
        "Num gosto de vê passarinho morto… Mas gosto menos de vê minhas filha com fome."
    };

    private bool acertou;
    private bool conversando;
    private bool movimentoAnterior;
    private int indice;
    private int quadro;

    public bool TentarCacar(ItemData item)
    {
        ControleJornada jornada = ControleJornada.Instance;
        if (jornada == null || !jornada.ParadaAtiva || jornada.ParadaAtual != 1 ||
            (dialogoInicial != null && !dialogoInicial.Finished) || acertou ||
            mari == null || InventoryManager.Instance == null)
            return false;

        bool pronto = item == estilingue && InventoryManager.Instance.Items.Contains(estilingue);
        bool improvisado = item == estilingueSemPedra && InventoryManager.Instance.Items.Contains(estilingueSemPedra) && pedra != null && InventoryManager.Instance.Items.Contains(pedra);
        if (!pronto && !improvisado) return false;


        if (dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null)
        {
            Debug.LogError("CacaPassarinho: referencias de dialogo ausentes.", this);
            return true;
        }

        if (improvisado) InventoryManager.Instance.RemoveItem(pedra);
        if (passaro != null) InventoryManager.Instance.AddItem(passaro);
        acertou = true;
        Collider2D colliderPassaro = GetComponent<Collider2D>();
        if (colliderPassaro != null) colliderPassaro.enabled = false;
        SpriteRenderer desenho = GetComponent<SpriteRenderer>();
        if (desenho != null) desenho.enabled = false;
        IniciarDialogo();
        return true;
    }

    private void IniciarDialogo()
    {
        conversando = true;
        indice = 0;
        movimentoAnterior = mari.enabled;
        mari.enabled = false;
        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        AtivarGrupo(dialogueUI);
        AtivarGrupo(dialoguePanel);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        Mostrar();
    }

    private static void AtivarGrupo(GameObject alvo)
    {
        CanvasGroup grupo = alvo.GetComponent<CanvasGroup>();
        if (grupo == null) return;
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void Mostrar()
    {
        characterNameText.text = nomes[indice];
        dialogueText.text = falas[indice];
        if (portraitImage != null)
        {
            bool painho = nomes[indice] == "Painho";
            portraitImage.sprite = painho ? retratoPainho : null;
            portraitImage.enabled = painho && retratoPainho != null;
        }
        quadro = Time.frameCount;
    }

    private void Update()
    {
        if (!conversando || Time.timeScale == 0f) return;
        if (Time.frameCount <= quadro || !Input.GetMouseButtonDown(0)) return;
        indice++;
        if (indice < falas.Length) { Mostrar(); return; }
        conversando = false;
        dialogueUI.SetActive(false);
        mari.enabled = movimentoAnterior;
        ControleJornada.Instance?.ConcluirParada(1);
        HistoriaPaiCaminhada historia = gameObject.AddComponent<HistoriaPaiCaminhada>();
        historia.Iniciar(dialogueUI, dialoguePanel, choicesContainer,
            characterNameText, dialogueText, portraitImage, retratoPainho);
        Debug.Log("Primeira parada concluida. Caminho liberado.", this);
    }

    private void OnDisable()
    {
        if (!conversando) return;
        conversando = false;
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (mari != null) mari.enabled = movimentoAnterior;
    }
}
