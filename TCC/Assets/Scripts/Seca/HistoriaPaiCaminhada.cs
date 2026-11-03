using UnityEngine;

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
    private bool escolhendo;
    private bool ativo;
    private GUIStyle textoEstilo;
    private GUIStyle nomeEstilo;
    private GUIStyle botaoEstilo;
    private Texture2D fundo;

    public void Iniciar()
    {
        if (ativo) return;
        ativo = true;
        escolhendo = false;
        indice = 0;
        falas = Introducao;
    }

    private void Escolher(int escolha)
    {
        falas = escolha == 1 ? CorpoSeco : escolha == 2 ? ComadreFulozinha : MaeDAgua;
        indice = 0;
        escolhendo = false;
        if (GameProgress.Instance != null)
            GameProgress.Instance.Data.estradaHistoriaEscolhida = escolha;
    }

    private void Avancar()
    {
        indice++;
        if (indice < falas.Length) return;
        if (falas == Introducao)
            escolhendo = true;
        else
        {
            ativo = false;
            Destroy(this);
        }
    }

    private void PrepararEstilos()
    {
        if (textoEstilo != null) return;
        textoEstilo = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 16 };
        textoEstilo.normal.textColor = Color.white;
        nomeEstilo = new GUIStyle(textoEstilo) { fontStyle = FontStyle.Bold, fontSize = 18 };
        botaoEstilo = new GUIStyle(GUI.skin.button) { fontSize = 15, wordWrap = true };
        fundo = new Texture2D(1, 1);
        fundo.SetPixel(0, 0, new Color(0.10f, 0.09f, 0.08f, 0.93f));
        fundo.Apply();
    }

    private void OnGUI()
    {
        if (!ativo || Time.timeScale == 0f) return;
        PrepararEstilos();
        float largura = Mathf.Min(660f, Screen.width - 24f);
        float altura = escolhendo ? 245f : 190f;
        Rect painel = new Rect((Screen.width - largura) / 2f, Screen.height - altura - 16f, largura, altura);
        GUI.DrawTexture(painel, fundo);
        GUILayout.BeginArea(new Rect(painel.x + 16, painel.y + 12, painel.width - 32, painel.height - 24));
        if (escolhendo)
        {
            GUILayout.Label("Escolha a história que painho vai contar:", nomeEstilo);
            if (GUILayout.Button("Corpo-seco", botaoEstilo, GUILayout.Height(42))) Escolher(1);
            if (GUILayout.Button("Comadre Fulozinha", botaoEstilo, GUILayout.Height(42))) Escolher(2);
            if (GUILayout.Button("Mãe-D’Água", botaoEstilo, GUILayout.Height(42))) Escolher(3);
        }
        else
        {
            GUILayout.Label(falas[indice].Nome, nomeEstilo);
            GUILayout.Label(falas[indice].Texto, textoEstilo, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Continuar", botaoEstilo, GUILayout.Height(36))) Avancar();
        }
        GUILayout.EndArea();
    }

    private void OnDestroy()
    {
        if (fundo != null) Destroy(fundo);
    }
}
