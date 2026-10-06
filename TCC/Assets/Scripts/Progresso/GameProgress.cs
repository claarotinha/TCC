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

        NormalizeData();
        DontDestroyOnLoad(gameObject);
    }

    public void NormalizeData()
    {
        if (data == null) data = new GameProgressData();
        if (data.foundPhotos == null) data.foundPhotos = new System.Collections.Generic.List<int>();
        if (data.readPhotos == null) data.readPhotos = new System.Collections.Generic.List<int>();
        if (data.unlockedDiaryPages == null) data.unlockedDiaryPages = new System.Collections.Generic.List<string>();
        if (data.collectedObjects == null) data.collectedObjects = new System.Collections.Generic.List<string>();
        if (data.usedTargets == null) data.usedTargets = new System.Collections.Generic.List<string>();
    }

    public void ResetProgress()
    {
        data = new GameProgressData();
        NotifyChange();
    }

    public void MarkKeyCollected()
    {
        if (data.keyCollected) return;
        data.keyCollected = true;
        NotifyChange();
    }

    public bool WasObjectCollected(string id) => data.collectedObjects.Contains(id);
    public bool WasTargetUsed(string id) => data.usedTargets.Contains(id);

    public void MarkObjectCollected(string id)
    {
        if (string.IsNullOrEmpty(id) || data.collectedObjects.Contains(id)) return;
        data.collectedObjects.Add(id);
        NotifyChange();
    }

    public void MarkTargetUsed(string id)
    {
        if (string.IsNullOrEmpty(id) || data.usedTargets.Contains(id)) return;
        data.usedTargets.Add(id);
        NotifyChange();
    }

    // Guarda o endereço original antes de qualquer objeto coletado sair da cena.
    public static string ObjectId(GameObject target)
    {
        string path = "";
        for (Transform node = target.transform; node != null; node = node.parent)
        {
            int sameNameIndex = 0;
            if (node.parent != null)
            {
                foreach (Transform sibling in node.parent)
                {
                    if (sibling == node) break;
                    if (sibling.name == node.name) sameNameIndex++;
                }
            }
            else
            {
                foreach (GameObject sibling in target.scene.GetRootGameObjects())
                {
                    if (sibling.transform == node) break;
                    if (sibling.name == node.name) sameNameIndex++;
                }
            }
            path = "/" + Uri.EscapeDataString(node.name) + "[" + sameNameIndex + "]" + path;
        }
        return target.scene.path + path;
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
