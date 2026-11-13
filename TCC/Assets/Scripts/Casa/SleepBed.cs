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

    [Header("Leitura antes de dormir")]
    [SerializeField] private GameObject readingDiaryPanel;
    [SerializeField] private SpriteRenderer readingPose;
    [SerializeField] private SpriteRenderer[] playerRenderers;
    [SerializeField] private Button closeReadingButton;
    [SerializeField] private GameObject sleepingImage;
    [SerializeField] private ExamineObject tiredExamine;
    private bool readBeforeSleeping;
    private bool readingClosed;
    private bool[] previousRendererStates;

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
        GameProgress.Instance.Data.diaryCollected;

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
            fadeImage != null && readingDiaryPanel != null && readingPose != null &&
            sleepingImage != null && tiredExamine != null && closeReadingButton != null &&
            playerRenderers != null && playerRenderers.Length > 0;

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
        noButton.onClick.AddListener(DeclineReading);
        closeReadingButton.onClick.AddListener(CloseReading);
        readingDiaryPanel.SetActive(false);
        readingPose.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!CanInteract ||
            !Input.GetMouseButtonDown(0) ||
            InvestigationGuard.Blocked ||
            Camera.main == null)
            return;

        if (!InvestigationGuard.TryGetPointerPosition(out Vector2 mousePosition)) return;

        if (bedCollider.OverlapPoint(mousePosition))
            OpenConfirmation();
    }

    private void OpenConfirmation()
    {
        current = this;

        questionText.text = CanSleep
            ? "Quer ler o diário antes de dormir?"
            : "Não quero dormir agora.";

        yesButton.gameObject.SetActive(CanSleep);
        TMP_Text yesText = yesButton.GetComponentInChildren<TMP_Text>(true);
        if (yesText != null) yesText.text = "Sim";

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

    private void DeclineReading()
    {
        if (!CanSleep) { CloseConfirmation(); return; }
        BeginSleep(false);
    }

    private void ConfirmSleep()
    {
        BeginSleep(true);
    }

    private void BeginSleep(bool read)
    {
        if (runningSequence || !configured ||
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

        readBeforeSleeping = read;
        CloseConfirmation();
        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        current = this;
        runningSequence = true;
        IsSleeping = true;

        ExamineObject.HideCurrentPanel();
        CollectableExamine.HideCurrentPanel();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();

        if (readBeforeSleeping)
        {
            previousRendererStates = new bool[playerRenderers.Length];
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] == null) continue;
                previousRendererStates[i] = playerRenderers[i].enabled;
                playerRenderers[i].enabled = false;
            }
            // A posição e a escala são as que foram editadas na Hierarchy.
            readingPose.gameObject.SetActive(true);
            readingClosed = false;
            readingDiaryPanel.transform.SetAsLastSibling();
            readingDiaryPanel.SetActive(true);
            while (!readingClosed) yield return null;
            readingDiaryPanel.SetActive(false);
            GameProgress.Instance?.MarkFirstDiaryPageRead();

            tiredExamine.ShowMessage("Tô cansada… Melhor dormir e continuar a leitura outro dia.");
            yield return null;
            while (tiredExamine.PanelIsOpen)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) tiredExamine.HidePanel();
                yield return null;
            }
        }

        SetFadeAlpha(0f);
        sleepingImage.SetActive(false);
        sequencePanel.SetActive(true);
        yield return FadeTo(1f);
        sleepingImage.SetActive(true);


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

    public void CloseReading()
    {
        if (!runningSequence || readingDiaryPanel == null || !readingDiaryPanel.activeInHierarchy) return;
        readingClosed = true;
        readingDiaryPanel.SetActive(false);
        InvestigationGuard.BlockCurrentClick();
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
            if (tiredExamine != null && tiredExamine.PanelIsOpen) tiredExamine.HidePanel();
            if (readingDiaryPanel != null) readingDiaryPanel.SetActive(false);
            if (readingPose != null) readingPose.gameObject.SetActive(false);
            if (previousRendererStates != null)
                for (int i = 0; i < previousRendererStates.Length; i++)
                    if (playerRenderers[i] != null) playerRenderers[i].enabled = previousRendererStates[i];

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
        if (closeReadingButton != null)
            closeReadingButton.onClick.RemoveListener(CloseReading);
        if (yesButton != null)
            yesButton.onClick.RemoveListener(ConfirmSleep);

        if (noButton != null)
            noButton.onClick.RemoveListener(DeclineReading);
    }
}
