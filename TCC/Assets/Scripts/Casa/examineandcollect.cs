using UnityEngine;
using TMPro;

public class CollectableExamine : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;
    
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool isTutorialBall;
    [TextArea]
    public string message;

    private bool isShowing = false;
    private static CollectableExamine currentObject = null;
    private Collider2D myCollider;

    void Start()
    {
        myCollider = GetComponent<Collider2D>();
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
            TutorialManager tutorial = isTutorialBall ? TutorialManager.Instance : null;
            if (tutorial != null && tutorial.CurrentStep <= TutorialManager.TutorialStep.Run)
            {
#if UNITY_EDITOR
                if (Camera.main != null && myCollider != null &&
                    myCollider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
                    Debug.Log("Bola: investigue após aprender A/D e Shift.", this);
#endif
                return;
            }

            // O segundo clique no próprio objeto ainda confirma a coleta;
            // outros objetos ficam bloqueados enquanto o painel está aberto.
            if (InvestigationGuard.Blocked && !(isShowing && currentObject == this))
                return;

            if (Camera.main == null) return;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            // A bola pode estar na frente de um collider do cenário.
            bool clickedThis = isTutorialBall
                ? myCollider != null && myCollider.OverlapPoint(mousePos)
                : hit.collider != null && hit.collider.gameObject == gameObject;

            // Verifica se clicou em UM objeto
            if (hit.collider != null || clickedThis)
            {
                // Se clicou neste objeto
                if (clickedThis)
                {
                    // Antes da pista dos garotos, a bola pode ser investigada,
                    // mas não pode entrar no inventário.
                    if (isShowing && currentObject == this)
                    {
                        HidePanel();
                        if (!isTutorialBall ||
                            (tutorial != null &&
                             tutorial.CurrentStep == TutorialManager.TutorialStep.FindBall))
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
                        if (tutorial != null)
                            tutorial.ReportInvestigation(gameObject);
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

        InvestigationGuard.BlockCurrentClick();
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
        if (isTutorialBall && TutorialManager.Instance != null)
            TutorialManager.Instance.ReportBallCollected();
        
        Destroy(gameObject);
    }

    public static bool IsShowing()
    {
        if (currentObject != null && currentObject.examinePanel != null)
        {
            return currentObject.examinePanel.activeInHierarchy;
        }
        return false;
    }

    public static void HideCurrentPanel()
    {
        if (currentObject != null)
            currentObject.HidePanel();
    }

    private void OnDestroy()
    {
        // O painel de mensagens é compartilhado entre os objetos da cena.
        if (currentObject == this)
        {
            currentObject = null;
        }
    }
}
