using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Conversa opcional da primeira parada: acontece durante a caminhada.
// A/D continuam funcionando enquanto o jogador escolhe e le as falas.
public class HistoriaPaiCaminhada : MonoBehaviour
{
    private struct Fala
    {
        public string Nome;
        public string Texto;
        public Fala(string nome, string texto) { Nome = nome; Texto = texto; }
    }

    private static readonly Fala[] Introducao =
    {
        new Fala("Mariquinha", "Painho, conta uma de suas histórias."),
        new Fala("Pai", "Tu não cansa dessas histórias… Quer qual dessa vez? Corpo-seco? Comadre Fulozinha? Mãe-D’Água?"),
        new Fala("Mariquinha", "Escolhe tu zefinha.")
    };

    private static readonly Fala[] CorpoSeco =
    {
        new Fala("Pai", "Corpo-seco? Ave Maria… Essa é de arrepiar até cabelo de defunto."),
        new Fala("Mariquinha", "Conta, painho! Num tenho medo não!"),
        new Fala("Pai", "Pois escuta. Diz o povo antigo que tinha um homem tão ruim, mas tão ruim, que maltratava até a própria mãe. Quando morreu, nem o céu quis, nem o inferno aceitou. A terra também num quis guardar o corpo dele."),
        new Fala("Mariquinha", "E fizeram o quê com ele?"),
        new Fala("Pai", "Deixaram o danado por aí. O corpo foi secando, secando… até virar só pele e osso. Mas dizem que ele num morreu de verdade. Fica andando pelas estrada, procurando quem passa sozinho."),
        new Fala("Mariquinha", "E se ele encontrar a gente?"),
        new Fala("Pai", "Aí tu corre, menina! Porque se for esperar por ele, num sobra nem o chinelo!"),
        new Fala("Mariquinha", "Zefinha, tu num vai me deixar sozinha não, né?"),
        new Fala("Zefinha", "Num vou não, Mariquinha."),
        new Fala("Pai", "Pois pronto. Enquanto tiver todo mundo junto, num tem assombração que faça essa família parar.")
    };

    private static readonly Fala[] ComadreFulozinha =
    {
        new Fala("Pai", "Comadre Fulozinha… Essa é danada de esperta. Dizem que é uma menina encantada que vive escondida nas mata."),
        new Fala("Mariquinha", "Uma menina? Igual a nós?"),
        new Fala("Pai", "Menina, mas num se engane. Tem cabelo comprido e gosta de fazer trança na crina dos cavalo. Quem entra na mata fazendo maldade, ela castiga."),
        new Fala("Mariquinha", "E quem num faz maldade?"),
        new Fala("Pai", "Aí depende do humor dela. Tem gente que deixa um agrado pra Comadre, pra num ser aperreado. Mas se tu ouvir um assobio fininho no meio da mata, é melhor num sair procurando de onde veio."),
        new Fala("Mariquinha", "Por quê, painho?"),
        new Fala("Pai", "Porque dizem que, quanto mais perto parece o assobio, mais longe ela tá. E quando parece longe demais… é porque tá bem pertinho de tu!"),
        new Fala("Mariquinha", "Vixe Maria! Zefinha, se eu ouvir assobio, vou segurar tua mão!"),
        new Fala("Zefinha", "E eu vou segurar a tua."),
        new Fala("Pai", "É melhor mesmo. A mata tem dono, minhas filha, e a gente tem que respeitar.")
    };

    private static readonly Fala[] MaeDAgua =
    {
        new Fala("Pai", "Mãe-D’Água… Essa história meu pai contava quando nós ia pescar no açude."),
        new Fala("Mariquinha", "Ela mora dentro d'água, painho?"),
        new Fala("Pai", "Mora nos rio, nos açude, onde a água é funda. Dizem que é uma mulher muito bonita, de cabelo comprido, que canta tão bonito que faz o cabra esquecer até o próprio nome."),
        new Fala("Mariquinha", "E ela canta o quê?"),
        new Fala("Pai", "Ninguém sabe dizer direito. Quem escuta fica encantado e vai seguindo a voz até entrar na água. E quando vê… já foi fundo demais."),
        new Fala("Mariquinha", "E num volta mais?"),
        new Fala("Pai", "Tem quem diga que não. Tem quem diga que ela leva o sujeito pro reino dela, lá no fundo."),
        new Fala("Mariquinha", "Eu queria conhecer uma Mãe-D’Água…"),
        new Fala("Pai", "Tu trate de conhecer é o chão debaixo dos teus pé, menina! Água funda num é lugar de brincadeira."),
        new Fala("Zefinha", "Melhor a gente ficar longe, Mariquinha."),
        new Fala("Mariquinha", "Tá bom… Mas eu queria ouvir ela cantar só um pouquinho."),
        new Fala("Pai", "Pois peça a Deus pra ouvir é chuva caindo. Esse é o canto que nós tá precisando agora.")
    };

    private Fala[] falas = Introducao;
    private int indice;
    private int quadro;
    private bool escolhendo;
    private bool ativo;
    private GameObject dialogueUI, dialoguePanel, choicesContainer;
    private TMP_Text characterNameText, dialogueText;
    private Image portraitImage;
    private Sprite retratoPainho;
    private readonly List<Button> botoes = new List<Button>();

    public void Iniciar(GameObject ui, GameObject panel, GameObject choices,
        TMP_Text nome, TMP_Text texto, Image retrato, Sprite painho)
    {
        dialogueUI = ui; dialoguePanel = panel; choicesContainer = choices;
        characterNameText = nome; dialogueText = texto;
        portraitImage = retrato; retratoPainho = painho;
        if (dialogueUI == null || dialoguePanel == null || characterNameText == null ||
            dialogueText == null || choicesContainer == null)
        {
            Debug.LogError("HistoriaPaiCaminhada: referencias do dialogo ausentes.", this);
            Destroy(this);
            return;
        }
        ativo = true;
        indice = 0;
        falas = Introducao;
        dialogueUI.SetActive(true);
        dialoguePanel.SetActive(true);
        AtivarGrupo(dialogueUI);
        AtivarGrupo(dialoguePanel);
        choicesContainer.SetActive(false);
        Mostrar();
    }

    private static void AtivarGrupo(GameObject go)
    {
        CanvasGroup grupo = go.GetComponent<CanvasGroup>();
        if (grupo == null) return;
        grupo.alpha = 1f;
        grupo.interactable = true;
        grupo.blocksRaycasts = true;
    }

    private void Mostrar()
    {
        characterNameText.text = falas[indice].Nome;
        dialogueText.text = falas[indice].Texto;
        if (portraitImage != null)
        {
            bool pai = falas[indice].Nome == "Pai";
            portraitImage.sprite = pai ? retratoPainho : null;
            portraitImage.enabled = pai && retratoPainho != null;
        }
        quadro = Time.frameCount;
    }

    private void Update()
    {
        if (!ativo || escolhendo || Time.timeScale == 0f ||
            Time.frameCount <= quadro || !Input.GetMouseButtonDown(0)) return;
        indice++;
        if (indice < falas.Length) { Mostrar(); return; }
        if (falas == Introducao) MostrarEscolhas();
        else Encerrar();
    }

    private void MostrarEscolhas()
    {
        escolhendo = true;
        characterNameText.text = "Escolha";
        dialogueText.text = "Qual história painho deve contar?";
        if (portraitImage != null) portraitImage.enabled = false;
        choicesContainer.SetActive(true);
        AtivarGrupo(choicesContainer);

        // Reutiliza os tres botoes ja existentes na cena, como MotherDialogue.
        botoes.Clear();
        foreach (Transform filho in choicesContainer.transform)
        {
            Button botao = filho.GetComponent<Button>();
            if (botao != null) botoes.Add(botao);
        }
        if (botoes.Count < 3)
        {
            Debug.LogError("HistoriaPaiCaminhada: ChoicesContainer precisa dos tres botoes existentes.", this);
            Encerrar();
            return;
        }
        string[] titulos = { "Corpo-seco", "Comadre Fulozinha", "Mãe-D’Água" };
        for (int i = 0; i < botoes.Count; i++)
        {
            Button botao = botoes[i];
            bool usar = i < 3;
            botao.gameObject.SetActive(usar);
            if (!usar) continue;
            TMP_Text rotulo = botao.GetComponentInChildren<TMP_Text>(true);
            if (rotulo != null) rotulo.text = titulos[i];
            int escolha = i + 1;
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(() => Escolher(escolha));
            botao.interactable = true;
        }
    }

    private void Escolher(int escolha)
    {
        foreach (Button botao in botoes)
        {
            if (botao != null)
            {
                botao.onClick.RemoveAllListeners();
                botao.gameObject.SetActive(false);
            }
        }
        botoes.Clear();
        choicesContainer.SetActive(false);
        falas = escolha == 1 ? CorpoSeco : escolha == 2 ? ComadreFulozinha : MaeDAgua;
        if (GameProgress.Instance != null)
            GameProgress.Instance.Data.estradaHistoriaEscolhida = escolha;
        indice = 0;
        escolhendo = false;
        Mostrar();
    }

    private void Encerrar()
    {
        ativo = false;
        dialogueUI.SetActive(false);
        Destroy(this);
    }

    private void OnDisable()
    {
        if (ativo && dialogueUI != null) dialogueUI.SetActive(false);
        foreach (Button botao in botoes)
            if (botao != null) botao.onClick.RemoveAllListeners();
        botoes.Clear();
    }
}
