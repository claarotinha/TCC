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
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        if (hit.collider != null &&
            (hit.collider.GetComponent<ExamineObject>() != null ||
             hit.collider.GetComponent<CollectableExamine>() != null ||
             hit.collider.GetComponent<QuartoBaguncaDoor>() != null ||
             hit.collider.GetComponent<MotherDialogue>() != null))
            SetLupa();
        else
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
