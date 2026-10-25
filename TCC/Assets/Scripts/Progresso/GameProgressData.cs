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

    // Registrado somente ao sair da Seca, depois da despedida.
    // Permite testar este capítulo diretamente sem marcar o tutorial como concluído.
    public bool secaDepartureCompleted;

    public bool CanSave => tutorialCompleted ||
        (secaDepartureCompleted && secaMealCompleted &&
         secaFoodChoice >= 1 && secaFoodChoice <= 3);

    public List<string> collectedObjects = new List<string>();
    public List<string> usedTargets = new List<string>();

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();

    public List<string> unlockedDiaryPages = new List<string>();
}
