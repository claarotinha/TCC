using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotherDialogue : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TMP_Text characterNameText;
    public TMP_Text dialogueText;

    [Header("Choices")]
    public Button choice1;
    public Button choice2;
    public Button choice3;

    [Header("Portraits")]
    public Sprite motherPortrait;
    public Sprite mariPortrait;

    private CanvasGroup canvasGroup;

    private bool dialogueOpen = false;
    private bool inConversation = false;

    private bool conversou = false;
    private bool perguntouTrabalho = false;

    private string[] currentLines;
    private Sprite[] currentPortraits;
    private string[] currentNames;

    private int currentLine = 0;

    private void Start()
    {
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        // NÃO PERMITE AVANÇAR O DIÁLOGO DURANTE O PAUSE
        if (PauseManager.IsPaused)
            return;

        if (inConversation)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                NextLine();
            }
        }
    }

    private void OnMouseEnter()
    {
        // NÃO MUDA O CURSOR DURANTE O PAUSE
        if (PauseManager.IsPaused)
            return;

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
        }
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }

    private void OnMouseDown()
    {
        // NÃO PERMITE ABRIR DIÁLOGO DURANTE O PAUSE
        if (PauseManager.IsPaused)
            return;

        if (dialogueOpen)
            return;

        OpenChoices();
    }

    void OpenChoices()
    {
        dialogueOpen = true;
        inConversation = false;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        portraitImage.sprite = motherPortrait;
        characterNameText.text = "Mãe";
        dialogueText.text = "O que foi, Mari?";

        if (!conversou)
        {
            choice1.gameObject.SetActive(true);

            choice1.GetComponentInChildren<TMP_Text>().text = "Conversar";

            choice1.onClick.RemoveAllListeners();
            choice1.onClick.AddListener(Conversar);
        }
        else
        {
            choice1.gameObject.SetActive(false);
        }

        if (!perguntouTrabalho)
        {
            choice2.gameObject.SetActive(true);

            choice2.GetComponentInChildren<TMP_Text>().text =
                "Perguntar sobre o trabalho";

            choice2.onClick.RemoveAllListeners();
            choice2.onClick.AddListener(PerguntarTrabalho);
        }
        else
        {
            choice2.gameObject.SetActive(false);
        }

        choice3.gameObject.SetActive(true);

        choice3.GetComponentInChildren<TMP_Text>().text = "Sair";

        choice3.onClick.RemoveAllListeners();
        choice3.onClick.AddListener(FecharDialogo);
    }

    void Conversar()
    {
        conversou = true;

        currentLines = new string[]
        {
            "Como foi a escola hoje, filha? Você parece pensativa.",
            "Foi normal... Só estou pensando nesse trabalho sobre a árvore genealógica.",
            "Talvez seja uma boa oportunidade para descobrir coisas que você ainda não conhece sobre a nossa família."
        };

        currentPortraits = new Sprite[]
        {
            motherPortrait,
            mariPortrait,
            motherPortrait
        };

        currentNames = new string[]
        {
            "Mãe",
            "Mari",
            "Mãe"
        };

        StartConversation();
    }

    void PerguntarTrabalho()
    {
        perguntouTrabalho = true;

        currentLines = new string[]
        {
            "Mãe... você acha que pode me ajudar com esse trabalho?",
            "Claro. Temos algumas fotografias antigas guardadas em casa.",
            "Vou procurar depois. Obrigada."
        };

        currentPortraits = new Sprite[]
        {
            mariPortrait,
            motherPortrait,
            mariPortrait
        };

        currentNames = new string[]
        {
            "Mari",
            "Mãe",
            "Mari"
        };

        StartConversation();
    }

    void StartConversation()
    {
        inConversation = true;
        currentLine = 0;

        choice1.gameObject.SetActive(false);
        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        ShowLine();
    }

    void ShowLine()
    {
        portraitImage.sprite = currentPortraits[currentLine];
        characterNameText.text = currentNames[currentLine];
        dialogueText.text = currentLines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= currentLines.Length)
        {
            OpenChoices();
            return;
        }

        ShowLine();
    }

    void FecharDialogo()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        dialogueOpen = false;
        inConversation = false;
    }
}