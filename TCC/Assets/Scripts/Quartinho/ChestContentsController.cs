using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestContentsController : MonoBehaviour
{
    public static ChestContentsController Instance { get; private set; }

    public static bool IsOpen =>
        Instance != null &&
        (Instance.PanelActive(Instance.contentsPanel) ||
         Instance.PanelActive(Instance.itemPanel));

    [Header("Conteúdo do baú")]
    [SerializeField] private GameObject contentsPanel;
    [SerializeField] private Button closeContentsButton;

    [Header("Objetos: 0 = diário, 1 e 2 = outros objetos")]
    [SerializeField] private Button[] itemButtons = new Button[3];
    [SerializeField] private Sprite[] itemSprites = new Sprite[3];

    [TextArea(3, 6)]
    [SerializeField] private string[] descriptions = new string[3];

    [Header("Investigação do objeto")]
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button collectDiaryButton;

    [Header("Painel antigo (mantido fechado)")]
    [SerializeField] private GameObject diaryConfirmPanel;

    private bool configured;
    private bool diaryCollectedThisSession;
    private int selectedItem = -1;
    private UnityEngine.Events.UnityAction[] examineActions;

    private bool DiaryCollected =>
        diaryCollectedThisSession ||
        (GameProgress.Instance != null &&
         GameProgress.Instance.Data.diaryCollected);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Existe mais de um ChestContentsController na cena.",
                this
            );
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HidePanels();
        configured = ValidateReferences();

        if (!configured)
            return;

        examineActions = new UnityEngine.Events.UnityAction[3];

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            examineActions[i] = () => ExamineItem(index);
            itemButtons[i].onClick.AddListener(examineActions[i]);
        }

        closeContentsButton.onClick.AddListener(Close);
        backButton.onClick.AddListener(BackToContents);
        collectDiaryButton.onClick.AddListener(ConfirmDiary);

        RefreshDiary();
    }

    private bool ValidateReferences()
    {
        if (contentsPanel == null ||
            closeContentsButton == null ||
            itemPanel == null ||
            itemImage == null ||
            itemText == null ||
            backButton == null ||
            collectDiaryButton == null ||
            itemButtons == null || itemButtons.Length != 3 ||
            itemSprites == null || itemSprites.Length != 3 ||
            descriptions == null || descriptions.Length != 3)
        {
            Debug.LogError(
                "ChestContentsController: preencha os campos " +
                "e deixe as três listas com Size 3.",
                this
            );
            return false;
        }

        for (int i = 0; i < 3; i++)
        {
            if (itemButtons[i] == null)
            {
                Debug.LogError(
                    "ChestContentsController: falta o botão " +
                    "do objeto " + i + ".",
                    this
                );
                return false;
            }
        }

        return true;
    }

    private bool PanelActive(GameObject panel)
    {
        return panel != null && panel.activeInHierarchy;
    }

    // Chamado pelo baú após a senha ou ao clicar nele destrancado.
    public void Open()
    {
        if (!configured)
            return;

        HidePanels();
        selectedItem = -1;
        RefreshDiary();

        contentsPanel.transform.SetAsLastSibling();
        contentsPanel.SetActive(true);
        BlockClick();
    }

    private void ExamineItem(int index)
    {
        if (!PanelActive(contentsPanel))
            return;

        if (index == 0 && DiaryCollected)
            return;

        selectedItem = index;

        itemImage.sprite = itemSprites[index];
        itemImage.enabled = itemSprites[index] != null;
        itemImage.preserveAspect = true;
        itemText.text = descriptions[index];

        if (diaryConfirmPanel != null)
            diaryConfirmPanel.SetActive(false);
        collectDiaryButton.gameObject.SetActive(index == 0);
        backButton.interactable = true;

        contentsPanel.SetActive(false);
        itemPanel.transform.SetAsLastSibling();
        itemPanel.SetActive(true);

        BlockClick();
    }

    private void BackToContents()
    {
        Open();
    }

    private void ConfirmDiary()
    {
        if (selectedItem != 0 ||
            !PanelActive(itemPanel) ||
            DiaryCollected)
            return;

        if (GameProgress.Instance == null)
        {
            Debug.LogError(
                "Não foi possível guardar o diário: " +
                "GameProgress não está presente.",
                this
            );
            return;
        }

        // O diário não passa pelo InventoryManager.
        GameProgress.Instance.MarkDiaryCollected();
        diaryCollectedThisSession = true;

        if (diaryConfirmPanel != null)
            diaryConfirmPanel.SetActive(false);
        collectDiaryButton.gameObject.SetActive(false);
        collectDiaryButton.interactable = true;
        backButton.interactable = true;

        itemText.text =
            "Guardei o diário. Posso consultá-lo pelo menu de pausa.";

        RefreshDiary();
        BlockClick();
    }

    private void RefreshDiary()
    {
        itemButtons[0].gameObject.SetActive(!DiaryCollected);
    }

    public void Close()
    {
        HidePanels();
        selectedItem = -1;
        BlockClick();
    }

    private void HidePanels()
    {
        if (diaryConfirmPanel != null)
            diaryConfirmPanel.SetActive(false);

        if (itemPanel != null)
            itemPanel.SetActive(false);

        if (contentsPanel != null)
            contentsPanel.SetActive(false);
    }

    private void BlockClick()
    {
        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void OnDestroy()
    {
        if (configured)
        {
            for (int i = 0; i < 3; i++)
            {
                if (itemButtons[i] != null)
                    itemButtons[i].onClick.RemoveListener(examineActions[i]);
            }

            closeContentsButton.onClick.RemoveListener(Close);
            backButton.onClick.RemoveListener(BackToContents);
            collectDiaryButton.onClick.RemoveListener(ConfirmDiary);
        }

        if (Instance == this)
            Instance = null;
    }
}
