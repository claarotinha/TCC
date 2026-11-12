using UnityEngine;

public partial class EntradaCurrais
{
    public static Vector3 AtualizarPosicaoCenario(Vector3 posicao, GameProgressData dados)
    {
        float anterior = posicao.x;
        if (anterior >= 23.3f) posicao.x = 23.3f + (anterior - 23.3f) * 1.8f;
        if (dados.curraisEtapaEntrada == 2) posicao.x = 33.45f;
        else if (dados.curraisEtapaPosto >= 1 && dados.curraisEtapaConsulta == 0 &&
            anterior >= 106.5f && anterior <= 113.5f) posicao.x += 40f;
        return posicao;
    }
}
