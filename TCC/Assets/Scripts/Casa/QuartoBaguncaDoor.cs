using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuartoBaguncaDoor : MonoBehaviour
{
    private static int lastClickFrame = -1;
    private static bool unlocked;

    public static bool Unlocked => unlocked;

    [Header("Painel de confirmação")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button buttonSim;
    public Button buttonNao;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        unlocked = false;
        lastClickFrame = -1;
    }

    private void Start()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (!PauseHelper.BlockInput() && CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    public bool TryHandleClick()
    {
        if (lastClickFrame == Time.frameCount)
            return false;

        lastClickFrame = Time.frameCount;
        return true;
    }

    public void AbrirConfirmacao()
    {
        if (confirmPanel == null || confirmText == null ||
            buttonSim == null || buttonNao == null)
            return;

        confirmPanel.SetActive(true);

        buttonSim.onClick.RemoveAllListeners();
        buttonNao.onClick.RemoveAllListeners();

        TMP_Text noText = buttonNao.GetComponentInChildren<TMP_Text>();

        if (!unlocked)
        {
            confirmText.text = "Esse quarto está trancado.";
            buttonSim.gameObject.SetActive(false);
            buttonNao.gameObject.SetActive(true);

            if (noText != null)
                noText.text = "Fechar";
        }
        else
        {
            confirmText.text = "Quer procurar vestígios?";
            buttonSim.gameObject.SetActive(true);
            buttonNao.gameObject.SetActive(true);

            if (noText != null)
                noText.text = "Não";

            buttonSim.onClick.AddListener(EntrarNoQuartinho);
        }

        buttonNao.onClick.AddListener(FecharConfirmacao);
    }

    // Este método será chamado pelo Item Use Target ao receber a chave.
    public void Unlock()
    {
        unlocked = true;
        FecharConfirmacao();
    }

    private void EntrarNoQuartinho()
    {
        if (unlocked)
            SceneManager.LoadScene("Quartinho");
    }

    private void FecharConfirmacao()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}