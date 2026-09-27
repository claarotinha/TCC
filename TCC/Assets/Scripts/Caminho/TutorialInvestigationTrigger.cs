using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ExamineObject))]
public class TutorialInvestigationTrigger : MonoBehaviour
{
    [SerializeField] private bool isCryingBoy;

    private ExamineObject examineObject;
    private SpriteRenderer visibleSprite;

    private void OnEnable()
    {
        examineObject = GetComponent<ExamineObject>();
        visibleSprite = GetComponent<SpriteRenderer>();
        examineObject.Opened += HandleInvestigation;
    }

    private void OnDisable()
    {
        if (examineObject != null)
            examineObject.Opened -= HandleInvestigation;
    }

    private void LateUpdate()
    {
        if (!isCryingBoy || !Input.GetMouseButtonDown(0) ||
            visibleSprite == null || visibleSprite.sprite == null ||
            Camera.main == null || PauseHelper.BlockInput())
            return;

        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial == null ||
            (tutorial.CurrentStep != TutorialManager.TutorialStep.CryingBoy &&
             tutorial.CurrentStep != TutorialManager.TutorialStep.Investigate))
            return;

        // A mensagem anterior pode ter sido fechada neste mesmo clique.
        // A trava de um quadro impede o Update normal de abrir a fala do garoto,
        // mas um painel que ainda esteja aberto continua bloqueando a interação.
        if (InvestigationGuard.PanelOpen)
            return;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Bounds hitArea = visibleSprite.bounds;
        hitArea.Expand(new Vector3(0.45f, 0.35f, 0f));

        if (hitArea.Contains(new Vector3(mouse.x, mouse.y, hitArea.center.z)))
            examineObject.ShowPanel();
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
