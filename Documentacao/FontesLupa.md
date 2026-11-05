# Fonte Xilosa_PT e lupa

## Fonte

238 componentes TextMeshPro em 19 arquivos de cenas/prefabs passaram a usar `TextMesh Pro/Fonts/Xilosa_PT SDF.asset`, ligado à `Xilosa_PT.ttf` adicionada ao projeto. A troca inclui menus, avisos, diálogos, opções, objetivos, inventário, diário, pausa e textos dos demais ambientes do projeto.

A fonte padrão do TMP Settings também foi trocada. Assim, os textos criados em execução pelos componentes existentes recebem Xilosa_PT, sem um novo gerenciador de estilo.

Foram alteradas somente as referências da fonte e de seu material nos textos. Tamanhos, limites de autosize, cores, gradientes, alinhamento, posições, espaçamentos, estilos e conteúdo foram comparados com a versão anterior e mantidos. Os materiais anteriores e o novo usam a mesma cor de face branca, preservando a cor atribuída a cada texto.

A fonte PT contém os glifos acentuados portugueses, incluindo versões maiúsculas. O atlas dinâmico foi habilitado para usar mais de uma textura, evitando esgotar a primeira página durante a exibição de diálogos extensos. O fallback já existente do LiberationSans foi vinculado apenas como alternativa para símbolos ausentes da fonte fornecida, como reticências e travessões. As letras e os acentos são fornecidos pela Xilosa_PT principal. O arquivo TTF adicionado pela autora não foi modificado.

## Lupa

Foi reutilizado `prefab/CursorManager.prefab`, com a imagem e o recorte da lupa já existentes. Estrada e Currais receberam uma instância cada, na Hierarchy. Seca e CaminhoParaCasa mantêm suas instâncias existentes. A vinculação do painel de diálogo em Seca, Estrada e Currais impede a lupa nos objetos atrás das conversas.

CursorManager agora consulta os controladores locais de Estrada e Currais e reconhece suas áreas de interação. São consultas de leitura: não executam diálogos, coletas, uso de itens ou avanço de etapa ao passar o mouse.

- **Seca:** cama quando liberada, espelho, investigações, porta, mesa, mãe e objetos coletáveis já habilitados.
- **Estrada:** coleta e alvos de uso dos itens existentes, Mariquinha, pássaro, riacho, pegada da vez, Baleia ao concluir as pistas, casa, barriguda, planta, retirantes, mãe do acampamento, materiais e fogueira conforme a etapa.
- **Currais:** laço inicial, conversas e alvos da busca, lavagem, corredor, pátio, atendimento, posto, quarto, família, esteira, última consulta, portão e páginas da memória, conforme as áreas ativas.
- **CaminhoParaCasa:** preservadas as restrições do tutorial e os comportamentos existentes; interações genéricas e ações por E também são reconhecidas quando disponíveis.

A lupa continua sendo retirada em pausa, inventário e menus, durante diálogos/encenações e sobre controles de interface. Os cliques, as distâncias e os requisitos das interações permanecem os mesmos. A consulta da Estrada também considera o desenho real de Baleia e os alvos definidos por posição/visual, sem depender de um collider decorativo interceptando o ponteiro.

As interações por componentes existentes ganharam propriedades públicas de disponibilidade, usadas somente pela decisão do cursor. Nenhum objeto ganhou uma ação que não possuía antes.

A posse do CursorManager foi ajustada para a troca de cena: um cursor de outra cena sendo descarregada não deve desativar o gerenciador da cena que está chegando. Ao desativar o gerenciador atual, o cursor normal é restaurado.

## Verificações

- Todos os 238 textos encontrados usam a fonte PT e seu material de atlas; a fonte padrão também aponta para PT.
- Comparação dos demais campos dos textos e componentes: conteúdo, tamanho, cor e layout preservados.
- Fonte TTF inspecionada: letras portuguesas e acentuadas presentes, com contornos não vazios.
- Uma instância do prefab do cursor em cada uma das quatro cenas; referências dos painéis e SceneRoots verificadas, incluindo os Transforms herdados do prefab.
- Recorte da lupa existente e Read/Write da textura conferidos.
- Sintaxe dos scripts e ausência de aliases YAML nos assets serializados conferidas.
- As consultas de hover não chamam os métodos que modificam inventário ou progressão.

Não há Unity/Play Mode disponível neste ambiente. Testar no Editor começando pelo início e também diretamente por Seca/Estrada/Currais: objetos bloqueados/liberados, diálogos abertos/fechados, coleta concluída, sequência das pegadas, Baleia, água, pausa, inventário, transição entre cenas e retomada do save. Conferir a fonte com acentos em menus, diálogo, diário, opções e porcentagem do volume. Parar e reiniciar o Play Mode após atualizar o projeto; não é necessário apagar saves.
