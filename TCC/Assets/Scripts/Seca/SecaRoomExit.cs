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
    [SerializeField] private Transform entradaQuarto;
    [SerializeField] private ExamineObject returnDoor;
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
    private bool returning;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.enabled = false;
        examineObject.Opened += OpenConfirmation;
        if (returnDoor != null) returnDoor.Opened += OpenReturnConfirmation;
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
        if (configured && sleepSequence.Finished && !examineObject.enabled)
            examineObject.enabled = true;
    }

    private void OpenConfirmation()
    {
        if (!configured || !sleepSequence.Finished) return;
        returning = false;
        examineObject.HidePanel();
        questionText.text = "Você quer sair do quarto?";
        // O Canvas compartilhado continua ativo mesmo após a sequência
        // de dormir; traz a confirmação acima dos demais elementos.
        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);
        openedPanel = confirmPanel;
        confirming = true;
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;
    }

    private void OpenReturnConfirmation()
    {
        if (!configured || !EnteredRoom || entradaQuarto == null) return;
        returnDoor.HidePanel();
        if (!playerMovement.enabled) return;
        returning = true;
        questionText.text = "Você quer voltar ao quarto?";
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
        if (!confirming) return;
        bool goingBack = returning;
        CloseConfirmation();
        bool firstEntry = !EnteredRoom;
        EnteredRoom = true;
        GameObject destinationRoom = goingBack ? quarto : salaCozinha;
        GameObject previousRoom = goingBack ? salaCozinha : quarto;
        destinationRoom.SetActive(true);
        Vector3 destination = (goingBack ? entradaQuarto : entradaSala).position;
        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(destination.x, destination.y);
        }
        playerMovement.transform.position = destination;
        previousRoom.SetActive(false);
        Physics2D.SyncTransforms();
        if (CursorManager.Instance != null) CursorManager.Instance.SetNormal();
        if (firstEntry && !goingBack) onEnteredRoom.Invoke();
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
        if (returnDoor != null) returnDoor.Opened -= OpenReturnConfirmation;
        if (yesButton != null) yesButton.onClick.RemoveListener(EnterKitchen);
        if (noButton != null) noButton.onClick.RemoveListener(CloseConfirmation);
        if (openedPanel == confirmPanel) openedPanel = null;
    }
}
