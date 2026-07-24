using UnityEngine;
using TMPro;

public class ExamineObject : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;

    [TextArea]
    public string message;

    private bool isShowing = false;
    private static ExamineObject currentObject = null;

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
        // Bloqueia qualquer interação se o jogo estiver pausado
        if (PauseHelper.BlockInput())
            return;

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
                // Se clicou em OUTRO objeto
                else
                {
                    if (isShowing && currentObject == this)
                    {
                        HidePanel();
                        currentObject = null;
                    }
                }
            }
            // Se clicou no vazio
            else
            {
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
        if (PauseHelper.BlockInput())
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
        if (PauseHelper.BlockInput())
            return;

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