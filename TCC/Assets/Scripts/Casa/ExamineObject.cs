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

    private Collider2D myCollider;

    private void Start()
    {
        myCollider = GetComponent<Collider2D>();

        if (examinePanel != null)
        {
            examinePanel.SetActive(false);
            AddClickDetector();
        }

        if (myCollider == null)
        {
            Debug.LogWarning(
                "ExamineObject: " +
                gameObject.name +
                " não possui Collider2D."
            );
        }
    }

    private void Update()
    {
        if (PauseHelper.BlockInput())
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (Camera.main == null || myCollider == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Verifica se o clique realmente aconteceu
        // dentro do Collider DESTE objeto.
        if (!myCollider.OverlapPoint(mousePosition))
            return;

        // Se outro objeto estava aberto, fecha.
        if (currentObject != null &&
            currentObject != this)
        {
            currentObject.HidePanel();
        }

        // Abre
        if (!isShowing)
        {
            ShowPanel();
            currentObject = this;
        }
        // Se clicar novamente no mesmo objeto, fecha.
        else
        {
            HidePanel();
            currentObject = null;
        }
    }

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput())
            return;

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
        }
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
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
            examinePanel.SetActive(true);

        if (examineText != null)
            examineText.text = message;

        isShowing = true;
    }

    public void HidePanel()
    {
        if (examinePanel != null)
            examinePanel.SetActive(false);

        isShowing = false;

        if (currentObject == this)
            currentObject = null;
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
                Destroy(detector);
        }

        if (currentObject == this)
            currentObject = null;
    }
}