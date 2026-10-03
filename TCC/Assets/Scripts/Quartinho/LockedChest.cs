using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LockedChest : MonoBehaviour
{
    [Header("Gerenciador da senha")]
    [SerializeField]
    private ChestCodePanelController codeController;

    [Header("Investigação depois de destrancar")]
    [SerializeField]
    private ExamineObject unlockedExamine;

    private Collider2D interactionCollider;

    public bool CanInteract =>
        isActiveAndEnabled &&
        codeController != null &&
        !codeController.Unlocked;

    private void Awake()
    {
        interactionCollider = GetComponent<Collider2D>();

        if (unlockedExamine == null)
            unlockedExamine = GetComponent<ExamineObject>();

        // A investigação normal só será liberada depois da senha.
        if (unlockedExamine != null)
            unlockedExamine.enabled = false;
    }

    private void Start()
    {
        if (codeController == null)
            codeController = ChestCodePanelController.Instance;

        if (codeController == null || unlockedExamine == null)
        {
            Debug.LogError(
                "LockedChest: preencha Code Controller e " +
                "Unlocked Examine no objeto " + name + ".",
                this
            );

            enabled = false;
            return;
        }

        codeController.ChestUnlocked += HandleUnlocked;

        if (codeController.Unlocked)
            HandleUnlocked();
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

        // O chão e a personagem não interceptam esse teste.
        if (!interactionCollider.OverlapPoint(mousePosition))
            return;

        codeController.Open();
    }

    private void HandleUnlocked()
    {
        if (unlockedExamine != null)
            unlockedExamine.enabled = true;
    }

    private void OnDestroy()
    {
        if (codeController != null)
            codeController.ChestUnlocked -= HandleUnlocked;
    }
}