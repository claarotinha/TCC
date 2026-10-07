using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Collider2D))]
public class HouseEntrance : MonoBehaviour
{
    private static GameObject openedPanel;
    private static HouseEntrance transitioningDoor;

    public static bool IsPanelOpen =>
        openedPanel != null && openedPanel.activeInHierarchy;
    public static bool IsChangingScene =>
        transitioningDoor != null && transitioningDoor.isTransitioning;

    [Header("Confirmação")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button buttonSim;
    public Button buttonNao;

    [Header("Transição")]
    public GameObject doorText;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool isTransitioning;
    private Collider2D doorCollider;
    private ExamineObject houseExamine;

    public bool CanInteract =>
        isActiveAndEnabled &&
        !isTransitioning &&
        doorCollider != null && doorCollider.enabled &&
        confirmPanel != null && !confirmPanel.activeInHierarchy &&
        TutorialManager.Instance != null &&
        TutorialManager.Instance.IsCompleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        openedPanel = null;
        transitioningDoor = null;
    }

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
        houseExamine = GetComponentInParent<ExamineObject>();
    }

    private void Start()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
        if (doorText != null)
            doorText.SetActive(false);
    }

    private void Update()
    {
        // A investigação inicial da casa dá lugar à entrada após o tutorial.
        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.IsCompleted &&
            houseExamine != null && houseExamine.enabled)
        {
            houseExamine.HidePanel();
            houseExamine.enabled = false;
        }

        // O clique depende da porta e do tutorial, sem distância da Mari.
        if (!CanInteract || InvestigationGuard.Blocked ||
            !Input.GetMouseButtonDown(0) || Camera.main == null)
            return;

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (doorCollider.OverlapPoint(mouse))
            AbrirConfirmacao();
    }

    private void AbrirConfirmacao()
    {
        if (confirmText == null || buttonSim == null || buttonNao == null)
        {
            Debug.LogError("PortaCasa: ligue o texto e os botões Sim/Não.", this);
            return;
        }

        confirmText.text = "Deseja entrar em casa?";
        buttonSim.gameObject.SetActive(true);
        buttonNao.gameObject.SetActive(true);
        buttonSim.interactable = true;
        buttonNao.interactable = true;
        buttonSim.onClick.RemoveAllListeners();
        buttonNao.onClick.RemoveAllListeners();
        buttonSim.onClick.AddListener(ConfirmarEntrada);
        buttonNao.onClick.AddListener(FecharConfirmacao);

        FitPanelToCanvas();
        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);
        openedPanel = confirmPanel;
        BlockClick();
    }

    private void ConfirmarEntrada()
    {
        if (!IsPanelOpen || !CanFinishTutorial() || isTransitioning)
            return;

        isTransitioning = true;
        transitioningDoor = this;
        FecharConfirmacao();
        StartCoroutine(FadeAndLoadScene());
    }

    private bool CanFinishTutorial()
    {
        return TutorialManager.Instance != null &&
               TutorialManager.Instance.IsCompleted;
    }

    private void FecharConfirmacao()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
        if (openedPanel == confirmPanel)
            openedPanel = null;
        BlockClick();
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (doorText != null)
            doorText.SetActive(false);
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.transform.SetAsLastSibling();
            fadeImage.raycastTarget = true;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = Mathf.Clamp01(timer / fadeDuration);
                fadeImage.color = color;
            }
            yield return null;
        }

        SceneManager.LoadScene("Casa_Manha");
    }

    private void FitPanelToCanvas()
    {
        RectTransform panel = confirmPanel.transform as RectTransform;
        RectTransform parent = panel != null ? panel.parent as RectTransform : null;
        if (parent == null || panel.rect.width <= 0f || panel.rect.height <= 0f)
            return;

        float scale = Mathf.Min(1f,
            (parent.rect.width - 24f) / panel.rect.width,
            (parent.rect.height - 24f) / panel.rect.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }

    private void BlockClick()
    {
        InvestigationGuard.BlockCurrentClick();
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void OnDestroy()
    {
        if (openedPanel == confirmPanel)
            openedPanel = null;
        if (transitioningDoor == this)
            transitioningDoor = null;
        if (buttonSim != null)
            buttonSim.onClick.RemoveListener(ConfirmarEntrada);
        if (buttonNao != null)
            buttonNao.onClick.RemoveListener(FecharConfirmacao);
    }
}
