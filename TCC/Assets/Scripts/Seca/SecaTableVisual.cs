using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SecaTableVisual : MonoBehaviour
{
    [SerializeField] private SecaMealDialogue mealDialogue;
    [SerializeField] private Sprite comMari;
    [SerializeField] private Sprite semMari;
    [SerializeField] private SpriteRenderer mari;
    private static SecaTableVisual activeTable;
    private ExamineObject interaction;
    private Collider2D area;
    private void OnEnable() => activeTable = this;

    public static bool CoversBackgroundExamine(ExamineObject target, Vector2 point)
    {
        return activeTable != null && activeTable.interaction != null &&
            activeTable.interaction != target && activeTable.interaction.enabled &&
            activeTable.area != null && activeTable.area.OverlapPoint(point);
    }

    private SpriteRenderer table;
    private bool hidMari;

    private void Awake()
    {
        table = GetComponent<SpriteRenderer>();
        if (mealDialogue != null)
        {
            interaction = mealDialogue.GetComponent<ExamineObject>();
            area = mealDialogue.GetComponent<Collider2D>();
        }
    }

    private void LateUpdate()
    {
        bool seated = mealDialogue != null && mealDialogue.MealInProgress;
        table.sprite = seated ? comMari : semMari;
        if (seated && mari != null && !hidMari)
        {
            hidMari = mari.enabled;
            mari.enabled = false;
        }
        else if (!seated) RestoreMari();
    }

    private void RestoreMari()
    {
        if (hidMari && mari != null) mari.enabled = true;
        hidMari = false;
    }

    private void OnDisable()
    {
        if (activeTable == this) activeTable = null;
        RestoreMari();
    }
}
