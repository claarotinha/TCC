using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExamineObject : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;

    [TextArea]
    public string message;

    private static bool isShowing = false;
    private static ExamineObject currentObject = null;

    void Start()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);

            // ADICIONA O ClosePanelOnClick AUTOMATICAMENTE
            AddClickDetector();
        }
    }

    void Update()
    {
        // NÃO PERMITE INTERAÇÃO SE O JOGO ESTIVER PAUSADO
        if (PauseManager.IsPaused)
            return;

        // Clique em 2D
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // Clicou no objeto
                if (currentObject != null && currentObject != this)
                {
                    currentObject.HidePanel();
                }

                if (!isShowing)
                {
                    ShowPanel();
                    currentObject = this;
                }
                else
                {
                    HidePanel();
                    currentObject = null;
                }
            }
        }
    }

    void OnMouseEnter()
    {
        // NÃO MUDA O CURSOR DURANTE O PAUSE
        if (PauseManager.IsPaused)
            return;

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
        // Remove detector antigo se existir
        ClosePanelOnClick oldDetector = examinePanel.GetComponent<ClosePanelOnClick>();
        if (oldDetector != null)
        {
            Destroy(oldDetector);
        }

        // Adiciona novo detector
        ClosePanelOnClick detector = examinePanel.AddComponent<ClosePanelOnClick>();
        detector.SetExamineObject(this);
        Debug.Log("✅ ClosePanelOnClick adicionado ao painel!");
    }

    public void ShowPanel()
    {
        // NÃO ABRE PAINEL DURANTE O PAUSE
        if (PauseManager.IsPaused)
            return;

        if (examinePanel != null)
        {
            examinePanel.SetActive(true);
            Debug.Log("✅ Painel ABERTO por: " + gameObject.name);
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
            Debug.Log("❌ Painel FECHADO por: " + gameObject.name);
        }

        isShowing = false;

        if (currentObject == this)
            currentObject = null;
    }
}