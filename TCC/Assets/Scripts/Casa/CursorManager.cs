using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public Texture2D normalCursor;
    public Texture2D lupaCursor;

    private void Awake()
    {
        Instance = this;

        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        TutorialManager tutorial = TutorialManager.Instance;
        if (InvestigationGuard.Blocked || Camera.main == null ||
            (tutorial != null && tutorial.CurrentStep <= TutorialManager.TutorialStep.Run))
        {
            SetNormal();
            return;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);
        foreach (Collider2D hit in hits)
        {
            TutorialInvestigationTrigger trigger = hit.GetComponent<TutorialInvestigationTrigger>();
            if (trigger != null && trigger.IsCryingBoy && tutorial != null &&
                tutorial.CurrentStep < TutorialManager.TutorialStep.CryingBoy)
                continue;

            if (hit.TryGetComponent(out TutorialBoysDialogue boys))
            {
                if (tutorial != null && tutorial.CurrentStep == TutorialManager.TutorialStep.Boys &&
                    boys.isActiveAndEnabled)
                {
                    SetLupa();
                    return;
                }
                continue;
            }

            if ((hit.TryGetComponent(out ExamineObject examine) && examine.isActiveAndEnabled) ||
                (hit.TryGetComponent(out QuartinhoExit roomExit) && roomExit.isActiveAndEnabled) ||
                (hit.TryGetComponent(out CollectableExamine collectible) &&
                 collectible.CanInteract) ||
                hit.GetComponent<QuartoBaguncaDoor>() != null ||
                hit.GetComponent<MotherDialogue>() != null)
            {
                SetLupa();
                return;
            }
        }

        SetNormal();
    }

    public void SetLupa()
    {
        Cursor.SetCursor(lupaCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetNormal()
    {
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }
}
