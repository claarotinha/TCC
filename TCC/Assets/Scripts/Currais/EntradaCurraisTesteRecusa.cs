using UnityEngine;

public partial class EntradaCurrais
{
    private bool PrepararTesteRecusa(bool restaurandoPartida)
    {
        if (!Application.isEditor || !CheatRecusaCurrais.Ativo || restaurandoPartida) return false;
        GameProgress.Instance.ResetProgress();
        // Os marcos que habilitam CanSave continuam falsos nesta sessão de teste.
        Dados.ato1Concluido = true;
        Dados.curraisEtapaEntrada = 4;
        Dados.curraisRevistaApresentada = true;
        Dados.curraisPertencesRetidos = true;
        Dados.curraisAbrigoApresentado = true;
        Dados.curraisLuziaApresentada = true;
        Dados.curraisFamiliaParou = true;
        Dados.curraisPaiAcomodado = true;
        Dados.curraisPaiFerido = true;
        Dados.curraisDistribuicaoIniciada = true;
        Dados.curraisAguaOrientada = true;
        Dados.curraisAguaRecebida = true;
        Dados.curraisCabacaDevolvida = true;
        Dados.curraisEscolhaAgua = 1;
        Dados.curraisRegistroConcluido = true;
        Dados.curraisTarefaPanosIniciada = true;
        Dados.curraisLotePanosRecolhido = true;
        Dados.curraisLotePanosEntregue = true;
        Dados.curraisPanelaConcluida = true;
        Dados.curraisPanosSeparados = 3;
        Dados.curraisTiraPanoRecolhida = true;
        Dados.curraisPassagemVigiadaConcluida = true;
        Dados.curraisRaimundoConhecido = true;
        Dados.curraisEtapaBusca = 6;
        Dados.curraisBentoMorto = true;
        Dados.curraisPanoSepultamentoEntregue = true;
        Dados.curraisCabacaSocorroRecolhida = true;
        Dados.curraisSepultamentoConcluido = true;
        Dados.curraisEscolhaFuneral = 1;
        Dados.curraisEscolhaPosto = 1;
        Dados.curraisEtapaPosto = 4;
        Dados.curraisEtapaConsulta = 2;
        Dados.curraisEscolhaConsulta = 1;
        Dados.curraisConsultaApresentada = true;
        Dados.curraisDestinoConfirmado = true;
        Dados.curraisIdentidadeConfirmada = true;
        RegistrarConversaEntrada("raimundo-encontro-encerrado");
        RegistrarConversaEntrada("quarto-retorno-familia");
        RegistrarConversaEntrada("consulta-familia-reunida");
        RegistrarConversaEntrada("consulta-familia-apos-semanas");
        RegistrarConversaEntrada("consulta-registro-visual-organizado");
        RegistrarConversaEntrada("consulta-familia-portao");
        Dados.chapterCardsSeen.Add("josefa-06");
        Dados.chapterCardsSeen.Add("josefa-10");
        InventoryManager.Instance.ReplaceItems(new[] { cabacaVaziaBusca, itemCanhoto });
        Debug.Log("CHEAT recusa ativo. Desative ou remova o GameObject CHEAT para jogar normalmente. O save real não será alterado.", this);
        return true;
    }

    private void PosicionarInicioTesteRecusa()
    {
        fundoPretoSeparacao.SetActive(false);
        foreach (GameObject quadro in quadrosEntrada) if (quadro != null) quadro.SetActive(false);
        mostrarAjudaTemporaria = false;
        if (painelAjudaTemporaria != null) painelAjudaTemporaria.SetActive(false);
        objetivoTexto.gameObject.SetActive(false);
        Posicionar(mari.transform, mariSaidaConsulta);
        Posicionar(pai, paiSaidaConsulta); Posicionar(mae, maeSaidaConsulta);
        Physics2D.SyncTransforms();
        cameraCena.transform.position = cameraSaidaConsulta.position;
        CorFade(0f);
    }
}
