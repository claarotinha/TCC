using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    private bool NegacaoAcolhida => ViuConversaEntrada("negacao-mao-da-mae");
    private int InsistenciasNaBusca => ViuConversaEntrada("negacao-insistencia-2") ? 2 :
        ViuConversaEntrada("negacao-insistencia-1") ? 1 : 0;

    private IEnumerator NegarFimDaBusca()
    {
        if (NegacaoAcolhida) yield break;
        yield return DizerEntradaUmaVez("negacao-inicio", new[] {
            new Fala { personagem = "Zefinha", texto = "Eu num vou deitar. Ainda tem coisa que eu num perguntei." },
            new Fala { personagem = "Pai", texto = "Nós passou o dia perguntando, minha filha." },
            new Fala { personagem = "Zefinha", texto = "Então a gente perguntou errado." }
        });
        while (!NegacaoAcolhida)
        {
            if (InsistenciasNaBusca < 2)
            {
                yield return Escolher("Zefinha", InsistenciasNaBusca == 0 ?
                    "Tem que ter alguma coisa que eu deixei passar." : "Num pode acabar só nisso. Eu preciso de outra pista.",
                    new[] { "Conferir as pistas de novo", "Voltar à porteira", "Sair" });
                int opcao = escolhaRecebida;
                if (opcao == 2) { FecharMenu(); yield break; }
                yield return InsistirEmMariquinha(opcao == 0);
                int tentativa = InsistenciasNaBusca + 1;
                RegistrarConversaEntrada("negacao-insistencia-" + tentativa);
                Salvar();
                if (tentativa == 2)
                {
                    yield return FalarPosto("Zefinha", "Ela tá esperando. Ela tá esperando e eu tô aqui parada!",
                        "Pai", "Zefinha, nós também quer trazer tua irmã.",
                        "Zefinha", "Então por que ninguém faz nada?",
                        "Mãe", "Olhe pra mim, minha filha. Nós tá aqui contigo.");
                    yield return Esperar(1.5f);
                }
                continue;
            }
            yield return Escolher("Mãe", "Eu num vou pedir pra tu esquecer tua irmã. Só fique perto de mim agora.",
                new[] { "Apertar a mão da mãe", "Eu preciso continuar procurando", "Sair" });
            int resposta = escolhaRecebida;
            if (resposta == 2) { FecharMenu(); yield break; }
            if (resposta == 1)
            {
                yield return FalarPosto("Zefinha", "Se eu parar, parece que eu tô deixando ela lá.",
                    "Pai", "Descansar um pouco num apaga o que tu fez por ela.",
                    "Mãe", "Eu num sei onde ela tá. Eu queria saber. Mas tu também é minha filha, e eu tô vendo tu sofrer aqui.");
                continue;
            }
            yield return ApertarMaoDaMae();
            yield return FalarPosto("Zefinha", "Eu prometi, mainha... Eu prometi pra ela.",
                "Mãe", "Eu sei. Tu num precisa prometer mais nada agora.",
                "Pai", "Nós guarda os nomes e o lugar. Amanhã nós vê o que ainda pode fazer.",
                "Zefinha (em pensamento)", "A mão de mainha tava quente. Eu ainda queria levantar. Mas fiquei.");
            RegistrarConversaEntrada("negacao-mao-da-mae"); Salvar();
        }
    }

    private IEnumerator InsistirEmMariquinha(bool reverPistas)
    {
        Vector3 destino = mari.transform.position;
        // Passos curtos dentro do abrigo: os pais interceptam sem mudar de cenário.
        destino.x += reverPistas ? 1f : -1f;
        yield return Mover(mari.transform, destino, .65f);
        if (reverPistas)
        {
            if (Dados.curraisDestinoConfirmado)
                yield return FalarPosto("Zefinha", "Lagoa do Meio. Foi isso que tava escrito. Tem que ter mais alguma coisa.");
            if (Dados.curraisIdentidadeConfirmada)
                yield return FalarPosto("Zefinha", "Nair Pereira. E se ela deixou outro nome? E se ninguém procurou direito?");
            yield return FalarPosto("Pai", "Foi o que nós conseguiu confirmar. Num apareceu outra informação.",
                "Zefinha", InsistenciasNaBusca == 0 ? "Olhe de novo, painho. Por favor." : "Nome, lugar... Mas cadê ela? Cadê minha irmã?");
        }
        else
        {
            Vector3 perto = pai.position; perto.x = destino.x - 1.2f;
            yield return Mover(pai, perto, .5f);
            yield return FalarPosto("Zefinha", "Eu vou falar com eles de novo. Eu mesma procuro lá fora.",
                "Pai", "Tu ouviu o que aquele homem disse. Eu num vou deixar ele encostar em tu.",
                "Zefinha", "E Mariquinha? Quem tá perto dela?",
                "Pai", "Eu num sei, minha filha.");
        }
        yield return FalarPosto("Mãe", "Venha pra perto de nós.");
    }

    private IEnumerator ApertarMaoDaMae()
    {
        Vector3 perto = mae.position; perto.x = mari.transform.position.x + 1.1f;
        yield return Mover(mae, perto, .65f);
        FecharDialogo();
        dialogando = false; menuAberto = false;
        escolhas.SetActive(false); AbrirPainelDialogo();
        nomeTexto.text = "Mãe";
        if (retrato != null) retrato.enabled = false;
        float tempo = 0f;
        try
        {
            while (tempo < 2.5f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f && !InventarioAbertoMemoria() &&
                    (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0))) tempo += Time.deltaTime;
                falaTexto.text = "Eu tô aqui. Aperte minha mão.\nSegure E ou o botão esquerdo do mouse.\n" +
                    Mathf.Min(100, Mathf.FloorToInt(tempo / 2.5f * 100f)) + "%";
                yield return null;
            }
        }
        finally { FecharDialogo(); InvestigationGuard.BlockCurrentClick(); quadroBloqueado = Time.frameCount; }
    }

    private IEnumerator RetomarNegacaoDaBusca()
    {
        yield return NegarFimDaBusca();
        if (!NegacaoAcolhida) yield break;
        RegistrarConversaEntrada("consulta-relato-final");
        AtualizarEstadoBusca(); Salvar();
        MenuNomesFinais();
    }
}
