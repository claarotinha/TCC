using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestCodePanelController : MonoBehaviour
{
    public static ChestCodePanelController Instance { get; private set; }

    public static bool IsOpen =>
        Instance != null &&
        Instance.codePanel != null &&
        Instance.codePanel.activeInHierarchy;

    [Header("Painel")]
    [SerializeField] private GameObject codePanel;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Dígitos — preencher da esquerda para a direita")]
    [SerializeField] private TMP_Text[] digitTexts = new TMP_Text[5];
    [SerializeField] private Button[] upButtons = new Button[5];
    [SerializeField] private Button[] downButtons = new Button[5];

    [Header("Botões principais")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private const string CorrectCode = "10124";
    private readonly int[] digits = new int[5];

    private UnityEngine.Events.UnityAction[] upActions;
    private UnityEngine.Events.UnityAction[] downActions;

    private bool unlockedThisSession;
    public bool Unlocked => GameProgress.Instance != null
        ? GameProgress.Instance.Data.chestOpened
        : unlockedThisSession;
    public event System.Action ChestUnlocked;

    private bool configured;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Existe mais de um ChestCodePanelController na cena.",
                this
            );
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (codePanel != null)
            codePanel.SetActive(false);

        configured = ValidateReferences();

        if (!configured)
            return;

        upActions = new UnityEngine.Events.UnityAction[5];
        downActions = new UnityEngine.Events.UnityAction[5];

        for (int i = 0; i < 5; i++)
        {
            int index = i;

            upActions[i] = () => ChangeDigit(index, 1);
            downActions[i] = () => ChangeDigit(index, -1);

            upButtons[i].onClick.AddListener(upActions[i]);
            downButtons[i].onClick.AddListener(downActions[i]);
        }

        confirmButton.onClick.AddListener(TryCode);
        cancelButton.onClick.AddListener(Close);

        ResetDigits();
    }

    private bool ValidateReferences()
    {
        if (codePanel == null ||
            feedbackText == null ||
            confirmButton == null ||
            cancelButton == null ||
            digitTexts == null || digitTexts.Length != 5 ||
            upButtons == null || upButtons.Length != 5 ||
            downButtons == null || downButtons.Length != 5)
        {
            Debug.LogError(
                "ChestCodePanelController: preencha todos os campos. " +
                "Cada lista de dígitos precisa ter exatamente 5 elementos.",
                this
            );
            return false;
        }

        for (int i = 0; i < 5; i++)
        {
            if (digitTexts[i] == null ||
                upButtons[i] == null ||
                downButtons[i] == null)
            {
                Debug.LogError(
                    "ChestCodePanelController: faltam referências " +
                    "no dígito " + (i + 1) + ".",
                    this
                );
                return false;
            }
        }

        return true;
    }

    public void Open()
    {
        if (!configured || Unlocked || IsOpen ||
            InvestigationGuard.Blocked)
            return;

        ResetDigits();

        codePanel.transform.SetAsLastSibling();
        codePanel.SetActive(true);

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void ResetDigits()
    {
        for (int i = 0; i < 5; i++)
        {
            digits[i] = 0;
            digitTexts[i].text = "0";
        }

        feedbackText.text = "";
    }

    private void ChangeDigit(int index, int amount)
    {
        if (!IsOpen || Unlocked)
            return;

        // Depois de 9 vem 0. Antes de 0 vem 9.
        digits[index] = (digits[index] + amount + 10) % 10;
        digitTexts[index].text = digits[index].ToString();

        feedbackText.text = "";
    }

    private void TryCode()
    {
        if (!IsOpen || Unlocked)
            return;

        string enteredCode = "";

        for (int i = 0; i < 5; i++)
            enteredCode += digits[i].ToString();

        if (enteredCode != CorrectCode)
        {
            feedbackText.text =
                "Essa combinação não abriu o baú. Vou tentar outra.";
            return;
        }

        unlockedThisSession = true;
        GameProgress.Instance?.MarkChestOpened();
        Close();

        Debug.Log("Baú destrancado.", this);
        ChestUnlocked?.Invoke();
    }

    public void Close()
    {
        if (codePanel != null)
            codePanel.SetActive(false);

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    // Permite abrir pelo menu do componente para testar na Unity.
    [ContextMenu("Testar painel")]
    private void TestPanel()
    {
        if (Application.isPlaying)
            Open();
    }

    private void OnDestroy()
    {
        if (configured)
        {
            for (int i = 0; i < 5; i++)
            {
                if (upButtons[i] != null)
                    upButtons[i].onClick.RemoveListener(upActions[i]);

                if (downButtons[i] != null)
                    downButtons[i].onClick.RemoveListener(downActions[i]);
            }

            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(TryCode);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(Close);
        }

        if (Instance == this)
            Instance = null;
    }
}
