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
    public bool estradaFortalezaIniciada;
    public bool estradaDialogoFortalezaConcluido;
    public bool estradaAbordagemIniciada;
    public bool ato1Concluido;

    // Ato 2, entrada: 0 separação, 1 laço, 2 revista, 3 abrigo, 4 setor livre.
    public int curraisEtapaEntrada;
    public bool curraisPaiFerido;
    public bool curraisLacoRecolhido;
    public bool curraisLacoNaPanela;
    public bool curraisPertencesRetidos;
    public List<string> curraisItensRetidos = new List<string>();

    // Ato 2, etapa 2: escolhas independentes, preservadas para as etapas seguintes.
    public bool curraisDistribuicaoIniciada;
    public bool curraisCabacaDevolvida;
    public bool curraisAguaRecebida;
    // 0 sem escolha, 1 mãe, 2 Luzia. Não determina um final.
    public int curraisEscolhaAgua;
    public bool curraisMeninoVisitado;
    public int curraisPerguntasMenino;
    public bool curraisRegistroConcluido;
    public bool curraisPanelaConcluida;
    // 0 sem escolha, 1 laço, 2 canhoto.
    public int curraisEscolhaPertence;
    public bool curraisLacoGuardado;
    public bool curraisDestinoConfirmado;
    public bool curraisIdentidadeConfirmada;

    // Etapa 3: 0 Raimundo, 1 lavagem, 2 apoiar pano, 3 corredor, 4 pátio, 5 atendimento, 6 concluída.
    public int curraisEtapaBusca;
    public bool curraisRaimundoConhecido;
    public int curraisPerguntasRaimundo;

    // Etapa 4: 0 posto, 1 quarto, 2 notícias aos pais, 3 preparar noite, 4 noite concluída.
    public int curraisEtapaPosto;
    // 0 sem decisão, 1 ajudar Raimundo, 2 copiar o registro.
    public int curraisEscolhaPosto;
    public bool curraisRefeicaoRelembrada;
    // 0 sem decisão, 1 contar a última fala, 2 pedir companhia.
    public int curraisEscolhaNoite;

    // Etapa 5: 0 amanhecer, 1 consulta, 2 pedido de saída, 3 retorno, 4 concluída.
    public int curraisEtapaConsulta;
    // 0 sem decisão, 1 confirmar destino, 2 confirmar acompanhante.
    public int curraisEscolhaConsulta;
    public bool curraisConsultaApresentada;
    public bool curraisLacoRemendado;

    // Etapa 6: número de passagens de tempo já lidas (0..3), sem alterar pistas ou final.
    public bool curraisMemoriaIniciada;
    public int curraisPaginaMemoria;
    public bool ato2Concluido;

    public bool CanSave => tutorialCompleted ||
        (secaDepartureCompleted && secaMealCompleted &&
         secaFoodChoice >= 1 && secaFoodChoice <= 3);

    public List<string> collectedObjects = new List<string>();
    public List<string> usedTargets = new List<string>();

    public List<int> foundPhotos = new List<int>();
    public List<int> readPhotos = new List<int>();

    public List<string> unlockedDiaryPages = new List<string>();
}




