# Currais — etapa 2: primeiras pistas

Implementa os passos 5–8 na cena Currais. O epílogo e os finais não foram alterados.

## Percurso para testar

1. Após a chegada ao abrigo, aproxime Mari de Luzia e escolha **Buscar água**. Caminhe com A/D e Shift até o funcionário da distribuição, à direita. Clique nele ou pressione E perto dele.
2. O funcionário devolve a cabaça dos pertences retidos e entrega uma única porção. Volte ao abrigo e interaja com a mãe ou Luzia. As duas opções aparecem juntas: dar à mãe ou dar a Luzia. Não há terceira porção ou cronômetro.
3. A destinatária acompanha Mari até a mesa dos nomes. No caminho, o menino é opcional: perguntar sobre a espera, dizer o nome de Mariquinha ou sair. As respostas podem ser consultadas novamente.
4. Converse com o escrivão. A mãe e Luzia têm respostas diferentes, conforme a escolha anterior. O guarda encerra a consulta. Depois, volte à panela junto da entrada.
5. Escolha entre o laço e o canhoto. Somente o item escolhido entra no inventário. O fiscal recolhe a panela depois disso. A busca por quem viu o outro grupo continuará na etapa 3.

Cliques distantes pedem aproximação, sem transportar Mari. Os menus opcionais têm Sair. O diálogo de despedida do menino segue o roteiro. Escape mantém a pausa existente.

## Consequências e progresso

| Escolha | Consequência |
|---|---|
| Água à mãe | A mãe acompanha a consulta; Luzia fica no abrigo. |
| Água a Luzia | Luzia acompanha; a mãe fica com o pai. |
| Laço | Guarda a recordação no inventário, sem confirmar o destino nesta cena. |
| Canhoto | Guarda o papel e confirma Lagoa do Meio; não identifica a acompanhante de Mariquinha. |

A escolha da água não determina final nem provoca morte automática. Os registros são preservados para as próximas etapas.

O recebimento da porção e sua entrega são estados separados. Continuar com a cabaça cheia mantém a porção; depois da entrega, ela fica vazia. Somente uma cabaça é retirada dos itens retidos. Os demais pertences continuam registrados. Reabrir diálogos não concede itens nem permite escolher novamente.

O laço e o canhoto foram adicionados ao catálogo existente de salvamento. Sete anotações foram acrescentadas ao diário existente, desbloqueadas pelas interações correspondentes. O diário físico do presente não é adicionado ao inventário de 1932. A acompanhante reaparece perto da posição restaurada de Mari ao continuar.

## Organização

A cena existente foi ampliada com distribuição, menino e mesa dos nomes. Personagens, mesas, cercas, recipientes, laço e canhoto são objetos independentes na Hierarchy. Os alvos têm colliders que não bloqueiam a caminhada. As marcas de posição da panela e do menino ficam no grupo de encenação.

Foram reutilizados o Canvas, os quatro botões de opções, o diário, o inventário, o sistema SeguidorFamilia, os policiais e outros personagens existentes. Os botões receberam altura e ajuste de texto para opções maiores, mantendo o modelo visual. As prioridades de desenho corrigidas anteriormente foram preservadas.

O controlador EntradaCurrais foi ampliado, sem criar outro sistema de diálogo. GameProgressData recebeu os registros desta etapa. A cena Estrada, a transição anterior e as outras cenas não foram alteradas.

Os novos ItemData estão em Assets/coletaveis/Currais. A única imagem nova, Canhoto.png, fica em Assets/Imagens/Currais; o laço usa o atlas anterior. Nenhuma imagem é embutida no código.

## Conferência realizada

Referências locais, campos serializados, sprites, catálogo e sintaxe dos scripts conferidos. As 41 falas novas foram comparadas ao roteiro aprovado, incluindo as alternativas da água e da acompanhante. As 30 falas da etapa 1 permanecem preservadas. Catálogo e anotações não contêm entradas novas duplicadas.

Não foi possível executar Unity/Play Mode neste ambiente. Testar os quatro caminhos (duas destinatárias × dois pertences), menino ignorado/conversado, saída dos menus, pausa e Continuar antes da distribuição, com a porção cheia, depois da água, da consulta e da escolha do pertence. Confirmar movimento, inventário e ausência de escolhas repetidas.

## Arte

Imagem gerada para o canhoto: papel creme rasgado, marca de carroça e palavras LAGOA / DO MEIO em pixel art simples, com transparência. Revisão da geração: remover brilho/sombra externos, preservar as palavras e a marca, usar cores creme e marrom com bordas firmes. Importação com filtro de ponto e canal alpha preservado.
