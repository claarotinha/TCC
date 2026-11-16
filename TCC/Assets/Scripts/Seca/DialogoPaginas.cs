using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class DialogoPaginas
{
    private static readonly Dictionary<TMP_Text, Queue<string>> continuacoes = new Dictionary<TMP_Text, Queue<string>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar() => continuacoes.Clear();

    public static void Exibir(TMP_Text campo, string texto)
    {
        if (campo == null) return;
        Canvas.ForceUpdateCanvases();
        var paginas = new Queue<string>();
        string restante = texto ?? string.Empty;
        // Mede a fonte e a largura reais do balão, sem reduzir a letra.
        while (restante.Length > 0)
        {
            TMP_TextInfo info = campo.GetTextInfo(restante);
            if (info.lineCount <= 2) { paginas.Enqueue(restante); break; }
            int caractere = info.lineInfo[2].firstCharacterIndex;
            int corte = info.characterInfo[caractere].index;
            if (corte <= 0) { paginas.Enqueue(restante); break; }
            paginas.Enqueue(restante.Substring(0, corte).TrimEnd());
            restante = restante.Substring(corte).TrimStart();
        }
        campo.text = paginas.Count > 0 ? paginas.Dequeue() : string.Empty;
        if (paginas.Count > 0) continuacoes[campo] = paginas;
        else continuacoes.Remove(campo);
    }

    // O mesmo clique continua a fala antes de mudar o autor ou executar o fim.
    public static bool Avancar(TMP_Text campo)
    {
        if (campo == null || !continuacoes.TryGetValue(campo, out Queue<string> paginas) || paginas.Count == 0) return false;
        campo.text = paginas.Dequeue();
        if (paginas.Count == 0) continuacoes.Remove(campo);
        return true;
    }

    // Divide em pausas de leitura; preserva as palavras e o nome do personagem.
    public static IEnumerable<string> Quebrar(string texto, int limite = 200)
    {
        texto = texto ?? "";
        while (texto.Length > limite)
        {
            int corte = texto.LastIndexOfAny(new[] { '.', '!', '?', ';' }, limite - 1, limite);
            if (corte < limite / 3) corte = texto.LastIndexOf(' ', limite);
            else corte++;
            if (corte <= 0) corte = limite;
            yield return texto.Substring(0, corte).Trim();
            texto = texto.Substring(corte).TrimStart();
        }
        if (texto.Length > 0) yield return texto;
    }
}
