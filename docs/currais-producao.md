# Currais — organização e produção (etapa 2)

Base: `c9f0d66278b2f3909a6424ed7d660bf67e2c365f`. Cena: `TCC/Assets/Scenes/Currais.unity`.

## Onde encontrar as coisas

Abra a cena Currais e expanda `02 Currais - entrada e abrigo`. Os grupos estão em ordem: 00 Ambiente, 01 Revista, 02 Abrigo, 03 Água, 04 Espera, 05 Registros, 06 Busca, 07 Posto/quarto/noite, 08 Manhã/consulta e 09 Passagens. Os grupos de fases mantêm a ativação controlada pelos scripts existentes.

Selecione a área e consulte **Area Currais** no Inspector: acontecimentos, artes existentes, variantes pendentes, camadas e referências para marcas/estados. São notas editáveis; o componente não executa ações, não cria limites e não governa a progressão.

As marcas estão em `03 Marcas de encenacao - arraste para ajustar`, agrupadas por momento. Edite a arte e as marcas correspondentes: mover um grupo de artes não desloca automaticamente marcas que pertencem ao grupo de encenação.

Família, protagonista, câmera e interfaces continuam compartilhadas. Pano do pai, balde e caderno permanecem ligados às pessoas que os carregam. O sepultamento permanece no Canvas; cartelas e pausa mantêm seus espaços.

Os 34 grupos novos usam posição zero, rotação neutra e escala um. As matrizes mundiais dos **355 Transforms originais** foram conferidas e preservadas. Não foram alterados tamanhos, posições ou proporções autoradas.

Grupos **Legado** guardam laços e limites antigos desativados. Não são tarefas novas e não devem ser reativados para jogar.

## Mapa das áreas e variantes

| Área | Caminho na Hierarchy | Acontecimento | Arte existente | Variantes previstas |
|---|---|---|---|---|
| 01 - Entrada e separação | `01 Rua - separacao antes do portao` | Chegada, escolta, separação de Mariquinha e ferimento do pai. | EntradaObjetos: portão, cercas e carroça; currais.png; família; guardas; Nair em quatro quadros. | Mão estendida da irmã, reação do pai ao golpe e roda em movimento: etapa 3. |
| 02 - Revista | `02 Currais - entrada e abrigo/01 Revista - mesa, fiscal e diário` | Entregar todos os pertences e recuperar o caderno permitido. | Mesa e portão de EntradaObjetos; Fiscal animado; diário já ligado à cena. | Mesa com entregas parciais/completas e gestos de conferência: etapa 4. |
| 03 - Primeiro abrigo | `02 Currais - entrada e abrigo/02 Abrigo - Luzia e cuidado com o pai` | Luzia acolhe; Zefinha leva o pano para acomodar o pai. | casa2.png; esteira; pano disponível; pano no ombro do pai. | Mãe ajeitando o pano, pai acomodado e gestos de entrega: etapa 5. |
| 04 - Distribuição de água | `02 Currais - entrada e abrigo/03 Água - distribuição e cabaça` | Recuperar cabaça, receber porção e decidir destinatário no abrigo. | Mesa; cabaça; cozinheira1 a 4 ligados à funcionária da água. | Gesto de servir, beber e leitura de recipiente cheio/vazio: etapa 5. |
| 05 - Criança que espera | `02 Currais - entrada e abrigo/04 Espera - criança e lata` | Conversa sobre a irmã, espera do tio e lata. | Criança e lata presentes; pose de espera na cena. | Reação ao chamado, fala de reencontro e identidade visual própria: etapa 6. |
| 06 - Nomes e canhoto | `02 Currais - entrada e abrigo/05 Registros - nomes, canhoto e lote` | Perguntar por Mariquinha, transportar lote e consultar encaminhamento. | Escrivão animado; mesa; Canhoto.png; paninho.png usado no lote. | Folhas antes/depois, escrita e lote entregue: etapa 6. |
| 07 - Raimundo | `02 Currais - entrada e abrigo/06 Busca - Raimundo, lavagem e pátio/01 Lavagem e Raimundo/06 Raimundo - relato e baldes` | Relato sobre irmã e mulher; reencontro na manhã; balde opcional. | funcionario currais1 a 4 em QuadrosAmbiente; baldes e pano da alça. | Carregar/apoiar peso, mão ferida e alça protegida: etapas 7 e 10. |
| 08 - Lavagem | `02 Currais - entrada e abrigo/06 Busca - Raimundo, lavagem e pátio/01 Lavagem e Raimundo/07 Lavagem - tanque e panos` | Entregar lote, separar dois grupos e recolher tira limpa. | lavadeira1 a 4; tanque e panos de BuscaObjetos; tira de paninho.png. | Lavagem/torção, panos manchados distintos, varal e gesto de autorização: etapa 7. |
| 09 - Corredor e lembranças | `02 Currais - entrada e abrigo/06 Busca - Raimundo, lavagem e pátio/02 Corredor e lembrancas` | Ronda sem retorno forçado; lenda escolhida modifica percepção. | Guarda; sacos; tanque; galho; reflexos; pessoa da cerca em rasgo npc1 a 4. | Reações da protagonista e retorno de Luzia; preservar caráter de lembrança: etapa 7. |
| 10 - Pátio de trabalho | `02 Currais - entrada e abrigo/06 Busca - Raimundo, lavagem e pátio/03 Patio de trabalho/09 Trabalho - cargas e fiscalização` | Apoio com água e pano; pai trabalha ferido e Bento perde força. | saco.png; carroça; Fiscal; ponto de água e apoio do pano. | Pai com carga, Bento exausto e tarefas com diferença visual: etapa 8. |
| 11 - Socorro e despedida | `02 Currais - entrada e abrigo/06 Busca - Raimundo, lavagem e pátio/03 Patio de trabalho/10 Socorro - Bento, panos e cabaça` | Queda de Bento, pano e água, morte, pertences e retirada. | bento1 a 4: ciclo existente; corpo coberto, sangue e pano em BuscaObjetos. | Pose real de queda/ferimento, corpo sem respiração e sequência de cobertura: etapa 8. |
| 12 - Sepultamento | `CanvasDialogoCurrais - modelo Estrada/Sepultamento - plano estático editável` | Plano de despedida; corpo deixa a imagem; cruzes e memória de Bento. | Plano estático no Canvas; terra, cruzes e corpo coberto. | Familiares/companhia e poses de luto específicas: etapa 9. |
| 13 - Posto | `02 Currais - entrada e abrigo/07 Posto, quarto e noite/01 Posto - escolha E3` | Ajudar Raimundo ou copiar papel; sair adia a decisão. | Mesa, folha, baldes e vigia editáveis. | Peso apoiado, gesto de cópia e papel depois da decisão: etapa 9. |
| 14 - Quarto sem Mariquinha | `02 Currais - entrada e abrigo/07 Posto, quarto e noite/02 Quarto sem Mariquinha` | Ouvir lembrança, investigar porta, conversar e emprestar cabaça. | casa1.png; porta fechada; interior com duas esteiras/adultos; trabalhadora em vizinha dona zélia1 a 4. | Folha aberta com pivô, dois doentes próprios, busca/devolução da cabaça: etapa 9. |
| 15 - Abrigo à noite | `02 Currais - entrada e abrigo/07 Posto, quarto e noite/03 Abrigo - espaco vazio e noite` | Notícias, lembranças da família, acolhimento da mãe e descanso. | Lugar vazio da irmã; cabaça; caderno ligado à Mari; marcas noturnas. | Família deitada, mãe acolhendo e respiração em poses próprias: etapa 10. |
| 16 - Última consulta | `02 Currais - entrada e abrigo/08 Manhã e última consulta/01 Ultima consulta - documentos e testemunha` | Pela manhã: confirmar destino ou identidade, com testemunha. | Papel; Escrivão; testemunha com ciclo lavadeira1 a 4. | Gestos de consulta, entrada/saída da testemunha e confirmação sem duplicar personagem: etapa 11. |
| 17 - Portão e recusa | `02 Currais - entrada e abrigo/01 Revista - mesa, fiscal e diário/Portao fechado do setor` | Pai pede para procurar a filha; Cabo nega; movimento livre após conversa. | Mesmo portão do campo e marcas do pedido de saída. | Gesto de bloqueio, reação do pai e retorno da família: etapa 11. |
| 18 - Passagens do tempo | `02 Currais - entrada e abrigo/09 Passagens do tempo` | Páginas, espera, saída sem a irmã e retorno ao presente. | MemoriaObjetos: página e mão; três imagens atuais de objetos; quarto sobreposto. | Composições completas de dias/meses/anos: etapa 11. As imagens atuais são objetos de apoio. |
| Ambiente contínuo | `00 Ambiente contínuo - panoramas` | Panoramas distribuídos por posição; não é barreira de movimento. | Trechos existentes e sprites originais. | Conferir transições de dia/noite em Play Mode; acabamento nas etapas das áreas. |

## Artes próprias encontradas

Foram identificadas 37 imagens em `Assets/Imagens/Currais`. A tabela confirma arquivos e referências; a aprovação visual depende de conferência no Unity.

| Arquivo | Referenciado na cena |
|---|---|
| `BuscaObjetos.png` | Sim |
| `Canhoto.png` | Sim |
| `EntradaObjetos.png` | Sim |
| `MemoriaObjetos.png` | Sim |
| `bento1.png` | Sim |
| `bento2.png` | Sim |
| `bento3.png` | Sim |
| `bento4.png` | Sim |
| `casa1.png` | Sim |
| `casa2.png` | Sim |
| `cozinheira1.png` | Sim |
| `cozinheira2.png` | Sim |
| `cozinheira3.png` | Sim |
| `cozinheira4.png` | Sim |
| `curraidentros.png` | Sim |
| `currais.png` | Sim |
| `funcionario currais1.png` | Sim |
| `funcionario currais2.png` | Sim |
| `funcionario currais3.png` | Sim |
| `funcionario currais4.png` | Sim |
| `lavadeira1.png` | Sim |
| `lavadeira2.png` | Sim |
| `lavadeira3.png` | Sim |
| `lavadeira4.png` | Sim |
| `nair pereira1.png` | Sim |
| `nair pereira2.png` | Sim |
| `nair pereira3.png` | Sim |
| `nair pereira4.png` | Sim |
| `rasgo npc1.png` | Sim |
| `rasgo npc2.png` | Sim |
| `rasgo npc3.png` | Sim |
| `rasgo npc4.png` | Sim |
| `saco.png` | Sim |
| `vizinha dona zélia1.png` | Sim |
| `vizinha dona zélia2.png` | Sim |
| `vizinha dona zélia3.png` | Sim |
| `vizinha dona zélia4.png` | Sim |

### Ciclos existentes

| Personagem/elemento | Arquivos já ligados | O que ainda não está resolvido pelo ciclo |
|---|---|---|
| Bento | bento1–4.png | Queda, ferimento e corpo sem respiração precisam de poses próprias. |
| Funcionária da água | cozinheira1–4.png | Servir e receber o recipiente. |
| Raimundo | funcionario currais1–4.png | Carregar/apoiar peso e reação à alça. |
| Lavadeira e testemunha | lavadeira1–4.png | Lavar/torcer e continuidade sem duplicação visível. |
| Mulher com Mariquinha | nair pereira1–4.png | Gestos da separação; não criar encontro posterior com Nair. |
| Pessoa atrás da cerca | rasgo npc1–4.png | Reação/percepção conforme a lenda. |
| Trabalhadora do quarto | vizinha dona zélia1–4.png | Atendimento, empréstimo e devolução da cabaça. |
| Sacos | saco.png | Gestos de carregamento pertencem aos personagens. |
| Abrigo/quarto | casa2.png e casa1.png | Encaixe e estado aberto da porta precisam revisão na etapa 9. |
| Família e guardas | Animaçoes/Currais/PersonagensAnimados | Preservar proporções e acrescentar poses específicas por acontecimento. |

A cena contém **162 SpriteRenderers** e **15 componentes QuadrosAmbiente ativos** no arquivo. O inventário `currais-elementos.csv` lista área, caminho, IDs, posição local, camada, sprite serializado e artes do ciclo de cada renderer.

**Prévia versus ciclo:** QuadrosAmbiente prepara sprites normalizados em tempo de execução. Alguns SpriteRenderers ainda guardam a imagem anterior no arquivo. O CSV separa essa imagem dos arquivos efetivamente usados no ciclo. Esta etapa não troca esses sprites à força nem altera seus pivôs/escala.

**Passagens do tempo:** as três imagens atuais são objetos de apoio — baldes, sacos e esteira —, e não três composições completas da família ao longo dos anos. As composições completas estão previstas na etapa 11.

## Posições e marcas relacionadas

| Momento | Marcas editáveis |
|---|---|
| 01 - Entrada e separação | `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/cameraRua`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/irmaSeparada`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/irmaSaida`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/caboBarreira`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/carrocaSaida`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/pontoColeta`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/deslocamentoEscolta`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/guardaSeparacao`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/guardaGolpe`; `03 Marcas de encenacao - arraste para ajustar/01 Entrada e separação/paiReacao` |
| 02 - Revista | `03 Marcas de encenacao - arraste para ajustar/02 Revista/mariEntrada`; `03 Marcas de encenacao - arraste para ajustar/02 Revista/paiEntrada`; `03 Marcas de encenacao - arraste para ajustar/02 Revista/maeEntrada`; `03 Marcas de encenacao - arraste para ajustar/02 Revista/baleiaEntrada`; `03 Marcas de encenacao - arraste para ajustar/02 Revista/cameraRevista` |
| 03 - Primeiro abrigo | `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/cameraCampo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/mariAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/paiAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/maeAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/baleiaAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/luziaRepouso` |
| 04 - Distribuição de água | `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/cameraCampo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/mariAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/paiAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/maeAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/baleiaAbrigo`; `03 Marcas de encenacao - arraste para ajustar/03 Abrigo e água/luziaRepouso` |
| 05 - Criança que espera | `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/meninoFimPasso`; `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/panelaNoChao`; `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/panelaRecolhida` |
| 06 - Nomes e canhoto | `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/meninoFimPasso`; `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/panelaNoChao`; `03 Marcas de encenacao - arraste para ajustar/04 Criança e registros/panelaRecolhida` |
| 07 - Raimundo | `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - posto`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - perto do abrigo`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - outro setor`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Raimundo - perto do posto na manha`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Raimundo - outro setor na manha` |
| 09 - Corredor e lembranças | `03 Marcas de encenacao - arraste para ajustar/05 Corredor e ronda/rondaInicio`; `03 Marcas de encenacao - arraste para ajustar/05 Corredor e ronda/rondaFim`; `03 Marcas de encenacao - arraste para ajustar/05 Corredor e ronda/retornoRonda`; `03 Marcas de encenacao - arraste para ajustar/05 Corredor e ronda/fimPassagem` |
| 10 - Pátio de trabalho | `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Pai - posto de trabalho`; `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Bento - antes da queda`; `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Bento - caido junto a roda` |
| 11 - Socorro e despedida | `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Pai - posto de trabalho`; `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Bento - antes da queda`; `03 Marcas de encenacao - arraste para ajustar/06 Pátio e Bento/Bento - caido junto a roda` |
| 13 - Posto | `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - posto`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - outro setor` |
| 15 - Abrigo à noite | `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - posto`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - perto do abrigo`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Raimundo - outro setor`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Mari - noite sem a irma`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Mae - perto de Mari na noite`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Pai - repouso durante a noite`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Luzia - noite no abrigo`; `03 Marcas de encenacao - arraste para ajustar/07 Posto e noite/Baleia - gesto da refeicao` |
| 16 - Última consulta | `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Mae - limite do acompanhamento`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Raimundo - perto do posto na manha`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Pai - diante do escrivao`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Mari - diante do escrivao`; `03 Marcas de encenacao - arraste para ajustar/08 Manhã e consulta/Raimundo - outro setor na manha` |
| 17 - Portão e recusa | `03 Marcas de encenacao - arraste para ajustar/09 Portão e recusa/Portao - passagem vigiada`; `03 Marcas de encenacao - arraste para ajustar/09 Portão e recusa/Portao - fechado apos recusa`; `03 Marcas de encenacao - arraste para ajustar/09 Portão e recusa/Mari - pedido de saida`; `03 Marcas de encenacao - arraste para ajustar/09 Portão e recusa/Pai - pedido de saida`; `03 Marcas de encenacao - arraste para ajustar/09 Portão e recusa/Mae - pedido de saida` |
| 18 - Passagens do tempo | `03 Marcas de encenacao - arraste para ajustar/10 Páginas e câmera/Mari - leitura das paginas`; `03 Marcas de encenacao - arraste para ajustar/10 Páginas e câmera/Camera - abrigo e paginas` |

Use as referências do Inspector para localizar as marcas. Não usar coordenadas fixas de outro cenário nem aplicar reset de Transform nos personagens.

## Camadas de desenho

A Sorting Layer permanece a existente, Default (ID 0). A separação é feita pelo campo **Order in Layer**.

| Elemento | Ordem |
|---|---|
| Céu / panoramas / chão | -120 / -100 / -90 a -80 |
| Pessoa atrás da cerca | **-45**, corrigida de -35 |
| Cercas / portões ao fundo | -40 / -30 |
| Abrigo / esteiras | -20 / -10 |
| Pessoas de apoio | 17 a 28, conforme a composição |
| Mari | 30 |
| Panos entregues na lavagem | **27**, acima das pilhas de origem em 26 |
| Objetos carregados e detalhes de frente | 32 a 36 |
| Página / documento / mão / quarto da memória | 60 / 62 / 64 / 80 |

Porta fechada e Cabo do quarto preservam suas ordens 30 e 31. O encaixe da folha aberta será produzido na etapa 9. Os Canvas de diálogo, sepultamento, pausa e cartelas mantêm seus valores existentes.

## Produção pendente por etapa

| Etapa | Variantes e implementação |
|---|---|
| 3 — Entrada | Irmã chama/estende mão; pai protesta e se fere; mãe ampara; guarda interrompe; carroça sai. |
| 4 — Revista | Mesa com entregas parciais e totais; Fiscal confere; diário permitido/retirado; pertences persistentes. |
| 5 — Abrigo/água | Pai acomodado; mãe ajeita pano; entrega; servir/beber; cabaça cheia/vazia; acompanhante e respostas. |
| 6 — Criança/registros | Criança espera/reage; reencontros; Escrivão escreve/confere; papéis antes/depois; lote e canhoto. |
| 7 — Raimundo/lavagem | Peso e mão; lavar/torcer; panos distintos e varal; lembrança e retorno de Luzia. |
| 8 — Trabalho/Bento | Pai trabalha ferido; Bento perde força/cai; atendimento; corpo coberto; pertences e retirada. |
| 9 — Sepultamento/posto/quarto | Luto; família se representada; peso/cópia; porta fechada/aberta; doentes próprios; cabaça emprestada/devolvida. |
| 10 — Família/noite/manhã | Notícias condicionais; família deitada; mãe acolhe; lugar vazio; manhã; alça protegida. |
| 11 — Consulta/encerramento | Testemunha entra/sai; portão recusado; reação da família; composições de dias/meses/anos. |
| 12 — Revisão | Todos os NPCs respondem; poses acompanham fatos; duas linhas por balão; som, pausa e salvamento. |

Um arquivo importado não significa ação pronta. Quatro imagens de Bento não substituem sua queda; um portão em atlas não garante um estado aberto; uma imagem de balde não representa toda a espera da família.

## Verificação e limites

Conferidos: IDs únicos; todos os objetos/componentes anteriores presentes; reciprocidade de pais e filhos; ausência de ciclos; raízes válidas; matrizes mundiais preservadas; estados ativos e ancestrais controlados pela história preservados; referências do controlador e componentes anteriores de física, câmera, animação e UI intactos.

Somente três ordens de SpriteRenderer foram corrigidas. Os scripts da história e os dados de save não foram alterados nesta etapa. O novo AreaCurrais só contém campos de organização.

**Pendente:** importar/compilar no Unity, conferir visualmente os grupos e as três mudanças de desenho, percorrer as fases em Play Mode e testar Continuar. Este ambiente não executa o Unity.

## Etapas 3 e 4 — correção para preservar a arte da autora

- Removidas as duas folhas geradas para personagens, os filhos que as sobrepunham aos corpos originais e o componente exclusivo dessas poses.
- Mariquinha, Mainha, Painho, guarda e Fiscal conservam seus SpriteRenderers, Transforms, Animators e controladores originais. Nenhuma animação da autora é substituída.
- Deslocamento da mãe, saída da fila e aproximação do Fiscal usam as animações existentes. A lesão do pai continua registrada no progresso e nas falas.
- As variantes anteriores de Mariquinha ferida/com curativo e suas condições da Estrada são preservadas.
- Gestos específicos de chamar, amparar, proteger o braço, bloquear e receber objetos ficam pendentes de arte aprovada; não são simulados com novos desenhos dos personagens.
- Rodas independentes, marcas editáveis, diálogos, entrega de TODOS os itens, 40 lugares da mesa, diário e retomada dos saves são preservados.
- A conferência final também aproxima o Fiscal da mesa, usando sua caminhada existente.
- O inventário visual passa de 214 para 207 SpriteRenderers: sete sobreposições/detalhes adicionados nas etapas 3 e 4 foram retirados.

Verificações disponíveis: estrutura da cena, referências removidas e preservação dos componentes originais. Pendente: compilação, importação e Play Mode no Unity.

## Etapa 5 — primeiro abrigo e água

- Mãe, pai, Luzia e funcionária respondem de acordo com o progresso. Falta de pano/recipiente recebe fala indireta; não há comandos de teclado.
- O pai ganha uma área de interação própria, sem colisão física. Suas conversas não deslocam a câmera.
- A mãe aceita o pano pelo diálogo ou arraste, caminha até uma marca editável ao lado do pai e aplica o pano existente. O resultado é salvo antes da fala de confirmação e reconstituído em Continuar.
- Luzia explica a busca da cabaça e da água. A distribuição usa a cabaça existente e devolve somente uma unidade; os demais pertences continuam na mesa. Saves sem a unidade registrada recuperam o recipiente visível na distribuição.
- A entrega confere novamente a existência do item depois da escolha. A escolha de destinatário e as páginas do diário existentes são preservadas.
- Três cabaças ficam na Hierarchy: com Mari, mãe e Luzia. Reutilizam o mesmo ícone aprovado. Cheia usa o destaque azulado do inventário; vazia volta à cor normal. A inclinação durante o consumo move apenas o recipiente.
- Mãe: marca “Mãe - acomodar pai no primeiro abrigo”, no grupo de marcas do abrigo. Pano no ombro mantém seu objeto original sob Painho.
- As proporções, animações e controladores dos personagens não são substituídos. Gestos corporais de servir/beber/amparar que não possuem arte aprovada permanecem pendentes; não foram produzidas novas imagens de personagens.

Conferência necessária no Unity: coleta/cancelamento do pano; entrega por conversa e arraste; resposta do pai; Luzia antes/depois; retorno da cabaça; enchimento; ambos os destinatários; pausa durante deslocamento/inclinação; Continue após acomodação e escolha; saída para a busca seguinte.

## Etapa 6 — criança, mesa dos nomes e canhoto

- A criança passa a usar sua própria arte e o AnimatorOverrideController Crianca_471_50_0, com os quatro quadros de idle existentes. A imagem anterior de Mariquinha não é usada nela.
- A criança fica na posição definida na cena. Não desliza até outra marca ao conversar. Sua reação usa a orientação do sprite e o idle da autora.
- A lata ganha uma área de exame própria e falas. Conversas de retorno dependem de ter contado o nome da irmã e de já ter procurado a mesa. O NPC continua respondendo durante as etapas seguintes enquanto estiver visível.
- A mesa responde quando a família ainda precisa cuidar da água. Escrivão, criança e acompanhante mantêm suas identidades; ACOMPANHANTE é resolvido para Mãe ou Luzia.
- Duas folhas reutilizam Canhoto.png como representação dos papéis em espera e após o atendimento. A folha é aproximada de uma marca editável durante a consulta e volta à origem mesmo em cancelamento/desativação.
- O Escrivão conserva seu idle original. Não foi criada uma animação corporal de escrita.
- O lote antes colocado junto da criança passa para a mesa dos nomes. Uma cópia visual acompanha Mari entre a coleta e a entrega; o lote entregue existente continua junto da lavadeira.
- O canhoto ganha interação pelo próprio papel, além da panela. A área de clique é centralizada no desenho.
- Perguntar pelo encaminhamento abre a confirmação de coleta existente; cancelar mantém o papel disponível. A opção Voltar não confirma destino nem avança a história. Seguir com os panos mantém a alternativa sem canhoto.
- A confirmação do destino e a página do diário só são liberadas após confirmar a coleta. Os estados visuais usam os flags já salvos; nenhum save antigo é obrigado a repetir a criança ou as perguntas.
- Folhas, área de exame, marca de conferência, lata e lote carregado ficam na Hierarchy. Nenhuma imagem de personagem, sprite sheet ou animação da autora foi gerada ou substituída.

Pendente no Unity: compilação; proporções e encaixes; criança antes/depois de informar nome e consultar mesa; lata; papéis com ambos os acompanhantes; coleta/cancelamento/Voltar do canhoto; passagem com e sem canhoto; lote visível durante transporte; pausa durante consulta; Continuar nos estados intermediários. Gestos corporais específicos de escrita ou chamada aguardam animações aprovadas.

## Etapa 7 — Raimundo, lavagem e lembranças

- Raimundo, lavadeira, Luzia e guarda respondem durante a primeira busca. Raimundo não fica sem conversa depois da primeira despedida; repetir a despedida não faz a história voltar uma etapa. O nome da mulher continua dependendo da investigação posterior.
- Raimundo e lavadeira usam já no arquivo da cena os primeiros quadros de seus ciclos próprios, antes inicializados apenas no Play Mode. Os quatro quadros e as proporções originais são preservados. Retirada a tonalização azul que alterava as cores de Raimundo.
- A lavadeira aceita o lote por conversa ou arraste. Sem o lote, responde com uma indicação narrativa. Luzia fala conforme o momento da busca.
- Separar os panos pode ser feito pelo diálogo de cada pilha ou pelo arraste do item correspondente. Item errado não é consumido. Os dois grupos têm cores distintas, confirmação falada e estado de entrega salvo separadamente.
- O movimento de mergulho/torção usa somente o objeto pano e os ciclos já existentes da lavadeira. A animação respeita a pausa, revalida o item antes de consumir e restaura posição/escala/rotação ao terminar ou cancelar.
- Os panos entregues ficam alinhados às respectivas pilhas. A peça de uso posta a secar reutiliza PanoMolhado de BuscaObjetos, sem nova imagem. O varal aparece após a entrega dos panos de uso e permanece no cenário.
- O retalho limpo só aparece depois dos dois grupos separados. Cancelar a coleta o deixa disponível. Confirmar recolhe e salva junto com o avanço para a passagem; não fica um checkpoint intermediário sem acesso ao próximo evento. Saves anteriores com retalho recolhido na etapa 2 avançam para a passagem.
- A lembrança começa perto do tanque pela marca editável “Lembranças - início junto do tanque”. A família não precisa chegar ao outro lado da ronda para ver imagens que ficariam atrás da câmera.
- As três lendas preservam suas imagens e falas: galho/pessoa da cerca, assobio do guarda ou reflexos da irmã e de Zefinha. As imagens aparecem gradualmente e são ocultadas mesmo se a sequência for interrompida. Sem lenda escolhida, o retorno não afirma que Zefinha ouviu uma voz inexistente.
- Luzia retorna pela marca “Luzia - retorno ao lado de Mari”, usando sua caminhada existente. O checkpoint da passagem só é concluído após o retorno e as falas, permitindo retomar uma sequência interrompida.
- A ronda preserva a altura do guarda e vira pela direção real do deslocamento. Ela não empurra Mari, não a devolve aos sacos e não cria limites de caminhada.
- Falas longas de Raimundo e da lavadeira foram fragmentadas sem retirar palavras; as opções continuam indiretas, sem instruções de teclado.
- Hierarchy: pano em lavagem e marca de mergulho no grupo da lavagem; varal no mesmo grupo; marca de início junto do tanque nas marcas do corredor; marca de retorno de Luzia como filha de Mari, para acompanhar a posição da protagonista.
- Nenhuma imagem de personagem ou arquivo de animação da autora foi gerado/substituído. Baldes já presentes na arte de Raimundo foram preservados. Um desenho específico de sua mão ferida continua pendente de arte aprovada; a combinação e o empréstimo do balde da manhã permanecem na etapa correspondente.

Verificação no Unity ainda necessária: entrega por diálogo/arraste; item errado; duas ordens de separação; cancelamento/retomada; retalho antes/depois; pausa durante lavagem, aparição e caminhada; reencontros com NPCs; cada lenda e teste sem lenda; Continue antes/depois da lembrança; caminho livre até o pátio. Sem compilação ou Play Mode disponíveis neste ambiente.

## Etapa 8 — pátio de trabalho, Bento e socorro

- Pai, Bento e Fiscal ganham interação no pátio. Luzia passa a responder sobre as tarefas pendentes e abre o socorro quando Bento precisa de ajuda. O pai inicia a apresentação das tarefas; Bento fala da falta de ar e o Fiscal responde sobre a carga. A conversa comum não muda posição ou zoom da câmera.
- Água e pano podem ser apresentados por item selecionado/arraste ou pelas opções de conversa. Pontos já atendidos respondem sem consumir outra unidade. A cabaça cheia e o pano aparecem com Mari enquanto ainda estão com ela.
- O pano de apoio do ombro do pai volta a aparecer no pátio, usando o objeto existente e respeitando a acomodação anterior. Não foi inventada animação de levantar carga com o braço ferido.
- Terminar a segunda tarefa não inicia o acontecimento longe de Bento. A família é orientada a voltar ao pai, e a sequência só continua na proximidade dele.
- Posição de trabalho de Bento: marca em x 196. Socorro: marca em x 193,4, junto da área de interação, corpo coberto e mancha existentes. A marca de socorro deixa de ter rotação de 90 graus.
- A transição usa tela preta e reposicionamento pela marca; o idle de Bento não é girado nem desliza para fingir queda. Seu primeiro quadro próprio já fica no SpriteRenderer da cena, sem tonalização que altere suas cores.
- O filho editável “Bento - pose de socorro (atribuir arte aprovada)” permite receber um sprite aprovado no Inspector. O espaço está sem sprite: os quatro quadros existentes são em pé e não representam uma queda. Enquanto não houver essa arte, permanece a referência original, parada no socorro. Nenhuma imagem de personagem foi gerada.
- O atendimento exige os dois itens reais: pano limpo e cabaça cheia. Cada entrega é salva individualmente. Falta de item recebe resposta condicional. Com os dois já entregues em um checkpoint interrompido, voltar a conversar retoma a conclusão do atendimento.
- Pano usado no atendimento e cabaça deixada ficam visíveis. A cabaça cheia usa destaque azul; depois fica vazia. O corpo coberto existente aparece gradualmente, sem criar um novo desenho de Bento.
- A morte continua sendo parte da história, não uma punição por ordem de entrega. O diário e as falas originais foram mantidos.
- Depois, o jogador recolhe a cabaça e leva outro pano para os pertences. O pano entregue ganha representação própria. Cancelar a coleta não avança flags; a retomada restaura os estados.
- A despedida fica disponível quando pano dos pertences e cabaça estiverem resolvidos. O jogador escolhe “Acompanhar Bento”; o plano de sepultamento existente é mantido para a etapa seguinte.
- Hierarchy: três estados de Bento/socorro no grupo de atendimento; pose opcional como filha de Bento; cabaça carregada como filha de Mari; área de interação própria no pai; marcas de trabalho/socorro existentes reposicionadas para coincidirem com o acontecimento.
- Falas longas desta sequência foram fragmentadas sem retirar palavras. Artes, ciclos e proporções dos personagens continuam os da autora.

Pendências de arte: pose de Bento caído/em atendimento e gesto específico do pai com carga, caso a autora deseje essas representações. Os ciclos em pé existentes foram preservados.
Pendente no Unity: compilar; conferir encaixes/áreas; água e pano nas duas ordens; diálogo/arraste/item errado; voltar ao pai; pausa na transição/cobertura; atendimento nas duas ordens; Continue após cada entrega e antes da notícia; cancelamento de coleta; ambos os últimos itens da despedida em ordens diferentes; entrada no sepultamento. Nenhum teste em Play Mode foi executado neste ambiente.


## Etapa 9 — sepultamento, posto e quarto

- O sepultamento exige a morte registrada, o pano dos pertences entregue e a cabaça recolhida. Reentradas simultâneas não repetem a sequência.
- O Canvas existente recebe mãe, pai, Zefinha e Luzia usando os sprites de seus próprios personagens. As imagens acompanham os quadros de idle em execução, sem novas texturas nem substituição das animações da autora.
- Bento coberto é baixado gradualmente e desaparece; uma cruz com seu nome ocupa o plano. As posições, tamanhos e cores estão no Canvas da cena. Poses específicas de luto ainda dependem de arte aprovada.
- As escolhas de ajudar Raimundo ou copiar o registro continuam exclusivas. Sair adia a escolha. A cópia anima a folha existente e mostra uma anotação com Mari; rever a mesa não concede outra escolha nem outra pista.
- A ajuda deixa os baldes no chão e mantém Raimundo junto do posto nesta parte. A marca nova evita o salto direto para uma coordenada do abrigo durante a escolha. Seu ciclo original é preservado; um gesto corporal específico sem carga continua pendente.
- Luzia indica o quarto de atendimento e depois o retorno à família por falas indiretas, sem instruções de teclado.
- A porta tem dobradiça e folha aberta serializadas na Hierarchy. A abertura gira a madeira existente em torno da lateral, respeita a pausa e fica salva. A cobertura da casa passa ao grupo exterior, sem mudar sua posição mundial, para continuar visível com a porta fechada.
- Trabalhadora, Cabo e as duas pessoas do atendimento podem ser revisitados. Não se repete a voz da irmã depois de a porta já ter sido aberta. Os desenhos provisórios dos dois doentes foram mantidos como referências editáveis; artes próprias ainda estão pendentes. Nenhuma nova imagem de personagem foi gerada.
- O empréstimo retira temporariamente uma cabaça vazia do inventário e registra o estado antes dos eventos de inventário. A trabalhadora serve água com o objeto existente, devolve a mesma unidade e registra a ajuda. Sair não empresta; repetir conversa depois não duplica o item.
- Um checkpoint interrompido durante o empréstimo devolve a cabaça ao continuar e conclui a visita já apresentada. Saves antigos que já passaram do quarto recebem apenas o estado de porta aberta, sem nova obrigação.
- Campos novos no progresso: curraisQuartoAberto e curraisCabacaEmprestadaQuarto. Os valores padrão false preservam a leitura de saves antigos.
- Objetos novos no mundo: folha aberta, anotação copiada e cabaça do empréstimo. Marcas: dobradiça, posição da folha em leitura e apoio de Raimundo. No Canvas: quatro imagens de companhia, cruz e nome de Bento. O CSV continua contando SpriteRenderers; os elementos do Canvas estão descritos aqui.
- Falas longas foram fragmentadas sem retirar palavras. A câmera, seus limites e os arquivos de animação não foram alterados.

Pendente no Unity: compilar e importar; conferir enquadramento do plano; pausa e interrupção durante descida/porta/empréstimo; escolha de cada ramo e Sair; revisitas; Continue antes/depois da abertura e durante o empréstimo; confirmar uma única cabaça devolvida; passagem para a conversa noturna. Poses de luto, Raimundo apoiando a carga e artes próprias dos doentes aguardam aprovação/produção da autora.


## Etapa 10 — família, noite e amanhecer

- A esteira abre uma conversa da mãe com três opções: ficar perto, conversar mais e sair. Descansar só inicia depois das notícias, sem obrigar a esgotar todos os assuntos opcionais.
- Promessas, trabalho/Bento, refeição, notícias e caderno permanecem acessíveis. Relembrar a refeição não teleporta Baleia. As conversas comuns não alteram a câmera.
- A escolha noturna e a página do diário foram preservadas. A sequência usa as marcas existentes; interrupções removem o estado transitório e a iluminação noturna é restaurada ao desativar o controlador.
- Hierarchy: cada personagem (Zefinha, mãe, pai e Luzia) recebe um SpriteRenderer filho de repouso OPCIONAL, inicialmente vazio/inativo. Atribuir uma pose própria substitui apenas a visibilidade do sprite em pé enquanto a noite concluída aguarda o amanhecer. Slots vazios preservam os ciclos originais. Não foi gerada arte nem girado idle para simular sono; poses de repouso/acolhimento e respiração própria continuam pendentes da autora.
- Caderno, lugar vazio, laço/roupa e cabaça existentes continuam editáveis na cena. A cabaça e os estados noturnos saem ao amanhecer. O checkpoint da manhã antecede sua nova conversa familiar; a última consulta não foi antecipada.
- A mãe reconhece o lugar vazio; o pai menciona Raimundo por uma fala indireta. As falas novas são editáveis no componente EntradaCurrais.
- O empréstimo do balde marca a coleta antes do evento de inventário e adiciona exatamente uma unidade. A devolução exige esse empréstimo e o balde protegido, registra a conclusão antes de retirar uma unidade e não reposiciona toda a família. Uma tentativa incompleta recebe uma fala, em vez de silêncio. Perguntar pela mulher continua independente de ajudar o funcionário.

Pendente no Unity: compilar, conferir enquadramentos e slots opcionais com/sem arte; Sair na esteira; cada escolha noturna; pausa/interrupção; Continue durante noite e manhã; iluminação; balde nas ordens perguntar/ajudar, item errado, combinação, devolução repetida e salvamento durante eventos de inventário. Não houve compilação nem Play Mode neste ambiente.

## Etapa 11 — última consulta, portão e encerramento

- A mesa abre três opções: perguntar por Mariquinha, mostrar o canhoto e sair. Apresentar o documento mostra uma cópia visual do sprite existente junto de Mari, sem tirar o item do inventário nem criar informação. Sem o papel, há resposta e a consulta continua possível.
- Abrir a conversa não move Zefinha ou o pai nem muda a câmera. A consulta conserva a escolha exclusiva de confirmar destino ou identidade, inclusive respostas para informação já conhecida. Sair deixa a escolha pendente; revisitar uma consulta concluída não concede nova escolha. A relação permanece na mesa.
- A testemunha e o canhoto visual são recolhidos ao terminar/interromper a apresentação. Nenhuma arte de personagem ou animação foi gerada/substituída. A representação existente da testemunha precisa de revisão visual pela autora.
- Pai, mãe e Luzia respondem conforme o momento. Luzia deixa de mandar voltar ao quarto depois de já haver amanhecido. As falas dão pistas indiretas sobre mesa, portão e abrigo; não há comandos de teclado nem tutorial sobreposto.
- A tentativa de saída só ocorre depois da consulta. Marca de câmera editável na Hierarchy enquadra família e portão durante a sequência. O portão existente fecha com o tempo pausável já usado no jogo; a recusa fica salva. Interromper antes da conclusão restaura posição dos três, câmera e portão aberto. Os limites de movimento continuam desativados.
- O fim da recusa aponta o retorno ao abrigo por uma fala da mãe. Conversar com os pais abre o encerramento sem teletransportar os atores. A conversa é dividida em menus de até três opções; é possível sair sem concluir o ato. Lembrar um assunto nunca confirma pista ausente.
- Guardar os nomes mantém a página do diário e libera as passagens seguintes. Laço aparece no plano de memória apenas se tiver sido guardado. Sair da apresentação restaura posição, câmera, zoom e animadores que estavam ativos; uma cena de epílogo ausente não deixa o menu sob tela preta.
- Hierarchy: marca “Saída - enquadramento da família e portão” nas marcas da saída; “Canhoto apresentado - não consumido na consulta” como filho de Mari. Imagens de passagem, quarto, mão, documento e laço continuam os objetos existentes. Poses específicas de testemunho, mãe acolhendo e gesto do Cabo continuam dependentes das artes da autora.

Pendente no Unity: compilação/importação; proporção do canhoto e enquadramento do portão em várias resoluções; consulta com/sem papel e cada opção/Sair; identidades/destinos já conhecidos e ausentes; interrupção durante testemunho e portão; pause; Continue antes/depois da recusa; revisitas; encerramento em cada combinação de pistas; laço guardado/ausente; retomar cada página; epílogo habilitado/ausente. Não houve compilação nem Play Mode neste ambiente.

## Etapa 12 — revisão de integração

Correções: coleta sem segunda inserção (incluindo o balde duplicado da etapa 10); devolução de exatamente uma cabaça da mesa sem esvaziar outra; limpeza de walk e Rigidbody ao interromper uma sequência; estados de entrega definidos antes dos eventos de inventário; revisita da criança respeitando a fase posterior à lavagem. Nenhuma mudança nas artes, animações, posições ou limites da cena.

Verificações estáticas e roteiro completo de validação manual em [currais-revisao.md](currais-revisao.md). A compilação, Play Mode, build e revisão visual continuam pendentes; não considerar o ato totalmente validado apenas pela revisão de código.

## Revisão solicitada — família, ferimento, arraste e presença

- Pai, mãe e Baleia começam perto de Zefinha e a acompanham da revista até a primeira sombra. A marca pontoParadaFamilia, as distâncias e a proximidade são editáveis na Hierarchy. A família decide parar por conversa; as posições horizontais ficam salvas.
- O golpe e o ferimento do pai permanecem. Depois de parar, a mãe explica que precisa de um pano para apoiar o braço. O jogador coleta o pano e o arrasta até ela ou até o pai; não existe botão “Entregar pano”. Conversar não consome o item nem muda a câmera.
- As conversas tratam dos assuntos e necessidades. Entregas da revista, pano do pai, cabaças, lote da lavagem, peças das pilhas, água/pano do pátio, socorro/pertences de Bento, empréstimo no quarto e devolução do balde protegido passam por alvos explícitos de arraste. Item incompatível não é consumido. A destinatária da água é o alvo escolhido pelo jogador, sem menu posterior para escolher quem recebe.
- Os NPCs do campo foram separados dos grupos que escondiam áreas por etapa. Dezesseis personagens têm presença permanente, ciclos originais e assuntos neutros antes de sua participação principal. Progresso libera novos assuntos e ações, em vez de fazer o personagem aparecer. As posições mundiais foram preservadas no reparentamento; a ronda começa da posição existente.
- A testemunha e o trabalhador não são mais ligados/desligados durante as consultas. Bento mantém seu objeto ativo, mas sua representação muda para o atendimento/corpo coberto conforme a história; a morte não é revertida. A separação e saída da irmã continuam sendo acontecimentos narrativos.
- Save antigo já na exploração mantém a primeira parada concluída. Novos saves registram a parada separadamente dos cuidados com o pai. Nenhum sprite ou ciclo novo foi gerado.

Verificado estaticamente: sintaxe C#, grafo da cena, referências, cobertura do CSV, ausência das opções de entrega removidas, contrato de arraste e aplicação do patch sobre a base. Pendente no Unity: compilar, caminhar/pausar/Continuar, usar cada alvo com item correto e errado, repetir entregas, testar todos os assuntos e confirmar as representações de Bento/irmã. Não houve Play Mode nem build neste ambiente.
