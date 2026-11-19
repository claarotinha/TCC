using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectPrompt : MonoBehaviour
{
    public bool IsOpen => (collectPanel != null && collectPanel.activeInHierarchy) ||
        (examinationPanel != null && examinationPanel.activeInHierarchy);

    [SerializeField] private GameObject collectPanel;
    [Header("Examine — mesmo painel inferior das cenas")]
    [SerializeField] private GameObject examinationPanel;
    [SerializeField] private TMP_Text examinationText;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text collectText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private readonly HashSet<ItemData> examinedItems = new HashSet<ItemData>();
    private bool examining;
    private int openedFrame;
    private ItemData pendingItem;
    private Action onCollected;
    private Sprite previewSprite;

    private void Awake()
    {
        if (collectPanel != null) collectPanel.SetActive(false);
        if (examinationPanel != null) examinationPanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool Show(ItemData item, Action afterCollect = null, bool alreadyExamined = false)
    {
        if (item == null || collectPanel == null ||
            itemImage == null || collectText == null ||
            yesButton == null || noButton == null)
        {
            Debug.LogError("CollectPrompt: faltam referências no Inspector.");
            return false;
        }

        if (IsOpen) return false;

        pendingItem = item;
        onCollected = afterCollect;

        SetItemVisible(item.icon != null);
        itemImage.color = Color.white;
        SetPreview(item.icon);
        itemImage.enabled = item.icon != null;
        itemImage.preserveAspect = true;

        collectText.text = "Você deseja coletar " + item.itemName + "?";

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);

        examining = !alreadyExamined && !examinedItems.Contains(item);
        if (examining)
        {
            SetItemVisible(false);
            string description = string.IsNullOrWhiteSpace(item.description) ? item.itemName : item.description;
            if (examinationPanel != null && examinationText != null)
                examinationText.text = description;
            else collectText.text = description; // Compatibility with an older prefab.
            yesButton.gameObject.SetActive(false);
            noButton.gameObject.SetActive(false);
        }
        else
        {
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);
        }
        openedFrame = Time.frameCount;
        bool separateExamine = examining && examinationPanel != null && examinationText != null;
        collectPanel.SetActive(!separateExamine);
        if (examinationPanel != null) examinationPanel.SetActive(separateExamine);
        return true;
    }

    private void SetPreview(Sprite icon)
    {
        ReleasePreview();
        itemImage.sprite = icon;
        if (icon == null) return;
        Vector2[] vertices = icon.vertices;
        if (vertices == null || vertices.Length == 0) return;
        Vector2 min = vertices[0], max = vertices[0];
        foreach (Vector2 vertex in vertices) { min = Vector2.Min(min, vertex); max = Vector2.Max(max, vertex); }
        float left = Mathf.Clamp(Mathf.Floor(min.x * icon.pixelsPerUnit + icon.pivot.x), 0f, icon.rect.width);
        float bottom = Mathf.Clamp(Mathf.Floor(min.y * icon.pixelsPerUnit + icon.pivot.y), 0f, icon.rect.height);
        float right = Mathf.Clamp(Mathf.Ceil(max.x * icon.pixelsPerUnit + icon.pivot.x), left, icon.rect.width);
        float top = Mathf.Clamp(Mathf.Ceil(max.y * icon.pixelsPerUnit + icon.pivot.y), bottom, icon.rect.height);
        if (right <= left || top <= bottom) return;
        // Retira apenas a borda transparente da prévia; o asset do cenário permanece igual.
        previewSprite = Sprite.Create(icon.texture,
            new Rect(icon.rect.x + left, icon.rect.y + bottom, right - left, top - bottom),
            new Vector2(.5f, .5f), icon.pixelsPerUnit);
        itemImage.sprite = previewSprite;
    }

    private void ReleasePreview()
    {
        if (previewSprite != null) Destroy(previewSprite);
        previewSprite = null;
    }

    private void OnDestroy() => ReleasePreview();

    private void Update()
    {
        if (!examining || !IsOpen || PauseHelper.BlockInput() || Time.frameCount <= openedFrame ||
            !Input.GetMouseButtonDown(0)) return;
        examinedItems.Add(pendingItem);
        examining = false;
        if (examinationPanel != null) examinationPanel.SetActive(false);
        collectPanel.SetActive(true);
        SetItemVisible(pendingItem != null && pendingItem.icon != null);
        collectText.text = "Você deseja coletar " + pendingItem.itemName + "?";
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        InvestigationGuard.BlockCurrentClick();
    }

    private void SetItemVisible(bool visible)
    {
        // A moldura é irmã do texto e dos botões; não deve sobrar como quadrado vazio.
        Transform moldura = itemImage.transform.parent;
        if (moldura != null && moldura != collectPanel.transform && moldura.IsChildOf(collectPanel.transform))
            moldura.gameObject.SetActive(visible);
        itemImage.gameObject.SetActive(visible);
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
        ReleasePreview();
        examining = false;
        InvestigationGuard.BlockCurrentClick();
        if (collectPanel != null) collectPanel.SetActive(false);
        if (examinationPanel != null) examinationPanel.SetActive(false);

        if (yesButton != null)
            yesButton.onClick.RemoveAllListeners();

        if (noButton != null)
            noButton.onClick.RemoveAllListeners();

        pendingItem = null;
        onCollected = null;
    }
}

