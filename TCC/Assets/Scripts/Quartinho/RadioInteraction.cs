using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ExamineObject))]
public class RadioInteraction : MonoBehaviour
{
    [SerializeField] private AudioSource radioAudio;
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private static RadioInteraction currentRadio;
    public static bool IsPanelOpen => currentRadio != null &&
        currentRadio.confirmPanel != null && currentRadio.confirmPanel.activeInHierarchy;

    private ExamineObject examine;
    private bool examined;
    private bool waitingForFirstClose;
    private bool confirming;

    private void Awake()
    {
        examine = GetComponent<ExamineObject>();
        examine.Opened += OnExamined;
        if (confirmPanel != null) confirmPanel.SetActive(false);
        if (yesButton != null) yesButton.onClick.AddListener(TurnOn);
        if (noButton != null) noButton.onClick.AddListener(TurnOff);
    }

    private void OnExamined()
    {
        if (!examined)
        {
            examined = true;
            waitingForFirstClose = true;
            return;
        }

        // Nas próximas visitas ao rádio, abre somente a pergunta.
        examine.HidePanel();
        OpenConfirmation();
    }

    private void LateUpdate()
    {
        if (waitingForFirstClose && examine.ClosedThisFrame && !PauseHelper.BlockInput())
        {
            waitingForFirstClose = false;
            OpenConfirmation();
        }
    }

    private void OpenConfirmation()
    {
        if (confirmPanel == null || questionText == null ||
            yesButton == null || noButton == null || radioAudio == null)
        {
            Debug.LogError("RadioInteraction: confira o áudio e o painel do rádio.", this);
            return;
        }
        questionText.text = radioAudio.isPlaying
            ? "O rádio está ligado. Deseja manter a música tocando?"
            : "O rádio está desligado. Deseja ligar a música?";
        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        confirming = true;
        currentRadio = this;
        InvestigationGuard.BlockCurrentClick();
    }

    private void TurnOn()
    {
        if (!confirming) return;
        if (radioAudio != null && radioAudio.clip != null && !radioAudio.isPlaying)
            radioAudio.Play();
        Close();
    }

    private void TurnOff()
    {
        if (!confirming) return;
        if (radioAudio != null && radioAudio.isPlaying) radioAudio.Stop();
        Close();
    }

    private void Close()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
        confirming = false;
        if (currentRadio == this) currentRadio = null;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (waitingForFirstClose)
        {
            waitingForFirstClose = false;
            examine.HidePanel();
        }
        if (confirming) Close();
    }

    private void OnDestroy()
    {
        if (examine != null) examine.Opened -= OnExamined;
        if (yesButton != null) yesButton.onClick.RemoveListener(TurnOn);
        if (noButton != null) noButton.onClick.RemoveListener(TurnOff);
        if (currentRadio == this) currentRadio = null;
    }
}
