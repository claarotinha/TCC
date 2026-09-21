using UnityEngine;
using TMPro;

public class ExamineObject : MonoBehaviour
{
    [Header("Painel")]
    public GameObject examinePanel;

    [Header("Texto")]
    public TMP_Text examineText;

    [TextArea]
    public string message;

    private bool isShowing = false;

    private static ExamineObject currentObject = null;

    private void Start()
    {
        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
            AddClickDetector();
        }

        // Garante que o objeto tenha uma área de clique
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning(
                "O objeto " + gameObject.name +
                " não possui Collider2D. Adicione um Box Collider 2D."
            );
        }
    }

    private void Update()
    {
        if (PauseHelper.BlockInput())
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (Camera.main == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Procura TODOS os colliders naquele ponto
        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePosition);

        ExamineObject objetoEncontrado = null;

        foreach (Collider2D hit in hits)
        {
            // Procura o ExamineObject no próprio objeto
            objetoEncontrado =
                hit.GetComponent<ExamineObject>();

            if (objetoEncontrado != null)
                break;

            // Também procura no objeto pai
            objetoEncontrado =
                hit.GetComponentInParent<ExamineObject>();

            if (objetoEncontrado != null)
                break;
        }

        // Nenhum objeto investigável foi clicado
        if (objetoEncontrado == null)
        {
            if (isShowing && currentObject == this)
            {
                HidePanel();
            }

            return;
        }

        // Se clicou em outro objeto
        if (currentObject != null &&
            currentObject != objetoEncontrado)
        {
            currentObject.HidePanel();
        }

        // Abre ou fecha
        if (!objetoEncontrado.isShowing)
        {
            objetoEncontrado.ShowPanel();
            currentObject = objetoEncontrado;
        }
        else
        {
            objetoEncontrado.HidePanel();
            currentObject = null;
        }
    }

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput())
            return;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void AddClickDetector()
    {
        if (examinePanel == null)
            return;

        PanelClickHandler detector =
            examinePanel.GetComponent<PanelClickHandler>();

        if (detector == null)
        {
            detector =
                examinePanel.AddComponent<PanelClickHandler>();
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
        }

        isShowing = false;

        if (currentObject == this)
        {
            currentObject = null;
        }
    }

    public static bool IsShowing()
    {
        if (currentObject != null &&
            currentObject.examinePanel != null)
        {
            return currentObject.examinePanel.activeSelf;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (examinePanel != null)
        {
            PanelClickHandler detector =
                examinePanel.GetComponent<PanelClickHandler>();

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