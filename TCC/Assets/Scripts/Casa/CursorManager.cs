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

    public void SetLupa()
    {
        Cursor.SetCursor(lupaCursor, Vector2.zero, CursorMode.Auto);
    }

    public void SetNormal()
    {
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
    }
}