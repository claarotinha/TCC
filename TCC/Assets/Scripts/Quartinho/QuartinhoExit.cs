using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class QuartinhoExit : MonoBehaviour
{
    private static GameObject openedPanel;
    public static bool IsPanelOpen => openedPanel != null && openedPanel.activeInHierarchy;

    [SerializeField] private GameObject exitPanel;
    [SerializeField] private TMP_Text exitText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Collider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();

        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
            Canvas canvas = exitPanel.GetComponentInParent<Canvas>(true);
            if (canvas != null) canvas.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        if (exitPanel != null)
            exitPanel.SetActive(false);

        SetButtonsForDoor();
    }

    private void SetButtonsForDoor()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ReturnHome);
            yesButton.onClick.AddListener(ReturnHome);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(Close);
            noButton.onClick.AddListener(Close);
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            doorCollider == null ||
            exitPanel == null ||
            exitPanel.activeInHierarchy ||
            InvestigationGuard.Blocked)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!doorCollider.OverlapPoint(mousePosition))
            return;

        SetButtonsForDoor();

        if (exitText != null)
            exitText.text = "Você quer voltar para a casa?";

        ShowConfirmation();
    }

    private void ShowConfirmation()
    {
        exitPanel.transform.SetAsLastSibling();
        exitPanel.SetActive(true);
        InvestigationGuard.BlockCurrentClick();
        openedPanel = exitPanel;
    }

    private void ReturnHome()
    {
        SceneManager.LoadScene("Casa_Manha");
    }

    private void Close()
    {
        InvestigationGuard.BlockCurrentClick();
        if (exitPanel != null)
            exitPanel.SetActive(false);

        if (openedPanel == exitPanel)
            openedPanel = null;
    }

    private void OnDestroy()
    {
        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ReturnHome);
        }
        if (noButton != null)
            noButton.onClick.RemoveListener(Close);

        if (openedPanel == exitPanel)
            openedPanel = null;
    }

    private void OnMouseEnter()
    {
        if (!PauseHelper.BlockInput() && CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }
}
