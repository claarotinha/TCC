

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

    private enum DialogueState
    {
        Closed,
        Choosing,
        Answering,
        Thanking
    }

    private Collider2D clickCollider;
    private SpriteRenderer boysSprite;
    private CanvasGroup panelGroup;
    private Button choice1;
    private Button choice2;
    private Button choice3;

    private DialogueState state = DialogueState.Closed;
    private bool heardWhatHappened;
    private int openedFrame;

    public bool CanReceiveBall =>
        TutorialManager.Instance != null &&
        TutorialManager.Instance.CurrentStep ==
            TutorialManager.TutorialStep.ReturnBall &&
        current == null;

    private void Awake()
    {
        clickCollider = GetComponent<Collider2D>();
        boysSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (dialogueUI == null ||
            dialogueText == null ||
            characterNameText == null ||
            choicesContainer == null)
        {
            Debug.LogError(
                "Garotos: configure DialogueUI, DialogueText, " +
                "CharacterName e ChoicesContainer no Inspector.",
                this
            );
            return;
        }

        // No prefab, o CanvasGroup fica no filho DialoguePanel.
        panelGroup = dialogueUI.GetComponentInChildren<CanvasGroup>(true);

        choice1 = FindChoice("Choice1");
        choice2 = FindChoice("Choice2");
        choice3 = FindChoice("Choice3");

        if (panelGroup == null ||
            choice1 == null ||
            choice2 == null ||
            choice3 == null)
        {
            Debug.LogError(
                "Garotos: DialoguePanel precisa de CanvasGroup, " +
                "e ChoicesContainer precisa dos botões Choice1, " +
                "Choice2 e Choice3.",
                this
            );
            return;
        }

        SetChoiceText(choice1, "O que houve?");
        SetChoiceText(choice2, "Onde foi?");
        SetChoiceText(choice3, "Vou ajudar");

        choice1.onClick.AddListener(AskWhatHappened);
        choice2.onClick.AddListener(AskWhere);
        choice3.onClick.AddListener(OfferHelp);

        choicesContainer.SetActive(false);
        HidePanel();
    }

    private Button FindChoice(string name)
    {
        Transform child = choicesContainer.transform.Find(name);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private void SetChoiceText(Button button, string value)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
            text.text = value;
    }

    private void Update()
    {
        if (current != this ||
            !Input.GetMouseButtonDown(0) ||
            Time.frameCount <= openedFrame)
            return;

        if (state == DialogueState.Answering)
        {
            ShowChoices();
        }
        else if (state == DialogueState.Thanking)
        {
            Close();
            TutorialManager.Instance?.ReportBallReturned();
        }

        // Enquanto as escolhas aparecem, somente os botões respondem.
    }

    private void LateUpdate()
    {
        if (current != null ||
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

        if (dialogueUI == null ||
            dialogueText == null ||
            characterNameText == null ||
            choicesContainer == null ||
            panelGroup == null ||
            choice1 == null ||
            choice2 == null ||
            choice3 == null)
        {
            Debug.LogError(
                "Garotos: referências do painel incompletas.",
                this
            );
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

            // Primeiro fecha o painel anterior; o próximo clique conversa.
            return;
        }

        current = this;
        openedFrame = Time.frameCount;

        ShowPanel();
        ShowChoices();
    }

    private void ShowPanel()
    {
        dialogueUI.transform.SetAsLastSibling();
        dialogueUI.SetActive(true);

        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
    }

    private void HidePanel()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    private void ShowChoices()
    {
        state = DialogueState.Choosing;

        SetLine(
            "Mari",
            "O menino está chorando. E melhor tentar entender o que aconteceu",
            mariPortrait
        );

        choicesContainer.SetActive(true);

        // Antes de descobrir o ocorrido, Mari ainda não pode
        // encerrar a conversa e sair à procura da bola.
        choice3.interactable = heardWhatHappened;
    }

    private void AskWhatHappened()
    {
        if (current != this || state != DialogueState.Choosing)
            return;

        heardWhatHappened = true;

        ShowAnswer(
            "A bola escapou enquanto brincávamos perto da padaria. " +
            "Ele ficou muito triste porque não conseguimos encontrá-la."
        );
    }

    private void AskWhere()
    {
        if (current != this || state != DialogueState.Choosing)
            return;

        ShowAnswer(
            "Ela rolou pela calçada. Talvez tenha ido parar " +
            "perto do lixo; vale a pena procurar por ali."
        );
    }

    private void OfferHelp()
    {
        if (current != this ||
            state != DialogueState.Choosing ||
            !heardWhatHappened)
            return;

        Close();
        TutorialManager.Instance?.ReportBoysClue();
    }

    private void ShowAnswer(string answer)
    {
        state = DialogueState.Answering;
        openedFrame = Time.frameCount;
        choicesContainer.SetActive(false);

        SetLine(
            "Garotos",
            answer + "\n\nClique para continuar.",
            boysSprite != null ? boysSprite.sprite : null
        );
    }

    private void SetLine(string speaker, string line, Sprite portrait)
    {
        characterNameText.text = speaker;
        dialogueText.text = line;

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
        }
    }

    public void OnBallReturned()
    {
        if (!CanReceiveBall ||
            dialogueUI == null ||
            dialogueText == null ||
            characterNameText == null ||
            panelGroup == null)
            return;

        InventoryTabController.Instance?.Close();

        current = this;
        state = DialogueState.Thanking;
        openedFrame = Time.frameCount;

        choicesContainer.SetActive(false);
        ShowPanel();

        SetLine(
            "Garotos",
            "Encontrou a nossa bola! Muito obrigado, Mari. " +
            "Agora podemos voltar a brincar.\n\nClique para continuar.",
            boysSprite != null ? boysSprite.sprite : null
        );
    }

    private void Close()
    {
        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        HidePanel();

        current = null;
        state = DialogueState.Closed;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (current == this)
            Close();
    }

    private void OnDestroy()
    {
        if (choice1 != null)
            choice1.onClick.RemoveListener(AskWhatHappened);

        if (choice2 != null)
            choice2.onClick.RemoveListener(AskWhere);

        if (choice3 != null)
            choice3.onClick.RemoveListener(OfferHelp);

        if (current == this)
            current = null;
    }
}

