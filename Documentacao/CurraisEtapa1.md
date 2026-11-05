# Currais — Ato 2, etapa 1

Implementa os passos 1–4: continuação da abordagem, separação antes do portão, entrada/revista e abrigo de Luzia. O epílogo e as escolhas da etapa 2 permanecem fora desta entrega.

## Fluxo

1. Ao terminar a abordagem policial da `Estrada`, o jogo grava o destino e carrega `Currais`. Partidas antigas que terminaram o Ato 1 também seguem para a nova cena.
2. A família é separada do lado de fora. Mariquinha tem uma fala diferente se houve promessa na noite anterior. A irmã continua viva e acompanhada na outra fila. Não há comando para impedir a separação.
3. Depois da sequência, clique no laço no chão. A aproximação é guiada, sem teste de habilidade.
4. Na revista, os itens são retirados do inventário disponível e preservados por seus IDs no progresso. O laço fica na panela. A devolução da cabaça pertence à próxima etapa, na fila da água.
5. A família chega ao abrigo de Luzia. Ao terminar a conversa, A/D e Shift continuam funcionando dentro dos limites do setor. Clique na esteira, cerca, fila ou portão para examinar. Clique em Luzia para relembrar a conversa ou sair. Escape abre a pausa existente.

## Organização editável

Cena: `TCC/Assets/Scenes/Currais.unity`.

- `01 Rua - separacao antes do portao`: fundo de Fortaleza, portão, cercas, carroça e outra fila.
- `02 Currais - entrada e abrigo`: fundo da caatinga, chão separado, portão, cercas, mesa, panela, laço, esteira, lona, Luzia e policiais provisórios.
- `03 Marcas de encenacao - arraste para ajustar`: posições de câmera, separação, saída da carroça, coleta, revista e abrigo. Ajustar estas marcas altera a encenação sem editar coordenadas no código.
- `04 Sequencia da entrada`: referências, falas, textos de exame e duração de cada movimento/fade no Inspector.
- `CanvasDialogoCurrais - modelo Estrada`: cópia do Canvas existente, com os mesmos componentes e estilo. O painel de cutscene existente é utilizado para o fade. Pausa e diário usam o modelo existente.

Artes novas: `TCC/Assets/Imagens/Currais/EntradaObjetos.png`. É um atlas com oito sprites independentes no Sprite Editor; cada sprite tem seu próprio GameObject. Nenhuma imagem é carregada ou desenhada por código. Personagens, fundos e policiais reutilizam arquivos existentes. Os guardas Demônio e Porco não foram usados.

O pai usa uma região individual da ilustração já existente de Fortaleza. Os recortes anteriores dessa imagem foram preservados. O chão também usa uma região da arte existente de Fortaleza; os pixels originais não foram alterados.

`TCC/Assets/Animaçoes/Currais` contém um Animator Override para a protagonista no passado. Reutiliza o controlador de movimento existente e os frames já presentes de Zefinha, sem alterar as animações da protagonista nas outras cenas.

## Progresso e compatibilidade

`curraisEtapaEntrada`: 0 separação, 1 espera pela coleta, 2 revista, 3 chegada ao abrigo, 4 setor livre. Os checkpoints ficam em estados seguros; sair durante uma fala retoma o último estado salvo e pode repetir a conversa interrompida. Não grava o meio de uma animação.

`curraisItensRetidos` guarda IDs do catálogo existente, inclusive quantidades repetidas. A lista é capturada antes de limpar o inventário. Os indicadores de retenção e do laço impedem duplicação ao continuar. `NormalizeData` inicializa a lista para saves antigos. Nenhum save antigo é migrado ou apagado manualmente.

Abrir a nova cena diretamente usa os prefabs existentes de progresso e inventário somente se esses sistemas ainda não existirem. Esse teste isolado não marca o tutorial ou a Seca como concluídos e não sobrescreve um save válido só para permitir testar.

## Validação realizada

- Sintaxe de todos os scripts do projeto analisada com parser C#.
- Todas as referências locais da nova cena conferidas; campos do controlador preenchidos e com tipos corretos.
- Trinta falas conferidas com o documento aprovado, incluindo as duas alternativas da promessa.
- Recortes antigos das imagens e seus GUIDs preservados.
- Nova cena registrada na lista de build do projeto Unity em `TCC`. A configuração da pasta externa ao projeto não foi alterada.
- Cena `Estrada` e suas referências restauradas preservadas; a alteração no seu script se limita à transição para `Currais`.
- Composição visual conferida por prévias estáticas dos sprites.

Não foi possível executar Unity/Play Mode neste ambiente. Verificar no Editor: chegada pela Splash/Seca/Estrada, três escolhas noturnas, caminhos porta/janela (com e sem curativo), clique no laço, revista, encerramento dos diálogos, limites do setor, pausa e Continuar em cada checkpoint. Testar a cena diretamente é complementar e não substitui o percurso completo.

## Arte

Atlas produzido para esta etapa por geração de imagem, com transparência. Especificação: oito objetos em grade 4×2 — portão, cerca, mesa, carroça, esteira, abrigo, panela e laço — em pixel art simples, tons terrosos e sem personagens ou texto. Revisão aplicada: “Clean this exact atlas. Keep all eight objects in their same positions, same 4 column x 2 row arrangement, same proportions and colors. Remove all red/yellow edge specks and stray pixels and translucent halos. Every object must have crisp clean blocky pixel-art edges. Simplify each object to flat brown/tan/ochre areas and only one shadow shade, no mottled texture, no antialiasing. Real fully transparent alpha everywhere outside objects, including gaps and gate rails. Keep background transparent, not black. No new objects no text. Preserve cell layout.”
