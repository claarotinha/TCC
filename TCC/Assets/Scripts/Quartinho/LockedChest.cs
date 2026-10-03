using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedChest : MonoBehaviour
{
    [SerializeField]
    private ChestCodePanelController codeController;

    [SerializeField]
    private ChestContentsController contentsController;

    [SerializeField]
    private ExamineObject unlockedExamine;

    private Collider2D interactionCollider;

    public bool CanInteract =>
        isActiveAndEnabled &&
        codeController != null &&
        contentsController != null;

    private void Awake()
    {
        interactionCollider = GetComponent<Collider2D>();

        if (unlockedExamine == null)
            unlockedExamine = GetComponent<ExamineObject>();

        // O novo painel assume toda a interação do baú.
        if (unlockedExamine != null)
            unlockedExamine.enabled = false;
    }

    private void Start()
    {
        if (codeController == null)
            codeController = ChestCodePanelController.Instance;

        if (contentsController == null)
            contentsController = ChestContentsController.Instance;

        if (codeController == null || contentsController == null)
        {
            Debug.LogError(
                "LockedChest: preencha Code Controller " +
                "e Contents Controller.",
                this
            );
            enabled = false;
            return;
        }

        codeController.ChestUnlocked += HandleUnlocked;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) ||
            !CanInteract ||
            InvestigationGuard.Blocked ||
            Camera.main == null ||
            interactionCollider == null ||
            !interactionCollider.enabled)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!interactionCollider.OverlapPoint(mousePosition))
            return;

        if (codeController.Unlocked)
            contentsController.Open();
        else
            codeController.Open();
    }

    private void HandleUnlocked()
    {
        GameProgress.Instance?.MarkChestOpened();
        contentsController.Open();
    }

    private void OnDestroy()
    {
        if (codeController != null)
            codeController.ChestUnlocked -= HandleUnlocked;
    }
}