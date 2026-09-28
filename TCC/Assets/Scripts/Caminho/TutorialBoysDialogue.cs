using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[DefaultExecutionOrder(500)]
public class TutorialBoysDialogue : MonoBehaviour
{
    private static TutorialBoysDialogue current;

    public static bool IsShowing =>
        current != null &&
        current.dialogueUI != null &&
        current.dialogueUI.activeInHierarchy;

    [Header("DialogueUI da cena")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject choicesContainer;

    [Header("Retrato opcional")]
    [SerializeField] private Sprite mariPortrait;

    private readonly string[] lines =
    {
        "O menino ali está chorando. Vocês sabem o que aconteceu com a bola dele?",
        "A bola escapou quando estávamos brincando perto da padaria.",
        "Talvez tenha rolado para a calçada. Você pode procurar e trazê-la de volta?"
    };

    private Collider2D clickCollider;
    private SpriteRenderer boysSprite;
    private int lineIndex;
    private int openedFrame;
    private bool thanking;

    public bool CanReceiveBall =>
        TutorialManager.Instance != null &&
        TutorialManager.Instance.CurrentStep == TutorialManager.TutorialStep.ReturnBall &&
        current == null;

    private void Awake()
    {
        clickCollider = GetComponent<Collider2D>();
        boysSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (dialogueUI == null || dialogueText == null ||
            characterNameText == null)
        {
            Debug.LogError(
                "Garotos: configure DialogueUI, DialogueText e CharacterName no Inspector.",
                this
            );
            return;
        }

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        dialogueUI.SetActive(false);
    }

    private void Update()
    {
        if (current == this &&
            Input.GetMouseButtonDown(0) &&
            Time.frameCount > openedFrame)
        {
            NextLine();
        }
    }

    private void LateUpdate()
    {
        if (current == this ||
            !Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            clickCollider == null)
            return;

        Vector2 mouse =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!clickCollider.OverlapPoint(mouse))
            return;

        TutorialManager tutorial = TutorialManager.Instance;

        if (tutorial == null ||
            tutorial.CurrentStep != TutorialManager.TutorialStep.Boys)
            return;

        if (dialogueUI == null || dialogueText == null ||
            characterNameText == null)
        {
            Debug.LogError("Garotos: referências da DialogueUI ausentes.", this);
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

            // Um clique fecha a mensagem anterior; o próximo abre a conversa.
            return;
        }

        current = this;
        thanking = false;
        lineIndex = 0;
        openedFrame = Time.frameCount;

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        // Mantém o diálogo acima dos outros painéis desse Canvas.
        dialogueUI.transform.SetAsLastSibling();

        ShowLine();
        dialogueUI.SetActive(true);
    }

    private void ShowLine()
    {
        bool mariSpeaking = !thanking && lineIndex == 0;

        characterNameText.text = mariSpeaking ? "Mari" : "Garotos";
        dialogueText.text = thanking
            ? "Encontrou a nossa bola! Muito obrigado, Mari. Agora podemos voltar a brincar."
            : lines[lineIndex];

        if (portraitImage != null)
        {
            Sprite portrait = mariSpeaking
                ? mariPortrait
                : boysSprite != null ? boysSprite.sprite : null;

            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }
    }

    private void NextLine()
    {
        if (thanking)
        {
            Close();
            TutorialManager.Instance?.ReportBallReturned();
            return;
        }

        lineIndex++;

        if (lineIndex < lines.Length)
        {
            ShowLine();
            return;
        }

        Close();
        TutorialManager.Instance?.ReportBoysClue();
    }

    public void OnBallReturned()
    {
        if (!CanReceiveBall || dialogueUI == null || dialogueText == null ||
            characterNameText == null)
            return;

        InventoryTabController.Instance?.Close();
        current = this;
        thanking = true;
        openedFrame = Time.frameCount;

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        dialogueUI.transform.SetAsLastSibling();
        ShowLine();
        dialogueUI.SetActive(true);
    }

    private void Close()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        current = null;
        thanking = false;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (current == this)
            Close();
    }
}
