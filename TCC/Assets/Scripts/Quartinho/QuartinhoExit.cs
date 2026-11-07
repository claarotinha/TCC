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

    [Header("Rádio do quartinho - reutiliza o painel da saída")]
    [SerializeField] private ExamineObject radioExamine;
    [SerializeField] private AudioSource radioAudio;

    private Collider2D doorCollider;
    private bool radioExamined;
    private bool radioConfirmation;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();

        if (radioExamine != null)
            radioExamine.Opened += MarkRadioExamined;

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
            yesButton.onClick.RemoveListener(PlayRadio);
            yesButton.onClick.RemoveListener(ReturnHome);
            yesButton.onClick.AddListener(ReturnHome);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(Close);
            noButton.onClick.AddListener(Close);
        }
    }

    private void MarkRadioExamined()
    {
        // O primeiro clique mostra apenas o texto existente de ExamineObject.
        radioExamined = true;
    }

    private void LateUpdate()
    {
        // O segundo clique fecha o exame e imediatamente apresenta Sim/Não.
        // Usa o painel de confirmação já montado na cena, sem uma interface nova.
        if (!radioExamined || radioExamine == null ||
            !radioExamine.ClosedThisFrame ||
            !Input.GetMouseButtonDown(0) ||
            exitPanel == null || exitPanel.activeInHierarchy ||
            PauseHelper.BlockInput())
            return;

        OpenRadioConfirmation();
    }

    private void OpenRadioConfirmation()
    {
        radioConfirmation = true;
        if (exitText != null)
            exitText.text = "Deseja tocar a música?";

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ReturnHome);
            yesButton.onClick.RemoveListener(PlayRadio);
            yesButton.onClick.AddListener(PlayRadio);
            yesButton.gameObject.SetActive(true);
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveListener(Close);
            noButton.onClick.AddListener(Close);
            noButton.gameObject.SetActive(true);
        }

        ShowConfirmation();
    }

    private void PlayRadio()
    {
        if (!radioConfirmation)
            return;

        Close();

        if (radioAudio != null && radioAudio.clip != null)
        {
            radioAudio.Stop();
            radioAudio.Play();
        }
        else
        {
            Debug.LogWarning("QuartinhoExit: atribua o áudio do menu ao rádio.", this);
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

        radioConfirmation = false;
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
        if (!radioConfirmation)
            SceneManager.LoadScene("Casa_Manha");
    }

    private void Close()
    {
        InvestigationGuard.BlockCurrentClick();
        if (exitPanel != null)
            exitPanel.SetActive(false);

        radioConfirmation = false;

        if (openedPanel == exitPanel)
            openedPanel = null;
    }

    private void OnDestroy()
    {
        if (radioExamine != null)
            radioExamine.Opened -= MarkRadioExamined;

        if (yesButton != null)
        {
            yesButton.onClick.RemoveListener(ReturnHome);
            yesButton.onClick.RemoveListener(PlayRadio);
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
