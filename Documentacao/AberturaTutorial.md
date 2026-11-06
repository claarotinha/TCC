# Abertura, avisos e tutorial

## Abertura pela Boot

O fluxo permanece `_Boot → Intro → Splash → MenuPrincipal`. A cena que reproduz o vídeo é `Splash`. O clipe provisório foi substituído pelo `Animaçoes/Cutscene/currais cutscene.mov` do commit Animação Currais (22,199 s, 854 × 480, H.264/AAC). O arquivo original não foi alterado.

Na Hierarchy de `Splash`:

- `Abertura Currais / 00 Video Currais - ajustar tamanho`: o RectTransform controla o enquadramento da tela do vídeo.
- `Abertura Currais / 01 Creditos - editar textos e posicoes`: seis grupos separados, com os textos em seus filhos. Os nomes são provisórios.
- No componente SplashManager, a lista `Creditos` define início/fim de cada bloco como fração da duração do vídeo (0 a 1); `Tempo Fade Creditos` controla a entrada e a saída. Para pré-visualizar um crédito fora do Play Mode, coloque temporariamente seu CanvasGroup Alpha em 1. O controlador reinicia os alphas ao executar.
- A fonte continua Xilosa PT. O material `TextMesh Pro/Fonts/Xilosa_PT Creditos.mat` acrescenta contorno preto apenas aos créditos, para manter sua leitura sobre os trechos claros do vídeo.

A preferência anterior de pular com ESC após assistir à abertura inteira foi mantida. Um erro de decodificação é registrado e encaminha ao menu para não deixar a inicialização travada.

## Menu

Continuar fica visível na mesma posição e com as cores normais. Sem checkpoint válido, seu clique não inicia nada. Com checkpoint válido, a restauração e a validação de inventário/cena continuam iguais. O salvamento existente não foi modificado.

## Aviso antes da data

Em `AvisoInicial / Canvas / Orientacoes antes da data`, os dois textos e o sol de demonstração são objetos separados. O sol reutiliza o sprite e o material já existentes, gira sem depender do Time.timeScale e não dispara nenhum salvamento. As orientações entram e saem com fade; depois a data original é digitada e o jogo segue para Escola.

Tempo de leitura, fade e velocidade do sol podem ser alterados no componente AvisoInicial. A data é lida do próprio objeto TMP da cena, mantendo-a editável pela Hierarchy.

## Caminho para casa

`Main Camera / Ceu do tutorial - ajustar na Hierarchy` usa a imagem enviada em `Imagens/background/tutorialcaminho`, acompanha a câmera e fica atrás do cenário. Não tem collider e não muda os limites da caminhada.

Após A/D e Shift, o tutorial solicita ESC. A investigação é liberada depois de abrir e fechar a pausa. As condições de cursor, investigação e coleta da bola incluem esse novo passo, evitando interações antecipadas. As demais etapas continuam na sequência anterior.

## Primeira combinação real

No prefab `UI/InventoryCanvas`, o objeto `Ajuda da combinacao - editar aqui` reutiliza a aparência do painel de combinação existente. Sua mensagem aparece ao abrir o inventário com dois ingredientes de alguma receita disponível. Ensina a arrastar um item sobre o outro e confirmar em Sim. Fica oculta durante a confirmação e deixa de aparecer depois da primeira combinação bem-sucedida na sessão. Ao iniciar outra sessão de GameProgress, o ensino pode aparecer novamente.

Não foram criados itens, receitas ou puzzles novos. Assim, o ensino acontece na combinação real (por exemplo, no estilingue), e não na devolução da bola do tutorial.

## Verificação

Foram verificadas a sintaxe dos scripts alterados, as referências locais das cenas e prefabs, as referências dos novos recursos, os intervalos de créditos, a sequência da pausa e a preservação dos componentes preexistentes. Nenhum alias YAML foi emitido.

Unity não está disponível neste ambiente: compilação no Editor, reprodução de vídeo/áudio e teste do fluxo completo em Play Mode ainda precisam ser feitos no projeto atualizado.

Teste sugerido: iniciar pela Boot; assistir aos seis créditos; chegar ao menu sem save e clicar Continuar; iniciar Novo Jogo; ler orientações antes da data; concluir A/D, Shift e ESC; investigar e devolver a bola; testar a primeira combinação válida. Com um save existente, conferir também o carregamento por Continuar.
