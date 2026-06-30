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
        
        if (examineObject == null)
        {
            Debug.LogWarning("⚠ examineObject é NULL! Não foi possível fechar.");
            return;
        }

        if (examineObject is ExamineObject only)
        {
            only.HidePanel();
            Debug.Log("🔒 Painel fechado (ExamineObject)");
        }
        else if (examineObject is CollectableExamine collect)
        {
            collect.HidePanel();
            Debug.Log("🔒 Painel fechado (CollectableExamine)");
        }
    }
}