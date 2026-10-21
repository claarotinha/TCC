using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecaSleepSequence : MonoBehaviour
{
    [Header("Sequência inicial")]
    [SerializeField] private SecaOpeningDialogue openingDialogue;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Escurecimento")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.7f;
    [SerializeField, Min(0f)] private float darkDuration = 0.8f;

    [Header("Diálogo")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite mariPortrait;

    public bool Finished { get; private set; }

    private Rigidbody2D playerBody;
    private bool running;
    private bool movementLocked;
    private bool movementWasEnabled;
    private bool waitingForClick;
    private int displayedFrame;

    private void Awake()
    {
        SetFade(0f);
    }

    public void TrySleep()
    {
        if (running || Finished ||
            openingDialogue == null ||
            !openingDialogue.Finished)
        {
            return;
        }

        if (playerMovement == null || fadeImage == null ||
            dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null)
        {
            Debug.LogError(
                "SecaSleepSequence: preencha as referências no Inspector.",
                this
            );

            return;
        }

        running = true;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        movementWasEnabled = playerMovement.enabled;
        playerBody = playerMovement.GetComponent<Rigidbody2D>();

        playerMovement.enabled = false;
        movementLocked = true;
        StopPlayer();

        Animator animator = playerMovement.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }

        dialogueUI.SetActive(false);

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        yield return FadeTo(1f);
        yield return new WaitForSeconds(darkDuration);
        yield return FadeTo(0f);

        fadeImage.raycastTarget = false;

        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);

        ShowCanvasGroup(dialogueUI);
        ShowCanvasGroup(dialoguePanel);

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        characterNameText.text = "Mari (pensamento)";
        dialogueText.text =
            "No mesmo lugar novamente… E a menina não está mais aqui.";

        SetPortrait();

        yield return WaitForAdvance();

        dialogueText.text =
            "Talvez eu devesse entender onde estou. " +
            "Talvez assim eu acorde desse sonho.";

        yield return WaitForAdvance();

        dialogueUI.SetActive(false);

        Finished = true;
        running = false;
        RestoreMovement();
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float initialAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            SetFade(Mathf.Lerp(
                initialAlpha,
                targetAlpha,
                progress
            ));

            yield return null;
        }

        SetFade(targetAlpha);
    }

    private IEnumerator WaitForAdvance()
    {
        displayedFrame = Time.frameCount;
        waitingForClick = true;

        while (waitingForClick)
            yield return null;
    }

    private void Update()
    {
        if (!running || !waitingForClick || Time.timeScale == 0f)
            return;

        if (Time.frameCount > displayedFrame &&
            Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
        }
    }

    private void FixedUpdate()
    {
        if (movementLocked)
            StopPlayer();
    }

    private void StopPlayer()
    {
        if (playerBody == null)
            return;

        Vector2 velocity = playerBody.linearVelocity;
        velocity.x = 0f;
        playerBody.linearVelocity = velocity;
    }

    private void SetFade(float alpha)
    {
        if (fadeImage == null)
            return;

        fadeImage.color = new Color(0f, 0f, 0f, alpha);
    }

    private void SetPortrait()
    {
        if (portraitImage == null)
            return;

        portraitImage.sprite = mariPortrait;
        portraitImage.enabled = mariPortrait != null;
        portraitImage.preserveAspect = true;
    }

    private static void ShowCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (group == null)
            return;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void RestoreMovement()
    {
        if (!movementLocked)
            return;

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        movementLocked = false;
    }

    private void OnDisable()
    {
        if (!running)
            return;

        StopAllCoroutines();

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        SetFade(0f);

        if (fadeImage != null)
            fadeImage.raycastTarget = false;

        waitingForClick = false;
        running = false;
        RestoreMovement();
    }
}