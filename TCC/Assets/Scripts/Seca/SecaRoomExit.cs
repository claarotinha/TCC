using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ExamineObject))]
public class SecaRoomExit : MonoBehaviour
{
    [Header("Progresso do quarto")]
    [SerializeField] private SecaSleepSequence sleepSequence;
    [SerializeField] private SecaRoomInvestigation[] requiredObjects;
    [SerializeField] private SecaMirrorDiscovery mirrorDiscovery;

    [Header("Troca de ambiente")]
    [SerializeField] private GameObject quarto;
    [SerializeField] private GameObject salaCozinha;
    [SerializeField] private Transform entradaSala;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private Collider2D salaBounds;

    [Header("Ao entrar na sala")]
    [SerializeField] private UnityEvent onEnteredRoom = new UnityEvent();

    public bool EnteredRoom { get; private set; }

    private ExamineObject examineObject;
    private bool configured;

    private void Awake()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.enabled = false;
        examineObject.Opened += HandleDoorOpened;

        examineObject.message =
            "Antes de sair, preciso olhar melhor este quarto " +
            "e tentar entender o que aconteceu comigo.";
    }

    private void Start()
    {
        configured =
            sleepSequence != null &&
            mirrorDiscovery != null &&
            quarto != null &&
            salaCozinha != null &&
            entradaSala != null &&
            playerMovement != null &&
            quarto != salaCozinha &&
            requiredObjects != null &&
            requiredObjects.Length == 6;

        if (configured)
        {
            for (int i = 0; i < requiredObjects.Length; i++)
            {
                if (requiredObjects[i] == null)
                {
                    configured = false;
                    break;
                }

                for (int j = 0; j < i; j++)
                {
                    if (requiredObjects[i] == requiredObjects[j])
                        configured = false;
                }
            }
        }

        if (!configured)
        {
            Debug.LogError(
                "SecaRoomExit: confira as referências e os seis objetos distintos.",
                this
            );

            return;
        }

        salaCozinha.SetActive(false);
    }

    private void Update()
    {
        if (!configured || EnteredRoom)
            return;

        if (sleepSequence.Finished && !examineObject.enabled)
            examineObject.enabled = true;
    }

    private bool AllInvestigated()
    {
        if (!configured ||
            !sleepSequence.Finished ||
            !mirrorDiscovery.Discovered)
        {
            return false;
        }

        foreach (SecaRoomInvestigation item in requiredObjects)
        {
            if (item == null || !item.Inspected)
                return false;
        }

        return true;
    }

    private void HandleDoorOpened()
    {
        if (EnteredRoom || !AllInvestigated())
            return;

        EnteredRoom = true;

        // Fecha a investigação antes de ocultar o quarto.
        examineObject.HidePanel();

        salaCozinha.SetActive(true);
        if (salaBounds != null)
            playerMovement.SetMovementBounds(salaBounds);

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
        if (salaBounds != null && Camera.main != null)
        {
            CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
            if (follow != null)
                follow.SetLevelBounds(salaBounds);
        }

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();

        onEnteredRoom.Invoke();
    }

    private void OnDestroy()
    {
        if (examineObject != null)
            examineObject.Opened -= HandleDoorOpened;
    }
}
