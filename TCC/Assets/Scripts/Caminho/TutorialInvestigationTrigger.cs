using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(ExamineObject))]
public class TutorialInvestigationTrigger : MonoBehaviour
{
    [SerializeField] private bool isCryingBoy;

    public bool IsCryingBoy => isCryingBoy;

    private ExamineObject examineObject;
    private SpriteRenderer visibleSprite;
    private Collider2D clickCollider;
    private bool boyPanelOpen;

    private void OnEnable()
    {
        examineObject = GetComponent<ExamineObject>();
        visibleSprite = GetComponent<SpriteRenderer>();
        clickCollider = GetComponent<Collider2D>();
        examineObject.Opened += HandleInvestigation;
    }

    private void OnDisable()
    {
        if (examineObject != null)
            examineObject.Opened -= HandleInvestigation;
    }

    private void LateUpdate()
    {
        // O botão do próprio painel também pode fechá-lo pelo PanelClickHandler.
        if (isCryingBoy && boyPanelOpen &&
            (examineObject.examinePanel == null ||
             !examineObject.examinePanel.activeInHierarchy))
        {
            boyPanelOpen = false;
            return;
        }

        if (!isCryingBoy || !Input.GetMouseButtonDown(0) || Camera.main == null)
            return;

        // O painel do garoto é fechado pelo clique seguinte, inclusive fora dele.
        if (boyPanelOpen)
        {
            boyPanelOpen = false;
            if (examineObject.examinePanel != null && examineObject.examinePanel.activeSelf)
                examineObject.HidePanel();
            return;
        }

        if (PauseHelper.BlockInput())
            return;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool hit = clickCollider != null && clickCollider.OverlapPoint(mouse);
        if (!hit && visibleSprite != null && visibleSprite.sprite != null)
        {
            Bounds hitArea = visibleSprite.bounds;
            hitArea.Expand(new Vector3(0.45f, 0.35f, 0f));
            hit = hitArea.Contains(new Vector3(mouse.x, mouse.y, hitArea.center.z));
        }

        if (!hit)
            return;

        // A mensagem anterior pode ter sido fechada neste mesmo clique.
        // Só um painel que ainda esteja visível impede abrir a fala do garoto.
        string blockingPanel = InvestigationGuard.OpenPanelReason;
        if (blockingPanel != null)
        {
#if UNITY_EDITOR
            Debug.Log("Garoto: clique bloqueado por " + blockingPanel + ".", this);
#endif
            return;
        }

        if (examineObject.examinePanel == null || examineObject.examineText == null)
        {
            Debug.LogError("Garoto: painel ou texto de investigação não configurado.", this);
            return;
        }

        boyPanelOpen = true;
        examineObject.ShowPanel();
#if UNITY_EDITOR
        Debug.Log("Garoto: painel aberto e tutorial atualizado.", this);
#endif
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
