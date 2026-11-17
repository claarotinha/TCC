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
        if (areaUltimaConsulta == null) areaUltimaConsulta = areaPostoEscolha;
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
        return valido;
    }

    private string ObjetivoConsulta() => string.Empty;

    private string InformacoesConsulta() => rotuloLugar + ": " +
        (Dados.curraisDestinoConfirmado ? lugarConsulta : naoConfirmado) + "\n" + rotuloNome + ": " +
        (Dados.curraisIdentidadeConfirmada ? nomeConsulta : naoConfirmado);

    private Collider2D[] AlvosConsulta() => MemoriaDisponivel ? AlvosMemoria() : new[] { areaBaldeRaimundo, areaRaimundo, areaMae, areaPaiAbrigo, areaLuzia, areaAmanhecer, areaUltimaConsulta, areaSaidaConsulta, areaRetornoConsulta };

    private void AtualizarConsulta()
    {
        bool ativa = ConsultaDisponivel;
        int etapa = Dados.curraisEtapaConsulta;
        setorConsulta.SetActive(ativa && etapa >= 1);
        bloqueioSaida.enabled = false;
        AtualizarFamiliaAlternativa();
        if (!ativa) { AtualizarMemoria(); return; }
        objetivoTexto.text = string.Empty;
        areaAmanhecer.enabled = etapa == 0;
        areaUltimaConsulta.enabled = etapa >= 1;
        areaMae.enabled = etapa >= 1;
        areaPaiAbrigo.enabled = etapa >= 1;
        areaLuzia.enabled = etapa >= 1;
        areaSaidaConsulta.enabled = etapa == 2;
        areaRetornoConsulta.enabled = etapa >= 3;
        if (etapa == 0) { AtualizarMemoria(); return; } // A noite permanece até a interação da manhã.
        AplicarNoite(false);
        lacoNaMao.SetActive(false); maoNaRoupa.SetActive(false); cabacaNoite.SetActive(false);
        papelConsulta.SetActive(true); // A relação continua sobre a mesa após a consulta.
        if (!papelEmConsulta) canhotoApresentadoConsulta.SetActive(false);
        testemunhaConsulta.SetActive(false);
        trabalhadorRetorno.SetActive(etapa != 1);
        lacoRemendadoCenario.SetActive(false);
        Posicionar(areaRaimundo.transform, Dados.curraisEscolhaPosto == 1 ? raimundoConsultaPerto : raimundoConsultaLonge);
        Posicionar(portaoConsulta, etapa >= 3 ? portaoConsultaFechado : portaoConsultaAberto);
        Posicionar(luzia.transform, luziaRepouso);
        if (etapa == 4)
        {
            Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        }
        else if (etapa == 1 && mari.transform.position.x >= maeLimiteConsulta.position.x)
            Posicionar(mae, maeLimiteConsulta);
        AtualizarMemoria();
        Physics2D.SyncTransforms();
    }

    private void AtualizarAcompanhamentoConsulta(bool livre)
    {
        int etapa = Dados.curraisEtapaConsulta;
        bool acompanhar = etapa >= 1 && etapa <= 3;
        seguidorPaiConsulta.enabled = livre && acompanhar;
        seguidoraLuzia.enabled = false;
        bool maeAcompanha = acompanhar && (etapa == 3 ||
            (etapa == 1 && mari.transform.position.x < maeLimiteConsulta.position.x));
        seguidoraMae.enabled = livre && maeAcompanha;
        if (etapa == 1 && !maeAcompanha) Posicionar(mae, maeLimiteConsulta);
    }

    private void RestaurarFamiliaConsulta()
    {
        if (Dados.curraisEtapaConsulta < 1 || Dados.curraisEtapaConsulta > 3) return;
        Vector3 destinoPai = pai.position;
        destinoPai.x = mari.transform.position.x - seguidorPaiConsulta.DistanciaAtras;
        pai.position = destinoPai;
        if (Dados.curraisEtapaConsulta == 3 || mari.transform.position.x < maeLimiteConsulta.position.x)
        {
            Vector3 destinoMae = mae.position;
            destinoMae.x = mari.transform.position.x - seguidoraMae.DistanciaAtras - 1f;
            mae.position = destinoMae;
        }
        else Posicionar(mae, maeLimiteConsulta);
        Physics2D.SyncTransforms();
    }

    private bool InteragirConsulta(Vector2 ponto)
    {
        if (MemoriaDisponivel) return InteragirMemoria(ponto);
        foreach (Collider2D area in AlvosConsulta())
        {
            if (!area.enabled || !area.gameObject.activeInHierarchy || !area.OverlapPoint(ponto)) continue;

            if (area == areaBaldeRaimundo) Iniciar(PegarBaldeRaimundo());
            else if (area == areaRaimundo) MenuRaimundoManha();
            else if (area == areaAmanhecer) Iniciar(AmanhecerNoCampo());
            else if (area == areaUltimaConsulta) MenuUltimaConsulta();
            else if (area == areaLuzia) Iniciar(Dizer(Dados.curraisEtapaConsulta <= 2 ? luziaConsultaFinal : luziaRetornoFinal));
            else if (area == areaMae || area == areaPaiAbrigo)
            {
                if (Dados.curraisEtapaConsulta == 3) Iniciar(Dizer(retornoAbrigoOrientacao));
                else MenuFamiliaConsulta();
            }
            else if (area == areaSaidaConsulta) Iniciar(PedirSaida());
            else if (Dados.curraisEtapaConsulta == 3) Iniciar(ConcluirRetorno());
            else MostrarOpcoes("Família", InformacoesConsulta(), opcoesRetornoConsulta, new Action[] {
                () => Iniciar(RelatarInformacoes()), FecharMenu });
            return true;
        }
        return false;
    }

    private IEnumerator AmanhecerNoCampo()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 0) yield break;
        yield return Escurecer(1f);
        yield return ChapterCard.Show("josefa-10");
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
        if (Dados.curraisEscolhaConsulta != 0)
        { Iniciar(Dizer(consultaConcluida)); return; }
        MostrarOpcoes("Escrivão", "Ainda posso conferir uma coisa antes de levarem a relação.",
            new[] { "Perguntar por Mariquinha", "Mostrar o canhoto", "Sair" },
            new Action[] { () => Iniciar(FazerUltimaConsulta()), () => Iniciar(ApresentarCanhotoConsulta()), FecharMenu });
    }

    private void MenuFamiliaConsulta()
    {
        MostrarOpcoes("Família", Dados.curraisEtapaConsulta == 1
            ? "Ainda podemos perguntar na mesa. Eu fico contigo."
            : "Vamos falar com o homem do portão. Tua irmã não vai sair do nosso pensamento.",
            new[] { "O que sabemos", "Ficar um pouco juntos", "Sair" },
            new Action[] { () => Iniciar(RelatarInformacoes()), () => Iniciar(Dizer(familiaCaderno)), FecharMenu });
    }

    private IEnumerator ApresentarCanhotoConsulta()
    {
        if (Dados.curraisEtapaConsulta != 1 || Dados.curraisEscolhaConsulta != 0) yield break;
        if (!TemItem(itemCanhoto))
        {
            yield return Dizer(semCanhotoConsulta);
            MenuUltimaConsulta();
            yield break;
        }
        papelEmConsulta = true;
        canhotoApresentadoConsulta.SetActive(true);
        try { yield return Dizer(papelApresentado); }
        finally { EncerrarDetalhesConsulta(); }
        MenuUltimaConsulta();
    }

    private void EncerrarDetalhesConsulta()
    {
        papelEmConsulta = false;
        if (canhotoApresentadoConsulta != null) canhotoApresentadoConsulta.SetActive(false);
        if (testemunhaConsulta != null) testemunhaConsulta.SetActive(false);
        RestaurarDesenhoSaida();
    }

    private IEnumerator FazerUltimaConsulta()
    {
        if (Dados.curraisEtapaConsulta != 1 || Dados.curraisEscolhaConsulta != 0) yield break;
        // Conversar na mesa preserva posição dos personagens e câmera.
        if (!Dados.curraisConsultaApresentada)
        {
            if (Dados.curraisEscolhaPosto == 1) yield return Dizer(memoriaRaimundo);
            yield return Dizer(manhaOrdens);
            yield return Dizer(consultaEscrivao);
            Dados.curraisConsultaApresentada = true;
            Salvar();
        }
        if (TemItem(itemCanhoto) && InventoryManager.Instance.SelectedItem == itemCanhoto)
        {
            papelEmConsulta = true;
            canhotoApresentadoConsulta.SetActive(true);
            try { yield return Dizer(papelApresentado); }
            finally { EncerrarDetalhesConsulta(); }
        }
        string[] rotulos = (string[])opcoesConsulta.Clone();
        if (Dados.curraisDestinoConfirmado) rotulos[0] += " (já confirmado)";
        if (Dados.curraisIdentidadeConfirmada) rotulos[1] += " (já confirmado)";
        yield return Escolher("Escrivão", InformacoesConsulta() + "\n" + perguntaConsulta, rotulos);
        int opcao = escolhaRecebida;
        if (opcao == 2) yield break;
        bool repetida = opcao == 0 ? Dados.curraisDestinoConfirmado : Dados.curraisIdentidadeConfirmada;
        if (opcao == 1) testemunhaConsulta.SetActive(true);
        try
        {
            yield return Dizer(opcao == 0 ? consultaDestino : consultaNome);
            if (repetida) yield return Dizer(opcao == 0 ? memoriaDestinoRepetido : memoriaNomeRepetido);
        }
        finally { EncerrarDetalhesConsulta(); }
        Dados.curraisEscolhaConsulta = opcao + 1;
        if (opcao == 0) Dados.curraisDestinoConfirmado = true;
        else Dados.curraisIdentidadeConfirmada = true;
        Dados.curraisEtapaConsulta = 2;
        GameProgress.Instance.UnlockDiaryPage(opcao == 0 ? "currais-ultima-destino" : "currais-ultima-nome");
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Memória", texto = InformacoesConsulta() } });
    }

    private IEnumerator PedirSaida()
    {
        if (!ConsultaDisponivel || Dados.curraisEtapaConsulta != 2) yield break;
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
        if (Dados.curraisDestinoConfirmado && Dados.curraisIdentidadeConfirmada) yield return Dizer(retornoDuasPistas);
        else if (Dados.curraisDestinoConfirmado) yield return Dizer(retornoDestino);
        else if (Dados.curraisIdentidadeConfirmada) yield return Dizer(retornoNome);
        yield return Dizer(Dados.curraisLacoGuardado ? retornoLaco : retornoSemLaco);
    }

    private IEnumerator ConcluirRetorno()
    {
        if (Dados.curraisEtapaConsulta != 3) yield break;
        yield return Dizer(aberturaEncerramento);
        MenuNomesFinais();
    }
}
