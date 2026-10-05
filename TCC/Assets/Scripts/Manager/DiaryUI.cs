using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiaryUI : MonoBehaviour
{
    [Serializable]
    public class DiaryEntry
    {
        [Tooltip(
            "Vazio: disponível ao coletar o diário. " +
            "Preenchido: depende de UnlockDiaryPage com esse ID."
        )]
        public string unlockId;

        public string title;

        [TextArea(6, 20)]
        public string leftText;

        [TextArea(6, 20)]
        public string rightText;
    }

    [Header("Textos")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text leftPageText;
    [SerializeField] private TMP_Text rightPageText;
    [SerializeField] private TMP_Text pageNumberText;

    [Header("Navegação")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [Header("Entradas em ordem de leitura")]
    [SerializeField] private DiaryEntry[] entries;

    private readonly List<DiaryEntry> availableEntries =
        new List<DiaryEntry>();

    private int currentIndex;
    private bool configured;
    private GameProgress subscribedProgress;

    private void Awake()
    {
        configured =
            titleText != null &&
            leftPageText != null &&
            rightPageText != null &&
            pageNumberText != null &&
            previousButton != null &&
            nextButton != null;

        if (!configured)
        {
            Debug.LogError(
                "DiaryUI: preencha os textos e os botões de navegação.",
                this
            );
            return;
        }

        previousButton.onClick.AddListener(Previous);
        nextButton.onClick.AddListener(Next);
    }

    private void OnEnable()
    {
        if (!configured)
            return;

        subscribedProgress = GameProgress.Instance;

        if (subscribedProgress != null)
            subscribedProgress.ProgressChanged += Refresh;

        currentIndex = 0;
        Refresh();
    }

    private void OnDisable()
    {
        if (subscribedProgress != null)
            subscribedProgress.ProgressChanged -= Refresh;

        subscribedProgress = null;
    }

    private void Refresh()
    {
        DiaryEntry previouslySelected =
            availableEntries.Count > 0 &&
            currentIndex < availableEntries.Count
                ? availableEntries[currentIndex]
                : null;

        availableEntries.Clear();

        GameProgress progress = GameProgress.Instance;

        if (progress != null &&
            progress.Data.diaryCollected &&
            entries != null)
        {
            foreach (DiaryEntry entry in entries)
            {
                if (entry == null)
                    continue;

                bool unlocked =
                    string.IsNullOrWhiteSpace(entry.unlockId) ||
                    progress.HasDiaryPage(entry.unlockId);

                if (unlocked)
                    availableEntries.Add(entry);
            }
        }

        int previousIndex =
            previouslySelected != null
                ? availableEntries.IndexOf(previouslySelected)
                : -1;

        currentIndex = previousIndex >= 0
            ? previousIndex
            : Mathf.Clamp(
                currentIndex,
                0,
                Mathf.Max(0, availableEntries.Count - 1)
            );

        ShowCurrentEntry();
    }

    private void ShowCurrentEntry()
    {
        if (availableEntries.Count == 0)
        {
            titleText.text = "Diário";
            leftPageText.text = "Ainda não há páginas disponíveis.";
            rightPageText.text = "";
            pageNumberText.text = "";

            previousButton.interactable = false;
            nextButton.interactable = false;
            return;
        }

        DiaryEntry entry = availableEntries[currentIndex];

        titleText.text = entry.title;
        leftPageText.text = entry.leftText;
        rightPageText.text = entry.rightText;

        pageNumberText.text =
            (currentIndex + 1) + " / " + availableEntries.Count;

        previousButton.interactable = currentIndex > 0;
        nextButton.interactable =
            currentIndex < availableEntries.Count - 1;
    }

    private void Previous()
    {
        if (currentIndex <= 0)
            return;

        currentIndex--;
        ShowCurrentEntry();
    }

    private void Next()
    {
        if (currentIndex >= availableEntries.Count - 1)
            return;

        currentIndex++;
        ShowCurrentEntry();
    }

    private void OnDestroy()
    {
        if (subscribedProgress != null)
            subscribedProgress.ProgressChanged -= Refresh;

        if (previousButton != null)
            previousButton.onClick.RemoveListener(Previous);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(Next);
    }
}
