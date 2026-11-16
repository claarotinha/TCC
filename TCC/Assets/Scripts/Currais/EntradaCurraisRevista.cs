using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Revista — mesa e estados editáveis")]
    [SerializeField] private Collider2D areaMesaRevista, areaDiarioRetido;
    [SerializeField] private GameObject diarioRetido;
    [SerializeField] private MesaRevistaCurrais mesaPertences;
    [SerializeField] private PoseEntradaCurrais poseFiscalRevista;
    [SerializeField] private Transform fiscalConferencia;
    [SerializeField] private Fala[] revistaPendente, cadernoEmConferencia, cadernoAutorizado;
    private bool revistaEmConferencia;

    private bool ReferenciasRevistaValidas()
    {
        return ConferirReferencia(areaMesaRevista, nameof(areaMesaRevista)) &
            ConferirReferencia(areaDiarioRetido, nameof(areaDiarioRetido)) &
            ConferirReferencia(diarioRetido, nameof(diarioRetido)) &
            ConferirReferencia(mesaPertences, nameof(mesaPertences)) &
            ConferirReferencia(poseFiscalRevista, nameof(poseFiscalRevista)) &
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
        areaDiarioRetido.enabled = retido;
        areaMesaRevista.enabled = revista;
        mesaPertences.Atualizar(Dados.curraisItensRetidos);
    }

    private void MenuConferenciaRevista()
    {
        if (Dados.curraisPertencesRetidos)
        {
            MostrarOpcoes("Fiscal", "O caderno ficou separado. Esse a menina pode levar.",
                new[] { "Meu caderno", "Sair" },
                new Action[] { () => Iniciar(Dizer(cadernoAutorizado)), FecharMenu });
            return;
        }
        ItemData selecionado = InventoryManager.Instance != null ? InventoryManager.Instance.SelectedItem : null;
        bool entregar = selecionado != null && TemItem(selecionado);
        string texto = InventoryManager.Instance.Items.Count > 0
            ? "Os pertences ficam juntos na mesa. O caderno eu confiro depois."
            : "Os pertences estão aqui. Falta conferir o caderno da menina.";
        if (entregar)
            MostrarOpcoes("Fiscal", texto, new[] { "Entregar este pertence", "Conferir a revista", "Sair" },
                new Action[] { () => Iniciar(EntregarFerramenta(selecionado)),
                    () => Iniciar(ConcluirRevista()), FecharMenu });
        else
            MostrarOpcoes("Fiscal", texto, new[] { "Conferir a revista", "Sair" },
                new Action[] { () => Iniciar(ConcluirRevista()), FecharMenu });
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
            poseFiscalRevista.DefinirVisivel(true);
            yield return Esperar(.5f);
            // O jogador pode ter aberto o inventário e combinado o item durante o gesto.
            if (!TemItem(item) || Dados.curraisPertencesRetidos) yield break;
            Dados.curraisItensRetidos.Add(id);
            InventoryManager.Instance.RemoveItem(item);
            AtualizarVisuaisChegada();
            Salvar();
            yield return Dizer(new[] { new Fala { personagem = "Fiscal",
                texto = item.itemName + ": deixe junto dos outros." } });
            if (InventoryManager.Instance.Items.Count == 0)
                yield return Dizer(new[] { new Fala { personagem = "Fiscal",
                    texto = "Está tudo aqui. Agora falta conferir o caderno." } });
        }
        finally { poseFiscalRevista.DefinirVisivel(false); }
    }

    private IEnumerator ConferirTodosOsPertences()
    {
        if (Dados.curraisPertencesRetidos)
        { yield return Dizer(cadernoAutorizado); yield break; }
        if (InventoryManager.Instance == null) yield break;
        if (InventoryManager.Instance.Items.Count > 0)
        { yield return Dizer(revistaPendente); yield break; }
        try
        {
            revistaEmConferencia = true;
            poseFiscalRevista.DefinirVisivel(true);
            yield return Esperar(.5f);
            // Confere novamente após a animação; nenhuma lista fixa de ferramentas é usada.
            if (!ReterPertences())
            { yield return Dizer(revistaPendente); yield break; }
            AtualizarVisuaisChegada();
            yield return Dizer(revista);
        }
        finally
        {
            revistaEmConferencia = false;
            poseFiscalRevista.DefinirVisivel(false);
            if (!encerrando && GameProgress.Instance != null) AtualizarVisuaisChegada();
        }
        Salvar();
    }

    private void EncerrarGestoRevista()
    {
        revistaEmConferencia = false;
        if (poseFiscalRevista != null) poseFiscalRevista.DefinirVisivel(false);
    }
}
