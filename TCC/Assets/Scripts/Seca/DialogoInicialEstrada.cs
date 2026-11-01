using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogoInicialEstrada : MonoBehaviour
{
    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Canvas de dialogo existente")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Retratos opcionais")]
    [SerializeField] private Sprite paiPortrait;
    [SerializeField] private Sprite maePortrait;
    [SerializeField] private Sprite mariquinhaPortrait;
    [SerializeField] private Sprite mariPortrait;

    public bool Finished { get; private set; }

    private readonly string[] names =
    {
        "Mari (pensamento)", "Mainha", "Painho", "Mariquinha", "Painho"
    };

    // Falas provisórias para testar o início da caminhada.
    private readonly string[] lines =
    {
        "A estrada parecia não ter fim...",
        "Vamo seguindo. Ainda tem muito chão pela frente.",
        "Fiquem tudo perto. Ninguém se afaste.",
        "A gente vai demorar muito pra chegar?",
        "Não sei, minha filha. Mas precisamos continuar."
    };

    private Rigidbody2D playerBody;
    private bool movementWasEnabled;
    private bool movementLocked;
    private bool dialogueRunning;
    private int currentLine;
    private int displayedFrame;

    private void Start()
    {
        if (playerMovement == null || dialogueUI == null ||
            dialoguePanel == null || characterNameText == null ||
            dialogueText == null)
        {
            Debug.LogError("DialogoInicialEstrada: configure as referencias no Inspector.", this);
            enabled = false;
            return;
        }

        playerBody = playerMovement.GetComponent<Rigidbody2D>();
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        movementLocked = true;
        StopPlayer();

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
        dialogueRunning = true;
        ShowLine();
    }

    private void Update()
    {
        if (!dialogueRunning || Time.timeScale == 0f)
            return;

        if (Time.frameCount > displayedFrame && Input.GetMouseButtonDown(0))
            Advance();
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            StopPlayer();
    }

    private void Advance()
    {
        currentLine++;
        if (currentLine >= lines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowLine();
    }

    private void ShowLine()
    {
        characterNameText.text = names[currentLine];
        dialogueText.text = lines[currentLine];

        if (portraitImage != null)
        {
            Sprite portrait;
            switch (currentLine)
            {
                case 1: portrait = maePortrait; break;
                case 2:
                case 4: portrait = paiPortrait; break;
                case 3: portrait = mariquinhaPortrait; break;
                default: portrait = mariPortrait; break;
            }

            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        displayedFrame = Time.frameCount;
    }

    private void FinishDialogue()
    {
        dialogueRunning = false;
        Finished = true;
        dialogueUI.SetActive(false);
        RestoreMovement();
    }

    private void StopPlayer()
    {
        if (playerBody == null)
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
        if (dialogueRunning && dialogueUI != null)
            dialogueUI.SetActive(false);

        dialogueRunning = false;
        RestoreMovement();
    }
}
