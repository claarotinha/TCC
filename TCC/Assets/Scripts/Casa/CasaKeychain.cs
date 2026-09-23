using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CasaKeychain : MonoBehaviour
{
    public ItemData key;
    public GameObject examinePanel;
    public TMP_Text examineText;
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button buttonSim;
    public Button buttonNao;

    private void OnMouseDown()
    {
        if (PauseHelper.BlockInput()) return;
        ExamineObject.HideCurrentPanel();
        if (!MotherDialogue.FalouSobreTrabalho)
        {
            if (examineText != null) examineText.text = "Um chaveiro pendurado na parede.";
            if (examinePanel != null) examinePanel.SetActive(true);
            return;
        }
        if (InventoryManager.Instance == null || key == null) return;
        if (InventoryManager.Instance.Items.Contains(key))
        {
            if (examineText != null) examineText.text = "Já peguei a chave do quartinho.";
            if (examinePanel != null) examinePanel.SetActive(true);
            return;
        }
        if (confirmPanel == null || buttonSim == null || buttonNao == null) return;
        confirmPanel.SetActive(true);
        if (confirmText != null) confirmText.text = "Você quer pegar a chave?";
        buttonSim.gameObject.SetActive(true);
        buttonNao.gameObject.SetActive(true);
        buttonSim.onClick.RemoveAllListeners();
        buttonNao.onClick.RemoveAllListeners();
        buttonSim.onClick.AddListener(() => { InventoryManager.Instance.AddItem(key); confirmPanel.SetActive(false); });
        buttonNao.onClick.AddListener(() => confirmPanel.SetActive(false));
    }

    private void OnMouseEnter() { if (!PauseHelper.BlockInput() && CursorManager.Instance != null) CursorManager.Instance.SetLupa(); }
    private void OnMouseExit() { if (CursorManager.Instance != null) CursorManager.Instance.SetNormal(); }
}
