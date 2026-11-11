using System;
using System.Collections;
using UnityEngine;

// Cenas 4–7: apresentar o recipiente, entender a ordem e transportar os panos.
public partial class EntradaCurrais
{
    [Header("Água — conversa antes de apresentar a cabaça")]
    [SerializeField] private Fala[] orientacaoAgua;

    [Header("Tarefa da mesa dos nomes")]
    [SerializeField] private Collider2D areaLotePanos;
    [SerializeField] private GameObject lotePanosCenario, lotePanosRecebido;
    [SerializeField] private ItemData itemLotePanos;
    [SerializeField] private Fala[] pedidoPanos, qualLotePanos, destinatariaPanos, seguirPanos;
    [SerializeField, TextArea] private string objetivoLotePanos;

    private bool ReferenciasTarefaValidas()
    {
        bool valida = ConferirReferencia(areaLotePanos, nameof(areaLotePanos)) &
            ConferirReferencia(lotePanosCenario, nameof(lotePanosCenario)) &
            ConferirReferencia(lotePanosRecebido, nameof(lotePanosRecebido)) &
            ConferirReferencia(itemLotePanos, nameof(itemLotePanos));
        foreach (Fala[] falas in new[] { orientacaoAgua, pedidoPanos, qualLotePanos, destinatariaPanos, seguirPanos })
            valida &= FalasValidas(falas);
        return valida;
    }

    private void PrepararTarefaPanos()
    {
        // Não obriga um save do roteiro anterior a refazer uma tarefa que não existia.
        if (Dados.curraisPanelaConcluida && !Dados.curraisTarefaPanosIniciada)
        {
            Dados.curraisTarefaPanosIniciada = true;
            Dados.curraisLotePanosRecolhido = true;
            Dados.curraisLotePanosEntregue = true;
            Dados.curraisPerguntasTarefaPanos = 3;
        }
        else if (Dados.curraisRegistroConcluido) Dados.curraisTarefaPanosIniciada = true;
        if (Dados.curraisAguaRecebida) Dados.curraisAguaOrientada = true;
        // O lote não tem outra receita/destino: protege checkpoints anteriores à entrega.
        if (Dados.curraisLotePanosRecolhido && !Dados.curraisLotePanosEntregue && !TemItem(itemLotePanos))
            InventoryManager.Instance.AddItem(itemLotePanos);
        AtualizarTarefaPanos();
    }

    private void AtualizarTarefaPanos()
    {
        bool disponivel = Dados.curraisEtapaEntrada == 4 && Dados.curraisTarefaPanosIniciada &&
            !Dados.curraisLotePanosRecolhido && !Dados.curraisPanelaConcluida;
        lotePanosCenario.SetActive(disponivel);
        areaLotePanos.enabled = disponivel;
        lotePanosRecebido.SetActive(Dados.curraisLotePanosEntregue && Dados.curraisEtapaBusca < 2);
    }

    private void MenuTarefaPanos()
    {
        MostrarOpcoes("Escrivão", "Leve os panos até a lavagem. Depois eu vejo a relação.",
            new[] { "Qual lote eu devo levar?", "A quem devo entregar?", "E a minha irmã?", "Sair" },
            new Action[] { () => Iniciar(PerguntarTarefaPanos(1, qualLotePanos)),
                () => Iniciar(PerguntarTarefaPanos(2, destinatariaPanos)),
                () => Iniciar(PerguntarIrmaNaMesa()), FecharMenu });
    }

    private IEnumerator PerguntarTarefaPanos(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisPerguntasTarefaPanos |= bit;
        AtualizarEstadoBusca(); Salvar();
        MenuTarefaPanos();
    }

    private IEnumerator PerguntarIrmaNaMesa()
    {
        yield return RelembrarRegistro();
        MenuTarefaPanos();
    }

    private IEnumerator RecolherLotePanos()
    {
        if (!Dados.curraisTarefaPanosIniciada || Dados.curraisLotePanosRecolhido) yield break;
        if ((Dados.curraisPerguntasTarefaPanos & 3) != 3)
        {
            yield return Dizer(new[] { new Fala { personagem = "Zefinha", texto =
                "Preciso saber qual lote levar e a quem entregar. Vou perguntar ao escrivão antes de mexer." } });
            yield break;
        }
        if (!TemItem(itemLotePanos)) InventoryManager.Instance.AddItem(itemLotePanos);
        Dados.curraisLotePanosRecolhido = true;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Zefinha", texto =
            "É este lote dobrado. Vou levar à lavadeira, junto do tanque." } });
    }
}
