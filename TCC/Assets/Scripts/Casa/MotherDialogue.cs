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

    private bool reclamouNome = false;
    private bool falouTrabalho = false;

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
        if (PauseHelper.BlockInput())
            return;

        if (inConversation)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextLine();
            }
        }
    }

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput())
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
        if (PauseHelper.BlockInput())
            return;

        if (dialogueOpen)
            return;

        OpenInitialDialogue();
    }

    // =========================================================
    // DIÁLOGO INICIAL
    // =========================================================

    void OpenInitialDialogue()
    {
        dialogueOpen = true;
        inConversation = true;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        currentLines = new string[]
        {
            "Ah… Oi, Maria Gabriely! Ainda bem que você já chegou da escola, minha filha. Venha me ajudar a preparar a janta.",
            "Detesto quando ela me chama assim! Talvez eu devesse falar sobre o trabalho… ou só ir para o meu quarto mesmo."
        };

        currentPortraits = new Sprite[]
        {
            motherPortrait,
            mariPortrait
        };

        currentNames = new string[]
        {
            "Mãe",
            "Mari (pensamento)"
        };

        currentLine = 0;

        choice1.gameObject.SetActive(false);
        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        ShowLine();
    }

    // =========================================================
    // ESCOLHAS
    // =========================================================

    void OpenChoices()
    {
        dialogueOpen = true;
        inConversation = false;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        portraitImage.sprite = motherPortrait;
        characterNameText.text = "Mãe";
        dialogueText.text = "...";

        // OPÇÃO 1
        if (!reclamouNome)
        {
            choice1.gameObject.SetActive(true);

            choice1.GetComponentInChildren<TMP_Text>().text =
                "Reclamar do nome";

            choice1.onClick.RemoveAllListeners();
            choice1.onClick.AddListener(ReclamarNome);
        }
        else
        {
            choice1.gameObject.SetActive(false);
        }

        // OPÇÃO 2
        if (!falouTrabalho)
        {
            choice2.gameObject.SetActive(true);

            choice2.GetComponentInChildren<TMP_Text>().text =
                "Falar sobre o trabalho";

            choice2.onClick.RemoveAllListeners();
            choice2.onClick.AddListener(FalarTrabalho);
        }
        else
        {
            choice2.gameObject.SetActive(false);
        }

        // OPÇÃO 3
        choice3.gameObject.SetActive(true);

        choice3.GetComponentInChildren<TMP_Text>().text =
            "Sair";

        choice3.onClick.RemoveAllListeners();
        choice3.onClick.AddListener(FecharDialogo);
    }

    // =========================================================
    // OPÇÃO 1 - RECLAMAR DO NOME
    // =========================================================

    void ReclamarNome()
    {
        reclamouNome = true;

        currentLines = new string[]
        {
            "Argh… Já pedi para me chamar de Mari, mãe!",
            "Tudo bem, minha filha, mas que coisa! Deveria ter orgulho do seu nome! Sua avó e sua Bisa também eram Marias!"
        };

        currentPortraits = new Sprite[]
        {
            mariPortrait,
            motherPortrait
        };

        currentNames = new string[]
        {
            "Mari",
            "Mãe"
        };

        StartConversation();
    }

    // =========================================================
    // OPÇÃO 2 - FALAR SOBRE O TRABALHO
    // =========================================================

    void FalarTrabalho()
    {
        falouTrabalho = true;

        currentLines = new string[]
        {
            "Tudo bem, mãe, mas antes… Eu queria saber se a senhora não poderia me ajudar com um trabalho escolar para o final da semana.",

            "Que tipo de trabalho, \"Mari\"?",

            "Eu preciso montar uma árvore genealógica e falar um pouco sobre o passado da nossa família, sobre a nossa história. Talvez eu devesse falar sobre a vovó? Ou melhor, sobre a Bisa.",

            "Querida… Eu gostaria que minha mãe ainda estivesse aqui para ela mesma conversar com você, mas eu posso te ajudar, sim, minha filha. E sobre a sua Bisa… você sabe que ela não tem mais condições de falar sobre muitas das coisas que já viveu. Não lembra da maior parte.",

            "Tudo bem, mãe… Eu só estava pensando que talvez fosse divertido falar sobre como era a vida na época da minha bisavó. Eu mesma não sei de nada.",

            "Eu sei, meu benzinho. Eu não estou dizendo que você não pode procurar sobre isso, mas a sua Bisa nunca foi muito de conversar. Então, nem mesmo eu consigo te falar muita coisa. Mas que tal você olhar lá no quartinho da bagunça? Eu sei que sua avó trouxe algumas coisas da mãe dela antes de falecer. Quem sabe você encontre algo!",

            "Certo! Obrigada, mãe… Vou agora mesmo.",

            "Antes, venha me ajudar com o jantar, Maria Gabriely!"
        };

        currentPortraits = new Sprite[]
        {
            mariPortrait,
            motherPortrait,
            mariPortrait,
            motherPortrait,
            mariPortrait,
            motherPortrait,
            mariPortrait,
            motherPortrait
        };

        currentNames = new string[]
        {
            "Mari",
            "Mãe",
            "Mari",
            "Mãe",
            "Mari",
            "Mãe",
            "Mari",
            "Mãe"
        };

        StartConversation();
    }

    // =========================================================
    // INICIAR CONVERSA
    // =========================================================

    void StartConversation()
    {
        inConversation = true;
        currentLine = 0;

        choice1.gameObject.SetActive(false);
        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);

        ShowLine();
    }

    // =========================================================
    // MOSTRAR FALA
    // =========================================================

    void ShowLine()
    {
        portraitImage.sprite = currentPortraits[currentLine];
        characterNameText.text = currentNames[currentLine];
        dialogueText.text = currentLines[currentLine];
    }

    // =========================================================
    // AVANÇAR
    // =========================================================

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

    // =========================================================
    // FECHAR
    // =========================================================

    void FecharDialogo()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        dialogueOpen = false;
        inConversation = false;
    }
}