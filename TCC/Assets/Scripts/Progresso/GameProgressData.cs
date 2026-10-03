using System;
using System.Collections.Generic;

[Serializable]
public class GameProgressData
{
    public bool tutorialCompleted;
    public bool motherWorkConversationCompleted;
    public bool quartinhoUnlocked;
    public bool chestOpened;
    public bool diaryCollected;
    public bool motherNightConversationCompleted;
    public bool firstDiaryPageRead;
    public bool nightCompleted;

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();
    public List<string> unlockedDiaryPages = new List<string>();
}