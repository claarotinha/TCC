using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class QuartinhoExit : MonoBehaviour
{
    [SerializeField] private GameObject exitPanel;
    [SerializeField] private TMP_Text exitText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Collider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (exitPanel != null)
            exitPanel.SetActive(false);

        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ReturnHome);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(Close);
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            doorCollider == null ||
            exitPanel == null ||
            exitPanel.activeSelf ||
            InvestigationGuard.Blocked)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!doorCollider.OverlapPoint(mousePosition))
            return;

        if (exitText != null)
            exitText.text = "Você quer voltar para a casa?";

        exitPanel.SetActive(true);
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
