# Correções de cenas e apresentação

Base: commit `7842112` (Fotos Animaçoes). A fonte e os ajustes da autora fora do escopo foram preservados.

## Vídeo

SplashManager usa VideoRenderMode.APIOnly e vincula RawImage à textura decodificada quando chega o primeiro quadro. Não passa mais pela RenderTexture de formato editável, e a imagem fica desabilitada até haver um quadro válido. O arquivo original do vídeo permanece intacto. Uma cópia `Animaçoes/Cutscene/CurraisAberturaCompativel.mp4`, em H.264 Constrained Baseline com áudio AAC original, é usada na abertura para compatibilidade. Os mesmos 666 quadros, a resolução 854 × 480, a velocidade e os créditos foram preservados. A textura antiga permanece como recurso não utilizado. O quadro do vídeo continua editável na Hierarchy de Splash.

## Sol de salvamento

O novo recurso `Assets/Imagens/Salvamento/SolXilogravura.png` tem transparência própria e substitui o antigo tanto em GameProgress quanto no aviso anterior à data. Não usa o shader de extração de fundo antigo. Gira a 90 graus por segundo; o sucesso fica visível por 3 segundos no canto superior direito. O texto de falha continua disponível.

Foi identificado override de escala zero em SaveCanvas na cena CaminhoParaCasa. Esse override foi corrigido e o controlador também normaliza a escala do Canvas ao apresentar o aviso, para tolerar instâncias antigas. SaveCanvas usa ordem 32760. O formato do save e a gravação de inventário, escolhas e posição não mudaram.

Edite o indicador em `GameProgress / SaveCanvas / SaveNotice / SolSalvamento`. Tamanho, posição e velocidade continuam configuráveis.

## Tutorial

O campinho estava com alpha 0,5529412. Voltou a 1, impedindo o céu de aparecer através dele. Os cinco céus e suas posições editadas pela autora foram mantidos atrás do cenário.

Foi adicionado o prefab de pausa existente, com um UniversalPauseManager da própria cena ligado aos painéis e botões do prefab. Som e porcentagem usam PauseSoundSettings já existente; os três botões de retorno dos subpainéis apontam para o controlador. ESC abre/fecha a pausa e o tutorial só prossegue após esse exercício.

## Quartinho

Os alvos HiddenPhoto deixam de abrir também o ExamineObject genérico no mesmo clique. A foto abre seu próprio painel, com imagem proporcional e sem bloquear a UI pelos textos/imagens decorativos.

A janela de fotos tem áreas separadas para imagem, legenda e botões. No exame genérico, overrides apenas desta cena removem a margem negativa e fazem o texto caber na caixa. Foram corrigidos acentos, capitalização e pontuação em mensagens como música, dá, vó, chapéu e não; o significado foi preservado.

## Diálogo com a mãe

A fala de Mari sobre o diário e a resposta da mãe são exibidas separadamente, com nome/retrato correspondentes. A fala avança com clique; as opções retornam ao fim da pequena sequência. Mari reutiliza seu próprio sprite da cena, pois o campo de retrato da casa estava nulo.

As posições dos botões não foram movidas: Diário ocupa o botão superior, Ler o seguinte, e Sair permanece embaixo. Progresso e condição para dormir são os mesmos.

## Objetos abertos

`Assets/Imagens/Interacoes/BauAberto.png` e `PortaQuartinhoAberta.png` foram produzidos com referência nos desenhos existentes, sem mudar colliders ou mecânicas de destravar. Os objetos abertos estão separados na Hierarchy e seguem os estados já salvos de chestOpened e quartinhoUnlocked.

- Casa_Manha: `Interactables / Porta do quartinho aberta - visual editavel`. O desenho fechado está integrado ao fundo; o novo sprite cobre somente a área da porta, com medidas derivadas do fundo original. Fica na frente da parede e atrás das personagens.
- Quartinho: `Bau aberto - visual editavel`, ao lado do objeto bau_0 na mesma estrutura do cenário. Largura e base acompanham o baú original; o desenho fechado é ocultado ao destravar. A imagem do baú dentro do painel de conteúdo também usa a versão aberta.

## Dormir

SleepingImage usa o sprite original `Sprites/Casa/mari dormindo.png`, adicionado em Fotos Animaçoes, com Preserve Aspect e o painel de fundo preto opaco. Os fades, o tempo de espera e a transição para Seca permanecem iguais.

## Recursos visuais

As três artes novas foram feitas com geração de imagem integrada. O sol foi solicitado como um símbolo branco simples de xilogravura, espiral aberta e raios equilibrados, transparente e legível em tamanho pequeno. Porta e baú foram solicitados como versões abertas dos respectivos desenhos de referência, preservando paleta, traço simples e proporções. Os recortes de importação excluem apenas margens transparentes e alinham os sprites ao cenário; as imagens originais do projeto não foram sobrescritas.

## Verificação

Verificados: sintaxe dos scripts, referências locais e dos prefabs de pausa, igualdade do trecho que grava o arquivo de save, estados dos visuais abertos, geometria do painel de fotos, referência do retrato de Mari e do sprite original de dormir. O vídeo original e a versão compatível também foram decodificados integralmente por FFmpeg sem erro; ambas têm 666 quadros. Nenhum alias YAML foi emitido.

Unity não está instalada neste ambiente. Compilação real, reprodução no Editor e teste do fluxo completo em Play Mode ainda precisam de confirmação no projeto atualizado.

Teste sugerido: Boot e vídeo; Novo Jogo e aviso do sol; tutorial com ESC e retorno de Som/Controles; concluir tutorial e observar salvamento; destravar porta e baú; examinar/ler fotos; retornar à mãe, escolher Diário e avançar ambas as falas; dormir; carregar um checkpoint e conferir os visuais abertos.
