using System;
using System.Collections.Generic;

[Serializable]
public class GameProgressData
{
    public bool tutorialCompleted;
    public bool motherWorkConversationCompleted;
    public bool quartinhoUnlocked;
    public bool keyCollected;
    public bool chestOpened;
    public bool diaryCollected;
    public bool motherNightConversationCompleted;
    public bool firstDiaryPageRead;
    public bool nightCompleted;

    public List<string> collectedObjects = new List<string>();
    public List<string> usedTargets = new List<string>();

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();
    public List<string> unlockedDiaryPages = new List<string>();
}
