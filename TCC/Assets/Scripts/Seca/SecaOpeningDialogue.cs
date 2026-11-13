using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecaOpeningDialogue : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Personagens")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject mariquinha;
    [SerializeField] private Transform saidaMariquinha;
    [SerializeField] private Sprite mariquinhaPortrait;
    [SerializeField] private Sprite mariPortrait;

    public bool Finished { get; private set; }

    private Rigidbody2D playerBody;
    private bool movementWasEnabled;
    private bool movementLocked;
    private bool dialogueRunning;
    private int currentLine;
    private int displayedFrame;

    private readonly string[] names =
    {
        "Mariquinha",
        "Mari (pensamento)",
        "Mari (pensamento)",
        "Mari (pensamento)"
    };

    private readonly string[] lines =
    {
        "Ainda bem que você acordou, Zefinha! Painho tá uma fera! " +
        "Mainha já tá juntando as trouxas, hoje nós pega estrada. " +
        "Eu vou ajudar eles lá fora, trata de levantar logo!",

        "Quem é essa menina? Onde eu estou? Isso é um sonho?",

        "Não… acho que… Ela é a menina da foto? Acho que conheço ela. " +
        "Estava nas coisas da minha bisavó…",

        "Talvez seja melhor eu voltar a dormir. Que sonho estranho… " +
        "É isso, vou dormir!"
    };

    private void Start()
    {
        if (dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null ||
            playerMovement == null)
        {
            Debug.LogError(
                "SecaOpeningDialogue: preencha a interface e o Player Movement no Inspector.",
                this
            );

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

        if (mariquinha != null)
            mariquinha.SetActive(true);

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

        // Cada clique avança somente uma fala.
        if (Time.frameCount > displayedFrame &&
            Input.GetMouseButtonDown(0))
        {
            Advance();
        }
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            StopPlayer();
    }

    private void Advance()
    {
        currentLine++;

        // A irmã sai antes dos pensamentos de Mari.
        if (currentLine == 1 && mariquinha != null)
        {
            dialogueRunning = false;
            StartCoroutine(SairDoQuarto());
            return;
        }

        if (currentLine >= lines.Length)
        {
            FinishDialogue();
            return;
        }

        ShowLine();
    }

    private IEnumerator SairDoQuarto()
    {
        Transform irma = mariquinha.transform;
        Vector3 destino = saidaMariquinha != null ? saidaMariquinha.position : irma.position + Vector3.right * 3f;
        destino.y = irma.position.y;
        destino.z = irma.position.z;
        Animator animador = mariquinha.GetComponent<Animator>();
        SpriteRenderer visual = mariquinha.GetComponent<SpriteRenderer>();
        if (visual != null) visual.flipX = (destino.x - irma.position.x) * irma.lossyScale.x < 0f;
        while (Vector3.Distance(irma.position, destino) > .02f)
        {
            bool andando = !PauseHelper.BlockInput() && Time.timeScale > 0f;
            if (animador != null) animador.SetBool("IsWalking", andando);
            if (andando) irma.position = Vector3.MoveTowards(irma.position, destino, 1.1f * Time.deltaTime);
            yield return null;
        }
        if (animador != null) animador.SetBool("IsWalking", false);
        mariquinha.SetActive(false);
        dialogueRunning = true;
        ShowLine();
    }

    private void ShowLine()
    {
        characterNameText.text = names[currentLine];
        dialogueText.text = lines[currentLine];

        if (portraitImage != null)
        {
            Sprite portrait = currentLine == 0
                ? mariquinhaPortrait
                : mariPortrait;

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
        GameProgress.Instance?.UnlockDiaryPage("josefa-02");

        // DialogueUI contém só a conversa; ExaminePanel é seu irmão no Canvas.
        dialogueUI.SetActive(false);
        dialoguePanel.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        RestoreMovement();
    }

    private void StopPlayer()
    {
        if (playerBody != null)
        {
            Vector2 velocity = playerBody.linearVelocity;
            velocity.x = 0f;
            playerBody.linearVelocity = velocity;
        }
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