using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Revista — mesa e estados editáveis")]
    [SerializeField] private Collider2D areaMesaRevista, areaDiarioRetido;
    [SerializeField] private GameObject diarioRetido;
    [SerializeField] private MesaRevistaCurrais mesaPertences;
    [SerializeField] private Transform fiscalConferencia;
    [SerializeField] private Fala[] revistaPendente, cadernoEmConferencia, cadernoAutorizado;
    private bool revistaEmConferencia;

    private bool ReferenciasRevistaValidas()
    {
        return ConferirReferencia(areaMesaRevista, nameof(areaMesaRevista)) &
            ConferirReferencia(areaDiarioRetido, nameof(areaDiarioRetido)) &
            ConferirReferencia(diarioRetido, nameof(diarioRetido)) &
            ConferirReferencia(mesaPertences, nameof(mesaPertences)) &
            ConferirReferencia(fiscalConferencia, nameof(fiscalConferencia)) &
            FalasValidas(revistaPendente) & FalasValidas(cadernoEmConferencia) &
            FalasValidas(cadernoAutorizado);
    }

    private void PrepararRevistaNoCheckpoint()
    {
        if (Dados.curraisEtapaEntrada >= 2 &&
            (Dados.curraisItensRetidos.Count > 0 || Dados.curraisPertencesRetidos))
            Posicionar(areaFiscalEntrada.transform, fiscalConferencia);
    }

    private IEnumerator FiscalVaiAteAMesa()
    {
        if (Mathf.Abs(areaFiscalEntrada.transform.position.x - fiscalConferencia.position.x) > .05f)
            yield return Mover(areaFiscalEntrada.transform, fiscalConferencia.position, .6f);
    }

    private void AtualizarMesaRevista()
    {
        bool revista = Dados.curraisEtapaEntrada == 2;
        bool retido = revista && (!Dados.curraisPertencesRetidos || revistaEmConferencia);
        diarioRetido.SetActive(retido);
        areaDiarioRetido.enabled = retido && !ViuConversaEntrada("caderno-conferencia");
        areaMesaRevista.enabled = revista;
        mesaPertences.Atualizar(Dados.curraisItensRetidos);
    }

    private void MenuConferenciaRevista()
    {
        if (Dados.curraisDiarioRecuperado || Dados.curraisDiarioRecusado) { FecharMenu(); return; }
        if (Dados.curraisPertencesRetidos)
        {
            MostrarMenuEntrada("revista-diario-autorizado", "Fiscal", string.Empty,
                new[] { "Diário", "Sair" },
                new Action[] { () => Iniciar(RecuperarDiarioEntrada()), FecharMenu });
            return;
        }
        var rotulos = new System.Collections.Generic.List<string>();
        var acoes = new System.Collections.Generic.List<Action>();
        if (!ViuConversaEntrada("revista-pendente"))
        {
            rotulos.Add("As coisas");
            acoes.Add(() => Iniciar(DizerEntradaUmaVez("revista-pendente", revistaPendente)));
        }
        // A resposta de pendência é dita uma vez; a conferência volta quando todos entregaram os itens.
        rotulos.Add("Terminou a conferência?"); acoes.Add(() => Iniciar(ConcluirRevista()));
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarMenuEntrada("revista-pertences", "Fiscal", "Os pertences ficam juntos na mesa. Depois confiro o caderno.",
            rotulos.ToArray(), acoes.ToArray());
        if (ViuConversaEntrada("revista-pendente") && InventoryManager.Instance.Items.Count > 0)
        {
            int indice = rotulos.IndexOf("Terminou a conferência?");
            if (indice >= 0 && indice < botoesEtapa2.Length) botoesEtapa2[indice].interactable = false;
        }
    }

    private IEnumerator EntregarPertenceRevista(ItemData item)
    {
        if (Dados.curraisPertencesRetidos || item == null || !TemItem(item)) yield break;
        InventorySaveCatalog catalogo = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalogo == null || !catalogo.TryGetId(item, out string id))
        {
            Debug.LogError("Currais: pertence sem registro no catálogo; entrega cancelada.", this);
            yield break;
        }
        try
        {
            yield return FiscalVaiAteAMesa();
            yield return Esperar(.5f);
            // O jogador pode ter aberto o inventário e combinado o item durante a conferência.
            if (!TemItem(item) || Dados.curraisPertencesRetidos) yield break;
            Dados.curraisItensRetidos.Add(id);
            InventoryManager.Instance.RemoveItem(item);
            AtualizarVisuaisChegada();
            Salvar();
            if (InventoryManager.Instance.Items.Count == 0)
                yield return DizerEntradaUmaVez("revista-itens-entregues", new[] { new Fala { personagem = "Fiscal",
                    texto = "Está tudo aqui. Agora falta conferir o caderno." } });
        }
        finally { EncerrarGestoRevista(); }
    }

    private IEnumerator ConferirTodosOsPertences()
    {
        if (Dados.curraisPertencesRetidos)
        { yield break; }
        if (InventoryManager.Instance == null) yield break;
        if (InventoryManager.Instance.Items.Count > 0)
        { yield return DizerEntradaUmaVez("revista-pendente", revistaPendente); yield break; }
        try
        {
            revistaEmConferencia = true;
            yield return FiscalVaiAteAMesa();
            yield return Esperar(.5f);
            // Confere novamente após a conferência; nenhuma lista fixa de ferramentas é usada.
            if (!ReterPertences())
            { yield return DizerEntradaUmaVez("revista-pendente", revistaPendente); yield break; }
            AtualizarVisuaisChegada();
            yield return Dizer(revista);
        }
        finally
        {
            revistaEmConferencia = false;
            if (!encerrando && GameProgress.Instance != null) AtualizarVisuaisChegada();
        }
        Salvar();
    }

    private void EncerrarGestoRevista()
    {
        revistaEmConferencia = false;
    }
}
