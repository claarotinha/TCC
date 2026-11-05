# Currais — etapa 5: manhã e última consulta

Implementa os passos 17–20 do plano, correspondentes às cenas 17–20 do Ato 2. Termina no retorno ao abrigo com as pistas preservadas. A passagem do tempo da cena 21, o epílogo e os finais continuam fora desta etapa.

## O que foi feito

| Passo | Implementação |
|---|---|
| 17 — Volta ao posto | Após a noite, uma interação na esteira faz amanhecer. Mari volta a caminhar com o pai. A mãe acompanha até a marca de limite do setor. Raimundo aparece perto ou mais longe, conforme a ajuda/cópia da etapa 4, mantendo o pano na mão. O diálogo das ordens e do pedido do pai usa as falas do roteiro. |
| 18 — Última consulta, E4 | O Canvas atual mostra “Lugar conhecido” e “Nome conhecido”, com os dados confirmados ou “Ainda não confirmado”. As opções de confirmar destino ou chamar testemunha aparecem juntas. Sair adia a decisão. A confirmação repetida é reconhecida; nenhuma informação anterior é apagada. |
| 19 — A saída recusada | O pai pede autorização no portão; a mãe chega e participa da conversa. O portão existente fecha com um movimento breve. O próprio guarda visível recebe um collider físico nesta etapa, impedindo atravessar a passagem. Não há combate ou missão de fuga. |
| 20 — O que ainda sabemos | A família volta caminhando ao abrigo. As falas variam entre duas pistas, só destino ou só identidade. Se o laço foi guardado, a mãe pede para ajeitá-lo e surge o tecido remendado; se foi perdido, o diálogo reconhece a perda. O inventário não recebe um segundo laço. |

As 32 falas das cenas 17–20 foram extraídas do roteiro sem reescrita. Três mensagens de Memória adaptam as indicações cênicas: Raimundo pede cuidado com o nome, o destino foi reconfirmado ou outra testemunha confirmou a acompanhante. Não há barra moral, porcentagem de final, cronômetro ou exigência de adivinhar um nome.

## Como iniciar e jogar

1. Termine a noite da etapa 4. Aproxime-se da esteira e clique ou pressione E para amanhecer.
2. Caminhe com A/D e Shift até o posto à direita do pátio. O pai acompanha. A mãe para no limite indicado pela marca da cena.
3. Interaja perto da mesa. Leia as informações conhecidas antes de escolher. A opção Sair permite andar e retornar à consulta sem confirmar nada. Depois de uma escolha válida, a consulta não pode ser refeita para obter o outro dado.
4. Volte ao portão do setor, à esquerda do abrigo, e interaja perto do guarda para acompanhar o pedido de saída.
5. Depois da recusa, volte ao abrigo com a família e interaja perto dos pais. Leia as notícias e o ramo do laço. Ao terminar, o movimento volta. É possível relembrar as informações ou sair desse menu.

A escolha do destino preserva a ressalva: encaminhamento não comprova chegada. A testemunha confirma Nair Pereira, mas não afirma saber onde chegaram depois. Mariquinha não reaparece e sua morte não é confirmada.

## Escolhas e salvamento

`curraisEtapaConsulta`: 0 amanhecer, 1 consulta, 2 pedido de saída, 3 retorno, 4 concluída.

`curraisEscolhaConsulta`: 0 nenhuma, 1 destino, 2 acompanhante. `curraisConsultaApresentada` evita repetir a apresentação ao sair e voltar. `curraisLacoRemendado` registra somente o caminho em que o laço foi guardado.

Os novos campos usam valores iniciais compatíveis com saves anteriores. Não é necessário apagar o save. A conclusão da noite da etapa 4 libera esta etapa; o ambiente só deixa de ser noturno quando o jogador interage para amanhecer.

A escolha E4 só acrescenta uma confirmação. As decisões de água, comida, promessa, lenda, laço/canhoto e ajuda/cópia são preservadas. Confirmar novamente uma pista não concede outro item. O outro dado permanece como estava. Destino e identidade continuam sendo os mesmos dois registros existentes.

Checkpoints: amanhecer, apresentação da consulta, decisão E4, recusa da saída e conversa de retorno. Ao continuar, os acompanhantes são reposicionados perto de Mari ou na marca de espera correspondente à fase. Trechos ainda não concluídos podem ser repetidos; escolhas concluídas não são reapresentadas como disponíveis.

## Objetos, interface e arquivos

A cena `Assets/Scenes/Currais.unity` continua sendo usada. Não foi criada outra cena nem gerada outra imagem. O grupo `06 Manha e ultima consulta - etapa 5` reúne a relação de saída, o escrivão, a testemunha e os elementos do retorno. Mesa e portão são os existentes. As áreas da esteira, posto, portão e família foram reutilizadas.

As posições da família, de Raimundo, do portão aberto/fechado e do acompanhamento são marcas independentes no grupo de encenação. O laço remendado e o pequeno tecido aplicado são filhos da mãe, com sprites separados e editáveis. O bloqueio físico pertence ao guarda visível e fica desativado antes desta etapa.

`EntradaCurraisConsulta.cs` é parte da mesma classe parcial `EntradaCurrais`. Não deve ser adicionado como um segundo componente. Foram reutilizados o Canvas, os botões, os seguidores, o diário, o inventário e o sistema de saves. O pai usa seu componente SeguidorFamilia original. Os modelos do escrivão e demais adultos continuam provisórios e reutilizados.

A prioridade de desenho da família é elevada somente na encenação do portão para que a mesa não encubra a conversa. Os valores anteriores são restaurados ao concluir ou desativar o controlador. A iluminação da manhã restaura as cores locais do cenário, sem alterar materiais compartilhados ou outras cenas.

Foram incluídas quatro anotações no diário: confirmação do destino, confirmação do nome, saída recusada e informações preservadas. Apenas o ramo escolhido da consulta é desbloqueado. A anotação final é geral; os dados completos permanecem visíveis na conversa de revisão.

## Integração e conferência

A conferência da Hierarchy encontrou e corrigiu uma falha do auxiliar usado na montagem anterior: o pai padrão de novos objetos ficava preso à raiz do campo. Os grupos das etapas 3–4 foram vinculados aos setores correspondentes. Os tecidos, curativos e laços passaram a ser filhos efetivos dos personagens, e os adultos do quarto ficaram dentro do seu interior. As posições locais dos objetos foram mantidas; os filhos de personagens agora usam de fato as posições locais previstas para acompanhar o movimento. As referências do controlador, falas e recursos anteriores permanecem as mesmas.

As atualizações das etapas 2–4 foram condicionadas para não devolver os pais às posições antigas durante a caminhada desta etapa. A noite antiga deixa de controlar suas posições depois do amanhecer. O estado do posto reconstrói papel, testemunha, portão e remendo ao continuar.

Verificados estaticamente: sintaxe dos scripts; referências e tipos na cena; manutenção das falas e referências anteriores; sprites reutilizados; áreas alcançáveis; ligação do pai ao seguidor existente; collider físico ligado ao guarda; entradas do diário sem IDs duplicados; lógica de preservação das pistas nas oito combinações de E2/E3/E4. As composições do posto e do portão foram inspecionadas em prévias estáticas.

Nenhuma outra cena foi modificada. Não há Unity/Play Mode disponível neste ambiente; estes testes não substituem a execução no Editor.

Verificar no Unity: chegar desde Início/Seca/Estrada; continuar o save do fim da etapa 4; amanhecer; acompanhar pai/mãe; laço/canhoto × ajuda/cópia × destino/testemunha; Sair da consulta antes de escolher; confirmação repetida; impedir uma segunda E4; fechar portão e respeitar a colisão; voltar ao abrigo; dois ramos do laço; pausa, inventário e diário; continuar antes/depois das interações e após a recusa; movimento liberado ao terminar cada conversa.
