using UnityEngine;

public class DiaryCollect : MonoBehaviour
{
    private bool examined = false;
    private bool collected = false;



    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }



    private void Update()
    {
        if (examined &&
            !collected &&
            Input.GetKeyDown(KeyCode.E))
        {
            CollectDiary();
        }
    }

    void CollectDiary()
    {
        collected = true;

        Debug.Log(
            "Mari guardou o diário."
        );

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }

        gameObject.SetActive(false);

        StartDiaryCutscene();
    }

    void StartDiaryCutscene()
    {
        Debug.Log(
            "Cutscene: Mari sente um arrepio ao tocar o diário..."
        );
    }
    private void OnMouseEnter()
{
    Debug.Log("Mouse entrou no diário!");

    if (!collected && CursorManager.Instance != null)
    {
        CursorManager.Instance.SetLupa();
    }
}
}