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
        {
            group = original.dialoguePanel.AddComponent<CanvasGroup>();
        }

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

        // Antes do diário, mantém a conversa original.
        // Depois, este script assume os cliques na mãe.
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
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!motherCollider.OverlapPoint(mousePosition))
            return;

        OpenDialogue();
    }

    private void OpenDialogue()
    {
        open = true;
        current = this;

        original.dialoguePanel.SetActive(true);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        original.dialogueText.enableAutoSizing = true;
        original.dialogueText.fontSizeMin = 14;
        original.dialogueText.fontSizeMax = 21;

        bool alreadyTalked =
            GameProgress.Instance.Data.motherNightConversationCompleted;

        ShowMother(alreadyTalked
            ? "Guarde esse diário com cuidado, minha filha. " +
              "Depois você pode continuar lendo. Já está na hora de descansar."
            : "E então, minha filha? Encontrou alguma coisa no quartinho?");

        SetChoice(original.choice1, "Mostrar o diário", ShowDiary);
        SetChoice(original.choice2, "Posso ler agora?", AskToRead);
        SetChoice(original.choice3, "Voltar", Close);

        InvestigationGuard.BlockCurrentClick();
    }

    private void ShowDiary()
    {
        ShowMother(
            "Mari: Encontrei um diário dentro daquele baú. " +
            "Talvez ele ajude no meu trabalho.\n\n" +
            "Mãe: Pode ser, minha filha. Guarde com cuidado. " +
            "Essas coisas fazem parte da nossa história."
        );

        SetChoice(original.choice1, "E sobre a leitura?", AskToRead);
        original.choice2.gameObject.SetActive(false);
        SetChoice(original.choice3, "Encerrar a conversa", FinishConversation);
    }

    private void AskToRead()
    {
        ShowMother(
            "Você pode olhar um pouco, mas já está ficando tarde. " +
            "É bom ir dormir e continuar a leitura depois."
        );

        SetChoice(
            original.choice1,
            "Vou olhar o diário primeiro",
            ChooseRead
        );

        SetChoice(
            original.choice2,
            "Tudo bem, vou descansar",
            FinishConversation
        );

        original.choice3.gameObject.SetActive(false);
    }

    private void ChooseRead()
    {
        RecordConversation();
        Close();

        // A leitura continua pelo menu de pausa.
        if (UniversalPauseManager.Instance != null)
            UniversalPauseManager.Instance.PauseGame();
    }

    private void FinishConversation()
    {
        RecordConversation();
        Close();
    }

    private void RecordConversation()
    {
        GameProgress.Instance.MarkMotherNightConversationCompleted();
    }

    private void ShowMother(string text)
    {
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

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = label;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
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
