using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(500)]
public class TutorialBoysDialogue : MonoBehaviour
{
    private static TutorialBoysDialogue current;
    public static bool IsShowing => current != null &&
                                    current.dialoguePanel != null &&
                                    current.dialoguePanel.activeInHierarchy;

    private readonly string[] lines =
    {
        "Mari: O menino ali está chorando. Vocês sabem o que aconteceu com a bola dele?",
        "Garotos: A bola escapou quando estávamos brincando perto da padaria.",
        "Garotos: Talvez tenha rolado para a calçada. Você pode procurar e trazê-la de volta?"
    };

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    private Collider2D clickCollider;
    private int lineIndex;
    private int openedFrame;

    private void Awake()
    {
        clickCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("Garotos: painel ou texto não configurado no Inspector.", this);
            return;
        }

        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (current == this && Input.GetMouseButtonDown(0) &&
            Time.frameCount > openedFrame)
            NextLine();
    }

    private void LateUpdate()
    {
        if (current == this || !Input.GetMouseButtonDown(0) ||
            Camera.main == null || clickCollider == null)
            return;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!clickCollider.OverlapPoint(mouse))
            return;

        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null || tutorial.CurrentStep != TutorialManager.TutorialStep.Boys)
        {
#if UNITY_EDITOR
            Debug.Log("Garotos: etapa atual = " +
                      (tutorial != null ? tutorial.CurrentStep.ToString() : "sem TutorialManager") + ".", this);
#endif
            return;
        }

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("Garotos: painel ou texto não configurado.", this);
            return;
        }

        if (PauseHelper.BlockInput())
            return;

        string blockedBy = InvestigationGuard.OpenPanelReason;
        if (blockedBy != null)
        {
            if (blockedBy == "investigação")
                ExamineObject.HideCurrentPanel();
            else if (blockedBy == "coleta")
                CollectableExamine.HideCurrentPanel();
#if UNITY_EDITOR
            Debug.Log("Garotos: feche " + blockedBy + " e clique novamente.", this);
#endif
            return;
        }

        current = this;
        lineIndex = 0;
        openedFrame = Time.frameCount;
        ShowLine();
        dialoguePanel.SetActive(true);
#if UNITY_EDITOR
        Debug.Log("Garotos: painel de investigação aberto com o diálogo.", this);
#endif
    }

    private void ShowLine()
    {
        dialogueText.text = lines[lineIndex];
    }

    private void NextLine()
    {
        lineIndex++;
        if (lineIndex < lines.Length)
        {
            ShowLine();
            return;
        }

        Close();
        TutorialManager.Instance?.ReportBoysClue();
#if UNITY_EDITOR
        Debug.Log("Garotos: pista da bola recebida; coleta liberada.", this);
#endif
    }

    private void Close()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        current = null;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (current == this)
            Close();
    }
}
