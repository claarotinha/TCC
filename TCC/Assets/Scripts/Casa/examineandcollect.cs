using UnityEngine;
using TMPro;

public class CollectableExamine : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;
    
    [SerializeField] private ItemData itemData;
    [TextArea]
    public string message;

    private bool isShowing = false;
    private static CollectableExamine currentObject = null;

    void Start()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
            AddClickDetector();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Verifica se clicou em UM objeto
            if (hit.collider != null)
            {
                // Se clicou neste objeto
                if (hit.collider.gameObject == gameObject)
                {
                    // Se o painel está mostrando, fecha E coleta
                    if (isShowing && currentObject == this)
                    {
                        HidePanel();
                        CollectItem();
                    }
                    else
                    {
                        // Se outro objeto está mostrando, fecha ele
                        if (currentObject != null && currentObject != this)
                        {
                            currentObject.HidePanel();
                        }
                        
                        ShowPanel();
                        currentObject = this;
                    }
                }
                // Se clicou em OUTRO objeto (não neste)
                else
                {
                    // Fecha o painel se estiver aberto
                    if (isShowing && currentObject == this)
                    {
                        HidePanel();
                        currentObject = null;
                    }
                }
            }
            // Se clicou no VAZIO (nenhum objeto)
            else
            {
                // Fecha o painel se estiver aberto
                if (isShowing && currentObject == this)
                {
                    HidePanel();
                    currentObject = null;
                }
            }
        }
    }

    void OnMouseEnter()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    void AddClickDetector()
    {
        if (examinePanel == null) return;

        PanelClickHandler detector = examinePanel.GetComponent<PanelClickHandler>();
        if (detector == null)
        {
            detector = examinePanel.AddComponent<PanelClickHandler>();
        }
        detector.SetExamineObject(this);
    }

    public void ShowPanel()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(true);
            Debug.Log("📖 Painel ABERTO: " + gameObject.name);
        }

        if (examineText != null)
        {
            examineText.text = message;
        }

        isShowing = true;
    }

    public void HidePanel()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
            Debug.Log("🔒 Painel FECHADO: " + gameObject.name);
        }

        isShowing = false;

        if (currentObject == this)
            currentObject = null;
    }

    private void CollectItem()
    {
        if (itemData == null)
        {
            Debug.LogWarning("⚠ ItemData não atribuído em: " + gameObject.name);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("❌ InventoryManager não existe!");
            return;
        }

        InventoryManager.Instance.AddItem(itemData);
        Debug.Log("✅ " + itemData.itemName + " coletado com sucesso!");
        
        Destroy(gameObject);
    }

    public static bool IsShowing()
    {
        if (currentObject != null && currentObject.examinePanel != null)
        {
            return currentObject.examinePanel.activeSelf;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (examinePanel != null)
        {
            PanelClickHandler detector = examinePanel.GetComponent<PanelClickHandler>();
            if (detector != null)
            {
                Destroy(detector);
            }
        }
        if (currentObject == this)
        {
            currentObject = null;
        }
    }
}