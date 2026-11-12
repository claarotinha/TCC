using System.Collections.Generic;

public static class DialogoPaginas
{
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
