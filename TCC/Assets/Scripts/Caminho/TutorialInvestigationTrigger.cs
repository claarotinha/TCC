using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ExamineObject))]
public class TutorialInvestigationTrigger : MonoBehaviour
{
    [SerializeField] private bool isCryingBoy;

    private Collider2D objectCollider;
    private ExamineObject examineObject;

    private void Awake()
    {
        objectCollider = GetComponent<Collider2D>();
        examineObject = GetComponent<ExamineObject>();
    }

    private void LateUpdate()
    {
        TutorialManager tutorial = TutorialManager.Instance;

        if (tutorial == null ||
            !Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            examineObject.examinePanel == null ||
            !examineObject.examinePanel.activeInHierarchy ||
            examineObject.examineText == null ||
            examineObject.examineText.text != examineObject.message ||
            !ExamineObject.IsShowing())
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!objectCollider.OverlapPoint(mousePosition))
            return;

        if (isCryingBoy &&
            tutorial.CurrentStep == TutorialManager.TutorialStep.CryingBoy)
        {
            tutorial.ReportCryingBoy();
        }
        else if (!isCryingBoy &&
                 tutorial.CurrentStep == TutorialManager.TutorialStep.Investigate)
        {
            tutorial.ReportInvestigation();
        }
    }
}