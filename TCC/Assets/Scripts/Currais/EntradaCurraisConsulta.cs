using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Última consulta, portão e retorno: continuação do componente e do Canvas já existentes.
public partial class EntradaCurrais
{
    [Header("Etapa 5 - manhã e última consulta")]
    [SerializeField] private GameObject setorConsulta, papelConsulta, testemunhaConsulta, lacoRemendadoCenario;
    [SerializeField] private Collider2D areaAmanhecer, areaUltimaConsulta, areaSaidaConsulta, areaRetornoConsulta, bloqueioSaida;
    [SerializeField] private SeguidorFamilia seguidorPaiConsulta;
    [SerializeField] private Transform maeLimiteConsulta, raimundoConsultaPerto, raimundoConsultaLonge, paiUltimaConsulta, mariUltimaConsulta;
    [SerializeField] private Transform portaoConsulta, portaoConsultaAberto, portaoConsultaFechado;
    [SerializeField] private Transform mariSaidaConsulta, paiSaidaConsulta, maeSaidaConsulta;
    [SerializeField] private Fala[] manhaOrdens, consultaEscrivao, consultaDestino, consultaNome, saidaRecusada;
    [SerializeField] private Fala[] retornoDuasPistas, retornoDestino, retornoNome, retornoLaco, retornoSemLaco;
    [SerializeField] private Fala[] memoriaDestinoRepetido, memoriaNomeRepetido, memoriaRaimundo;
    [SerializeField] private string[] opcoesConsulta, opcoesRetornoConsulta, objetivosConsulta;
    [SerializeField, TextArea(2, 5)] private string perguntaConsulta;
    [SerializeField] private string rotuloLugar, rotuloNome, lugarConsulta, nomeConsulta, naoConfirmado;
    [Header("Última consulta e saída — elementos editáveis na cena")]
    [SerializeField] private Transform cameraSaidaConsulta;
    [SerializeField] private GameObject canhotoApresentadoConsulta;
    [SerializeField] private Fala[] consultaConcluida, semCanhotoConsulta, retornoAbrigoOrientacao;
    [SerializeField] private Fala[] luziaConsultaFinal, luziaRetornoFinal;
    [Header("Porteira — saída recusada")]
    [SerializeField] private GameObject marcadorPorteira;
    [SerializeField] private Collider2D[] guardasPorcosSaida;
    private bool papelEmConsulta;
    private readonly Dictionary<SpriteRenderer, int> ordensSaida = new Dictionary<SpriteRenderer, int>();

    private void PrepararDesenhoSaida()
    {
        RestaurarDesenhoSaida();
        Transform[] familia = { mari.transform, pai, mae };
        foreach (Transform pessoa in familia)
            foreach (SpriteRenderer desenho in pessoa.GetComponentsInChildren<SpriteRenderer>(true))
            {
                ordensSaida[desenho] = desenho.sortingOrder;
                desenho.sortingOrder += 40;
            }
    }

    private void RestaurarDesenhoSaida()
    {
        foreach (KeyValuePair<SpriteRenderer, int> desenho in ordensSaida)
            if (desenho.Key != null) desenho.Key.sortingOrder = desenho.Value;
        ordensSaida.Clear();
    }

    private bool ConsultaDisponivel => PostoDisponivel && Dados.curraisEtapaPosto >= 4;

    private bool ReferenciasConsultaValidas()
    {
        // São as mesmas áreas reutilizadas da etapa 4, não novos objetos.
        // Recupera a ligação caso uma cópia antiga da cena tenha importado o campo vazio.
        if (areaAmanhecer == null) areaAmanhecer = areaEsteiraNoite;
        if (areaUltimaConsulta == null) areaUltimaConsulta = areaRegistro;
        if (areaRetornoConsulta == null) areaRetornoConsulta = areaFamiliaAbrigo;

        UnityEngine.Object[] referencias = { setorConsulta, papelConsulta, testemunhaConsulta, lacoRemendadoCenario,
            areaAmanhecer, areaUltimaConsulta, areaSaidaConsulta, areaRetornoConsulta, bloqueioSaida, seguidorPaiConsulta,
            maeLimiteConsulta, raimundoConsultaPerto, raimundoConsultaLonge, paiUltimaConsulta, mariUltimaConsulta, portaoConsulta, portaoConsultaAberto,
            portaoConsultaFechado, mariSaidaConsulta, paiSaidaConsulta, maeSaidaConsulta };
        string[] nomes = { nameof(setorConsulta), nameof(papelConsulta), nameof(testemunhaConsulta), nameof(lacoRemendadoCenario),
            nameof(areaAmanhecer), nameof(areaUltimaConsulta), nameof(areaSaidaConsulta), nameof(areaRetornoConsulta),
            nameof(bloqueioSaida), nameof(seguidorPaiConsulta), nameof(maeLimiteConsulta), nameof(raimundoConsultaPerto),
            nameof(raimundoConsultaLonge), nameof(paiUltimaConsulta), nameof(mariUltimaConsulta), nameof(portaoConsulta), nameof(portaoConsultaAberto), nameof(portaoConsultaFechado),
            nameof(mariSaidaConsulta), nameof(paiSaidaConsulta), nameof(maeSaidaConsulta) };
        bool valido = ConferirReferencia(cameraSaidaConsulta, nameof(cameraSaidaConsulta)) &
            ConferirReferencia(canhotoApresentadoConsulta, nameof(canhotoApresentadoConsulta));
        for (int i = 0; i < referencias.Length; i++) valido &= ConferirReferencia(referencias[i], nomes[i]);
        Fala[][] sequencias = { manhaOrdens, consultaEscrivao, consultaDestino, consultaNome, saidaRecusada,
            retornoDuasPistas, retornoDestino, retornoNome, retornoLaco, retornoSemLaco,
            memoriaDestinoRepetido, memoriaNomeRepetido, memoriaRaimundo, consultaConcluida,
            semCanhotoConsulta, retornoAbrigoOrientacao, luziaConsultaFinal, luziaRetornoFinal };
        foreach (Fala[] falas in sequencias) valido &= FalasValidas(falas);
        if (opcoesConsulta == null || opcoesConsulta.Length != 3 || opcoesRetornoConsulta == null ||
            opcoesRetornoConsulta.Length != 2 || objetivosConsulta == null || objetivosConsulta.Length != 5) valido = false;
        string[] textos = { perguntaConsulta, rotuloLugar, rotuloNome, lugarConsulta, nomeConsulta, naoConfirmado };
        foreach (string texto in textos) valido &= !string.IsNullOrWhiteSpace(texto);
        return valido && ConferirReferencia(marcadorPorteira, nameof(marcadorPorteira)) &&
            guardasPorcosSaida != null && guardasPorcosSaida.Length > 0 && ConferirReferencia(puzzleRegistro, nameof(puzzleRegistro)) && ConferirReferencia(exameMesaUltimaConsulta, nameof(exameMesaUltimaConsulta)) && ConferirReferencia(exameRaimundoManha, nameof(exameRaimundoManha)) &&
            ConferirReferencia(exameRaimundoManha.examinePanel, "Painel Examine — manhã") &&
            ConferirReferencia(exameRaimundoManha.examineText, "Texto Examine — manhã");
    }

    private string ObjetivoConsulta() => string.Empty;

    private string InformacoesConsulta() => rotuloLugar + ": " +
        (Dados.curraisDestinoConfirmado ? lugarConsulta : naoConfirmado) + "\n" + rotuloNome + ": " +
        (Dados.curraisIdentidadeConfirmada ? nomeConsulta : naoConfirmado);

    private Collider2D[] AlvosConsulta()
    {
        if (MemoriaDisponivel) return AlvosMemoria();
        var alvos = new List<Collider2D> { areaBaldeRaimundo, areaPanoLavagem, areaRaimundo, areaMae,
            areaPaiAbrigo, areaLuzia, areaAmanhecer, areaEsteiraNoite, areaUltimaConsulta,
            areaSaidaConsulta, areaRetornoConsulta };
        if (Dados.curraisEtapaConsulta == 2) alvos.AddRange(guardasPorcosSaida);
        return alvos.ToArray();
    }

    private void AtualizarConsulta()
    {
        bool ativa = ConsultaDisponivel;
        int etapa = Dados.curraisEtapaConsulta;
        setorConsulta.SetActive(true);
        bloqueioSaida.enabled = false;
        marcadorPorteira.SetActive(ativa && etapa == 2);
        if (ativa)
            foreach (Collider2D guarda in guardasPorcosSaida) guarda.enabled = etapa == 2;
        AtualizarFamiliaAlternativa();
        if (!ativa) { AtualizarMemoria(); return; }
        objetivoTexto.text = string.Empty;
        areaAmanhecer.enabled = etapa == 0;
        areaUltimaConsulta.enabled = etapa >= 1 && (etapa > 1 || FamiliaNaConsulta || Dados.curraisConsultaApresentada ||
            ViuConversaEntrada("raimundo-manha-encontro") || Dados.curraisIdentidadeConfirmada);
        areaMae.enabled = etapa >= 1;
        areaPaiAbrigo.enabled = etapa >= 1;
        areaLuzia.enabled = etapa >= 1;
        areaSaidaConsulta.enabled = etapa == 2;
        areaRetornoConsulta.enabled = etapa >= 3;
        areaEsteiraNoite.enabled = etapa == 0 || (etapa == 3 && ViuConversaEntrada("consulta-relato-final"));
        if (etapa == 0) { AtualizarMemoria(); return; } // A noite permanece até a interação da manhã.
        areaPanoLavagem.gameObject.SetActive(RetalhoDisponivelManha);
        areaPanoLavagem.enabled = RetalhoDisponivelManha;
        AplicarNoite(etapa >= 3);
        lacoNaMao.SetActive(false); maoNaRoupa.SetActive(false); cabacaNoite.SetActive(false);
        papelConsulta.SetActive(Dados.curraisEscolhaConsulta == 0);
        if (!papelEmConsulta) canhotoApresentadoConsulta.SetActive(false);
        testemunhaConsulta.SetActive(true);
        Vector3 pertoDoEscrivao = testemunhaConsulta.transform.position;
        pertoDoEscrivao.x = areaRegistro.transform.position.x + 3f;
        testemunhaConsulta.transform.position = pertoDoEscrivao;
        trabalhadorRetorno.SetActive(true);
        lacoRemendadoCenario.SetActive(false);
        Posicionar(areaRaimundo.transform, Dados.curraisEscolhaPosto == 1 ? raimundoConsultaPerto : raimundoConsultaLonge);
        Posicionar(portaoConsulta, etapa >= 3 ? portaoConsultaFechado : portaoConsultaAberto);
        if (FamiliaNaConsulta || !AntesDaUltimaConsulta || Dados.curraisEscolhaAgua == 1)
            Posicionar(luzia.transform, etapa >= 3 ? luziaNoite : luziaRepouso);
        if (AntesDaUltimaConsulta && !FamiliaNaConsulta)
        {
            Posicionar(pai, paiAbrigo);
            if (Dados.curraisEscolhaAgua != 1) Posicionar(mae, maeAbrigo);
        }
        if (etapa == 4)
        {
            Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        }
        else if (etapa <= 2 && FamiliaNaConsulta && !MaeNaConsulta) Posicionar(mae, maeAbrigo);
        AtualizarMemoria();
        Physics2D.SyncTransforms();
    }

    private void AtualizarAcompanhamentoConsulta(bool livre)
    {
        if (AntesDaUltimaConsulta && !FamiliaNaConsulta) { AtualizarCompanhiaManha(livre); return; }
        int etapa = Dados.curraisEtapaConsulta;
        bool acompanhar = etapa >= 1 && etapa <= 3 && !ViuConversaEntrada("consulta-retorno-chegada");
        seguidorPaiConsulta.enabled = livre && acompanhar;
        seguidoraLuzia.enabled = false;
        bool maeAcompanha = acompanhar && (etapa == 3 || MaeNaConsulta);
        seguidoraMae.enabled = livre && maeAcompanha;
        if (etapa <= 2 && !maeAcompanha) Posicionar(mae, maeAbrigo);
    }

    private void RestaurarFamiliaConsulta()
    {
        if (Dados.curraisEtapaConsulta < 1 || Dados.curraisEtapaConsulta > 3 ||
            ViuConversaEntrada("consulta-retorno-chegada")) return;
        if (AntesDaUltimaConsulta && !FamiliaNaConsulta) return;
        Vector3 destinoPai = pai.position;
        destinoPai.x = mari.transform.position.x - seguidorPaiConsulta.DistanciaAtras;
        pai.position = destinoPai;
        if (Dados.curraisEtapaConsulta == 3 || MaeNaConsulta)
        {
            Vector3 destinoMae = mae.position;
            destinoMae.x = mari.transform.position.x - seguidoraMae.DistanciaAtras - 1f;
            mae.position = destinoMae;
        }
        else Posicionar(mae, maeAbrigo);
        Physics2D.SyncTransforms();
    }

    private bool InteragirConsulta(Vector2 ponto)
    {
        if (MemoriaDisponivel) return InteragirMemoria(ponto);
        if (InteragirFamiliaNomeNovo(ponto)) return true;
        foreach (Collider2D area in AlvosConsulta())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            if (area == areaBaldeRaimundo) Iniciar(PegarBaldeRaimundo());
            else if (area == areaPanoLavagem) Iniciar(RecolherRetalhoManha());
            else if (area == areaRaimundo) MenuRaimundoManha();
            else if (area == areaAmanhecer && Dados.curraisEtapaConsulta == 0) Iniciar(AmanhecerNoCampo());
            else if (area == areaEsteiraNoite && Dados.curraisEtapaConsulta == 3) MenuEsteiraFinal();
            else if (area == areaUltimaConsulta) MenuUltimaConsulta();
            else if (area == areaSaidaConsulta) Iniciar(PedirSaida());
            else if (Dados.curraisEtapaConsulta == 3) Iniciar(ChegarAbrigoFinal());
            else if (area == areaMae || area == areaPaiAbrigo) MenuFamiliaConsulta();
            return true;
        }
        return false;
    }

    private IEnumerator AmanhecerNoCampo()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 0) yield break;
        yield return Escurecer(1f);
        Dados.curraisEtapaConsulta = 1;
        AtualizarEstadoBusca();
        Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        RestaurarFamiliaConsulta();
        // Checkpoint antes da conversa: Continue não repete a passagem da noite.
        Salvar();
        yield return Escurecer(0f);
        yield return Dizer(amanhecerFamilia);
    }

    private void MenuUltimaConsulta()
    {
        Iniciar(ClicarMesaUltimaConsulta());
    }

    private void MenuFamiliaConsulta()
    {
        if (Dados.curraisEtapaConsulta == 1)
        {
            MostrarOpcoes("Pai", string.Empty,
                new[] { "Tentar mais uma vez", "Relembrar as pistas", "Sair" },
                new Action[] { () => Iniciar(OrientarUltimaTentativa()),
                    () => Iniciar(RelembrarPistasConsulta()), FecharMenu });
        }
        else if (Dados.curraisEtapaConsulta == 2)
            MenuAssuntoNPC("Pai", string.Empty, "E agora?", OrientarPortaoConsulta);
        else FecharMenu();
    }

    private IEnumerator OrientarUltimaTentativa()
    {
        if (Dados.curraisEtapaConsulta != 1) yield break;
        yield return FalarPosto("Zefinha", "Faz tanto tempo, painho... E ninguém diz onde ela tá.",
            "Pai", "Vamos tentar na mesa do registro mais uma vez. Eu e tua mãe vai contigo.",
            "Mãe", "Pergunte pelo nome dela. Nós espera até escutarem.");
        RegistrarConversaEntrada("consulta-ultima-tentativa-orientada"); Salvar();
    }

    private IEnumerator RelembrarPistasConsulta()
    {
        if (Dados.curraisDestinoConfirmado)
            yield return FalarPosto("Zefinha", "Lagoa do Meio. Era o destino escrito pro grupo. Mas ninguém confirmou se ela chegou lá.");
        if (Dados.curraisIdentidadeConfirmada)
            yield return FalarPosto("Zefinha", "Nair Pereira tava com Mariquinha. Eu preciso saber o que aconteceu depois.");
        if (!Dados.curraisDestinoConfirmado && !Dados.curraisIdentidadeConfirmada)
            yield return FalarPosto("Zefinha", "Só sei que levaram ela com outro grupo. Preciso que procurem o nome dela no registro.");
    }

    private IEnumerator OrientarPortaoConsulta()
    {
        yield return FalarPosto("Pai", "Vamos ao portão. Se num dizem onde ela tá, precisam deixar nós procurar.");
    }

    private bool SobreGuardaPorcoSaida(Vector2 ponto)
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 2) return false;
        foreach (Collider2D guarda in guardasPorcosSaida)
            if (guarda.enabled && guarda.gameObject.activeInHierarchy && guarda.OverlapPoint(ponto)) return true;
        return false;
    }

    private bool AtualizarTentativaDePassagem()
    {
        if (ocupado || menuAberto || InventarioAbertoMemoria() || PainelAuxiliarAberto() ||
            !ConsultaDisponivel || Dados.curraisEtapaConsulta != 2) return false;
        float ladoDeDentro = Mathf.Sign(mariAbrigo.position.x - areaSaidaConsulta.bounds.center.x);
        if ((mari.transform.position.x - areaSaidaConsulta.bounds.center.x) * ladoDeDentro > 0f) return false;
        Iniciar(PedirSaida());
        return true;
    }

    private void MenuPedidoSaida()
    {
        if (Dados.curraisEtapaConsulta != 2) return;
        MostrarOpcoes("Zefinha", string.Empty,
            new[] { "Pedir para sair", "Relembrar as pistas", "Sair" },
            new Action[] { () => Iniciar(PedirSaida()),
                () => Iniciar(RelembrarPistasConsulta()), FecharMenu });
    }

    private IEnumerator ApresentarCanhotoConsulta()
    {
        yield return MostrarCanhotoLinear();
    }

    private void EncerrarDetalhesConsulta()
    {
        papelEmConsulta = false;
        if (canhotoApresentadoConsulta != null) canhotoApresentadoConsulta.SetActive(false);
        if (testemunhaConsulta != null) testemunhaConsulta.SetActive(true);
        RestaurarDesenhoSaida();
    }

    private IEnumerator FazerUltimaConsulta()
    {
        yield return ConsultaFinalLinear();
    }

    private IEnumerator PedirSaida()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 2) yield break;
        yield return ReunirFamiliaPortao();
        Vector3 antesMari = mari.transform.position, antesPai = pai.position, antesMae = mae.position;
        Vector3 antesCamera = cameraCena.transform.position;
        try
        {
            yield return Escurecer(1f);
            Posicionar(mari.transform, mariSaidaConsulta);
            Posicionar(pai, paiSaidaConsulta); Posicionar(mae, maeSaidaConsulta);
            cameraCena.transform.position = cameraSaidaConsulta.position;
            PrepararDesenhoSaida();
            yield return Escurecer(0f);
            yield return Dizer(saidaRecusada);
            yield return Mover(portaoConsulta, portaoConsultaFechado.position, tempoPlano);
            Dados.curraisEtapaConsulta = 3;
            GameProgress.Instance.UnlockDiaryPage("currais-saida");
            AtualizarEstadoBusca();
            // Mantém a composição do portão; ninguém é puxado pelo seguidor durante a fala.
            Salvar();
            yield return Dizer(retornoAbrigoOrientacao);
        }
        finally
        {
            RestaurarDesenhoSaida();
            if (Dados.curraisEtapaConsulta == 2)
            {
                mari.transform.position = antesMari; pai.position = antesPai; mae.position = antesMae;
                cameraCena.transform.position = antesCamera;
                Posicionar(portaoConsulta, portaoConsultaAberto);
            }
            Physics2D.SyncTransforms();
        }
    }

    private IEnumerator RelatarInformacoes()
    {
        yield return RelatoFinalLinear();
    }

    private IEnumerator ConcluirRetorno()
    {
        yield return RelatoFinalLinear();
    }
}
