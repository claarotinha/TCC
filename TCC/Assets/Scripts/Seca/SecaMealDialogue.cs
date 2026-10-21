using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ExamineObject))]
public class SecaMealDialogue : MonoBehaviour
{
    [Header("Liberação")]
    [SerializeField] private SecaRoomExit roomExit;

    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform lugarMari;

    [Header("Interface compartilhada")]
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

    public bool IntroCompleted { get; private set; }

    private ExamineObject tableInteraction;
    private Rigidbody2D playerBody;

    private bool configured;
    private bool started;
    private bool running;
    private bool waitingForSilence;
    private bool movementWasEnabled;
    private bool movementLocked;

    private int currentLine;
    private int displayedFrame;

    private readonly string[] names =
    {
        "Pai",
        "Mãe",
        "Pai",
        "Mãe",
        "Pai",
        "Mãe",
        "Pai",
        "Mãe",
        "Pai",
        "Mãe",
        "Pai",
        "Mariquinha",
        "Mãe",
        "Mari (pensamento)"
    };

    private readonly string[] lines =
    {
        "Não dá mais… A terra morreu. O açude morreu. " +
        "Os bichos morreram. Se a gente fica, morremos junto… " +
        "Hoje nós vai embora.",

        "Vamos cumê. É pouco, mas é melhor do que nada.",

        "Coma você e as menina. Já comi hoje mais cedo, " +
        "perto da bodega do seu Zé.",

        "Não, coma também. O dia vai ser longo…",

        "Já disse que estou bem, Zulema…",

        "Coma…",

        "Mulher teimosa…",

        "E tu é orgulhoso.",

        "Orgulho num enche barriga.",

        "Então coma.",

        "As menina primeiro.",

        "Mainha, posso dar um pedacinho para Baleia?",

        "Pode, minha filha. Ela também tá com fome.",

        "Talvez eu também devesse dividir um pouco da minha comida " +
        "com a cachorra… Mas tem tão pouco, e eu realmente estou " +
        "com fome. Mariquinha deu um pedaço para ela… " +
        "O que eu faço com a pouca comida que me resta?"
    };

    private void Awake()
    {
        tableInteraction = GetComponent<ExamineObject>();
        tableInteraction.enabled = false;
        tableInteraction.Opened += BeginMeal;
    }

    private void Start()
    {
        configured =
            roomExit != null &&
            playerMovement != null &&
            lugarMari != null &&
            dialogueUI != null &&
            dialoguePanel != null &&
            characterNameText != null &&
            dialogueText != null;

        if (!configured)
        {
            Debug.LogError(
                "SecaMealDialogue: preencha as referências da mesa no Inspector.",
                this
            );
        }
    }

    private void Update()
    {
        if (!configured)
            return;

        if (!started && roomExit.EnteredRoom &&
            !tableInteraction.enabled)
        {
            tableInteraction.enabled = true;
        }

        if (!running || waitingForSilence || Time.timeScale == 0f)
            return;

        if (Time.frameCount > displayedFrame &&
            Input.GetMouseButtonDown(0))
        {
            currentLine++;

            if (currentLine >= lines.Length)
            {
                FinishIntro();
                return;
            }

            ShowLine();
        }
    }

    private void BeginMeal()
    {
        if (!configured || started || !roomExit.EnteredRoom)
            return;

        started = true;

        tableInteraction.HidePanel();
        tableInteraction.enabled = false;

        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        movementLocked = true;

        playerBody = playerMovement.GetComponent<Rigidbody2D>();

        Vector3 destination = lugarMari.position;

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.position = new Vector2(destination.x, destination.y);
        }

        playerMovement.transform.position = destination;

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

        running = true;
        StartCoroutine(InitialSilence());
    }

    private IEnumerator InitialSilence()
    {
        waitingForSilence = true;

        characterNameText.text = "";
        dialogueText.text = "…";

        if (portraitImage != null)
            portraitImage.enabled = false;

        yield return new WaitForSeconds(1.5f);

        currentLine = 0;
        waitingForSilence = false;
        ShowLine();
    }

    private void ShowLine()
    {
        characterNameText.text = names[currentLine];
        dialogueText.text = lines[currentLine];

        if (portraitImage != null)
        {
            Sprite portrait = null;

            switch (names[currentLine])
            {
                case "Pai":
                    portrait = paiPortrait;
                    break;
                case "Mãe":
                    portrait = maePortrait;
                    break;
                case "Mariquinha":
                    portrait = mariquinhaPortrait;
                    break;
                default:
                    portrait = mariPortrait;
                    break;
            }

            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;
        }

        displayedFrame = Time.frameCount;
    }

    private void FixedUpdate()
    {
        if (!movementLocked || playerBody == null)
            return;

        Vector2 velocity = playerBody.linearVelocity;
        velocity.x = 0f;
        playerBody.linearVelocity = velocity;
    }

    private void FinishIntro()
    {
        running = false;
        IntroCompleted = true;
        dialogueUI.SetActive(false);
        RestoreMovement();
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
        StopAllCoroutines();

        if (running && dialogueUI != null)
            dialogueUI.SetActive(false);

        running = false;
        RestoreMovement();
    }

    private void OnDestroy()
    {
        if (tableInteraction != null)
            tableInteraction.Opened -= BeginMeal;
    }
}