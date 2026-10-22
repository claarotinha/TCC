using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SecaFarewellSequence : MonoBehaviour
{
    [Header("Liberação")]
    [SerializeField] private SecaMotherReturn motherReturn;

    [Header("Ambientes")]
    [SerializeField] private GameObject salaCozinha;
    [SerializeField] private GameObject despedidaCasa;
    [SerializeField] private Transform lugarMariDespedida;

    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Escurecimento")]
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

    [Header("Após a despedida")]
    [SerializeField] private UnityEvent onFarewellCompleted =
        new UnityEvent();

    public bool FarewellCompleted { get; private set; }

    private Rigidbody2D playerBody;
    private bool running;
    private bool waitingForClick;
    private bool movementWasEnabled;
    private bool movementLocked;
    private int displayedFrame;

    private void Start()
    {
        if (despedidaCasa != null)
            despedidaCasa.SetActive(false);
    }

    public void BeginFarewell()
    {
        if (running || FarewellCompleted)
            return;

        if (motherReturn == null || !motherReturn.DepartureReady)
            return;

        if (salaCozinha == null || despedidaCasa == null ||
            salaCozinha == despedidaCasa ||
            lugarMariDespedida == null || playerMovement == null ||
            fadeImage == null || dialogueUI == null ||
            dialoguePanel == null || characterNameText == null ||
            dialogueText == null)
        {
            Debug.LogError(
                "SecaFarewellSequence: preencha as referências no Inspector.",
                this
            );
            return;
        }

        running = true;
        StartCoroutine(PlayFarewell());
    }

    private IEnumerator PlayFarewell()
    {
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        movementLocked = true;

        playerBody = playerMovement.GetComponent<Rigidbody2D>();

        if (playerBody != null)
            playerBody.linearVelocity = Vector2.zero;

        Animator animator = playerMovement.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }

        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();

        dialogueUI.SetActive(false);

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        yield return FadeTo(1f);

        despedidaCasa.SetActive(true);
        salaCozinha.SetActive(false);

        Vector3 destination = lugarMariDespedida.position;

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.position =
                new Vector2(destination.x, destination.y);
        }

        playerMovement.transform.position = destination;

        SpriteRenderer visual =
            playerMovement.GetComponent<SpriteRenderer>();

        if (visual != null)
            visual.flipX = false;

        Physics2D.SyncTransforms();

        // Aguarda a câmera existente acompanhar a nova posição.
        yield return new WaitForSeconds(0.8f);

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
            "Foi o avô de vocês que levantou essas paredes. " +
            "Cada rachadura dessa eu conheço. " +
            "Cuida da nossa casa, meu Deus…";

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
        if (!running || !waitingForClick ||
            InvestigationGuard.Blocked || Time.timeScale == 0f)
        {
            return;
        }

        if (Time.frameCount <= displayedFrame ||
            !Input.GetMouseButtonDown(0))
        {
            return;
        }

        waitingForClick = false;
        dialogueUI.SetActive(false);

        FarewellCompleted = true;
        running = false;

        RestoreMovement();
        InvestigationGuard.BlockCurrentClick();

        onFarewellCompleted.Invoke();
    }

    private void FixedUpdate()
    {
        if (!movementLocked || playerBody == null)
            return;

        Vector2 velocity = playerBody.linearVelocity;
        velocity.x = 0f;
        playerBody.linearVelocity = velocity;
    }

    private void RestoreMovement()
    {
        if (!movementLocked)
            return;

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        movementLocked = false;
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

        running = false;
        waitingForClick = false;
        RestoreMovement();
    }
}