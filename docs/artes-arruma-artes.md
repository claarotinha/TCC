# Organização do commit Arruma artes

Base: 3374d3c92c6aa01fd2d722e396216765ec56f40c. Os PNGs originais não foram alterados nem substituídos por imagens geradas.

| Artes | Uso e edição |
| --- | --- |
| Carroça 1–5 | Carroça da outra leva e carroça do pátio, em Currais. Ciclo QuadrosAmbiente nos próprios objetos; rodas extras antigas ficam desativadas na Hierarchy. |
| Baú | Objeto Baú dos pertences, próximo da mesa da revista. Posição e escala serializadas. |
| Mesa e livro novos | Alternativas desativadas junto dos objetos correspondentes em Currais. Os arquivos possuem fundo opaco; as versões atuais continuam no jogo para não cobrir o cenário. Ativar as alternativas permite ajustar o encaixe, mas a transparência precisa de tratamento no arquivo. |
| Casa arrumada — Estrada | Recortes corrigidos para fechada, porta aberta e janela quebrada; referências na SegundaParada e fachada editável. |
| Casa arrumada — Seca | Fachada casinha de barro, com o novo recorte da casa fechada. Outros dois recortes permanecem disponíveis no asset. |
| Árvore seca e dois cactos | Objetos individuais de cenário na Estrada, próximos do acampamento, sem colisores novos. |
| Erva e pilha de madeira | Planta medicinal e galhos secos coletáveis da Estrada, preservando suas interações. |
| Baleia com a caça 1–4 | Ciclo após encontrar Baleia. Quadros e intervalo na SegundaParada. |
| Fogueira com bichos 1–5 | Ciclo apenas no estado de refeição assando. Preparo, fogo simples e brasas mantidos. |
| Mariquinha com sangue | Idle 1–4, walk 1–6 e run 1–10 no componente MariquinhaFerimentoVisual da própria personagem, na Estrada e em Currais. |
| Mariquinha enfaixada | Idle 1–4 e walk 1–6 no mesmo componente. Ao correr com curativo, mantém o ciclo enfaixado de caminhada porque não foi fornecida corrida enfaixada nova. |
| Espelho novo | Imagem de revelação no Canvas da Seca, com proporção preservada. |
| Família à mesa, com e sem Mari | Estados da mesa na Seca. A variante anterior sem cabaça é preservada; os três estados têm altura normalizada para evitar saltos. |

Os recortes do baú e das casas foram corrigidos no importador sem editar os PNGs. Filtro Point, sem mipmaps e sem compressão para as novas texturas. Quadros exportados na mesma tela usam recortes alinhados para reduzir saltos entre poses. IDs/GUIDs foram preservados.

Verificações estáticas: referências locais e grafo das cenas; correspondência entre sprites usados e IDs dos importadores; sintaxe dos scripts; CSV de Currais; PNGs mantidos e patch sobre o commit base. Compilação, Play Mode e revisão visual no Unity continuam pendentes: proporções, fundo das alternativas, animações, três estados da mesa, porta/janela, cura de Mariquinha e retomada de save.
