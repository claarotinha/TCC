using System;
using UnityEngine;

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance { get; private set; }

    [SerializeField]
    private GameProgressData data = new GameProgressData();

    public GameProgressData Data => data;

    public event Action ProgressChanged;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (data == null)
            data = new GameProgressData();

        DontDestroyOnLoad(gameObject);
    }

    public void MarkTutorialCompleted()
    {
        if (data.tutorialCompleted)
            return;

        data.tutorialCompleted = true;
        NotifyChange();
    }

    public void MarkMotherWorkConversationCompleted()
    {
        if (data.motherWorkConversationCompleted)
            return;

        data.motherWorkConversationCompleted = true;
        NotifyChange();
    }

    public void UnlockQuartinho()
    {
        if (data.quartinhoUnlocked)
            return;

        data.quartinhoUnlocked = true;
        NotifyChange();
    }

    public void MarkPhotoFound(int photoId)
    {
        if (!ValidPhotoId(photoId) ||
            data.foundPhotos.Contains(photoId))
            return;

        data.foundPhotos.Add(photoId);
        NotifyChange();
    }

    public void MarkPhotoRead(int photoId)
    {
        if (!ValidPhotoId(photoId))
            return;

        bool changed = false;

        if (!data.foundPhotos.Contains(photoId))
        {
            data.foundPhotos.Add(photoId);
            changed = true;
        }

        if (!data.readPhotos.Contains(photoId))
        {
            data.readPhotos.Add(photoId);
            changed = true;
        }

        if (changed)
            NotifyChange();
    }

    public bool HasFoundPhoto(int photoId)
    {
        return data.foundPhotos.Contains(photoId);
    }

    public bool HasReadPhoto(int photoId)
    {
        return data.readPhotos.Contains(photoId);
    }

    public void MarkChestOpened()
    {
        if (data.chestOpened)
            return;

        data.chestOpened = true;
        NotifyChange();
    }

    public void MarkDiaryCollected()
    {
        if (data.diaryCollected)
            return;

        data.diaryCollected = true;
        NotifyChange();
    }

    public void UnlockDiaryPage(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId) ||
            data.unlockedDiaryPages.Contains(pageId))
            return;

        data.unlockedDiaryPages.Add(pageId);
        NotifyChange();
    }

    public bool HasDiaryPage(string pageId)
    {
        return data.unlockedDiaryPages.Contains(pageId);
    }

    public void MarkMotherNightConversationCompleted()
    {
        if (data.motherNightConversationCompleted)
            return;

        data.motherNightConversationCompleted = true;
        NotifyChange();
    }

    public void MarkFirstDiaryPageRead()
    {
        if (data.firstDiaryPageRead)
            return;

        data.firstDiaryPageRead = true;
        NotifyChange();
    }

    public void MarkNightCompleted()
    {
        if (data.nightCompleted)
            return;

        data.nightCompleted = true;
        NotifyChange();
    }

    private bool ValidPhotoId(int photoId)
    {
        if (photoId >= 1 && photoId <= 5)
            return true;

        Debug.LogWarning(
            "GameProgress: o número da foto deve estar entre 1 e 5.",
            this
        );

        return false;
    }

    private void NotifyChange()
    {
        ProgressChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}