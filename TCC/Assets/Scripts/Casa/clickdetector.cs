using UnityEngine;
using UnityEngine.EventSystems;

public class ClosePanelOnClick : MonoBehaviour, IPointerClickHandler
{
    private ExamineObject examineObject;
    private bool isDestroyed = false;

    private void OnDestroy()
    {
        isDestroyed = true;
        examineObject = null;
    }

    public void SetExamineObject(ExamineObject obj)
    {
        if (isDestroyed) return;
        
        examineObject = obj;
        Debug.Log("🔗 ClosePanelOnClick vinculado a: " + (obj != null ? obj.name : "null"));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDestroyed || examineObject == null)
        {
            Debug.LogWarning("⚠ Não foi possível fechar - ExamineObject é null ou foi destruído");
            return;
        }
        
        Debug.Log("🖱 CLICOU NO PAINEL! Fechando: " + examineObject.name);
        examineObject.HidePanel();
    }
}