using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Painel e ilustração em blocos editáveis no Canvas da cena.
public sealed class AcolhimentoMaoUI : MonoBehaviour
{
    [SerializeField] private RectTransform painel, maoMae, maoMenina;
    [SerializeField] private Image preenchimento;
    [SerializeField] private Image ilustracaoMao;
    [SerializeField] private TMP_Text porcentagem, mensagem;
    [SerializeField] private Button sair;
    private float tempo, esperaFinal;
    public bool Concluido { get; private set; }
    public bool Aberto => gameObject.activeInHierarchy;

    private void Awake() => sair.onClick.AddListener(() => {
        if (!PauseHelper.BlockInput() && !Concluido) Fechar();
    });

    public void Abrir()
    {
        tempo = 0f; esperaFinal = 0f; Concluido = false;
        sair.interactable = true;
        gameObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        AtualizarVisual();
    }

    private void Update()
    {
        AjustarPainel();
        if (PauseHelper.BlockInput() || Time.timeScale <= 0f) return;
        if (Concluido)
        {
            esperaFinal += Time.deltaTime;
            if (esperaFinal >= 1.4f) Fechar();
            return;
        }
        if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(0)) tempo = Mathf.Min(3f, tempo + Time.deltaTime);
        if (tempo >= 3f) { Concluido = true; sair.interactable = false; }
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        float progresso = Mathf.Clamp01(tempo / 3f);
        preenchimento.fillAmount = progresso;
        porcentagem.text = Mathf.RoundToInt(progresso * 100f) + "%";
        mensagem.text = Concluido ? "Ainda dói. Mas você não está sozinha." :
            "Segure E ou o botão esquerdo do mouse.\nPode soltar e continuar quando quiser.";
        // Ilustração fixa: o gesto já está acontecendo ao abrir o painel.
        AjustarPainel();
    }

    private void AjustarPainel()
    {
        Rect area = ((RectTransform)transform).rect;
        if (area.width <= 0f || area.height <= 0f) return;
        painel.localScale = Vector3.one * Mathf.Min(1f, area.width * .9f / 900f, area.height * .9f / 590f);
    }

    public void Fechar() => gameObject.SetActive(false);
}
