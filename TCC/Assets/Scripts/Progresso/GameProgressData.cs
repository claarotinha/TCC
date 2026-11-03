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

    // Evita repetir a conversa inicial ao continuar a viagem.
    public bool estradaOpeningCompleted;
    public bool estradaElasticoEntregue;
    // 0: nenhuma; 1: Corpo-seco; 2: Comadre Fulozinha; 3: Mae-D'Agua.
    public int estradaHistoriaEscolhida;
    // 0: nao escolheu; 1: porta; 2: janela.
    public int estradaEntradaAbrigo;
    public bool estradaMariquinhaFerida;
    public bool estradaAbrigoConcluido;
    public int estradaParadasConcluidas;
    public bool estradaTerceiraAguaIniciada;
    public bool estradaDialogoBarrigudaConcluido;
    public bool estradaBarrigudaConcluida;
    public bool estradaPlantaMedicinalColetada;
    public bool estradaCurativoPreparado;
    public bool estradaCurativoAplicado;
    public bool estradaEncontroVisitado;
    public bool estradaEncontroConcluido;
    // Bits 0, 1 e 2: perguntas respondidas; nenhuma e obrigatoria.
    public int estradaPerguntasAntonio;
    public bool estradaJuazeiroIniciado;
    public bool estradaPlacaJuazeiroLida;
    public bool estradaLetrasJuazeiroConcluidas;
    public bool estradaJuazeiroConcluido;
    public bool estradaAcampamentoIniciado;
    public bool estradaIntroAcampamentoConcluida;
    // Bits 0, 1 e 2: galhos secos, folhas secas e alecrim coletados.
    public int estradaMateriaisAcampamento;
    public bool estradaFogueiraPreparada;
    public bool estradaPedidoAlecrimConcluido;
    public bool estradaRefeicaoPreparada;
    public bool estradaConversaFogueiraConcluida;
    // Primeira: 0 nenhuma, 1 silencio, 2 responder.
    public int estradaPrimeiraEscolhaNoturna;
    // Segunda: 0 nenhuma, 1 promessa, 2 tranquilizar, 3 silencio.
    public int estradaSegundaEscolhaNoturna;
    public bool estradaConversaIrmasConcluida;
    public bool estradaConversaPaisConcluida;
    public bool estradaAcampamentoConcluido;

    public bool CanSave => tutorialCompleted ||
        (secaDepartureCompleted && secaMealCompleted &&
         secaFoodChoice >= 1 && secaFoodChoice <= 3);

    public List<string> collectedObjects = new List<string>();
    public List<string> usedTargets = new List<string>();

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();

    public List<string> unlockedDiaryPages = new List<string>();
}



