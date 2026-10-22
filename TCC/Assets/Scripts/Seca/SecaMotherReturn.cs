using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ExamineObject))]
public class SecaMotherReturn : MonoBehaviour
{
    [Header("Preparativos")]
    [SerializeField] private SecaPreparationRequest preparationRequest;
    [SerializeField] private SecaPreparationTracker preparationTracker;

    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Interface compartilhada")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Retratos opcionais")]
    [SerializeField] private Sprite mariPortrait;
    [SerializeField] private Sprite maePortrait;

    [Header("Após terminar a conversa")]
    [SerializeField] private UnityEvent onReadyToLeave = new UnityEvent();

    public bool DepartureReady { get; private set; }

    private ExamineObject examineObject;
    private Rigidbody2D playerBody;
    private bool configured;
    private bool started;
    private bool running;
    private bool movementWasEnabled;
    private bool movementLocked;
    private int interactionReleaseFrame = -1;
    private int currentLine;
    private int displayedFrame;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.enabled = false;
        examineObject.Opened += HandleInteraction;

        examineObject.message =
            "Ainda preciso pegar tudo que mainha pediu: " +
            "o cantil, o facão, o pano, o chapéu e a farinha.";
    }

    private void Start()
    {
        configured =
            preparationRequest != null &&
            preparationTracker != null &&
            playerMovement != null &&
            dialogueUI != null &&
            dialoguePanel != null &&
            characterNameText != null &&
            dialogueText != null;

        if (!configured)
        {
            Debug.LogError(
                "SecaMotherReturn: preencha as referências no Inspector.",
                this
            );
        }
    }

    private void Update()
    {
        if (!configured)
            return;

        if (!started && preparationRequest.RequestCompleted &&
            !examineObject.enabled)
        {
            // Não usa o clique que acabou de fechar o pedido da mãe.
            if (interactionReleaseFrame < 0)
                interactionReleaseFrame = Time.frameCount;
            else if (Time.frameCount > interactionReleaseFrame)
                examineObject.enabled = true;
        }

        if (!running || InvestigationGuard.Blocked || Time.timeScale == 0f)
            return;

        if (Time.frameCount <= displayedFrame ||
            !Input.GetMouseButtonDown(0))
        {
            return;
        }

        currentLine++;

        if (currentLine >= 2)
        {
            FinishConversation();
            return;
        }

        ShowLine();
    }

    private void HandleInteraction()
    {
        if (!configured || started ||
            !preparationRequest.RequestCompleted)
        {
            return;
        }

        // Se faltar algo, mantém a mensagem de investigação.
        if (!preparationTracker.AllItemsCollected)
            return;

        started = true;
        examineObject.HidePanel();
        examineObject.enabled = false;

        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        movementLocked = true;

        playerBody = playerMovement.GetComponent<Rigidbody2D>();
        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        Animator animator = playerMovement.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }

        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        ShowCanvasGroup(dialogueUI);
        ShowCanvasGroup(dialoguePanel);

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        currentLine = 0;
        running = true;
        ShowLine();
    }

    private void ShowLine()
    {
        Sprite portrait;

        if (currentLine == 0)
        {
            characterNameText.text = "Mari";
            dialogueText.text = "Peguei tudo que a senhora mandou, mainha.";
            portrait = mariPortrait;
        }
        else
        {
            characterNameText.text = "Mãe";
            dialogueText.text = "Então já está na hora de pegar estrada.";
            portrait = maePortrait;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        displayedFrame = Time.frameCount;
    }

    private void FinishConversation()
    {
        running = false;
        DepartureReady = true;
        dialogueUI.SetActive(false);
        preparationTracker.HideObjective();

        RestoreMovement();
        InvestigationGuard.BlockCurrentClick();
        onReadyToLeave.Invoke();
    }

    private void FixedUpdate()
    {
        if (!movementLocked || playerBody == null)
            return;

        Vector2 velocity = playerBody.linearVelocity;
        velocity.x = 0f;
        playerBody.linearVelocity = velocity;
    }

    private void RestoreMovement()
    {
        if (!movementLocked)
            return;

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        movementLocked = false;
    }

    private static void ShowCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            return;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void OnDisable()
    {
        if (running && dialogueUI != null)
            dialogueUI.SetActive(false);

        running = false;
        RestoreMovement();
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= HandleInteraction;
    }
}
