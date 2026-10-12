using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MotherDialogue))]
public class MotherDiaryDialogue : MonoBehaviour
{
    private static MotherDiaryDialogue current;

    public static bool IsShowing =>
        current != null && current.open;

    private MotherDialogue original;
    private Collider2D motherCollider;
    private CanvasGroup group;
    private GameProgress progress;

    private bool open;
    private bool ready;
    private bool discussedDiary;

    private void Start()
    {
        original = GetComponent<MotherDialogue>();
        motherCollider = GetComponent<Collider2D>();
        progress = GameProgress.Instance;

        if (original.dialoguePanel == null ||
            original.dialogueText == null ||
            original.characterNameText == null ||
            original.portraitImage == null ||
            original.choice1 == null ||
            original.choice2 == null ||
            original.choice3 == null ||
            motherCollider == null)
        {
            Debug.LogError(
                "MotherDiaryDialogue: confira as referências " +
                "do MotherDialogue e o Collider2D da mãe.",
                this
            );

            enabled = false;
            return;
        }

        group = original.dialoguePanel.GetComponent<CanvasGroup>();

        if (group == null)
            group = original.dialoguePanel.AddComponent<CanvasGroup>();

        ready = true;

        if (progress != null)
            progress.ProgressChanged += RefreshMode;

        RefreshMode();
    }

    private void RefreshMode()
    {
        if (!ready)
            return;

        bool hasDiary =
            GameProgress.Instance != null &&
            GameProgress.Instance.Data.diaryCollected;

        // Antes de coletar o diário, usa a conversa original.
        // Depois da coleta, este script assume a interação.
        original.enabled = !hasDiary;
    }

    private void Update()
    {
        if (!ready)
            return;

        RefreshMode();

        if (GameProgress.Instance == null ||
            !GameProgress.Instance.Data.diaryCollected ||
            open ||
            !Input.GetMouseButtonDown(0) ||
            InvestigationGuard.Blocked ||
            Camera.main == null ||
            !motherCollider.enabled)
        {
            return;
        }

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (motherCollider.OverlapPoint(mousePosition))
            OpenDialogue();
    }

    private void OpenDialogue()
    {
        open = true;
        discussedDiary = false;
        current = this;

        original.dialoguePanel.SetActive(true);

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        original.ConfigureDialogueText();

        bool alreadyTalked =
            GameProgress.Instance.Data.motherNightConversationCompleted;

        ShowMother(alreadyTalked
            ? "Guarde esse diário com cuidado, minha filha. " +
              "Você encontrou uma parte da história da nossa família."
            : "E então, minha filha? Encontrou alguma coisa no quartinho?");

        ShowChoices();

        InvestigationGuard.BlockCurrentClick();
    }

    private void ShowChoices()
    {
        SetChoice(
            original.choice1,
            "Diário",
            TellAboutDiary
        );

        SetChoice(
            original.choice2,
            "ler",
            AskToRead
        );

        SetChoice(
            original.choice3,
            "Sair",
            LeaveDialogue
        );
    }

    private void TellAboutDiary()
    {
        if (!open || current != this)
            return;

        discussedDiary = true;

        ShowMother(
            "Mari: Encontrei um diário dentro daquele baú. " +
            "Talvez ele ajude no meu trabalho.\n" +
            "Mãe: Pode ser, minha filha. Guarde com cuidado. " +
            "Essas coisas fazem parte da nossa história."
        );

        ShowChoices();
        InvestigationGuard.BlockCurrentClick();
    }

    private void AskToRead()
    {
        if (!open || current != this)
            return;

        discussedDiary = true;

        ShowMother(
            "Pode ler um pouco, minha filha, mas já está ficando tarde. " +
            "É bom ir dormir e continuar a leitura depois."
        );

        ShowChoices();
        InvestigationGuard.BlockCurrentClick();
    }

    private void ShowMother(string text)
    {
        original.ConfigureDialogueText();
        original.portraitImage.sprite = original.motherPortrait;
        original.characterNameText.text = "Mãe";
        original.dialogueText.text = text;
    }

    private void SetChoice(
        Button button,
        string label,
        UnityEngine.Events.UnityAction action)
    {
        button.gameObject.SetActive(true);
        button.interactable = true;

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>(true);

        if (text != null)
            text.text = label;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void LeaveDialogue()
    {
        if (!open || current != this)
            return;

        // Só registra a conversa se o jogador falou sobre o diário.
        if (discussedDiary && GameProgress.Instance != null)
        {
            GameProgress.Instance
                .MarkMotherNightConversationCompleted();
        }

        Close();
    }

    private void Close()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        original.choice1.gameObject.SetActive(false);
        original.choice2.gameObject.SetActive(false);
        original.choice3.gameObject.SetActive(false);

        open = false;
        discussedDiary = false;

        if (current == this)
            current = null;

        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (open && group != null)
            Close();
    }

    private void OnDestroy()
    {
        if (progress != null)
            progress.ProgressChanged -= RefreshMode;

        if (current == this)
            current = null;
    }
}