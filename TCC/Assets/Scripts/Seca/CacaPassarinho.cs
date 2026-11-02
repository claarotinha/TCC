using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Primeira parada: usar o estilingue no passarinho, com pedra no inventario.
public class CacaPassarinho : MonoBehaviour
{
    [SerializeField] private ItemData estilingue;
    [SerializeField] private ItemData pedra;
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
        "Consegui acertar o passarinho com o estilingue!",
        "Muito bem, minha filha. Agora temos o que comer.",
        "Vou guardar o estilingue. A gente pode precisar dele de novo.",
        "Vamo seguir viagem. Ainda temos muito chao pela frente."
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
            item != estilingue || mari == null || InventoryManager.Instance == null)
            return false;

        if (!InventoryManager.Instance.Items.Contains(estilingue))
            return false;

        if (!InventoryManager.Instance.Items.Contains(pedra))
        {
            Debug.Log("Para cazar, Mari precisa de uma pedra no inventario.", this);
            return true;
        }

        if (dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null)
        {
            Debug.LogError("CacaPassarinho: referencias de dialogo ausentes.", this);
            return true;
        }

        InventoryManager.Instance.RemoveItem(pedra);
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
