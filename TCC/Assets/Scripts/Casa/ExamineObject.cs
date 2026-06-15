using UnityEngine;
using TMPro;

public class ExamineObject : MonoBehaviour
{
    public GameObject examinePanel;
    public TMP_Text examineText;

    [TextArea]
    public string message;

    private bool isShowing = false;

    private void OnMouseEnter()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
        }
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetNormal();
        }
    }

    private void OnMouseDown()
    {
        if (!isShowing)
        {
            if (examinePanel != null)
            {
                examinePanel.SetActive(true);
            }

            if (examineText != null)
            {
                examineText.text = message;
            }

            isShowing = true;
        }
        else
        {
            if (examinePanel != null)
            {
                examinePanel.SetActive(false);
            }

            isShowing = false;
        }
    }
}