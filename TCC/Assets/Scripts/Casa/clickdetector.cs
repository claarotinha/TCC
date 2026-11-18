using UnityEngine;
using UnityEngine.EventSystems;

public class PanelClickHandler : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    private bool examineOnPress;

    public void SetExamineObject(MonoBehaviour obj)
    {
        // The panel is shared. The static active object determines which message closes.
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        examineOnPress = ExamineObject.IsShowing();
        if (examineOnPress) ExamineObject.TryCloseCurrentPanelFromPointer();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        // Releasing the same click must not close the next message or collect an item.
        if (examineOnPress) { examineOnPress = false; return; }
        if (ExamineObject.IsShowing()) return;
        if (CollectableExamine.IsShowing()) CollectableExamine.HandleCurrentPanelClick();
    }
}
