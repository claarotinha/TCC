using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NPCDialogue : MonoBehaviour
{
    // =========================================================
    // FALA
    // =========================================================

    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 5)]
        public string text;
        public string characterName;
        public Sprite portrait;
    }

    // =========================================================
    // OPÇÃO DE PERGUNTA
    // =========================================================

    [System.Serializable]
    public class DialogueOption
    {
        public string optionText;
        public DialogueLine[] dialogue;
    }

    // =========================================================
    // REAÇÃO A ITEM
    // =========================================================

    [System.Serializable]
    public class ItemReaction
    {
        public ItemData item;
        public DialogueLine[] dialogue;
    }

    // =========================================================
    // UI
    // =========================================================

    [Header("Configuração do NPC")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcPortrait;

    [Header("Diálogo Inicial")]
    [SerializeField] private DialogueLine[] initialDialogue;

    [Header("Opções")]
    [SerializeField] private DialogueOption[] dialogueOptions;

    [Header("Reações")]
    [SerializeField] private ItemReaction[] itemReactions;

    // =========================================================
    // REFERÊNCIAS DA UI (Criadas em tempo de execução)
    // =========================================================

    private GameObject canvasParent;        // Canvas(1) existente
    private GameObject dialogueUI;          // Criado dentro do Canvas(1)
    private GameObject dialoguePanel;       // Criado dentro do DialogueUI
    private GameObject choicesContainer;    // Criado dentro do DialoguePanel
    
    private TMP_Text dialogueText;
    private TMP_Text characterNameText;
    private Image portraitImage;
    private Button[] optionButtons;
    private TMP_Text[] optionTexts;

    // =========================================================
    // CONTROLE
    // =========================================================

    private DialogueLine[] currentDialogue;
    private int currentLine;
    private bool isShowing = false;
    private bool showingOptions = false;
    private static NPCDialogue currentObject = null;
    private TutorialManager tutorialManager;

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        tutorialManager = FindFirstObjectByType<TutorialManager>();

        // ENCONTRA O CANVAS(1) EXISTENTE
        canvasParent = GameObject.Find("Canvas(1)");
        if (canvasParent == null)
        {
            canvasParent = FindFirstObjectByType<Canvas>()?.gameObject;
        }

        if (canvasParent == null)
        {
            Debug.LogError("❌ Nenhum Canvas encontrado! Criando um novo...");
            canvasParent = new GameObject("Canvas");
            Canvas canvas = canvasParent.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasParent.AddComponent<CanvasScaler>();
            canvasParent.AddComponent<GraphicRaycaster>();
        }

        Debug.Log($"📋 Canvas encontrado: {canvasParent.name}");

        // CRIA A UI DENTRO DO CANVAS(1)
        CriarUIDentroDoCanvas();

        // Esconde tudo no início
        HidePanel();

        ConfigureButtons();

        Debug.Log($"✅ NPC {npcName} inicializado!");
    }

    // =========================================================
    // CRIAR UI DENTRO DO CANVAS(1) EXISTENTE
    // =========================================================

    private void CriarUIDentroDoCanvas()
    {
        Debug.Log("🆕 Criando UI dentro do Canvas(1)...");

        // 1. DIALOGUE UI (Container principal)
        dialogueUI = new GameObject("DialogueUI_Criado");
        dialogueUI.transform.SetParent(canvasParent.transform, false);
        
        RectTransform uiRect = dialogueUI.AddComponent<RectTransform>();
        uiRect.anchorMin = new Vector2(0.5f, 0.5f);
        uiRect.anchorMax = new Vector2(0.5f, 0.5f);
        uiRect.sizeDelta = new Vector2(800, 600);
        uiRect.anchoredPosition = Vector2.zero;

        // Fundo semi-transparente
        Image uiBg = dialogueUI.AddComponent<Image>();
        uiBg.color = new Color(0, 0, 0, 0.6f);
        uiBg.raycastTarget = true;

        // 2. DIALOGUE PANEL
        dialoguePanel = new GameObject("DialoguePanel_Criado");
        dialoguePanel.transform.SetParent(dialogueUI.transform, false);
        
        RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.05f, 0.05f);
        panelRect.anchorMax = new Vector2(0.95f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImg = dialoguePanel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.30f, 0.95f);
        panelImg.raycastTarget = true;

        // 3. NOME DO NPC
        GameObject nameObj = new GameObject("CharacterName");
        nameObj.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.85f);
        nameRect.anchorMax = new Vector2(0.5f, 0.95f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        characterNameText = nameObj.AddComponent<TextMeshProUGUI>();
        characterNameText.text = npcName;
        characterNameText.fontSize = 28;
        characterNameText.color = Color.yellow;
        characterNameText.fontStyle = FontStyles.Bold;

        // 4. TEXTO DO DIÁLOGO
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.35f);
        textRect.anchorMax = new Vector2(0.95f, 0.80f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        dialogueText = textObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "Diálogo inicial...";
        dialogueText.fontSize = 24;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.textWrappingMode = TextWrappingModes.Normal;

        // 5. PORTRAIT
        GameObject portraitObj = new GameObject("Portrait");
        portraitObj.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform portraitRect = portraitObj.AddComponent<RectTransform>();
        portraitRect.anchorMin = new Vector2(0.85f, 0.75f);
        portraitRect.anchorMax = new Vector2(0.98f, 0.95f);
        portraitRect.offsetMin = Vector2.zero;
        portraitRect.offsetMax = Vector2.zero;
        
        portraitImage = portraitObj.AddComponent<Image>();
        portraitImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        // 6. CHOICES CONTAINER
        choicesContainer = new GameObject("ChoicesContainer");
        choicesContainer.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform containerRect = choicesContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.05f, 0.05f);
        containerRect.anchorMax = new Vector2(0.95f, 0.30f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        Image containerImg = choicesContainer.AddComponent<Image>();
        containerImg.color = new Color(0, 0, 0, 0.3f);
        
        VerticalLayoutGroup layout = choicesContainer.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        
        ContentSizeFitter fitter = choicesContainer.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 7. BOTÃO FECHAR (X)
        GameObject closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(dialoguePanel.transform, false);
        
        RectTransform closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.92f, 0.92f);
        closeRect.anchorMax = new Vector2(0.98f, 0.98f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;
        
        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        Button closeBtn = closeObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => {
            Debug.Log("❌ Fechando diálogo");
            HidePanel();
        });
        
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "X";
        closeText.fontSize = 20;
        closeText.color = Color.white;
        closeText.alignment = TextAlignmentOptions.Center;

        // 8. CRIA OS BOTÕES DE OPÇÃO
        CriarBotoesOpcao();

        // Esconde o container inicialmente
        choicesContainer.SetActive(false);
        dialogueUI.SetActive(false);

        Debug.Log("✅ UI criada dentro do Canvas(1)!");
    }

    // =========================================================
    // CRIAR BOTÕES DE OPÇÃO
    // =========================================================

    private void CriarBotoesOpcao()
    {
        optionButtons = new Button[4];
        optionTexts = new TMP_Text[4];

        for (int i = 0; i < 4; i++)
        {
            GameObject btnObj = new GameObject($"OptionButton_{i}");
            btnObj.transform.SetParent(choicesContainer.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0, 45);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.5f, 0.7f, 1f);

            Button btn = btnObj.AddComponent<Button>();
            optionButtons[i] = btn;

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);

            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = $"Opção {i + 1}";
            txt.fontSize = 18;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            optionTexts[i] = txt;

            LayoutElement layout = btnObj.AddComponent<LayoutElement>();
            layout.minHeight = 45;

            btnObj.SetActive(false);
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!isShowing) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HidePanel();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject != gameObject)
                {
                    HidePanel();
                }
            }
            else
            {
                HidePanel();
            }
        }
    }

    // =========================================================
    // CLICOU NO NPC
    // =========================================================

    private void OnMouseDown()
    {
        if (PauseHelper.BlockInput()) return;

        if (InventoryManager.Instance != null &&
            InventoryManager.Instance.SelectedItem != null)
        {
            UseSelectedItem();
            return;
        }

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

    // =========================================================
    // DIÁLOGO INICIAL
    // =========================================================

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

    // =========================================================
    // INICIAR DIÁLOGO
    // =========================================================

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

    // =========================================================
    // GARANTIR UI VISÍVEL
    // =========================================================

    private void EnsureUIVisible()
    {
        if (dialogueUI != null) 
        {
            dialogueUI.SetActive(true);
            Debug.Log("✅ DialogueUI ativado");
        }
        
        if (dialoguePanel != null) 
        {
            dialoguePanel.SetActive(true);
            Debug.Log("✅ DialoguePanel ativado");
        }
        
        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
            Debug.Log("✅ ChoicesContainer ativado");
        }
        
        isShowing = true;

        // Força a atualização do Canvas
        if (canvasParent != null)
        {
            canvasParent.SetActive(false);
            canvasParent.SetActive(true);
        }
    }

    // =========================================================
    // MOSTRAR FALA
    // =========================================================

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

        Debug.Log($"💬 {characterNameText.text}: {dialogueText.text}");
    }

    // =========================================================
    // PRÓXIMA FALA
    // =========================================================

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

    // =========================================================
    // MOSTRAR OPÇÕES
    // =========================================================

    private void ShowOptions()
    {
        showingOptions = true;
        currentDialogue = null;
        currentLine = 0;

        EnsureUIVisible();

        if (dialogueText != null)
            dialogueText.text = "O que você gostaria de saber?";

        if (portraitImage != null)
            portraitImage.sprite = npcPortrait;

        if (characterNameText != null)
            characterNameText.text = npcName;

        if (choicesContainer != null)
            choicesContainer.SetActive(true);

        int activeOptions = 0;

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

                activeOptions++;
                Debug.Log($"🔘 Botão {i}: {dialogueOptions[i].optionText}");
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"💬 Mostrando {activeOptions} opções para {npcName}");
    }

    // =========================================================
    // ESCOLHER PERGUNTA
    // =========================================================

    private void SelectOption(int index)
    {
        if (index < 0 || index >= dialogueOptions.Length) return;

        Debug.Log($"🖱️ Opção selecionada: {dialogueOptions[index].optionText}");
        
        if (choicesContainer != null)
            choicesContainer.SetActive(false);
        
        StartDialogue(dialogueOptions[index].dialogue);
    }

    // =========================================================
    // MOSTRAR PAINEL
    // =========================================================

    public void ShowPanel()
    {
        if (tutorialManager != null)
        {
            tutorialManager.HideTutorial();
        }

        EnsureUIVisible();
        Debug.Log($"💬 Diálogo ABERTO: {npcName}");
    }

    // =========================================================
    // FECHAR PAINEL
    // =========================================================

    public void HidePanel()
    {
        if (dialogueUI != null) 
            dialogueUI.SetActive(false);
        
        if (dialoguePanel != null) 
            dialoguePanel.SetActive(false);
        
        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        isShowing = false;
        showingOptions = false;
        currentDialogue = null;
        currentLine = 0;

        HideOptionButtons();

        if (currentObject == this)
            currentObject = null;

        if (tutorialManager != null)
        {
            tutorialManager.ShowTutorial();
        }

        Debug.Log($"🔒 Diálogo FECHADO: {npcName}");
    }

    // =========================================================
    // ESCONDER BOTÕES
    // =========================================================

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

    // =========================================================
    // CONFIGURAR BOTÕES
    // =========================================================

    private void ConfigureButtons()
    {
        foreach (Button button in optionButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }

    // =========================================================
    // USAR ITEM NO NPC
    // =========================================================

    private void UseSelectedItem()
    {
        ItemData selectedItem = InventoryManager.Instance.SelectedItem;
        if (selectedItem == null) return;

        foreach (ItemReaction reaction in itemReactions)
        {
            if (reaction.item == selectedItem)
            {
                Debug.Log($"🔑 Usando {selectedItem.itemName} em {gameObject.name}");

                if (!isShowing)
                {
                    if (currentObject != null && currentObject != this)
                        currentObject.HidePanel();

                    ShowPanel();
                    currentObject = this;
                }

                StartDialogue(reaction.dialogue);
                InventoryManager.Instance.Deselect();
                return;
            }
        }

        Debug.Log($"❌ {gameObject.name} não possui reação para {selectedItem.itemName}");
        InventoryManager.Instance.Deselect();
    }

    // =========================================================
    // CLICK DETECTOR DO PAINEL
    // =========================================================

    private void AddClickDetector()
    {
        if (dialoguePanel == null) return;

        PanelClickHandler detector = dialoguePanel.GetComponent<PanelClickHandler>();

        if (detector == null)
        {
            detector = dialoguePanel.AddComponent<PanelClickHandler>();
        }

        detector.SetExamineObject(this);
    }

    // =========================================================
    // STATUS
    // =========================================================

    public static bool IsShowing()
    {
        return currentObject != null && currentObject.isShowing;
    }

    // =========================================================
    // CURSOR
    // =========================================================

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput()) return;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (dialoguePanel != null)
        {
            PanelClickHandler detector = dialoguePanel.GetComponent<PanelClickHandler>();
            if (detector != null) Destroy(detector);
        }

        if (currentObject == this) currentObject = null;
    }
}