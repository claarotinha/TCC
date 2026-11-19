using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Ajuda temporária para testar Currais — F2")]
    [SerializeField] private bool mostrarAjudaTemporaria = true;
    [SerializeField] private GameObject painelAjudaTemporaria;

    private void LateUpdate()
    {
        AtualizarCenaNoFimDoQuadro();
        AtualizarAjudaTemporaria();
    }

    private void AtualizarAjudaTemporaria()
    {
        if (Input.GetKeyDown(KeyCode.F2)) mostrarAjudaTemporaria = !mostrarAjudaTemporaria;
        if (painelAjudaTemporaria != null) painelAjudaTemporaria.SetActive(mostrarAjudaTemporaria);
        if (objetivoTexto == null) return;
        objetivoTexto.gameObject.SetActive(mostrarAjudaTemporaria);
        objetivoTexto.enabled = mostrarAjudaTemporaria;
        if (!mostrarAjudaTemporaria) return;
        if (!PodeAtualizarCena)
        { objetivoTexto.text = "TESTE CURRAIS | Inicializando a cena.\nSe esta mensagem permanecer, confira os erros no Console.\nF2: ocultar ajuda"; return; }
        string estado = puzzleRetalhos != null && puzzleRetalhos.Aberto ? "Clique nos três retalhos para unir a costura. Sair permite tentar depois." :
            ExamineObject.IsShowing() ? "Examine aberto: clique novamente para fechar." :
            menuAberto ? "Escolha uma opção no balão. Sair fecha o menu." :
            dialogando ? "Clique para avançar a fala." :
            ocupado ? "Aguarde a ação em andamento." : ProximaAcaoTemporaria();
        objetivoTexto.text = "<b>TESTE CURRAIS — AJUDA TEMPORÁRIA</b>\n" + estado +
            "\nA/D: andar | Shift: correr | E/clique: interagir | Tab: inventário\nArraste o item até a pessoa/objeto. F2: ocultar/mostrar ajuda." +
            "\n<color=#E0C58E>Etapas: entrada " + Dados.curraisEtapaEntrada + " | busca " + Dados.curraisEtapaBusca +
            " | posto " + Dados.curraisEtapaPosto + " | consulta " + Dados.curraisEtapaConsulta + "</color>";
    }

    private string ProximaAcaoTemporaria()
    {
        if (Dados.curraisEtapaEntrada <= 1) return "Acompanhe a chegada. Clique para avançar as falas.";
        if (Dados.curraisEtapaEntrada == 2) return "Revista: abra o inventário e arraste os pertences até a mesa/fiscal. Depois fale sobre o caderno.";
        if (!Dados.curraisFamiliaParou) return "Caminhe com a família até a parada no abrigo.";
        if (!Dados.curraisPaiAcomodado) return !PedidoPanoConhecido ? "Fale com Luzia e com sua mãe para saber como ajudar o pai." :
            !TemItem(itemPanoAbrigo) ? "Recolha o pano do abrigo, confirmando a coleta." : "Arraste o pano do inventário até sua mãe para amparar o pai.";
        if (!ConviteAguaConhecido) return "Converse com Luzia sobre Mariquinha e o campo até receber a orientação da água.";
        if (Dados.curraisEscolhaAgua != 0 && !Dados.curraisPanelaConcluida)
        {
            if (!Dados.curraisMeninoVisitado) return "Converse com a criança que espera perto da mesa dos nomes.";
            if (!Dados.curraisRegistroConcluido) return "Fale com o escrivão na mesa dos nomes sobre Mariquinha.";
            if (!Dados.curraisLotePanosRecolhido) return "Recolha o lote de panos na mesa dos nomes. Confirme a coleta.";
            return "Examine o canhoto junto da panela e escolha perguntar pelo destino ou seguir com os panos.";
        }
        if (!Dados.curraisPanelaConcluida)
        {
            if (!CabacaAutorizada) return "Clique na funcionária da distribuição e fale da cabaça.";
            if (!Dados.curraisCabacaDevolvida) return "Recolha a cabaça vazia da mesa da distribuição.";
            if (!VezAguaChamada) return "Aguarde a funcionária chamar sua vez para buscar água.";
            if (!Dados.curraisAguaRecebida) return "Tab: arraste a cabaça vazia até a distribuição para enchê-la.";
            if (!ViuConversaEntrada("agua-retorno-abrigo")) return "Volte ao pai ou à mãe. Aproxime-se ou clique para conversar sobre a água.";
            return "Ofereça a água à mãe ou a Luzia pelo menu ou por arraste. A escolha define a acompanhante.";
        }
        if (!PostoDisponivel)
        {
            switch (Dados.curraisEtapaBusca)
            {
                case 0: return "Entregue o lote à lavadeira por arraste ou pela opção Entregar o lote de panos.";
                case 1: return "Arraste o lote de panos até a lavadeira para entregar e receber as instruções.";
                case 2: return Dados.curraisPanosSeparados != 3 ? "Clique na lavadeira e escolha Remendar o pano. Gire os retalhos para unir a costura." : "Recolha o retalho limpo separado pela lavadeira. Guarde-o para Raimundo.";
                case 3: return "Siga pela passagem vigiada. Converse com o guarda e acompanhe a lembrança.";
                case 4:
                    if (!Dados.curraisTrabalhoPatioIniciado) return "No pátio, fale com seu pai e acompanhe o trabalho de Bento.";
                    if (!ApoioPatioAutorizado) return "Fale com o trabalhador no apoio do pátio para receber as orientações.";
                    if (!Dados.curraisAguaPatioPreparada) return "Arraste a cabaça vazia até o ponto de água do pátio.";
                    if (!Dados.curraisPanosPatioEntregues) return "Arraste o pano entregue pelo pai até o apoio do pátio. Preserve o retalho de Raimundo.";
                    return "Leve a água até perto do pai e acompanhe o que acontece no pátio.";
                case 5:
                    if (!Dados.curraisBentoMorto) return (Dados.curraisItensAtendimentoBento & 3) == 3 ? "Aguarde o atendimento de Bento e a chegada da família." : "No atendimento, arraste o pano do pai e a cabaça cheia do inventário até Bento.";
                    if (!PertencesBentoResolvidos) return "Converse com o familiar de Bento e resolva a entrega dos pertences.";
                    if (!Dados.curraisCabacaSocorroRecolhida) return "Recolha sua cabaça vazia junto do atendimento.";
                    return "Converse com a acompanhante para seguir com a família ao sepultamento.";
            }
        }
        if (Dados.curraisEtapaPosto == 0) return "No posto, examine Raimundo e a mesa. Escolha ajudar ou copiar a folha; Sair adia.";
        if (Dados.curraisEtapaPosto == 1) return "Volte a Raimundo no posto para concluir a conversa.";
        if (Dados.curraisEtapaPosto == 2) return "Fale com Raimundo ou com a família para concluir a conversa e acompanhar as semanas de procura.";
        if (Dados.curraisEtapaPosto == 3) return "Converse com a família se quiser. Na esteira, escolha ficar perto da mãe para iniciar a noite.";
        if (Dados.curraisEtapaConsulta == 0) return "Clique na esteira para passar ao amanhecer.";
        if (Dados.curraisEtapaConsulta == 1)
        {
            if (!ViuConversaEntrada("raimundo-manha-encontro") && !Dados.curraisIdentidadeConfirmada) return "Fale com Raimundo antes da última consulta. Perguntar confirma o nome; proteger a alça é opcional.";
            if (Dados.curraisBaldeRecolhido && !Dados.curraisBaldeApoiado) return "Ajuda opcional: combine balde + tira limpa no inventário e devolva por arraste. Para seguir a busca, vá ao escrivão.";
            return "Vá à mesa do escrivão com o pai. O canhoto é opcional. Escolha uma informação para confirmar.";
        }
        if (Dados.curraisEtapaConsulta == 2) return "Siga com a família até o cabo no portão e peça para sair.";
        if (Dados.curraisEtapaConsulta == 3) return !ViuConversaEntrada("consulta-relato-final") ? "Volte ao abrigo, perto de Luzia, e conte o que ficou confirmado." : "Converse com a família se quiser. Na esteira, Tentar descansar inicia a passagem do tempo.";
        return Dados.ato2Concluido ? "Retornando ao presente e ao epílogo." : "Acompanhe as três passagens do tempo. O retorno ao presente acontece automaticamente.";
    }
}
