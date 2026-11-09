using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CombinationPrompt : MonoBehaviour
{
    public bool IsOpen => combinePanel != null && combinePanel.activeInHierarchy;

    [SerializeField] private GameObject combinePanel;
    [SerializeField] private TMP_Text combineText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private ItemCombination[] recipes;

    [Header("Ajuda da primeira combinação")]
    [SerializeField] private TMP_Text combinationHint;
    [SerializeField] private GameObject combinationHintPanel;
    [SerializeField] private TMP_Text estradaCombinationHint;
    private bool combinationLearned;
    private GameProgress teachingProgress;
    private string teachingScene;

    private ItemData first;
    private ItemData second;
    private ItemCombination selectedRecipe;

    private void Awake()
    {
        Close();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Close();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single) Close();
    }

    private void Update()
    {
        if (combinationHint == null && estradaCombinationHint == null) return;
        if (teachingProgress != GameProgress.Instance)
        {
            teachingProgress = GameProgress.Instance;
            combinationLearned = false;
        }
        string activeScene = SceneManager.GetActiveScene().name;
        if (teachingScene != activeScene)
        {
            teachingScene = activeScene;
            if (activeScene == "Estrada") combinationLearned = false;
        }
        bool visible = !combinationLearned && !IsOpen && !PauseHelper.BlockInput() &&
            InventoryTabController.Instance != null && InventoryTabController.Instance.IsOpen &&
            HasAvailableRecipe();
        SetCombinationHint(visible);
    }

    private void SetCombinationHint(bool visible)
    {
        GameObject panel = combinationHintPanel != null ? combinationHintPanel :
            (combinationHint != null ? combinationHint.gameObject : null);
        bool estrada = SceneManager.GetActiveScene().name == "Estrada" &&
            estradaCombinationHint != null;
        bool showPanel = visible && !estrada;
        if (panel != null && panel.activeSelf != showPanel) panel.SetActive(showPanel);
        if (estradaCombinationHint != null)
        {
            bool showText = visible && estrada;
            if (estradaCombinationHint.gameObject.activeSelf != showText)
                estradaCombinationHint.gameObject.SetActive(showText);
        }
    }

    private bool HasAvailableRecipe()
    {
        if (InventoryManager.Instance == null || recipes == null) return false;
        foreach (ItemCombination recipe in recipes ?? System.Array.Empty<ItemCombination>())
        {
            if (recipe == null || recipe.itemA == null || recipe.itemB == null) continue;
            int countA = 0, countB = 0;
            foreach (ItemData item in InventoryManager.Instance.Items)
            {
                if (item == recipe.itemA) countA++;
                if (item == recipe.itemB) countB++;
            }
            if (recipe.itemA == recipe.itemB ? countA >= 2 : countA > 0 && countB > 0)
                return true;
        }
        return false;
    }

    public void Show(ItemData firstItem, ItemData secondItem)
    {
        if (combinePanel == null || combineText == null ||
            yesButton == null || noButton == null ||
            firstItem == null || secondItem == null ||
            combinePanel.activeSelf)
            return;

        first = firstItem;
        second = secondItem;
        selectedRecipe = null;

        foreach (ItemCombination recipe in recipes ?? System.Array.Empty<ItemCombination>())
        {
            if (recipe == null) continue;

            bool matches =
                (recipe.itemA == first && recipe.itemB == second) ||
                (recipe.itemA == second && recipe.itemB == first);

            if (matches)
            {
                selectedRecipe = recipe;
                break;
            }
        }

        combineText.text = "Você deseja combinar estes itens?";
        yesButton.gameObject.SetActive(true);

        TMP_Text noLabel = noButton.GetComponentInChildren<TMP_Text>();
        if (noLabel != null) noLabel.text = "Não";

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Close);

        combinePanel.SetActive(true);
    }

    private void Confirm()
    {
        if (selectedRecipe == null ||
            InventoryManager.Instance == null ||
            !InventoryManager.Instance.TryCombine(first, second, selectedRecipe))
        {
            combineText.text = "Estes itens não podem ser combinados.";
            yesButton.gameObject.SetActive(false);

            TMP_Text noLabel = noButton.GetComponentInChildren<TMP_Text>();
            if (noLabel != null) noLabel.text = "Fechar";
            return;
        }

        combinationLearned = true;
        SetCombinationHint(false);
        Close();
    }

    private void Close()
    {
        if (IsOpen) InvestigationGuard.BlockCurrentClick();
        if (combinePanel != null) combinePanel.SetActive(false);
        first = null;
        second = null;
        selectedRecipe = null;
    }
}
