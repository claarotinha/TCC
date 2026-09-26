using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombinationPrompt : MonoBehaviour
{
    [SerializeField] private GameObject combinePanel;
    [SerializeField] private TMP_Text combineText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private ItemCombination[] recipes;

    private ItemData first;
    private ItemData second;
    private ItemCombination selectedRecipe;

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

        foreach (ItemCombination recipe in recipes)
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

        Close();
    }

    private void Close()
    {
        InvestigationGuard.BlockCurrentClick();
        combinePanel.SetActive(false);
        first = null;
        second = null;
        selectedRecipe = null;
    }
}
