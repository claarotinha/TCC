using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Etapa 5, cenas 17–20: continuação do componente e do Canvas já existentes.
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
        bool valido = true;
        for (int i = 0; i < referencias.Length; i++) valido &= ConferirReferencia(referencias[i], nomes[i]);
        Fala[][] sequencias = { manhaOrdens, consultaEscrivao, consultaDestino, consultaNome, saidaRecusada,
            retornoDuasPistas, retornoDestino, retornoNome, retornoLaco, retornoSemLaco,
            memoriaDestinoRepetido, memoriaNomeRepetido, memoriaRaimundo };
        foreach (Fala[] falas in sequencias) valido &= FalasValidas(falas);
        if (opcoesConsulta == null || opcoesConsulta.Length != 3 || opcoesRetornoConsulta == null ||
            opcoesRetornoConsulta.Length != 2 || objetivosConsulta == null || objetivosConsulta.Length != 5) valido = false;
        string[] textos = { perguntaConsulta, rotuloLugar, rotuloNome, lugarConsulta, nomeConsulta, naoConfirmado };
        foreach (string texto in textos) valido &= !string.IsNullOrWhiteSpace(texto);
        return valido;
    }

    private string ObjetivoConsulta() => MemoriaDisponivel ? ObjetivoMemoria() : objetivosConsulta[Mathf.Clamp(Dados.curraisEtapaConsulta, 0, 4)];

    private string InformacoesConsulta() => rotuloLugar + ": " +
        (Dados.curraisDestinoConfirmado ? lugarConsulta : naoConfirmado) + "\n" + rotuloNome + ": " +
        (Dados.curraisIdentidadeConfirmada ? nomeConsulta : naoConfirmado);

    private Collider2D[] AlvosConsulta() => MemoriaDisponivel ? AlvosMemoria() : new[] { areaAmanhecer, areaUltimaConsulta, areaSaidaConsulta, areaRetornoConsulta };

    private void AtualizarConsulta()
    {
        bool ativa = ConsultaDisponivel;
        int etapa = Dados.curraisEtapaConsulta;
        setorConsulta.SetActive(ativa && etapa >= 1);
        bloqueioSaida.enabled = ativa && etapa >= 1;
        if (!ativa) { AtualizarMemoria(); return; }
        objetivoTexto.text = ObjetivoConsulta();
        areaAmanhecer.enabled = etapa == 0;
        areaUltimaConsulta.enabled = etapa == 1;
        areaSaidaConsulta.enabled = etapa == 2;
        areaRetornoConsulta.enabled = etapa >= 3;
        if (etapa == 0) { AtualizarMemoria(); return; } // A noite permanece até a interação da manhã.
        AplicarNoite(false);
        lacoNaMao.SetActive(false); maoNaRoupa.SetActive(false); cabacaNoite.SetActive(false);
        papelConsulta.SetActive(Dados.curraisEscolhaConsulta == 0);
        testemunhaConsulta.SetActive(false);
        trabalhadorRetorno.SetActive(etapa != 1);
        lacoRemendadoCenario.SetActive(Dados.curraisLacoRemendado && Dados.curraisLacoGuardado);
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
        destinoPai.x = Mathf.Clamp(mari.transform.position.x - seguidorPaiConsulta.DistanciaAtras,
            limiteSetor.bounds.min.x + 1f, limiteSetor.bounds.max.x - 1f);
        pai.position = destinoPai;
        if (Dados.curraisEtapaConsulta == 3 || mari.transform.position.x < maeLimiteConsulta.position.x)
        {
            Vector3 destinoMae = mae.position;
            destinoMae.x = Mathf.Clamp(mari.transform.position.x - seguidoraMae.DistanciaAtras - 1f,
                limiteSetor.bounds.min.x + 1f, limiteSetor.bounds.max.x - 1f);
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
            if (Mathf.Abs(mari.transform.position.x - area.bounds.center.x) > alcanceInteracao)
            { objetivoTexto.text = "Aproxime-se para interagir. " + ObjetivoConsulta(); return true; }
            if (area == areaAmanhecer) Iniciar(AmanhecerNoCampo());
            else if (area == areaUltimaConsulta) Iniciar(FazerUltimaConsulta());
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
        yield return Escurecer(1f);
        Dados.curraisEtapaConsulta = 1;
        AtualizarEstadoBusca();
        Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        RestaurarFamiliaConsulta();
        yield return Escurecer(0f);
        Salvar();
    }

    private IEnumerator FazerUltimaConsulta()
    {
        if (Dados.curraisEscolhaConsulta != 0) yield break;
        yield return Mover(mari.transform, mariUltimaConsulta.position, tempoPlano);
        Posicionar(pai, paiUltimaConsulta);
        if (!Dados.curraisConsultaApresentada)
        {
            if (Dados.curraisEscolhaPosto == 1) yield return Dizer(memoriaRaimundo);
            yield return Dizer(manhaOrdens);
            yield return Dizer(consultaEscrivao);
            Dados.curraisConsultaApresentada = true;
            Salvar();
        }
        yield return Escolher("Escrivão", InformacoesConsulta() + "\n" + perguntaConsulta, opcoesConsulta);
        int opcao = escolhaRecebida;
        if (opcao == 2) yield break;
        bool repetida = opcao == 0 ? Dados.curraisDestinoConfirmado : Dados.curraisIdentidadeConfirmada;
        if (opcao == 1) testemunhaConsulta.SetActive(true);
        yield return Dizer(opcao == 0 ? consultaDestino : consultaNome);
        if (repetida) yield return Dizer(opcao == 0 ? memoriaDestinoRepetido : memoriaNomeRepetido);
        testemunhaConsulta.SetActive(false);
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
        yield return Escurecer(1f);
        Posicionar(mari.transform, mariSaidaConsulta);
        Posicionar(pai, paiSaidaConsulta); Posicionar(mae, maeSaidaConsulta);
        cameraCena.transform.position = new Vector3(mari.transform.position.x, cameraCena.transform.position.y, cameraCena.transform.position.z);
        PrepararDesenhoSaida();
        yield return Escurecer(0f);
        yield return Dizer(saidaRecusada);
        yield return Mover(portaoConsulta, portaoConsultaFechado.position, tempoPlano);
        Dados.curraisEtapaConsulta = 3;
        GameProgress.Instance.UnlockDiaryPage("currais-saida");
        RestaurarDesenhoSaida();
        AtualizarEstadoBusca(); RestaurarFamiliaConsulta(); Salvar();
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
        Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        yield return RelatarInformacoes();
        if (Dados.curraisLacoGuardado)
        {
            lacoRemendadoCenario.SetActive(true);
            yield return Esperar(tempoPlano);
            Dados.curraisLacoRemendado = true;
        }
        Dados.curraisEtapaConsulta = 4;
        GameProgress.Instance.UnlockDiaryPage("currais-pistas-finais");
        AtualizarEstadoBusca(); Salvar();
    }
}
