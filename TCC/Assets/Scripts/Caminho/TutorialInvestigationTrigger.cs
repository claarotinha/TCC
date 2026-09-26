using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ExamineObject))]
public class TutorialInvestigationTrigger : MonoBehaviour
{
    [SerializeField] private bool isCryingBoy;

    private ExamineObject examineObject;

    private void OnEnable()
    {
        examineObject = GetComponent<ExamineObject>();
        examineObject.Opened += HandleInvestigation;
    }

    private void OnDisable()
    {
        if (examineObject != null)
            examineObject.Opened -= HandleInvestigation;
    }

    private void HandleInvestigation()
    {
        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null)
            return;

        if (isCryingBoy)
        {
            if (tutorial.CurrentStep == TutorialManager.TutorialStep.Investigate)
                tutorial.ReportInvestigation();

            tutorial.ReportCryingBoy();
        }
        else
        {
            tutorial.ReportInvestigation();
        }
    }
}
