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
    private Collider2D motherCollider;

    private bool dialogueOpen = false;
    private bool inConversation = false;

    private bool reclamouNome = false;

    public static bool FalouSobreTrabalho { get; private set; } = false;

    private string[] currentLines;
    private Sprite[] currentPortraits;
    private string[] currentNames;

    private int currentLine = 0;

    private void Start()
    {
        FalouSobreTrabalho = false;
        motherCollider = GetComponent<Collider2D>();

        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // O texto da cena tem altura fixa; ajuste o tamanho para que as
        // falas agrupadas caibam inteiras no painel existente.
        dialogueText.enableAutoSizing = true;
        dialogueText.fontSizeMin = 14;
        dialogueText.fontSizeMax = 21;

        if (motherCollider == null)
        {
            Debug.LogWarning(
                "MotherDialogue: a mãe não possui Collider2D."
            );
        }
    }

    private void Update()
    {
        if (PauseHelper.BlockInput())
            return;

        // ==========================================
        // ESTÁ EM UMA FALA
        // ==========================================

        if (inConversation)
        {
            if (Input.GetMouseButtonDown(0))
            {
                NextLine();
            }

            return;
        }

        // ==========================================
        // DIÁLOGO NÃO ESTÁ ABERTO
        // ==========================================

        if (dialogueOpen)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (motherCollider == null)
            return;

        if (Camera.main == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (motherCollider.OverlapPoint(mousePosition))
        {
            OpenInitialDialogue();
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

    // ==================================================
    // PRIMEIRO DIÁLOGO
    // ==================================================

    private void OpenInitialDialogue()
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

        currentPortraits = new Sprite[] { motherPortrait, mariPortrait };
        currentNames = new string[] { "Mãe", "Mari (pensamento)" };

        currentLine = 0;

        EsconderOpcoes();

        ShowLine();
    }

    // ==================================================
    // OPÇÕES
    // ==================================================

    private void OpenChoices()
    {
        dialogueOpen = true;
        inConversation = false;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        portraitImage.sprite = motherPortrait;
        characterNameText.text = "Mãe";
        dialogueText.text = "...";

        // As 3 opções sempre voltam a aparecer
        choice1.gameObject.SetActive(true);
        choice2.gameObject.SetActive(true);
        choice3.gameObject.SetActive(true);

        // RECLAMAR DO NOME
        choice1.GetComponentInChildren<TMP_Text>().text =
            "Reclamar do nome";

        choice1.onClick.RemoveAllListeners();
        choice1.onClick.AddListener(ReclamarNome);

        // FALAR SOBRE O TRABALHO
        choice2.GetComponentInChildren<TMP_Text>().text =
            "Falar sobre o trabalho";

        choice2.onClick.RemoveAllListeners();
        choice2.onClick.AddListener(FalarTrabalho);

        // SAIR
        choice3.GetComponentInChildren<TMP_Text>().text =
            "Sair";

        choice3.onClick.RemoveAllListeners();
        choice3.onClick.AddListener(FecharDialogo);
    }

    // ==================================================
    // RECLAMAR DO NOME
    // ==================================================

    private void ReclamarNome()
    {
        reclamouNome = true;

        EsconderOpcoes();

        currentLines = new string[]
        {
            "Argh… Já pedi para me chamar de Mari, mãe!",
            "Tudo bem, minha filha, mas que coisa! Deveria ter orgulho do seu nome! Sua avó e sua Bisa também eram Marias!"
        };

        currentPortraits = new Sprite[] { mariPortrait, motherPortrait };
        currentNames = new string[] { "Mari", "Mãe" };

        StartConversation();
    }

    // ==================================================
    // FALAR SOBRE O TRABALHO
    // ==================================================

    private void FalarTrabalho()
    {
        EsconderOpcoes();

        currentLines = new string[]
        {
            "Tudo bem, mãe, mas antes… Eu queria saber se a senhora não poderia me ajudar com um trabalho escolar para o final da semana.",
            "Que tipo de trabalho, \"Mari\"?",
            "Eu preciso montar uma árvore genealógica e falar um pouco sobre o passado da nossa família, sobre a nossa história. Talvez eu devesse falar sobre a vovó? Ou melhor, sobre a Bisa.",
            "Querida… Eu gostaria que minha mãe ainda estivesse aqui para ela mesma conversar com você, mas eu posso te ajudar, sim, minha filha.",
            "E sobre a sua Bisa… você sabe que ela não tem mais condições de falar sobre muitas das coisas que já viveu. Não lembra da maior parte.",
            "Tudo bem, mãe… Eu só estava pensando que talvez fosse divertido falar sobre como era a vida na época da minha bisavó. Eu mesma não sei de nada.",
            "Eu sei, meu benzinho. Eu não estou dizendo que você não pode procurar sobre isso, mas a sua Bisa nunca foi muito de conversar. Então, nem mesmo eu consigo te falar muita coisa.",
            "Mas que tal você olhar lá no quartinho da bagunça? Eu sei que sua avó trouxe algumas coisas da mãe dela antes de falecer. Quem sabe você encontre algo!",
            "Certo! Obrigada, mãe… Vou agora mesmo.",
            "Antes, venha me ajudar com o jantar, Maria Gabriely!"
        };

        currentPortraits = new Sprite[]
        {
            mariPortrait, motherPortrait, mariPortrait, motherPortrait,
            motherPortrait, mariPortrait, motherPortrait, motherPortrait,
            mariPortrait, motherPortrait
        };
        currentNames = new string[]
        {
            "Mari", "Mãe", "Mari", "Mãe", "Mãe", "Mari", "Mãe", "Mãe", "Mari", "Mãe"
        };

        StartConversation();
    }

    // ==================================================
    // COMEÇA UMA CONVERSA
    // ==================================================

    private void StartConversation()
    {
        inConversation = true;
        currentLine = 0;

        EsconderOpcoes();

        ShowLine();
    }

    // ==================================================
    // MOSTRA FALA
    // ==================================================

    private void ShowLine()
    {
        portraitImage.sprite = currentPortraits[currentLine];

        characterNameText.text =
            currentNames[currentLine];

        dialogueText.text =
            currentLines[currentLine];
    }

    // ==================================================
    // PRÓXIMA FALA
    // ==================================================

    private void NextLine()
    {
        currentLine++;

        if (currentLine >= currentLines.Length)
        {
            // Se terminamos a conversa sobre o trabalho,
            // libera o quartinho.
            if (currentNames.Length > 0 &&
                currentNames[0] == "Mari" &&
                currentLines.Length == 10)
            {
                // Essa verificação identifica a conversa
                // longa do trabalho.
                if (currentLines[0].StartsWith("Tudo bem, mãe"))
                {
                    FalouSobreTrabalho = true;
                }
            }

            inConversation = false;

            OpenChoices();

            return;
        }

        ShowLine();
    }

    // ==================================================
    // ESCONDE OS BOTÕES DURANTE AS FALAS
    // ==================================================

    private void EsconderOpcoes()
    {
        choice1.gameObject.SetActive(false);
        choice2.gameObject.SetActive(false);
        choice3.gameObject.SetActive(false);
    }

    // ==================================================
    // SAIR
    // ==================================================

    private void FecharDialogo()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        dialogueOpen = false;
        inConversation = false;

        EsconderOpcoes();
    }
}
