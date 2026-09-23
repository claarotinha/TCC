using UnityEngine;
using UnityEngine.EventSystems;

public class PanelClickHandler : MonoBehaviour, IPointerClickHandler
{
    private MonoBehaviour examineObject;

    public void SetExamineObject(MonoBehaviour obj)
    {
        examineObject = obj;
        Debug.Log("🔗 PanelClickHandler vinculado a: " + (obj != null ? obj.name : "null"));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("🖱 CLICOU NO PAINEL!");

        if (ExamineObject.IsShowing())
        {
            ExamineObject.HideCurrentPanel();
            return;
        }

        if (CollectableExamine.IsShowing())
        {
            CollectableExamine.HideCurrentPanel();
        }
    }
}
