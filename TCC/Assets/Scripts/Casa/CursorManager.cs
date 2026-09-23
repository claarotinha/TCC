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
        if (PauseHelper.BlockInput() || Camera.main == null)
        {
            SetNormal();
            return;
        }

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition);
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<ExamineObject>() != null ||
                hit.GetComponent<CollectableExamine>() != null ||
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
