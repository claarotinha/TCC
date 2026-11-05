# Currais — etapa 6: as páginas seguintes e revisão

Conclui os passos 21–24 do plano até antes do epílogo, implementando a cena 21 do roteiro. O capítulo termina com a preparação visual da volta ao presente. Não há carta, conversa na escola, reencontro com Mariquinha, confirmação de sua morte ou definição de um final.

## O que foi feito

| Passo | Conteúdo |
|---|---|
| 21 — Passagem do tempo | Três páginas: “Dias depois”, “Meses depois” e “Mais tarde”, com os textos exatos do roteiro. A posição da câmera e as animações da família ficam congeladas durante a leitura. |
| 22 — Lembrança preservada | A folha, a mão e o laço remendado ou documento são objetos independentes. O laço aparece quando foi guardado; o outro caminho mostra o canhoto/cópia. A mão cobre parte da página antes da reflexão final. |
| 23 — Encerramento do passado | A reflexão de Mari usa a fala do roteiro. Uma sobreposição mostra a imagem existente do quarto atual e o chamado textual “Maria Gabriely...”. Aparece “Fim do Ato 2”, sem carregar uma cena jogável do presente ou iniciar o epílogo. |
| 24 — Revisão e continuidade | Foram conferidos scripts, referências, cenas da build, caminhos das pistas e checkpoints. Ao terminar, o Canvas existente oferece rever páginas, consultar as pistas, voltar ao abrigo e voltar ao menu. A execução integral ainda precisa ser verificada no Unity. |

## Como jogar e testar

1. Conclua a conversa de retorno ao abrigo da etapa 5.
2. Aproxime Mari da esteira, à direita da família, e clique ou pressione E. Entre os pais, a interação continua permitindo consultar as informações; a esteira inicia as páginas.
3. Cada texto fica por até oito segundos. Após dois segundos e meio, um clique fora dos botões pode antecipar o avanço. Ambos os tempos são editáveis no Inspector.
4. Escape pausa normalmente. Abrir o inventário suspende a contagem da leitura; feche-o para continuar. Abrir o diário pelo menu de pausa também interrompe o tempo.
5. Ao terminar, escolha rever as páginas, revisar as pistas, voltar ao abrigo ou voltar ao menu. No abrigo, o movimento é liberado; as escolhas concluídas não são reabertas.

A imagem do quarto é somente a preparação da transição. Não são executados os gatilhos da Casa_Manha, da Escola ou do epílogo. A mãe do presente é identificada no texto para não ser confundida com Zulema.

## Progresso e salvamento

`curraisMemoriaIniciada` registra o início. `curraisPaginaMemoria` guarda quantas das três passagens foram concluídas. `ato2Concluido` registra o encerramento deste capítulo, sem escolher um desfecho.

Há checkpoint ao iniciar, após cada página e ao concluir. Interromper uma página permite retomá-la; passagens já concluídas são puladas na retomada. Se a interrupção acontecer depois da terceira página, a reflexão e o corte final ainda são apresentados. Depois do encerramento, Continuar abre as opções de revisão.

Rever as páginas não zera o índice salvo, não altera escolhas e não concede itens. Os dois dados — destino e identidade — permanecem independentes. Água, comida, lenda, promessa, laço/canhoto, ajuda/cópia e E4 são preservados. O inventário não recebe cópias adicionais dos objetos ilustrados.

O checkpoint permanece em Currais, já incluída na build. As quatro anotações novas no diário correspondem às três passagens e à reflexão final. As entradas anteriores foram mantidas.

## Organização e recursos

A própria cena Currais foi ampliada. O grupo `07 As paginas seguintes - etapa 6` contém a folha, a mão, a lembrança e a sobreposição do quarto. O grupo começa desativado e é exibido durante a sequência. As marcas de Mari e da câmera ficam no grupo de encenação, ajustáveis na Hierarchy.

A imagem nova está em `Assets/Imagens/Currais/MemoriaObjetos.png`: dois recortes independentes, página sem texto e mão. O laço, o remendo e o documento reutilizam os atlas atuais. O quarto usa `Imagens/background/casamainha/quarto mari (1).png`, sem modificar esse arquivo ou sua importação. As imagens não estão embutidas em C# e nenhum elemento visual é instanciado pelo código desta etapa.

`EntradaCurraisMemoria.cs` é mais uma parte da classe parcial EntradaCurrais; não é outro componente para anexar. Foram reutilizados o Canvas, os quatro botões, o fade, o inventário, o diário, o menu de pausa e os checkpoints.

As animações são desativadas temporariamente e têm seus estados originais restaurados ao voltar ao abrigo ou sair da cena. A câmera só é fixada durante o plano de leitura. Não há alteração global de volume, Time.timeScale ou material compartilhado. A saída para o menu usa UniversalPauseManager, já existente.

## Conferência realizada

- Sintaxe dos scripts e referências locais da cena, incluindo os campos novos e as áreas reutilizadas.
- Metadados dos sprites e recortes dentro dos limites da imagem.
- Correspondência das três leituras e da reflexão com a cena 21.
- Preservação dos campos, falas e ligações anteriores do controlador, do Canvas e da pausa.
- Parent/child recíprocos na Hierarchy e ausência de aliases YAML nas referências gravadas.
- IDs do diário sem duplicações; checkpoint apontando para Currais; nenhuma chamada que carregue Casa_Manha/Escola/epílogo.
- Caminhos do laço/documento e das oito combinações E2/E3/E4, sem apagar confirmações anteriores.
- Existência das 13 cenas configuradas na build e resolução de 172 referências de componentes C# próprios nessas cenas.
- Prévias estáticas da página com laço/documento e da imagem do presente inspecionadas.

Esses são exames estáticos, não testes de Play Mode nem compilação pelo Unity. Não há Unity disponível neste ambiente. Nenhuma outra cena ou configuração da build foi modificada.

## Percurso de regressão no Editor

Começar por Início/Splash e percorrer o tutorial, Seca, Estrada e Currais. Confirmar também entrada direta em cada capítulo suportado e Continuar de saves anteriores. Conferir água, inventário, combinações, movimento, paradas, diálogos e transições que já estavam validados.

Em Currais, testar água à mãe/Luzia, laço/canhoto, ajuda/cópia e as duas opções da última consulta. Confirmar lugar e nome conhecidos na revisão. Verificar as três lendas, os ramos da comida e da promessa sem alterações.

Na etapa 6, testar início por clique e E, pausa e inventário em cada página, avanço mínimo por clique e avanço automático, sair ao menu e continuar em cada checkpoint, rever páginas depois de concluir, revisar pistas, voltar ao abrigo e voltar ao menu. Confirmar que o quarto é uma imagem de transição, que não surgem diálogos do presente e que todas as opções permitem sair da apresentação.

## Arte criada

Foi usada a ferramenta integrada de geração de imagens para uma folha transparente com dois objetos separados: papel creme vazio e mão pequena de pele marrom com manga clara. Direção: pixel art simples, pixels grandes, poucas cores terrosas, sem texto, fundo, brilho ou detalhes excessivos. Uma revisão pediu simplificação das sombras e preservação das silhuetas e transparência. O resultado foi copiado para a pasta de imagens do projeto; somente o importador faz os dois recortes.
