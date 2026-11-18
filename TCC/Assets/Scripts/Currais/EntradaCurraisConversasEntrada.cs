using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    private bool ViuConversaEntrada(string id) => Dados.curraisConversasEntradaVistas.Contains(id);

    private void RegistrarConversaEntrada(string id)
    {
        if (!ViuConversaEntrada(id)) Dados.curraisConversasEntradaVistas.Add(id);
    }

    private IEnumerator DizerEntradaUmaVez(string id, Fala[] falas)
    {
        if (ViuConversaEntrada(id)) yield break;
        yield return Dizer(falas);
        // Só conclui após o último balão. Uma interrupção mantém o assunto disponível.
        RegistrarConversaEntrada(id);
        Salvar();
    }

    private void MostrarMenuEntrada(string id, string pessoa, string abertura, string[] rotulos, Action[] acoes)
    {
        if (rotulos.Length <= 1) { FecharMenu(); return; }
        if (pessoa == "Luzia" && !Dados.curraisLuziaApresentada)
        {
            MostrarMenuEntrada("luzia-sem-nome", "Mulher", "A mulher faz um pouco de espaço debaixo da lona.",
                new[] { "Com licença...", "Sair" },
                new Action[] { () => Iniciar(ApresentarLuzia(() => MostrarMenuEntrada(id, pessoa, abertura, rotulos, acoes))), FecharMenu });
            return;
        }
        string chave = "abertura:" + id;
        MostrarOpcoes(pessoa, ViuConversaEntrada(chave) ? string.Empty : abertura, rotulos, acoes);
        RegistrarConversaEntrada(chave);
        Salvar();
    }

    private void AbrirAssuntosEntrada(string id, string pessoa, string abertura,
        string[] assuntos, string[] rotulos, Func<IEnumerator>[] respostas)
    {
        var disponiveis = new List<string>();
        var acoes = new List<Action>();
        for (int i = 0; i < assuntos.Length; i++)
        {
            if (ViuConversaEntrada(assuntos[i])) continue;
            int indice = i;
            disponiveis.Add(rotulos[i]);
            acoes.Add(() => Iniciar(ResponderAssuntoEntrada(assuntos[indice], respostas[indice])));
        }
        disponiveis.Add("Sair"); acoes.Add(FecharMenu);
        MostrarMenuEntrada(id, pessoa, abertura, disponiveis.ToArray(), acoes.ToArray());
    }

    private IEnumerator ResponderAssuntoEntrada(string id, Func<IEnumerator> resposta)
    {
        if (ViuConversaEntrada(id)) yield break;
        yield return resposta();
        RegistrarConversaEntrada(id);
        Salvar();
    }

    private void FormarFilaDaFamilia(bool incluirIrma)
    {
        SpriteRenderer visual = mari.GetComponent<SpriteRenderer>();
        float direcao = visual != null && visual.flipX ? -1f : 1f;
        Transform[] pessoas = incluirIrma ? new[] { irma, mae, pai, baleia } : new[] { mae, pai, baleia };
        foreach (Transform pessoa in pessoas)
        {
            if (pessoa == null) continue;
            SeguidorFamilia seguidor = pessoa.GetComponent<SeguidorFamilia>();
            if (seguidor == null) continue;
            Vector3 posicao = pessoa.position;
            posicao.x = mari.transform.position.x - direcao * seguidor.DistanciaAtras;
            pessoa.position = posicao;
            Rigidbody2D corpo = pessoa.GetComponent<Rigidbody2D>();
            if (corpo != null) { corpo.position = posicao; corpo.linearVelocity = Vector2.zero; }
        }
        Physics2D.SyncTransforms();
    }

    private void MenuPresencaSemRepetir(PessoaPresente pessoa)
    {
        string id = "presenca:" + GameProgress.ObjectId(pessoa.personagem);
        AbrirAssuntosEntrada(id, pessoa.nome, "Diga.", new[] { id },
            new[] { "Como estão as coisas por aqui?" }, new Func<IEnumerator>[] { () => Dizer(pessoa.conversa) });
    }
}
