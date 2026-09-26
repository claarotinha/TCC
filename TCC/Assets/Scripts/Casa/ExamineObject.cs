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
        if (!Input.GetMouseButtonDown(0))
            return;

        if (InvestigationGuard.Blocked)
            return;

        if (Camera.main == null || myCollider == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Verifica se o clique realmente aconteceu
        // dentro do Collider DESTE objeto.
        if (!myCollider.OverlapPoint(mousePosition))
            return;

        QuartoBaguncaDoor door = GetComponent<QuartoBaguncaDoor>();
        if (door != null && !door.TryHandleClick())
            return;

        if (door != null &&
            (MotherDialogue.FalouSobreTrabalho || QuartoBaguncaDoor.Unlocked))
        {
            HidePanel();
            door.AbrirConfirmacao();
            return;
        }

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
        if (InvestigationGuard.Blocked)
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
        ShowMessage(message);
    }

    public void ShowMessage(string text)
    {
        if (currentObject != null && currentObject != this)
            currentObject.HidePanel();

        if (examinePanel != null)
            examinePanel.SetActive(true);

        if (examineText != null)
            examineText.text = text;

        isShowing = true;
        currentObject = this;
    }

    public void HidePanel()
    {
        if (examinePanel != null)
            examinePanel.SetActive(false);

        isShowing = false;

        if (currentObject == this)
            currentObject = null;

        InvestigationGuard.BlockCurrentClick();
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

    public static void HideCurrentPanel()
    {
        if (currentObject != null)
            currentObject.HidePanel();
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
