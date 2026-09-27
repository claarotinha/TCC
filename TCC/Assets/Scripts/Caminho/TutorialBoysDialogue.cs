using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(500)]
public class TutorialBoysDialogue : MonoBehaviour
{
    private static TutorialBoysDialogue current;
    public static bool IsShowing => current != null && current.dialogueUI != null &&
                                    current.dialogueUI.activeInHierarchy;

    private readonly string[] lines =
    {
        "Mari: O menino ali está chorando. Vocês sabem o que aconteceu com a bola dele?",
        "Garotos: A bola escapou quando estávamos brincando perto da padaria.",
        "Garotos: Talvez tenha rolado para a calçada. Você pode procurar e trazê-la de volta?"
    };

    private Collider2D clickCollider;
    [SerializeField] private GameObject dialogueCanvas;
    private GameObject dialogueUI;
    private GameObject choicesContainer;
    private TMP_Text dialogueText;
    private TMP_Text nameText;
    private Image portrait;
    private SpriteRenderer spriteRenderer;
    private int lineIndex;
    private int openedFrame;

    private void Start()
    {
        clickCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject canvas = dialogueCanvas;
        if (canvas == null)
            canvas = GameObject.Find("CanvasINTERAÇÕES") ?? GameObject.Find("CanvasINTERACOES");
        Transform panel = canvas != null ? canvas.transform.Find("DialogueUI/DialoguePanel") : null;
        if (panel == null)
        {
            Debug.LogError("Garotos: CanvasINTERAÇÕES/DialogueUI/DialoguePanel não encontrado.", this);
            return;
        }

        dialogueUI = panel.parent.gameObject;
        choicesContainer = panel.Find("ChoicesContainer")?.gameObject;
        dialogueText = panel.Find("DialogueText")?.GetComponent<TMP_Text>();
        nameText = panel.Find("CharacterName")?.GetComponent<TMP_Text>();
        portrait = panel.Find("Portrait")?.GetComponent<Image>();

        if (dialogueText == null || nameText == null)
        {
            Debug.LogError("Garotos: faltam campos de texto no DialoguePanel.", this);
            return;
        }

        if (choicesContainer != null)
            choicesContainer.SetActive(false);
        dialogueUI.SetActive(false);
    }

    private void Update()
    {
        if (current == this)
        {
            if (Input.GetMouseButtonDown(0) && Time.frameCount > openedFrame)
                NextLine();
        }
    }

    private void LateUpdate()
    {
        if (current == this)
            return;

        if (!Input.GetMouseButtonDown(0) || Camera.main == null ||
            clickCollider == null)
            return;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!clickCollider.OverlapPoint(mouse))
            return;

        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null || tutorial.CurrentStep != TutorialManager.TutorialStep.Boys)
        {
#if UNITY_EDITOR
            Debug.Log("Garotos: clique recebido; etapa atual = " +
                      (tutorial != null ? tutorial.CurrentStep.ToString() : "sem TutorialManager") + ".", this);
#endif
            return;
        }

        if (dialogueUI == null || dialogueText == null || nameText == null)
        {
            Debug.LogError("Garotos: painel de diálogo não configurado.", this);
            return;
        }

        if (PauseHelper.BlockInput())
            return;

        // O clique que fechou o painel anterior ainda tem a trava do quadro.
        // Verificamos o painel que continua aberto depois de todos os Updates.
        string blockedBy = InvestigationGuard.OpenPanelReason;
        if (blockedBy != null)
        {
            if (blockedBy == "investigação")
                ExamineObject.HideCurrentPanel();
            else if (blockedBy == "coleta")
                CollectableExamine.HideCurrentPanel();
#if UNITY_EDITOR
            Debug.Log("Garotos: clique recebido; bloqueado por " + blockedBy +
                      ". Feche o painel e clique novamente.", this);
#endif
            return;
        }

        current = this;
        lineIndex = 0;
        openedFrame = Time.frameCount;
        if (choicesContainer != null)
            choicesContainer.SetActive(false);
        dialogueUI.SetActive(true);
        ShowLine();
#if UNITY_EDITOR
        Debug.Log("Garotos: diálogo iniciado.", this);
#endif
    }

    private void ShowLine()
    {
        nameText.text = lineIndex == 0 ? "Mari" : "Garotos";
        dialogueText.text = lines[lineIndex];
        if (portrait != null && spriteRenderer != null)
            portrait.sprite = spriteRenderer.sprite;
    }

    private void NextLine()
    {
        lineIndex++;
        if (lineIndex < lines.Length)
        {
            ShowLine();
            return;
        }

        dialogueUI.SetActive(false);
        current = null;
        InvestigationGuard.BlockCurrentClick();
        TutorialManager.Instance?.ReportBoysClue();
#if UNITY_EDITOR
        Debug.Log("Garotos: pista da bola recebida; coleta liberada.", this);
#endif
    }

    private void OnDisable()
    {
        if (current != this)
            return;

        if (dialogueUI != null)
            dialogueUI.SetActive(false);
        current = null;
    }
}
