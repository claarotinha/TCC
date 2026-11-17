# Currais — revisão da etapa 12

Base: `9dd252f8a263822dfe8a9dbb1ca487086759578c`.

## Correções desta revisão

- Coleta: `CollectPrompt.Confirm` já insere uma unidade no inventário. Os consumidores de `ExaminarOuColetar` passam apenas a registrar a coleta. Foi removida a segunda inserção incondicional do balde de Raimundo, introduzida na etapa 10, e as inserções redundantes dos outros seis coletáveis.
- Devolução da cabaça da revista: retorna exatamente uma unidade da mesa, inclusive se já houver outra igual no inventário. A operação não esvazia outra cabaça cheia. O estado de devolução é registrado antes do evento de inventário, e repetir a conversa não devolve novamente.
- Movimento de sequência: o encerramento do enumerador limpa `IsWalking` e a velocidade do Rigidbody mesmo quando interrompido. Isso não altera os ciclos, coordenadas, velocidades ou limites de movimento.
- Entregas: acomodação do pai, separação da lavagem, pano do pátio e atendimento/pertences de Bento registram o recebimento antes do evento de remoção do item.
- Criança: depois da panela/lavagem, a revisita mantém a fala posterior e não reabre o menu antigo, mesmo se a apresentação inicial tiver sido pulada. As falas anteriores continuam intactas.

## Verificações executadas

- Parsing de sintaxe dos 27 arquivos C# disponíveis neste recorte do projeto. Isso não equivale a compilação no Unity.
- Referências locais da cena, IDs únicos, associação de componentes, reciprocidade entre pais/filhos e ausência de ciclos na Hierarchy.
- Inventário de 227 SpriteRenderers corresponde ao CSV de elementos.
- Campos ItemData do controlador constam no catálogo de saves; IDs e referências do catálogo não se repetem.
- Epílogo está habilitado nas configurações de build do repositório.
- Cena, componentes de câmera, física e ciclos dos personagens preservados; nenhuma nova textura, pose ou animação foi criada.
- Patch aplicado a cópias dos arquivos exatos do commit base; arquivos resultantes conferidos por hash.

## Validação pendente no Unity

Todos os casos abaixo estão **pendentes**. Não houve compilação, Play Mode ou build neste ambiente. Executar pela entrada normal do jogo e repetir os pontos de retomada com Continuar.

| Parte | Casos a executar | Resultado esperado |
| --- | --- | --- |
| Entrada | Estrada → Currais; abrir Currais diretamente; pausa na separação | Referências prontas, separação ordenada e movimento liberado ao terminar |
| Revista | Inventário vazio, um item, vários itens e duas unidades iguais; conferir antes de terminar | Todos os pertences entregues; nenhuma conclusão antecipada; caderno recuperável |
| Primeiro abrigo | Coletar/cancelar pano; entregar; repetir conversa; Continue antes/depois | Uma coleta por confirmação; pai acomodado uma vez; próxima parte disponível |
| Cabaça da mesa | Sem outra cabaça; com vazia; com cheia; repetir devolução | Soma uma vazia na primeira devolução; preserva recipientes que já estavam com a família |
| Água | Conversa antes de apresentar recipiente; item errado; mãe e Luzia em sessões separadas | Orientação narrativa; uma entrega; escolhas e falas correspondentes |
| Criança e registro | Visitar antes/depois do registro; pular encontro inicial; revisitar depois da lavagem | Falas da fase correta; sem reabrir assunto inicial como se nada tivesse acontecido |
| Lote e panela | Cancelar coleta; transportar lote; escolher canhoto ou seguir sem ele | Lote entregue antes da continuação; canhoto concedido uma vez; pista só quando obtida |
| Lavagem | Separar panos nas duas ordens; item errado; coletar/cancelar tira; Continue após cada entrega | Uma unidade por peça; duas entregas exigidas; tira e passagem acessíveis |
| Passagem | Cada lenda e sessão sem lenda; pausa e Continue durante lembrança | Retomada sem travar movimento; ronda não empurra nem reinstala limites |
| Pátio e Bento | Água/pano nas duas ordens; voltar ao pai; socorro nas duas ordens; cancelar últimas coletas | Acontecimento perto do pai; itens não consumidos duas vezes; despedida só com pertences resolvidos |
| Sepultamento e posto | Pausa/interrupção; cada escolha do posto e Sair; revisitas | Plano e retorno limpos; escolha exclusiva; nenhuma pista extra por repetição |
| Quarto | Abrir porta; Continue; emprestar cabaça; interromper/retomar; repetir | Porta aberta persistente; uma unidade emprestada e devolvida; fala da irmã não repetida |
| Família e noite | Todos os assuntos; Sair na esteira; as duas escolhas; interromper caminhada; Continue | Câmera estável em conversa; idle limpo; escolha noturna preservada; manhã disponível |
| Balde | Pedir/coletar/cancelar; combinar; devolver; repetir; perguntar por Nair sem ajudar | Uma unidade coletada; uma protegida após combinação; uma consumida na devolução; informação independente da ajuda |
| Última consulta | Papel selecionado, apresentação pelo menu, sem papel, Sair; pista já conhecida | Canhoto permanece; escolha exclusiva; testemunha recolhida depois; câmera não salta |
| Portão e encerramento | Pausa/interrupção no fechamento; Continue; retorno ao abrigo; sair dos menus | Portão e família recuperáveis; recusa salva; não encerrar antes do retorno; pistas ausentes não inventadas |
| Memória e epílogo | Retomar cada página; rever; laço presente/ausente; inventário/pausa; transição | Páginas e pistas preservadas; nenhum clique atravessa UI; presente carregado e controles recuperados |
| Tela e edição | 16:9, 16:10 e 4:3; editar marcas e proporções na Hierarchy | Legendas legíveis; portão/família visíveis; recortes e posições aprovados pela autora |

## Arte ainda pendente de aprovação da autora

Os espaços opcionais não substituem a arte que falta. Conferir poses de repouso/acolhimento da família, luto, Bento em atendimento, pai com carga, Raimundo apoiando o peso, gesto do Cabo e representação própria dos doentes/testemunha. Os desenhos e ciclos existentes permanecem preservados. A aprovação visual e os testes acima são necessários antes de considerar o ato inteiramente validado.
