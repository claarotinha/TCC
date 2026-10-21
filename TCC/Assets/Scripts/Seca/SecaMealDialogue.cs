using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [Header("Escolhas da refeição")]
    [SerializeField] private Button[] choiceButtons = new Button[3];

    [Header("Após terminar a refeição")]
    [SerializeField] private UnityEvent onMealCompleted = new UnityEvent();

    public bool IntroCompleted { get; private set; }
    public bool MealCompleted { get; private set; }

    // -1: sem escolha; 0: comer; 1: Baleia; 2: irmã.
    public int FoodChoiceIndex { get; private set; } = -1;

    private ExamineObject tableInteraction;
    private Rigidbody2D playerBody;
    private UnityAction[] choiceActions;

    private bool configured;
    private bool started;
    private bool running;
    private bool waitingForSilence;
    private bool showingChoices;
    private bool showingResponse;
    private bool movementWasEnabled;
    private bool movementLocked;

    private int currentLine;
    private int displayedFrame;

    private readonly string[] names =
    {
        "Pai", "Mãe", "Pai", "Mãe", "Pai", "Mãe", "Pai",
        "Mãe", "Pai", "Mãe", "Pai", "Mariquinha", "Mãe",
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

    private readonly string[] choiceLabels =
    {
        "Comer tudo",
        "Dividir com Baleia",
        "Dividir com a irmã"
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
            choicesContainer != null &&
            characterNameText != null &&
            dialogueText != null &&
            choiceButtons != null &&
            choiceButtons.Length == 3;

        if (configured)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null ||
                    choiceButtons[i].GetComponentInChildren<TMP_Text>(true) == null)
                {
                    configured = false;
                    break;
                }

                for (int j = 0; j < i; j++)
                {
                    if (choiceButtons[i] == choiceButtons[j])
                        configured = false;
                }
            }
        }

        if (!configured)
        {
            Debug.LogError(
                "SecaMealDialogue: confira as referências e os três botões distintos.",
                this
            );
            return;
        }

        choiceActions = new UnityAction[3];

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;

            choiceButtons[i]
                .GetComponentInChildren<TMP_Text>(true)
                .text = choiceLabels[i];

            choiceActions[i] = () => SelectFood(index);
            choiceButtons[i].onClick.AddListener(choiceActions[i]);
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

        if (!running || waitingForSilence ||
            showingChoices || Time.timeScale == 0f)
        {
            return;
        }

        if (Time.frameCount <= displayedFrame ||
            !Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (showingResponse)
        {
            FinishMeal();
            return;
        }

        currentLine++;

        if (currentLine >= lines.Length)
        {
            ShowChoices();
            return;
        }

        ShowLine();
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
            playerBody.position =
                new Vector2(destination.x, destination.y);
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
        SetPortrait(names[currentLine]);
        displayedFrame = Time.frameCount;
    }

    private void ShowChoices()
    {
        IntroCompleted = true;
        showingChoices = true;

        choicesContainer.SetActive(true);
        ShowCanvasGroup(choicesContainer);

        foreach (Button button in choiceButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;
        }
    }

    private void SelectFood(int index)
    {
        if (!running || !showingChoices ||
            index < 0 || index >= 3 || Time.timeScale == 0f)
        {
            return;
        }

        FoodChoiceIndex = index;
        showingChoices = false;
        showingResponse = true;

        choicesContainer.SetActive(false);

        switch (index)
        {
            case 0:
                characterNameText.text = "Mari (pensamento)";
                dialogueText.text =
                    "Estou com muita fome… Vou comer o que restou.";
                SetPortrait("Mari");
                break;

            case 1:
                characterNameText.text = "Mari";
                dialogueText.text =
                    "Vem, Baleia. Tem um pedacinho para você também.";
                SetPortrait("Mari");
                break;

            case 2:
                characterNameText.text = "Mariquinha";
                dialogueText.text =
                    "Obrigada, Zefinha… Pode ficar com um pouco do meu também.";
                SetPortrait("Mariquinha");
                break;
        }

        displayedFrame = Time.frameCount;
    }

    private void FinishMeal()
    {
        running = false;
        showingResponse = false;
        MealCompleted = true;

        choicesContainer.SetActive(false);
        dialogueUI.SetActive(false);

        RestoreMovement();
        onMealCompleted.Invoke();
    }

    private void SetPortrait(string speaker)
    {
        if (portraitImage == null)
            return;

        Sprite portrait;

        switch (speaker)
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
        StopAllCoroutines();

        if (running)
        {
            if (choicesContainer != null)
                choicesContainer.SetActive(false);

            if (dialogueUI != null)
                dialogueUI.SetActive(false);
        }

        running = false;
        showingChoices = false;
        RestoreMovement();
    }

    private void OnDestroy()
    {
        if (tableInteraction != null)
            tableInteraction.Opened -= BeginMeal;

        if (choiceActions == null || choiceButtons == null)
            return;

        for (int i = 0; i < choiceActions.Length; i++)
        {
            if (i < choiceButtons.Length &&
                choiceButtons[i] != null &&
                choiceActions[i] != null)
            {
                choiceButtons[i].onClick.RemoveListener(choiceActions[i]);
            }
        }
    }
}