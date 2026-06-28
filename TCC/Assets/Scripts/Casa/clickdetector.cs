using UnityEngine;
using UnityEngine.EventSystems;

public class ClosePanelOnClick : MonoBehaviour, IPointerClickHandler
{
    private ExamineObject examineObject;

    public void SetExamineObject(ExamineObject obj)
    {
        examineObject = obj;
        Debug.Log("🔗 ClosePanelOnClick vinculado a: " + (obj != null ? obj.name : "null"));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("🖱️ CLICOU NO PAINEL! Fechando: " + (examineObject != null ? examineObject.name : "null"));
        
        if (examineObject != null)
        {
            examineObject.HidePanel();
        }
        else
        {
            Debug.LogWarning("⚠️ examineObject é NULL! Não foi possível fechar.");
        }
    }
}