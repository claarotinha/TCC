using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class QuartoBaguncaDoor : MonoBehaviour
{
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

    private void OnMouseDown()
    {
        if (PauseHelper.BlockInput())
            return;

        // ANTES de falar com a mãe:
        // não faz absolutamente nada aqui.
        // O ExamineObject será responsável pela investigação.
        if (!MotherDialogue.FalouSobreTrabalho)
            return;

        // DEPOIS de falar com a mãe:
        // abre o painel de vestígios.
        AbrirConfirmacao();
    }

    private void AbrirConfirmacao()
    {
        if (confirmPanel == null)
            return;

        confirmPanel.SetActive(true);

        if (confirmText != null)
            confirmText.text = "Quer procurar vestígios?";

        if (buttonSim != null)
        {
            buttonSim.gameObject.SetActive(true);

            buttonSim.onClick.RemoveAllListeners();
            buttonSim.onClick.AddListener(EntrarNoQuartinho);
        }

        if (buttonNao != null)
        {
            buttonNao.gameObject.SetActive(true);

            buttonNao.onClick.RemoveAllListeners();
            buttonNao.onClick.AddListener(FecharConfirmacao);
        }
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