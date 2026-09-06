using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)]
        public string text;
        public string characterName;
        public Sprite portrait;
    }

    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public DialogueLine[] dialogue;
    }

    [Header("Configuração do NPC")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcPortrait;

    [Header("Diálogo")]
    [SerializeField] private DialogueLine[] initialDialogue;
    [SerializeField] private DialogueOption[] dialogueOptions;

    // Referências da UI (arraste no Inspector)
    [Header("UI - Arraste os objetos da cena")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionTexts;

    private DialogueLine[] currentDialogue;
    private int currentLine;
    private bool isShowing = false;
    private bool showingOptions = false;
    private static NPCDialogue currentObject = null;

    private void Start()
    {
        HidePanel();
        ConfigureButtons();
    }

    private void Update()
    {
        if (!isShowing) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (!showingOptions)
            {
                NextLine();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel();
        }
    }

    private void OnMouseDown()
    {
        if (!isShowing)
        {
            if (currentObject != null && currentObject != this)
            {
                currentObject.HidePanel();
            }

            ShowPanel();
            currentObject = this;
            StartInitialDialogue();
        }
    }

    private void StartInitialDialogue()
    {
        if (initialDialogue != null && initialDialogue.Length > 0)
        {
            StartDialogue(initialDialogue);
        }
        else
        {
            ShowOptions();
        }
    }

    private void StartDialogue(DialogueLine[] dialogue)
    {
        if (dialogue == null || dialogue.Length == 0)
        {
            ShowOptions();
            return;
        }

        currentDialogue = dialogue;
        currentLine = 0;
        showingOptions = false;

        HideOptionButtons();
        ShowCurrentLine();
        EnsureUIVisible();
    }

    private void EnsureUIVisible()
    {
        if (dialogueUI != null) dialogueUI.SetActive(true);
        isShowing = true;
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null || currentLine >= currentDialogue.Length)
        {
            HidePanel();
            return;
        }

        DialogueLine line = currentDialogue[currentLine];

        if (dialogueText != null)
            dialogueText.text = line.text;

        if (characterNameText != null)
        {
            characterNameText.text = string.IsNullOrEmpty(line.characterName) ?
                npcName : line.characterName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait != null ? line.portrait : npcPortrait;
        }
    }

    private void NextLine()
    {
        if (showingOptions) return;

        currentLine++;

        if (currentDialogue == null || currentLine >= currentDialogue.Length)
        {
            ShowOptions();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowOptions()
    {
        showingOptions = true;
        currentDialogue = null;
        currentLine = 0;

        EnsureUIVisible();

        if (dialogueText != null)
            dialogueText.text = "O que você gostaria de saber?";

        if (characterNameText != null)
            characterNameText.text = npcName;

        if (choicesContainer != null)
            choicesContainer.SetActive(true);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < dialogueOptions.Length)
            {
                optionButtons[i].gameObject.SetActive(true);

                if (i < optionTexts.Length && optionTexts[i] != null)
                {
                    optionTexts[i].text = dialogueOptions[i].optionText;
                }

                optionButtons[i].onClick.RemoveAllListeners();

                int index = i;
                optionButtons[i].onClick.AddListener(() => SelectOption(index));
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectOption(int index)
    {
        if (index < 0 || index >= dialogueOptions.Length) return;

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        StartDialogue(dialogueOptions[index].dialogue);
    }

    public void ShowPanel()
    {
        EnsureUIVisible();
    }

    public void HidePanel()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);

        isShowing = false;
        showingOptions = false;
        currentDialogue = null;
        currentLine = 0;

        if (currentObject == this)
            currentObject = null;
    }

    private void HideOptionButtons()
    {
        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
    }

    private void ConfigureButtons()
    {
        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    public static bool IsShowing()
    {
        return currentObject != null && currentObject.isShowing;
    }

    private void OnMouseEnter()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }
}