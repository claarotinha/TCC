using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExamineAndCollect : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;
    public Button closeButton;
    
    [SerializeField] private ItemData itemData;
    [TextArea]
    public string message;

    private bool isShowing = false;
    private static ExamineAndCollect currentExaminingObject = null;

    void Start()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnButtonClick);
        }
    }

    void OnMouseDown()
    {
        if (currentExaminingObject != null && currentExaminingObject != this)
        {
            currentExaminingObject.HidePanel();
        }

        if (!isShowing || currentExaminingObject != this)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
            CollectItem();
        }
    }

    public void OnButtonClick()
    {
        if (isShowing && currentExaminingObject == this)
        {
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(true);
        }
        if (examineText != null)
        {
            examineText.text = message;
        }
        isShowing = true;
        currentExaminingObject = this;
        Debug.Log("📖 Painel aberto: " + gameObject.name);
    }

    public void HidePanel()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
        }
        isShowing = false;
        if (currentExaminingObject == this)
        {
            currentExaminingObject = null;
        }
        Debug.Log("🔒 Painel fechado: " + gameObject.name);
    }

    private void CollectItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("⚠ ItemData não atribuído!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("❌ InventoryManager não existe!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);
        Debug.Log("✅ " + itemData.itemName + " coletado!");
        Destroy(gameObject);
    }

    // CORRIGIDO: Agora verifica se o painel está ativo
    public static bool IsShowing()
    {
        // Verifica se o currentExaminingObject existe e se o painel dele está ativo
        if (currentExaminingObject != null && currentExaminingObject.examinePanel != null)
        {
            return currentExaminingObject.examinePanel.activeSelf;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnButtonClick);
        }
        if (currentExaminingObject == this)
        {
            currentExaminingObject = null;
        }
    }
}