using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Os retalhos e a costura são objetos do Canvas, editáveis na Hierarchy.
public sealed class PuzzleRetalhos : MonoBehaviour
{
    [SerializeField] private Button[] retalhos;
    [SerializeField] private Button sair;
    [SerializeField] private TMP_Text instrucao;
    private readonly int[] giros = { 1, 2, 3 };
    private float tempoConclusao;
    public bool Concluido { get; private set; }
    public bool Aberto => gameObject.activeInHierarchy;

    private void Awake()
    {
        for (int i = 0; i < retalhos.Length; i++)
        {
            int indice = i;
            retalhos[i].onClick.AddListener(() => Girar(indice));
        }
        sair.onClick.AddListener(Sair);
    }

    public void Abrir()
    {
        Concluido = false;
        tempoConclusao = 0f;
        gameObject.SetActive(true);
        for (int i = 0; i < retalhos.Length; i++)
        {
            retalhos[i].interactable = true;
            AtualizarPeca(i);
        }
        instrucao.text = "Clique nos retalhos para girar e unir a costura.";
    }

    private void Girar(int indice)
    {
        if (PauseHelper.BlockInput() || Concluido) return;
        giros[indice] = (giros[indice] + 1) % 4;
        AtualizarPeca(indice);
        for (int i = 0; i < giros.Length; i++)
            if (giros[i] != 0) return;
        Concluido = true;
        instrucao.text = "Costura pronta!";
        foreach (Button retalho in retalhos) retalho.interactable = false;
    }

    private void AtualizarPeca(int indice) =>
        retalhos[indice].transform.localRotation = Quaternion.Euler(0f, 0f, -90f * giros[indice]);

    private void Update()
    {
        if (!Concluido || PauseHelper.BlockInput()) return;
        tempoConclusao += Time.deltaTime;
        if (tempoConclusao >= .6f) Fechar();
    }

    private void Sair()
    {
        if (!PauseHelper.BlockInput() && !Concluido) Fechar();
    }

    public void Fechar() => gameObject.SetActive(false);
}
