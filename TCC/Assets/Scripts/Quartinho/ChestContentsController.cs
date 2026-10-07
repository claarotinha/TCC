using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChestContentsController : MonoBehaviour
{
    public static ChestContentsController Instance { get; private set; }
    public static bool IsOpen =>
        Instance != null && Instance.PanelActive(Instance.contentsPanel);

    [Header("Conteúdo do baú")]
    [SerializeField] private GameObject contentsPanel;
    [SerializeField] private Button closeContentsButton;

    [Header("Objetos: 0 = diário, 1 e 2 = outros objetos")]
    [SerializeField] private Button[] itemButtons = new Button[3];
    [SerializeField] private Sprite[] itemSprites = new Sprite[3];
    [TextArea(3, 6)]
    [SerializeField] private string[] descriptions = new string[3];

    [Header("Descrição embaixo, dentro do mesmo painel")]
    [SerializeField] private GameObject itemPanel;
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Button collectDiaryButton;

    [Header("Confirmação antiga (mantida fechada)")]
    [SerializeField] private GameObject diaryConfirmPanel;

    private bool configured;
    private bool diaryCollectedThisSession;
    private int selectedItem = -1;
    private UnityEngine.Events.UnityAction[] examineActions;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    private bool DiaryCollected =>
        diaryCollectedThisSession ||
        (GameProgress.Instance != null && GameProgress.Instance.Data.diaryCollected);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Existe mais de um ChestContentsController na cena.", this);
            enabled = false;
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HidePanels();
        configured = ValidateReferences();
        if (!configured) return;

        examineActions = new UnityEngine.Events.UnityAction[3];
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            examineActions[i] = () => ExamineItem(index);
            itemButtons[i].onClick.AddListener(examineActions[i]);
            Image image = itemButtons[i].GetComponent<Image>();
            if (image != null && itemSprites[i] != null)
            {
                image.sprite = itemSprites[i];
                image.color = Color.white;
                image.preserveAspect = true;
            }
        }
        closeContentsButton.onClick.AddListener(Close);
        collectDiaryButton.onClick.AddListener(ConfirmDiary);
        RefreshDiary();
    }

    private bool ValidateReferences()
    {
        if (contentsPanel == null || closeContentsButton == null ||
            itemPanel == null || itemText == null || collectDiaryButton == null ||
            itemButtons == null || itemButtons.Length != 3 ||
            itemSprites == null || itemSprites.Length != 3 ||
            descriptions == null || descriptions.Length != 3)
        {
            Debug.LogError("ChestContentsController: configure os painéis, texto, botões e as três listas com Size 3.", this);
            return false;
        }
        for (int i = 0; i < 3; i++)
        {
            if (itemButtons[i] == null)
            {
                Debug.LogError("ChestContentsController: falta o botão do objeto " + i + ".", this);
                return false;
            }
        }
        if (!itemPanel.transform.IsChildOf(contentsPanel.transform))
        {
            Debug.LogError("ChestContentsController: a descrição deve ficar dentro de ChestContentsPanel.", this);
            return false;
        }
        return true;
    }

    private bool PanelActive(GameObject panel) =>
        panel != null && panel.activeInHierarchy;

    // Usa o primeiro elemento atingido pela UI, respeitando painéis sobrepostos.
    public bool PointerOverItem()
    {
        if (!configured || !PanelActive(contentsPanel) ||
            UniversalPauseManager.IsPaused || EventSystem.current == null)
            return false;

        uiHits.Clear();
        EventSystem.current.RaycastAll(
            new PointerEventData(EventSystem.current) { position = Input.mousePosition },
            uiHits);
        if (uiHits.Count == 0) return false;

        Button hitButton = uiHits[0].gameObject.GetComponentInParent<Button>();
        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (hitButton == itemButtons[i] && hitButton != null &&
                hitButton.isActiveAndEnabled && hitButton.interactable)
                return true;
        }
        return false;
    }

    public void Open()
    {
        if (!configured) return;
        selectedItem = -1;
        if (diaryConfirmPanel != null)
            diaryConfirmPanel.SetActive(false);
        RefreshDiary();
        itemText.text = "Clique em um objeto para examiná-lo.";
        collectDiaryButton.gameObject.SetActive(false);
        itemPanel.SetActive(true);
        FitPanelToCanvas();
        contentsPanel.transform.SetAsLastSibling();
        contentsPanel.SetActive(true);
        BlockClick();
    }

    private void ExamineItem(int index)
    {
        if (!PanelActive(contentsPanel) || UniversalPauseManager.IsPaused ||
            index < 0 || index >= itemButtons.Length ||
            (index == 0 && DiaryCollected))
            return;

        selectedItem = index;
        itemText.text = descriptions[index];
        collectDiaryButton.gameObject.SetActive(index == 0);
        collectDiaryButton.interactable = true;
        // A imagem do baú e os objetos continuam visíveis.
        itemPanel.SetActive(true);
        BlockClick();
    }

    private void ConfirmDiary()
    {
        if (selectedItem != 0 || !PanelActive(contentsPanel) ||
            UniversalPauseManager.IsPaused || DiaryCollected)
            return;
        if (GameProgress.Instance == null)
        {
            Debug.LogError("Não foi possível guardar o diário: GameProgress não está presente.", this);
            return;
        }

        // Coleta direta: diário disponível na pausa, sem entrar no inventário.
        GameProgress.Instance.MarkDiaryCollected();
        diaryCollectedThisSession = true;
        selectedItem = -1;
        collectDiaryButton.gameObject.SetActive(false);
        itemText.text = "Guardei o diário. Posso consultá-lo pelo menu de pausa.";
        RefreshDiary();
        BlockClick();
    }

    private void RefreshDiary() =>
        itemButtons[0].gameObject.SetActive(!DiaryCollected);

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

    private void FitPanelToCanvas()
    {
        RectTransform panel = contentsPanel.transform as RectTransform;
        RectTransform parent = panel != null ? panel.parent as RectTransform : null;
        if (parent == null || panel.rect.width <= 0f || panel.rect.height <= 0f)
            return;

        float scale = Mathf.Min(1f,
            (parent.rect.width - 24f) / panel.rect.width,
            (parent.rect.height - 24f) / panel.rect.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
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
            collectDiaryButton.onClick.RemoveListener(ConfirmDiary);
        }
        if (Instance == this)
            Instance = null;
    }
}
