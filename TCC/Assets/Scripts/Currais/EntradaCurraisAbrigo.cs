using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Primeiro abrigo — elementos editáveis")]
    [SerializeField] private Collider2D areaPaiAbrigo;
    [SerializeField] private Transform maeCuidadoAbrigo;
    [SerializeField] private SpriteRenderer cabacaComMari, cabacaComMae, cabacaComLuzia;
    [SerializeField] private Fala[] maePrecisaPano, paiRepouso, paiDepoisAgua;
    [SerializeField] private Fala[] luziaBuscarAgua, aguaAguardar, aguaApresentar, aguaJaRecebida;
    [SerializeField] private Fala[] maeEsperaAgua, familiaDepoisAgua, cabacaIndisponivel;
    private int recebendoAguaAbrigo;

    private bool ReferenciasAbrigoValidas()
    {
        // São os mesmos objetos já usados pela cena; recupera cópias com os novos campos vazios.
        if (pontoParadaFamilia == null) pontoParadaFamilia = mariAbrigo;
        if (seguidoraBaleiaCurrais == null && baleia != null)
            seguidoraBaleiaCurrais = baleia.GetComponent<SeguidorFamilia>();
        return ReferenciasAguaLinearValidas() & ReferenciasAbrigoLinearValidas() & ConferirReferencia(areaPaiAbrigo, nameof(areaPaiAbrigo)) &
            ConferirReferencia(maeCuidadoAbrigo, nameof(maeCuidadoAbrigo)) &
            ConferirReferencia(cabacaComMari, nameof(cabacaComMari)) &
            ConferirReferencia(cabacaComMae, nameof(cabacaComMae)) &
            ConferirReferencia(cabacaComLuzia, nameof(cabacaComLuzia)) &
            FalasValidas(maePrecisaPano) & FalasValidas(paiRepouso) &
            FalasValidas(paiDepoisAgua) & FalasValidas(luziaBuscarAgua) &
            FalasValidas(aguaAguardar) & FalasValidas(aguaApresentar) &
            FalasValidas(aguaJaRecebida) & FalasValidas(maeEsperaAgua) &
            FalasValidas(familiaDepoisAgua) & FalasValidas(cabacaIndisponivel) &
            ConferirReferencia(pontoParadaFamilia, nameof(pontoParadaFamilia)) &
            ConferirReferencia(seguidoraBaleiaCurrais, nameof(seguidoraBaleiaCurrais)) &
            FalasValidas(familiaDecideParar) & FalasValidas(familiaSegueCaminho) & FalasValidas(familiaBuscaIrma) &
            PresencasValidas();
    }

    private void MenuMaePrimeiroAbrigo() => MenuParenteAbrigo(false);

    private IEnumerator IniciarBuscaAguaAbrigo()
    {
        yield return Dizer(luziaBuscarAgua);
        Dados.curraisDistribuicaoIniciada = true;
        AtualizarEstadoBusca();
        Salvar();
    }

    private void AtualizarAbrigoVisual()
    {
        bool primeiroAbrigo = Dados.curraisEtapaEntrada >= 3 &&
            Dados.curraisEtapaEntrada <= 4 && !Dados.curraisPanelaConcluida;
        areaPaiAbrigo.enabled = primeiroAbrigo;
        if (Dados.curraisEtapaEntrada == 3) areaMae.enabled = true;
        bool agua = primeiroAbrigo && Dados.curraisAguaRecebida;
        cabacaComMari.gameObject.SetActive(agua && Dados.curraisEscolhaAgua == 0 && recebendoAguaAbrigo == 0);
        cabacaComMae.gameObject.SetActive(agua && (recebendoAguaAbrigo == 1 || Dados.curraisEscolhaAgua == 1));
        cabacaComLuzia.gameObject.SetActive(agua && (recebendoAguaAbrigo == 2 || Dados.curraisEscolhaAgua == 2));
        cabacaComMari.color = new Color(.65f, .85f, 1f, 1f);
        Color corEntrega = recebendoAguaAbrigo != 0 ? cabacaComMari.color : Color.white;
        cabacaComMae.color = corEntrega;
        cabacaComLuzia.color = corEntrega;
    }

    private IEnumerator BeberAguaAbrigo(int destinatario)
    {
        SpriteRenderer visual = destinatario == 1 ? cabacaComMae : cabacaComLuzia;
        Quaternion original = visual.transform.localRotation;
        try
        {
            recebendoAguaAbrigo = destinatario;
            AtualizarAbrigoVisual();
            float tempo = 0f;
            while (tempo < .7f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    float inclinacao = Mathf.Sin(Mathf.Clamp01(tempo / .7f) * Mathf.PI) * 15f;
                    visual.transform.localRotation = original * Quaternion.Euler(0f, 0f, inclinacao);
                }
                yield return null;
            }
        }
        finally
        {
            visual.transform.localRotation = original;
            recebendoAguaAbrigo = 0;
            if (!encerrando && GameProgress.Instance != null) AtualizarAbrigoVisual();
        }
    }
}

public partial class EntradaCurrais
{
    [Header("Família — caminhada e primeira parada")]
    [SerializeField] private Transform pontoParadaFamilia;
    [SerializeField] private SeguidorFamilia seguidoraBaleiaCurrais;
    [SerializeField] private Fala[] familiaDecideParar, familiaSegueCaminho, familiaBuscaIrma;
    [SerializeField, Min(.5f)] private float proximidadeParada = 3f;

    private void MenuAssuntoNPC(string pessoa, string abertura, string assunto, Func<IEnumerator> fala,
        string outroAssunto = null, Func<IEnumerator> outraFala = null)
    {
        if (outraFala == null)
            MostrarOpcoes(pessoa, abertura, new[] { assunto, "Sair" },
                new Action[] { () => Iniciar(fala()), FecharMenu });
        else
            MostrarOpcoes(pessoa, abertura, new[] { assunto, outroAssunto, "Sair" },
                new Action[] { () => Iniciar(fala()), () => Iniciar(outraFala()), FecharMenu });
    }

    private void MenuPaiPrimeiroAbrigo() => MenuParenteAbrigo(true);

    private bool AtualizarCaminhadaFamilia(bool livre)
    {
        bool caminhando = (Dados.curraisEtapaEntrada == 2 || Dados.curraisEtapaEntrada == 3) && !Dados.curraisFamiliaParou;
        if (!ConsultaDisponivel) seguidorPaiConsulta.enabled = livre && caminhando;
        seguidoraBaleiaCurrais.enabled = livre && caminhando;
        if (!caminhando) return false;
        seguidoraMae.enabled = livre;
        seguidoraLuzia.enabled = false;
        if (Dados.curraisEtapaEntrada == 3 && livre && Mathf.Abs(mari.transform.position.x - pontoParadaFamilia.position.x) <= proximidadeParada)
        {
            Iniciar(PararFamilia());
        }
        return true;
    }

    private IEnumerator PararFamilia()
    {
        if (Dados.curraisFamiliaParou) yield break;
        if (Mathf.Abs(mari.transform.position.x - pontoParadaFamilia.position.x) > proximidadeParada)
        {
            yield return Dizer(familiaSegueCaminho);
            yield break;
        }
        yield return Dizer(familiaDecideParar);
        Dados.curraisFamiliaParou = true;
        // A família para primeiro; o pano é entregue depois por arraste.
        Dados.curraisParadaRegistrada = true;
        Dados.curraisParadaPaiX = pai.position.x;
        Dados.curraisParadaMaeX = mae.position.x;
        Dados.curraisParadaBaleiaX = baleia.position.x;
        Dados.curraisEtapaEntrada = 4;
        AtualizarVisuaisChegada(); AtualizarEstadoBusca(); Salvar();
    }

    private static void RestaurarXParada(Transform pessoa, float x)
    {
        if (float.IsNaN(x) || float.IsInfinity(x)) return;
        Vector3 posicao = pessoa.position; posicao.x = x; pessoa.position = posicao;
    }

    private void RestaurarFamiliaNaCaminhada()
    {
        if (Dados.curraisFamiliaParou) return;
        SpriteRenderer visual = mari.GetComponent<SpriteRenderer>();
        float direcao = visual != null && visual.flipX ? -1f : 1f;
        RestaurarXParada(mae, mari.transform.position.x - direcao * seguidoraMae.DistanciaAtras);
        RestaurarXParada(pai, mari.transform.position.x - direcao * seguidorPaiConsulta.DistanciaAtras);
        RestaurarXParada(baleia, mari.transform.position.x - direcao * seguidoraBaleiaCurrais.DistanciaAtras);
        Physics2D.SyncTransforms();
    }

    [SerializeField] private Fala[] cabacaSeparadaMesa;
    [SerializeField] private Collider2D areaDistribuicaoAgua;

    private Fala[] OrientacaoDaDistribuicao()
    {
        if (!Dados.curraisCabacaDevolvida) return cabacaSeparadaMesa;
        if (Dados.curraisEtapaEntrada != 4) return aguaAguardar;
        return Dados.curraisAguaRecebida ? aguaJaRecebida : aguaApresentar;
    }

    private void MenuFuncionarioAgua() => MenuDistribuicaoLinear();

    private void MenuPrimeiroRegistro()
    {
        if (Dados.curraisEscolhaAgua == 0)
        { Iniciar(ExamineNomesUmaVez("nomes-examine-folhas", exameFolhasNomes)); return; }
        if (Dados.curraisRegistroConcluido) { MenuTarefaPanos(); return; }
        MostrarMenuEntrada("nomes-escrivao-abertura", "Escrivão", "O nome de quem?",
            new[] { "Perguntar por Mariquinha", "Sair" },
            new Action[] { () => Iniciar(ConsultarRegistro()), FecharMenu });
    }
}

public partial class EntradaCurrais
{
    private bool UsarItemArrastado(Collider2D alvo, ItemData item)
    {
        bool antesBusca = !Dados.curraisPanelaConcluida;
        if (Dados.curraisFamiliaParou && PedidoPanoConhecido && alvo == areaMae &&
            item == itemPanoAbrigo && !Dados.curraisPaiAcomodado)
        { Iniciar(AcomodarPai()); return true; }
        // O recipiente pode ser apresentado na mesa ou à funcionária, sem uma opção de conversa obrigatória.
        if (antesBusca && (alvo == areaAgua || (areaDistribuicaoAgua != null && alvo == areaDistribuicaoAgua)) &&
            item == cabacaVaziaBusca && PodeEncherCabaca && Dados.curraisEtapaEntrada >= 3 && Dados.curraisEtapaEntrada <= 4)
        { Iniciar(ReceberAgua()); return true; }
        if (Dados.curraisEtapaEntrada != 4) return false;
        if (antesBusca && (alvo == areaMae || alvo == areaLuzia) && item == cabacaCheiaBusca && PodeEscolherAgua)
        { Iniciar(DestinarAgua(alvo == areaMae ? 1 : 2)); return true; }
        if (Dados.curraisEtapaBusca == 1 && (alvo == areaLavadeira || alvo == areaRecebimentoLavagem) && item == itemLotePanos && !Dados.curraisLotePanosEntregue)
        { Iniciar(ConversarLavadeira()); return true; }
        if (Dados.curraisEtapaBusca == 2 && (alvo == areaPanosUso || alvo == areaPanosAtendimento) &&
            (item == panoUsoLavagem || item == panoAtendimentoLavagem))
        { Iniciar(SepararPanoLavagem(alvo == areaPanosAtendimento)); return true; }
        if (Dados.curraisEtapaBusca == 4 && ApoioPatioAutorizado && !Dados.curraisAguaPatioPreparada && alvo == areaAguaPatio && item == cabacaVaziaBusca)
        { Iniciar(PrepararAguaPatio()); return true; }
        if (Dados.curraisEtapaBusca == 4 && ApoioPatioAutorizado && !Dados.curraisPanosPatioEntregues && alvo == areaPanosPatio && item == panoAuxilio)
        { Iniciar(EntregarPanosPatio()); return true; }
        if (Dados.curraisEtapaBusca == 5 && Dados.curraisBentoMorto &&
            alvo == areaFamiliarBento && item == itemEmbrulhoBento && !PertencesBentoResolvidos)
        { Iniciar(EntregarEmbrulhoBento()); return true; }
        if (Dados.curraisEtapaBusca == 5 && !Dados.curraisBentoMorto && (alvo == areaSocorroBento || alvo == areaBentoPatio || alvo == areaPaiPatio) &&
            (item == panoAuxilio || item == cabacaCheiaBusca))
        { Iniciar(UsarNoSocorro()); return true; }
        if (Dados.curraisQuartoAberto && alvo == areaTrabalhadoraQuarto && item == cabacaVaziaBusca)
        { Iniciar(AjudarNoQuarto()); return true; }
        if (ConsultaDisponivel && Dados.curraisEtapaConsulta == 1 && alvo == areaRaimundo && (item == baldeProtegidoItem || item == baldeRaimundoItem))
        { Iniciar(DevolverBaldeRaimundo()); return true; }
        if (ConsultaDisponivel && Dados.curraisEtapaConsulta == 1 && areaUltimaConsulta.enabled && alvo == areaUltimaConsulta && item == itemCanhoto)
        { Iniciar(ApresentarCanhotoConsulta()); return true; }
        return false;
    }

    [Serializable]
    private class PessoaPresente
    {
        public GameObject personagem;
        public SpriteRenderer desenho;
        public string nome;
        public Fala[] conversa;
    }
    [Header("Personagens presentes — assuntos antes da participação principal")]
    [SerializeField] private PessoaPresente[] pessoasPresentes;

    private bool PresencasValidas()
    {
        if (pessoasPresentes == null || pessoasPresentes.Length == 0) return false;
        bool ok = true;
        foreach (PessoaPresente pessoa in pessoasPresentes)
            ok &= pessoa != null && pessoa.personagem != null && pessoa.desenho != null &&
                !string.IsNullOrWhiteSpace(pessoa.nome) && FalasValidas(pessoa.conversa);
        return ok;
    }

    private void ManterPessoasPresentes()
    {
        if (pessoasPresentes == null) return;
        foreach (PessoaPresente pessoa in pessoasPresentes)
            if (pessoa != null && pessoa.personagem != null && !pessoa.personagem.activeSelf)
                pessoa.personagem.SetActive(true);
    }

    private PessoaPresente PessoaNoPonto(Vector2 ponto)
    {
        if (pessoasPresentes == null || !campo.activeInHierarchy) return null;
        foreach (PessoaPresente pessoa in pessoasPresentes)
        {
            if (pessoa == null || pessoa.desenho == null || !pessoa.desenho.enabled ||
                !pessoa.desenho.gameObject.activeInHierarchy) continue;
            Bounds limites = pessoa.desenho.bounds;
            if (limites.Contains(new Vector3(ponto.x, ponto.y, limites.center.z))) return pessoa;
        }
        return null;
    }

    private bool SobrePresenca(Vector2 ponto) => PessoaNoPonto(ponto) != null;

    private bool InteragirPresenca(Vector2 ponto)
    {
        PessoaPresente pessoa = PessoaNoPonto(ponto);
        if (pessoa == null) return false;
        MenuPresencaSemRepetir(pessoa);
        return true;
    }

    private void ConversarPresencaProxima()
    {
        if (pessoasPresentes == null || !campo.activeInHierarchy) return;
        PessoaPresente perto = null; float distancia = alcanceInteracao;
        foreach (PessoaPresente pessoa in pessoasPresentes)
        {
            if (pessoa == null || pessoa.desenho == null || !pessoa.desenho.enabled ||
                !pessoa.desenho.gameObject.activeInHierarchy) continue;
            float d = Mathf.Abs(mari.transform.position.x - pessoa.desenho.bounds.center.x);
            if (d < distancia) { distancia = d; perto = pessoa; }
        }
        if (perto != null) MenuPresencaSemRepetir(perto);
    }
}
