using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Cada cartela e seus dois textos existem na Hierarchy da própria cena.
public class ChapterCard : MonoBehaviour
{
    [Serializable]
    public class Card
    {
        public string id;
        public GameObject panel;
        public CanvasGroup group;
        public TMP_Text dateText;
        public TMP_Text titleText;
    }

    [SerializeField] private Card[] cards;
    [SerializeField, Min(.01f)] private float typingInterval = .065f;
    [SerializeField, Min(1f)] private float readingTime = 3f;
    [SerializeField, Min(.01f)] private float fadeDuration = .5f;
    public static bool IsShowing { get; private set; }
    private static ChapterCard active;
    private float previousTimeScale;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState() { active = null; IsShowing = false; }

    private void Awake()
    {
        foreach (Card card in cards)
            if (card.panel != null) card.panel.SetActive(false);
    }

    public static IEnumerator Show(string id, bool replay = false)
    {
        while (IsShowing || GameSaveSystem.IsRestoring) yield return null;
        GameProgress progress = GameProgress.Instance;
        if (progress != null && progress.Data.chapterCardsSeen == null)
            progress.Data.chapterCardsSeen = new System.Collections.Generic.List<string>();
        if (!replay && progress != null && progress.Data.chapterCardsSeen.Contains(id)) yield break;
        ChapterCard presenter = null;
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            presenter = root.GetComponentInChildren<ChapterCard>(true);
            if (presenter != null) break;
        }
        if (presenter == null) { Debug.LogError("Cartela ausente na cena: " + id); yield break; }
        Card selected = Array.Find(presenter.cards, card => card.id == id);
        if (selected == null || selected.panel == null || selected.group == null ||
            selected.dateText == null || selected.titleText == null)
        { Debug.LogError("Configure a cartela na Hierarchy: " + id, presenter); yield break; }
        yield return presenter.Play(selected);
        if (!replay && progress != null)
        {
            if (!progress.Data.chapterCardsSeen.Contains(id)) progress.Data.chapterCardsSeen.Add(id);
            if (id.StartsWith("josefa-")) progress.UnlockDiaryPage(id);
        }
    }

    private IEnumerator Play(Card card)
    {
        previousTimeScale = Time.timeScale;
        active = this;
        IsShowing = true;
        Time.timeScale = 0f;
        InventoryTabController.Instance?.Close();
        InvestigationGuard.BlockCurrentClick();
        card.dateText.maxVisibleCharacters = 0;
        card.titleText.maxVisibleCharacters = 0;
        card.group.alpha = 0f;
        card.panel.SetActive(true);
        try
        {
            yield return Fade(card.group, 0f, 1f);
            yield return Type(card.dateText);
            yield return Wait(.35f);
            yield return Type(card.titleText);
            yield return Wait(Mathf.Max(readingTime, card.titleText.text.Length / 14f));
            yield return Fade(card.group, 1f, 0f);
        }
        finally
        {
            card.panel.SetActive(false);
            card.dateText.maxVisibleCharacters = int.MaxValue;
            card.titleText.maxVisibleCharacters = int.MaxValue;
            Release();
        }
    }

    private IEnumerator Type(TMP_Text text)
    {
        text.ForceMeshUpdate(true);
        int count = text.textInfo.characterCount;
        for (int i = 1; i <= count; i++)
        {
            yield return Wait(typingInterval);
            text.maxVisibleCharacters = i;
        }
    }

    private IEnumerator Wait(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!UniversalPauseManager.IsPaused) elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (!UniversalPauseManager.IsPaused) elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = to;
    }

    private void Release()
    {
        if (active != this) return;
        Time.timeScale = previousTimeScale;
        active = null;
        IsShowing = false;
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnDisable()
    {
        if (cards != null) foreach (Card card in cards)
            if (card.panel != null) card.panel.SetActive(false);
        Release();
    }
}
