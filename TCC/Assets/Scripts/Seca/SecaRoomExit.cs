using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(ExamineObject))]
public class SecaRoomExit : MonoBehaviour
{
    [SerializeField] private SecaSleepSequence sleepSequence;
    [Header("Troca de ambiente")]
    [SerializeField] private GameObject quarto;
    [SerializeField] private GameObject salaCozinha;
    [SerializeField] private Transform entradaSala;
    [SerializeField] private PlayerMovement playerMovement;
    [Header("Confirmação de saída")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private UnityEvent onEnteredRoom = new UnityEvent();

    public bool EnteredRoom { get; private set; }
    private static GameObject openedPanel;
    public static bool IsPanelOpen => openedPanel != null && openedPanel.activeInHierarchy;
    private ExamineObject examineObject;
    private bool configured;
    private bool movementWasEnabled;
    private bool confirming;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.enabled = false;
        examineObject.Opened += OpenConfirmation;
    }

    private void Start()
    {
        configured = sleepSequence != null && quarto != null && salaCozinha != null &&
            entradaSala != null && playerMovement != null && quarto != salaCozinha &&
            confirmPanel != null && questionText != null && yesButton != null && noButton != null;
        if (!configured)
        {
            Debug.LogError("SecaRoomExit: confira os ambientes e a confirmação de saída.", this);
            return;
        }
        confirmPanel.SetActive(false);
        salaCozinha.SetActive(false);
        yesButton.onClick.AddListener(EnterKitchen);
        noButton.onClick.AddListener(CloseConfirmation);
    }

    private void Update()
    {
        if (configured && !EnteredRoom && sleepSequence.Finished && !examineObject.enabled)
            examineObject.enabled = true;
    }

    private void OpenConfirmation()
    {
        if (!configured || EnteredRoom || !sleepSequence.Finished) return;
        examineObject.HidePanel();
        questionText.text = "Você quer sair do quarto?";
        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);
        openedPanel = confirmPanel;
        confirming = true;
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    private void EnterKitchen()
    {
        if (!confirming || EnteredRoom) return;
        CloseConfirmation();
        EnteredRoom = true;
        salaCozinha.SetActive(true);
        Vector3 destination = entradaSala.position;
        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(destination.x, destination.y);
        }
        playerMovement.transform.position = destination;
        quarto.SetActive(false);
        Physics2D.SyncTransforms();
        if (CursorManager.Instance != null) CursorManager.Instance.SetNormal();
        onEnteredRoom.Invoke();
    }

    private void CloseConfirmation()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (openedPanel == confirmPanel) openedPanel = null;
        if (confirming && playerMovement != null) playerMovement.enabled = movementWasEnabled;
        confirming = false;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (confirming) CloseConfirmation();
    }

    private void OnDestroy()
    {
        if (examineObject != null) examineObject.Opened -= OpenConfirmation;
        if (yesButton != null) yesButton.onClick.RemoveListener(EnterKitchen);
        if (noButton != null) noButton.onClick.RemoveListener(CloseConfirmation);
        if (openedPanel == confirmPanel) openedPanel = null;
    }
}
