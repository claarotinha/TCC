using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class QuartoBaguncaDoor : MonoBehaviour
{
    private static int lastClickFrame = -1;

    [Header("Painel de Confirmação")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button buttonSim;
    public Button buttonNao;

    private void Start()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (PauseHelper.BlockInput())
            return;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    // A cena contém duas áreas do quartinho sobrepostas. Um clique deve
    // acionar apenas uma delas, inclusive antes da conversa com a mãe.
    public bool TryHandleClick()
    {
        if (lastClickFrame == Time.frameCount)
            return false;

        lastClickFrame = Time.frameCount;
        return true;
    }

    public void AbrirConfirmacao()
    {
        if (confirmPanel == null)
            return;

        confirmPanel.SetActive(true);

        if (confirmText != null)
            confirmText.text = "Quer procurar vestígios?";

        buttonSim.gameObject.SetActive(true);
        buttonNao.gameObject.SetActive(true);

        buttonSim.onClick.RemoveAllListeners();
        buttonNao.onClick.RemoveAllListeners();

        buttonSim.onClick.AddListener(EntrarNoQuartinho);
        buttonNao.onClick.AddListener(FecharConfirmacao);
    }

    private void EntrarNoQuartinho()
    {
        SceneManager.LoadScene("Quartinho");
    }

    private void FecharConfirmacao()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}
