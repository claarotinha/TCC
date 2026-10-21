using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SecaPreparationRequest : MonoBehaviour
{
    [Header("Refeição")]
    [SerializeField] private SecaMealDialogue mealDialogue;

    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Transição")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.7f;

    [Header("Interface compartilhada")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite maePortrait;

    [Header("Após terminar o pedido")]
    [SerializeField] private UnityEvent onRequestCompleted =
        new UnityEvent();

    public bool RequestCompleted { get; private set; }

    private Rigidbody2D playerBody;
    private bool running;
    private bool waitingForClick;
    private bool movementWasEnabled;
    private int displayedFrame;

    public void BeginRequest()
    {
        if (running || RequestCompleted)
            return;

        if (mealDialogue == null || !mealDialogue.MealCompleted)
        {
            Debug.LogWarning(
                "SecaPreparationRequest: termine a refeição primeiro.",
                this
            );
            return;
        }

        if (playerMovement == null || fadeImage == null ||
            dialogueUI == null || dialoguePanel == null ||
            characterNameText == null || dialogueText == null)
        {
            Debug.LogError(
                "SecaPreparationRequest: preencha as referências no Inspector.",
                this
            );
            return;
        }

        running = true;
        StartCoroutine(PlayRequest());
    }

    private IEnumerator PlayRequest()
    {
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;

        playerBody = playerMovement.GetComponent<Rigidbody2D>();

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

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
        yield return new WaitForSeconds(0.6f);
        yield return FadeTo(0f);

        fadeImage.raycastTarget = false;

        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);

        ShowCanvasGroup(dialogueUI);
        ShowCanvasGroup(dialoguePanel);

        if (choicesContainer != null)
            choicesContainer.SetActive(false);

        characterNameText.text = "Mãe";
        dialogueText.text =
            "Josefa, já que terminou de comer, vá pegar algumas coisas " +
            "antes de sairmos. Pegue o cantil, o facão, um pano, " +
            "o chapéu e o resto de farinha.";

        if (portraitImage != null)
        {
            portraitImage.sprite = maePortrait;
            portraitImage.enabled = maePortrait != null;
            portraitImage.preserveAspect = true;
        }

        displayedFrame = Time.frameCount;
        waitingForClick = true;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float initialAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(
                initialAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / fadeDuration)
            );

            fadeImage.color = new Color(0f, 0f, 0f, alpha);

            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    private void Update()
    {
        if (!running || !waitingForClick || Time.timeScale == 0f)
            return;

        if (Time.frameCount <= displayedFrame ||
            !Input.GetMouseButtonDown(0))
        {
            return;
        }

        waitingForClick = false;
        dialogueUI.SetActive(false);

        RequestCompleted = true;
        running = false;

        playerMovement.enabled = movementWasEnabled;

        onRequestCompleted.Invoke();
    }

    private void FixedUpdate()
    {
        if (!running || playerBody == null)
            return;

        Vector2 velocity = playerBody.linearVelocity;
        velocity.x = 0f;
        playerBody.linearVelocity = velocity;
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

    private void OnDisable()
    {
        if (!running)
            return;

        StopAllCoroutines();

        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        waitingForClick = false;
        running = false;
    }
}