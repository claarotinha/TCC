using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Folhas, textos, marcas de tinta e botões existem no Canvas da cena.
public sealed class PuzzleRegistro : MonoBehaviour
{
    [SerializeField] private Button[] folhas;
    [SerializeField] private TMP_Text instrucao;
    [SerializeField] private Button conferir, sair;
    private static readonly int[][] ordens = {
        new[] { 2, 0, 1 }, new[] { 0, 1, 2 }, new[] { 0, 2, 1 },
        new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 1, 0 }
    };
    private readonly Vector2[] posicoes = new Vector2[3];
    private readonly Color[] cores = new Color[3];
    private int[] ordem;
    private int selecionada = -1;
    private Action<int> aoTrocar;
    private float tempoConclusao;
    public bool Concluido { get; private set; }
    public bool Aberto => gameObject.activeInHierarchy;

    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            int folha = i;
            posicoes[i] = ((RectTransform)folhas[i].transform).anchoredPosition;
            cores[i] = folhas[i].image.color;
            folhas[i].onClick.AddListener(() => Selecionar(folha));
        }
        conferir.onClick.AddListener(Conferir);
        sair.onClick.AddListener(Sair);
    }

    public void Abrir(int estado, Action<int> salvar)
    {
        gameObject.SetActive(true);
        ordem = (int[])ordens[Mathf.Clamp(estado, 0, 5)].Clone();
        aoTrocar = salvar;
        selecionada = -1;
        Concluido = false;
        tempoConclusao = 0f;
        conferir.interactable = true;
        foreach (Button folha in folhas) folha.interactable = true;
        instrucao.text = "Clique em uma folha e depois em outra para trocar. Reconstrua o registro da esquerda para a direita.";
        AtualizarFolhas();
    }

    private void Selecionar(int folha)
    {
        if (PauseHelper.BlockInput() || Concluido) return;
        if (selecionada < 0) selecionada = folha;
        else if (selecionada == folha) selecionada = -1;
        else
        {
            int a = Array.IndexOf(ordem, selecionada), b = Array.IndexOf(ordem, folha);
            int valor = ordem[a]; ordem[a] = ordem[b]; ordem[b] = valor;
            selecionada = -1;
            int estado = Array.FindIndex(ordens, candidata => candidata[0] == ordem[0] &&
                candidata[1] == ordem[1] && candidata[2] == ordem[2]);
            aoTrocar?.Invoke(estado);
        }
        AtualizarFolhas();
    }

    private void AtualizarFolhas()
    {
        for (int slot = 0; slot < 3; slot++)
        {
            int folha = ordem[slot];
            ((RectTransform)folhas[folha].transform).anchoredPosition = posicoes[slot];
            folhas[folha].image.color = folha == selecionada ? new Color(1f, .77f, .36f, 1f) : cores[folha];
        }
    }

    private void Conferir()
    {
        if (PauseHelper.BlockInput() || Concluido) return;
        if (ordem[0] != 0 || ordem[1] != 1 || ordem[2] != 2)
        {
            instrucao.text = "Ainda não encaixou. A chegada vem antes da separação; a remessa foi a última etapa.";
            return;
        }
        Concluido = true;
        selecionada = -1;
        AtualizarFolhas();
        foreach (Button folha in folhas) folha.interactable = false;
        conferir.interactable = false;
        instrucao.text = "Registro reconstituído.";
    }

    private void Update()
    {
        if (!Concluido || PauseHelper.BlockInput() || Time.timeScale <= 0f) return;
        tempoConclusao += Time.deltaTime;
        if (tempoConclusao >= 1.2f) Fechar();
    }

    private void Sair()
    {
        if (!PauseHelper.BlockInput() && !Concluido) Fechar();
    }

    public void Fechar()
    {
        aoTrocar = null;
        gameObject.SetActive(false);
    }
}
