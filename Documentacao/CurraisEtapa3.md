# Currais — etapa 3: busca dentro do campo

Implementa os passos 9–12 do plano do Ato 2, cenas 8–12 do roteiro aprovado. A etapa seguinte começa no posto; a escolha E3, o epílogo e os finais ainda não fazem parte desta entrega.

## O que foi implementado

| Passo | Conteúdo |
|---|---|
| 9 — Raimundo | Apresentação, testemunho de Mariquinha viva com uma mulher, passado no cangaço e três opções juntas. As duas perguntas voltam ao menu; “Preciso continuar” encerra e libera a lavagem. |
| 10 — Lavagem | Conversa com a lavadeira e acompanhamento de Luzia. Quando a água foi dada à mãe, ela retorna ao abrigo e Mari precisa apoiar o pano. Quando Luzia recebeu a água, ela sustenta o peso e o percurso continua sem essa tarefa. |
| 11 — Corredor | Lembrança conforme Corpo-seco, Comadre Fulozinha ou Mãe-D’Água escolhida na Estrada. Galho e pessoa viva, sinal do guarda ou reflexo no tanque, seguidos pelo chamado de Luzia. Testar Currais diretamente sem escolha anterior não inventa uma lenda. |
| 12 — Trabalho e atendimento | Pai no trabalho, queda e agressão a Bento, breve encenação com os sprites atuais, mancha na roda, lembrança da professora e interação com o pano. A notícia da morte encerra a sequência; corpo coberto e roda manchada permanecem. |

As 44 falas das cenas 8–12 foram extraídas do roteiro sem reescrita. As duas mensagens de memória sobre a morte e o recomeço do serviço adaptam a narração cênica para leitura na tela. Não há teste de sanidade, cronômetro, pontuação, receita nova ou possibilidade oculta de salvar Bento. O caso é parte da história ficcional do roteiro.

## Como jogar

1. Conclua a escolha do laço ou canhoto da etapa 2. Vá para a direita e aproxime-se de Raimundo, ao lado dos baldes. Clique nele ou pressione E.
2. Pergunte o que desejar e escolha “Preciso continuar”. Caminhe com A/D e Shift até a lavadeira. Converse. Se o objetivo pedir, aproxime-se do pano molhado e clique ou pressione E.
3. Continue até o tanque maior no corredor. Interaja com a passagem para a lembrança. Depois, caminhe até o pátio e interaja para observar o trabalho.
4. Após os textos, aproxime-se do pano junto de Bento e interaja para ajudar no atendimento. Leia a notícia. O movimento volta ao terminar; a investigação no posto será desenvolvida na etapa 4.

Clicar longe pede aproximação; não transporta Mari. O movimento é interrompido nos diálogos e encenações, voltando entre as interações. A agressão é apresentada com os personagens existentes, queda, inclinação do fiscal, textos e sangue na roda; não foi criada uma animação detalhada de golpes. Os policiais humanos existentes continuam no lugar dos guardas especiais, conforme combinado.

## Cena e arquivos

A própria `Assets/Scenes/Currais.unity` foi ampliada, sem nova cena. O grupo `04 Busca dentro do campo - etapa 3` contém lavagem, corredor e pátio. Tanques, baldes, tecidos, carga, personagens, manchas e corpo coberto são objetos separados. Os alvos de interação e as marcas dos limites também são editáveis. A ponta de tecido carregada fica como filha de Mari.

A única imagem nova está em `Assets/Imagens/Currais/BuscaObjetos.png`, com oito sprites recortados no importador. Fundo, chão, cercas, carroça, pessoas e policiais reutilizam recursos do projeto. Raimundo e Bento usam provisoriamente o adulto existente, com tonalidades diferentes; não são modelos definitivos novos. Nenhuma imagem está embutida em C#.

`EntradaCurraisBusca.cs` é uma parte do mesmo componente `EntradaCurrais` (classe partial). Não deve ser adicionado como outro componente na Hierarchy. O controlador, Canvas, botões, seguidores, diário, inventário e salvamento continuam sendo os existentes.

`CameraFollow.AtualizarLimites()` só é chamado por Currais quando uma passagem é liberada. As outras cenas mantêm o comportamento anterior. Os limites são reconstruídos antes de limitar a posição restaurada, permitindo continuar uma partida já na lavagem ou no pátio. Não é preciso apagar o save.

## Progresso

Foram acrescentados `curraisEtapaBusca`, `curraisRaimundoConhecido` e `curraisPerguntasRaimundo` ao modelo existente, com valores iniciais compatíveis com partidas antigas. Os checkpoints registram saídas de conversas e conclusões de interações. Um carregamento durante uma encenação repete somente o trecho ainda não concluído. Anotações sobre Raimundo, lavagem, corredor e Bento usam o diário atual.

A etapa não altera a escolha de água, o pertence guardado, o inventário nem as confirmações de destino e identidade. Os dois dados da busca continuam disponíveis para as escolhas das próximas etapas.

## Verificações e limites

Conferidos estaticamente: sintaxe dos scripts, referências locais/GUIDs dos novos recursos, hierarquia, tipos dos campos, dimensões dos sprites, alvos alcançáveis em cada limite, ligações do Canvas e preservação das falas e objetos anteriores. Composições da lavagem e do pátio foram inspecionadas por uma prévia estática.

Não foi possível executar Unity/Play Mode neste ambiente. Conferir no Editor: iniciar por Início/Seca/Estrada e chegar a Currais; continuar o save anterior; água à mãe e a Luzia; laço e canhoto; três lendas e teste isolado sem lenda; perguntas opcionais e saída de Raimundo; clique e E; pausa; inventário/diário; continuar antes e depois de cada interação, especialmente no pátio e no atendimento. Confirmar que o campo preserva o cenário e que Mari volta a andar após cada diálogo.

## Arte adicionada

Atlas gerado com a ferramenta de imagens integrada: uma folha transparente com quatro colunas e duas linhas, contendo tanque, baldes, pano molhado, sacos, galho com tecido, mancha de sangue, corpo coberto e pano de atendimento. Direção usada: pixel art simples, poucos tons terrosos, vista lateral, sem texto, brilho, cenário embutido ou detalhes excessivos. A imagem original foi copiada para o projeto e recortada somente pelo importador de sprites.
