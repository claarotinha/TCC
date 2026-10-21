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

    // Progresso da refeição na casa antiga.
    public bool secaMealCompleted;

    // 0: nenhuma escolha.
    // 1: comeu tudo.
    // 2: dividiu com Baleia.
    // 3: dividiu com a irmã.
    public int secaFoodChoice;

    public List<string> collectedObjects = new List<string>();
    public List<string> usedTargets = new List<string>();

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();

    public List<string> unlockedDiaryPages = new List<string>();
}