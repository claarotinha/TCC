# Currais — etapa 4: posto, quarto e noite

Implementa os passos 13–16 do plano do Ato 2, correspondentes às cenas 13–16 do roteiro. A etapa termina na noite do abrigo. A consulta da manhã seguinte, a escolha E4, o epílogo e os finais permanecem para depois.

## Conteúdo implementado

| Passo | O que acontece |
|---|---|
| 13 — O posto por um instante | Luzia abre a oportunidade de conversa. As opções de ajudar Raimundo e copiar o registro aparecem juntas. Sair adia a decisão sem gravá-la. Ajudar confirma Nair Pereira; copiar confirma Lagoa do Meio. A repetição do destino já conhecido pelo canhoto é reconhecida sem produzir uma segunda pista. |
| 14 — O quarto sem Mariquinha | A lembrança da pergunta sobre sorvete leva à investigação de uma passagem. A abertura revela duas pessoas adultas deitadas e uma trabalhadora. Mariquinha não é criada no quarto. O policial humano atual ocupa o lugar do cabo especial. |
| 15 — Notícias no abrigo | As falas aos pais consideram destino e identidade confirmados, separadamente. A conversa opcional retoma comer tudo, dividir com Baleia ou dividir com a irmã na Seca. No teste isolado sem essa escolha, a conversa da comida não é inventada. |
| 16 — Uma noite sem a outra mão | A lembrança varia conforme promessa, tranquilização ou silêncio na Estrada. O laço aparece se foi guardado; caso contrário, há a dobra da roupa. As duas opções de expressão mantêm as falas do roteiro. A mãe permanece perto e a cabaça vazia aparece no chão ao final. |

As 40 falas das cenas 13–16 foram extraídas do roteiro e mantidas. A observação “É o mesmo lugar do canhoto.” segue a reconvergência prevista. Menus opcionais têm Sair. Não há cronômetro, nova receita, punição automática, pontuação por escolhas ou alteração dos critérios de final.

## Percurso para testar

1. Termine o atendimento de Bento na etapa 3. O objetivo passa a pedir a ida ao posto à direita do pátio. Caminhe até a mesa de encaminhamentos e use clique ou E perto dela.
2. Leia a fala de Luzia e escolha ajudar Raimundo ou copiar o registro. Sair permite voltar mais tarde. Só uma alternativa é registrada. O papel some após a decisão; Raimundo aparece com o pano de atendimento. Se recebeu ajuda, fica perto do abrigo; caso contrário, vai para outro setor.
3. Volte pelo corredor, aproxime-se da porta de madeira junto ao cabo e interaja. Leia a lembrança e a conversa após a abertura.
4. Volte ao abrigo e interaja perto dos pais. As notícias aparecem conforme as pistas conhecidas. Depois, é possível relembrar a refeição ou as notícias, ou sair do menu.
5. Aproxime-se da esteira à direita da família para iniciar a noite. Leia a lembrança da Estrada e escolha contar a última fala ou pedir companhia. Ao terminar, o jogo permanece naquela noite, com movimento liberado. A manhã seguinte será feita na etapa 5.

A caminhada continua com A/D e Shift entre interações. Cliques distantes pedem aproximação. Diálogos, abertura do quarto e passagem para a noite interrompem o movimento temporariamente; o controlador existente o libera ao concluir.

## Consequências preservadas

- Ajuda: `curraisIdentidadeConfirmada` fica verdadeiro; destino previamente confirmado continua verdadeiro.
- Cópia: `curraisDestinoConfirmado` fica verdadeiro; identidade previamente confirmada não é apagada.
- O laço e o canhoto guardados na etapa 2 permanecem no inventário. A cópia é anotada no diário, sem conceder um segundo canhoto.
- `secaFoodChoice`, a promessa, as três lendas e as decisões anteriores não são regravadas.
- A escolha da noite é registrada separadamente e não muda destino ou identidade.
- O destino de Bento não muda por causa dessas decisões. Copiar o registro não faz Raimundo ser executado ou ficar sem atendimento.

## Organização do projeto

A própria `Assets/Scenes/Currais.unity` foi ampliada. O grupo `05 Posto quarto e noite - etapa 4` contém os setores do posto, do quarto e do abrigo. Mesa, folha, baldes, passagem, cobertura, esteiras, pessoas e cabaça são objetos separados. Os tecidos e o laço em uso são filhos dos personagens para acompanhar seu posicionamento. As marcas de posição da família e de Raimundo continuam editáveis no grupo de encenação.

Todos os sprites foram reutilizados: `Imagens/Currais/EntradaObjetos.png`, `BuscaObjetos.png`, `Canhoto.png`, personagens existentes e cabaça. Não foi necessário gerar outra imagem ou criar uma cena nova. O interior é representado com a cobertura, as esteiras e os adultos disponíveis; não é uma nova pintura de quarto. Os gestos usam posicionamento e objetos, sem novas animações de mãos ou de dormir.

`EntradaCurraisPosto.cs` é outra parte da mesma classe parcial `EntradaCurrais`. Não deve ser anexado como um segundo componente. O Canvas, os quatro botões, o inventário, o diário, os seguidores e o salvamento existentes foram reutilizados.

A noite altera somente as cores dos sprites de ambiente explicitamente vinculados na cena. Não altera o Canvas, o volume global ou outras cenas. A família foi posicionada com espaço entre os personagens; a mãe fica perto de Mari, com a esteira vazia da irmã ao lado. A encenação termina com Mari acordada.

## Salvamento

Novos campos do modelo existente: `curraisEtapaPosto`, `curraisEscolhaPosto`, `curraisRefeicaoRelembrada` e `curraisEscolhaNoite`. Partidas antigas começam esta etapa com valores zero, sem precisar apagar o save.

A decisão do posto é gravada junto com a progressão depois da conversa. Fechar o menu com Sair não confirma uma alternativa. Os checkpoints registram o quarto investigado, as notícias contadas e a noite concluída. Uma interrupção durante um trecho ainda não concluído permite repeti-lo, sem perder as escolhas anteriores. O ambiente noturno e os objetos são reconstruídos ao continuar.

Foram acrescentadas cinco anotações ao diário: Nair, cópia do encaminhamento, quarto, notícias aos pais e noite. Só o ramo escolhido do posto é desbloqueado. A lista anterior permanece preservada.

## Conferência realizada e limites

Conferidos estaticamente: sintaxe de todos os scripts; campos e referências internas da cena; sprites e metadados reutilizados; correspondência das novas falas; preservação das referências e falas anteriores; alcance dos alvos; objetos filhos de seus personagens; ligações do Canvas e do controle de som; entradas do diário sem IDs duplicados. Prévias estáticas do quarto e da noite foram inspecionadas e o posicionamento da família foi ajustado para evitar sobreposição.

Nenhuma outra cena foi modificada nesta etapa. A integração continua pelo controlador de Currais e pelo modelo de progresso compartilhado. Não foi possível executar Unity/Play Mode neste ambiente.

Verificar no Editor: chegar por Início/Seca/Estrada e continuar o save anterior; laço/canhoto × ajuda/cópia; sair do posto antes de decidir; três escolhas da comida; promessa/tranquilização/silêncio; duas opções da noite; clique e E; pausa e saída dos menus; continuar antes/depois de cada interação e no meio de trechos; movimento após diálogos; ambiente noturno ao continuar; inventário e diário sem duplicações.
