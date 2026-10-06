using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class SleepBed : MonoBehaviour
{
    private static SleepBed current;

    public static bool IsSleeping { get; private set; }

    public static bool IsPanelOpen =>
        current != null &&
        current.confirmPanel != null &&
        current.confirmPanel.activeInHierarchy;

    [Header("Confirmação")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Sequência de dormir")]
    [SerializeField] private GameObject sequencePanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float sleepingImageDuration = 3f;

    [Header("Próxima cena")]
    [SerializeField] private string nextScene;

    private Collider2D bedCollider;
    private bool configured;
    private bool runningSequence;
    private float previousTimeScale = 1f;

    public bool CanInteract =>
        isActiveAndEnabled &&
        configured &&
        bedCollider != null &&
        bedCollider.enabled &&
        !IsSleeping;

    private bool CanSleep =>
        GameProgress.Instance != null &&
        GameProgress.Instance.Data.diaryCollected &&
        GameProgress.Instance.Data.motherNightConversationCompleted;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        current = null;
        IsSleeping = false;
    }

    private void Awake()
    {
        bedCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        configured =
            confirmPanel != null &&
            questionText != null &&
            yesButton != null &&
            noButton != null &&
            sequencePanel != null &&
            fadeImage != null;

        if (!configured)
        {
            Debug.LogError(
                "SleepBed: preencha os campos da confirmação " +
                "e da sequência no Inspector.",
                this
            );
            return;
        }

        ExamineObject examine = GetComponent<ExamineObject>();

        if (examine != null)
            examine.enabled = false;

        confirmPanel.SetActive(false);
        sequencePanel.SetActive(false);

        yesButton.onClick.AddListener(ConfirmSleep);
        noButton.onClick.AddListener(CloseConfirmation);
    }

    private void Update()
    {
        if (!CanInteract ||
            !Input.GetMouseButtonDown(0) ||
            InvestigationGuard.Blocked ||
            Camera.main == null)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (bedCollider.OverlapPoint(mousePosition))
            OpenConfirmation();
    }

    private void OpenConfirmation()
    {
        current = this;

        questionText.text = CanSleep
            ? "Você quer dormir agora?"
            : "Ainda preciso terminar minha investigação " +
              "e conversar com minha mãe.";

        yesButton.gameObject.SetActive(CanSleep);

        TMP_Text noText =
            noButton.GetComponentInChildren<TMP_Text>(true);

        if (noText != null)
            noText.text = CanSleep ? "Não" : "Voltar";

        confirmPanel.transform.SetAsLastSibling();
        confirmPanel.SetActive(true);

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void CloseConfirmation()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (current == this && !runningSequence)
            current = null;

        InvestigationGuard.BlockCurrentClick();
    }

    private void ConfirmSleep()
    {
        if (!configured ||
            !CanSleep ||
            IsSleeping ||
            !confirmPanel.activeInHierarchy)
            return;

        if (string.IsNullOrWhiteSpace(nextScene) ||
            !Application.CanStreamedLevelBeLoaded(nextScene))
        {
            questionText.text =
                "A próxima cena ainda não está configurada.";

            Debug.LogError(
                "SleepBed: preencha Next Scene com o nome exato " +
                "e inclua a cena na lista de cenas da build.",
                this
            );
            return;
        }

        CloseConfirmation();
        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        current = this;
        runningSequence = true;
        IsSleeping = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();

        SetFadeAlpha(1f);
        sequencePanel.SetActive(true);

        yield return FadeTo(0f);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, sleepingImageDuration)
        );

        yield return FadeTo(1f);

        GameProgress.Instance.MarkNightCompleted();

        Time.timeScale = previousTimeScale;

        // O bloqueio é retirado quando este objeto sai da cena.
        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float duration = Mathf.Max(0.01f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            SetFadeAlpha(
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    Mathf.Clamp01(elapsed / duration)
                )
            );

            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void OnDisable()
    {
        if (runningSequence)
        {
            StopAllCoroutines();
            Time.timeScale = previousTimeScale;
            runningSequence = false;
            IsSleeping = false;

            if (sequencePanel != null)
                sequencePanel.SetActive(false);
        }

        if (current == this)
        {
            current = null;

            if (confirmPanel != null)
                confirmPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(ConfirmSleep);

        if (noButton != null)
            noButton.onClick.RemoveListener(CloseConfirmation);
    }
}