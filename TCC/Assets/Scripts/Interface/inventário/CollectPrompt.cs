using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectPrompt : MonoBehaviour
{
    public bool IsOpen => collectPanel != null && collectPanel.activeInHierarchy;

    [SerializeField] private GameObject collectPanel;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text collectText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private ItemData pendingItem;
    private Action onCollected;

    private void Awake()
    {
        if (collectPanel != null)
            collectPanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool Show(ItemData item, Action afterCollect = null)
    {
        if (item == null || collectPanel == null ||
            itemImage == null || collectText == null ||
            yesButton == null || noButton == null)
        {
            Debug.LogError("CollectPrompt: faltam referências no Inspector.");
            return false;
        }

        if (collectPanel.activeSelf)
            return false;

        pendingItem = item;
        onCollected = afterCollect;

        itemImage.sprite = item.icon;
        itemImage.enabled = item.icon != null;
        itemImage.preserveAspect = true;

        collectText.text = "Você deseja coletar " + item.itemName + "?";

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);

        collectPanel.SetActive(true);
        return true;
    }

    private void Confirm()
    {
        if (pendingItem == null || InventoryManager.Instance == null)
        {
            Debug.LogError("CollectPrompt: item ou InventoryManager não encontrado.");
            return;
        }

        InventoryManager.Instance.AddItem(pendingItem);

        Action callback = onCollected;
        Close();
        callback?.Invoke();
    }

    private void Cancel()
    {
        Close();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Close();
    }

    private void Close()
    {
        InvestigationGuard.BlockCurrentClick();
        if (collectPanel != null)
            collectPanel.SetActive(false);

        if (yesButton != null)
            yesButton.onClick.RemoveAllListeners();

        if (noButton != null)
            noButton.onClick.RemoveAllListeners();

        pendingItem = null;
        onCollected = null;
    }
}
