# Navalha Barbearia

Aplicacao ASP.NET Core MVC (.NET 8) para gestao de barbearia, com foco em separacao de responsabilidades, regras de negocio por perfil e fluxo de agendamento.

## Visao Geral

O sistema atende tres perfis com acesso ao login:
- Administrador
- Funcionario (barbeiro)
- Cliente

O agendamento no fluxo publico continua disponivel sem autenticacao.

O projeto segue arquitetura em camadas:
- Controllers: orquestracao das requisicoes HTTP.
- Services: regras de negocio e validacoes de permissao.
- Repositories: persistencia em memoria (listas estaticas).

## Requisitos Nao Funcionais (RNF)

### RNF de arquitetura e qualidade
- RNF-01: Arquitetura em camadas com separacao de responsabilidades (Controller, Service e Repository).
- RNF-02: Uso de injecao de dependencias para servicos e repositorios.
- RNF-03: Padrao Repository com interfaces para desacoplar regra de negocio da persistencia.
- RNF-04: Tipagem forte com enums de dominio para reduzir erro por strings livres (tipo de acesso, status e procedimento).

### RNF de seguranca e acesso
- RNF-05: Sessao HTTP para contexto do usuario logado, com timeout de 4 horas.
- RNF-06: Protecao Anti-CSRF em acoes POST por meio de ValidateAntiForgeryToken.
- RNF-07: Redirecionamento HTTPS e HSTS no pipeline da aplicacao.
- RNF-08: Controle de autorizacao por perfil em nivel de regra de negocio (guard clauses e validacoes no service/controller).

### RNF de dados e operacao
- RNF-09: Persistencia em memoria para desenvolvimento e demonstracao.
- RNF-10: Estado inicial consistente para novos agendamentos (status pendente por padrao).
- RNF-11: Feedback de validacao via ModelState para formularios invalidos.

### Limitacoes nao funcionais conhecidas
- RNF-12: Nao ha banco de dados relacional; os dados sao perdidos ao reiniciar a aplicacao.
- RNF-13: Nao ha autenticacao robusta (Identity/JWT); login e sessao sao simulados em memoria.
- RNF-14: Senhas de usuarios de desenvolvimento estao em texto simples no repositorio em memoria.

## Regras de Negocio (RN)

### RN de controle de acesso
- RN-01: Somente Administrador pode visualizar todos os agendamentos.
- RN-02: Funcionario visualiza e altera apenas agendamentos vinculados ao seu proprio Id.
- RN-03: Agendamento publico (sem login) permite clientes agendarem rapidamente; quando autenticado, o cliente acessa apenas sua pagina de historico de agendamentos, em modo somente leitura com visualizacao do status.
- RN-04: Qualquer usuario pode visualizar o catalogo de procedimentos; somente Administrador cria, edita e exclui procedimentos do catalogo. O cadastro usa `Id` e `Nome` no lugar de um enum de procedimento.
- RN-29: Cliente faz login com CPF para consultar apenas o proprio historico de agendamentos em uma unica pagina de leitura, incluindo o status de cada agendamento.

### RN de clientes
- RN-05: Todo cliente cadastrado recebe TipoAcesso = Cliente.
- RN-06: Todo cliente fica vinculado ao barbeiro que o cadastrou.
- RN-07: Cliente novo nasce ativo, com data de cadastro preenchida.
- RN-08: Funcionario gerencia apenas os clientes que cadastrou.
- RN-09: Arquivamento de cliente e logica de ativacao/desativacao por perfil administrador.

### RN de agendamento
- RN-10: Agendamento exige barbeiro valido, CPF de cliente valido e procedimento.
- RN-11: Status inicial padrao do agendamento quando feito pelo Barbeiro é Agendado.
- RN-12: No fluxo publico da home(Agendamento rapido), ao confirmar agendamento, status vira AguardandoConfirmacaoBarbeiro.
- RN-13: Status de agendamento so pode ser alterado por perfis autorizados, respeitando o escopo do registro.
- RN-14: Funcionario pode excluir apenas os proprios agendamentos.
- RN-15: Data do agendamento nao pode ser anterior a data atual; validacao centralizada no service impede criacao de agendamentos retroativos.
- RN-24: Quando faltar 30 minutos ou menos para o horario do agendamento, o status transiciona automaticamente de Agendado para AguardandoConfirmacaoCliente; validacao ocorre sempre que agendamentos sao retornados do service.

### RN de procedimentos e precificacao
- RN-16: Catalogo de procedimentos usa PrecoBase como referencia.
- RN-17: PrecoPorBarbeiro usa fallback para PrecoBase quando nao houver customizacao.
- RN-18: Atualizacao de descricao e preco base do catalogo propaga para todos os barbeiros que realizam o procedimento.
- RN-19: Exclusao de procedimento remove o item do catalogo e da lista de procedimentos dos barbeiros.
- RN-20: Funcionario pode customizar apenas o proprio PrecoPorBarbeiro pelo painel do funcionario, sem alterar o cadastro do procedimento.
- RN-25: A relacao Barbeiro x Procedimento esta em transicao para N:N explicito com entidade de vinculo (`BarbeiroProcedimentoModel`), mantendo sincronizacao com a lista legada de `Procedimentos` para compatibilidade das telas atuais.
- RN-26: Leitura de preco por barbeiro prioriza a relacao N:N (`RelacoesProcedimentos` ativas) e usa fallback para a estrutura legada quando necessario.
- RN-27: A listagem de procedimentos do funcionario exibe `PrecoPorBarbeiro` e o status `Ativo` de cada vinculo.
- RN-28: O campo `Ativo` da listagem de procedimentos do funcionario e controlado por toggle-switch, ativando ou inativando o vinculo com o procedimento em tempo real.

### RN de experiencia no fluxo publico
- RN-21: Busca publica por CPF retorna apenas clientes ativos para auto preenchimento para Agendamento Rápido.
- RN-22: O Agendamento Rápido na Home calcula e exibe preco dinamico por barbeiro e procedimento selecionado.
- RN-23: Resumo de agendamento exibe um recibo com informações do usuario, do agendamento e o historico recente do cliente.
- RN-30: O fluxo de detalhes/resumo de agendamento foi unificado na view `Views/Agendamentos/ResumoAgendamento.cshtml`; o acesso por `Agendamentos/Details` e pela listagem aponta para o mesmo resumo.
- RN-31: Em qualquer tela de criacao de agendamento, a lista de procedimentos deve mostrar apenas os procedimentos ativos vinculados ao barbeiro selecionado; validacao da regra tambem ocorre no service.

## Stakeholders (Partes Interessadas)

### Administrador
- Objetivo: gerir operacao e padronizacao do negocio.
- Interesses: visao completa da agenda, gestao de barbeiros, catalogo de procedimentos e clientes.
- Acoes principais: CRUD de barbeiros, CRUD de procedimentos, visao total de agendamentos, gestao de clientes arquivados.

### Funcionario (Barbeiro)
- Objetivo: operar atendimento diario e agenda propria.
- Interesses: manter agenda atualizada, cadastrar clientes e ajustar precificacao propria.
- Acoes principais: visualizar/editar agendamentos proprios, cadastrar clientes vinculados, ajustar preco por procedimento.

### Cliente
- Objetivo: agendar servico.
- Interesses: facilidade para agendar e visibilidade do historico de atendimentos.
- Acoes principais: agendar via home publica, fazer login e consultar historico proprio de agendamentos.

### Dono/Gestor da Barbearia (Seu Zé Lucas Alexandrino)
- Objetivo: ganho de organizacao operacional e controle do atendimento.
- Interesses: produtividade da equipe, clareza de agenda e governanca de precos/catalogo.
- Acoes principais: acompanhar resultado por meio do perfil administrador.

## Casos de Uso

### UC-01 - Autenticar usuario
- Ator: Administrador, Funcionario, Cliente.
- Fluxo principal: informar email e senha, validar credenciais, gravar contexto em sessao e redirecionar para home do perfil.

### UC-02 - Realizar logout
- Ator: qualquer usuario autenticado.
- Fluxo principal: limpar contexto da sessao e retornar para home publica.

### UC-03 - Agendamento rápido pela home publica
- Ator: publico/cliente.
- Fluxo principal:
1. Informar CPF e auto preencher dados de cliente ativo.
2. Selecionar barbeiro e procedimento.
3. Informar data e hora.
4. Visualizar resumo e confirmar.
5. Gerar agendamento com status AguardandoConfirmacaoBarbeiro.

### UC-04 - Visualizar agendamentos por perfil
- Ator: Administrador, Funcionario, Cliente.
- Fluxo principal: listar agendamentos filtrados conforme permissao do perfil.

### UC-05 - Atualizar status de agendamento
- Ator: Administrador, Funcionario.
- Fluxo principal: alterar status respeitando as regras de autorizacao e escopo do agendamento.

### UC-06 - Gerenciar agendamentos (CRUD interno)
- Ator: Administrador e Funcionario.
- Fluxo principal: criar, editar e excluir agendamentos, com restricoes para o funcionario atuar apenas no proprio contexto.

### UC-07 - Gerenciar clientes
- Ator: Administrador e Funcionario.
- Fluxo principal: cadastrar/editar cliente, vinculando ao barbeiro responsavel.

### UC-08 - Arquivar e reativar clientes
- Ator: Administrador.
- Fluxo principal: desativar cliente na operacao e reativar pela tela de arquivados.

### UC-09 - Gerenciar procedimentos do catalogo
- Ator: Administrador.
- Fluxo principal: criar, editar e excluir procedimento com propagacao para dados relacionados. A consulta do catalogo e liberada para qualquer usuario autenticado ou nao autenticado.

### UC-10 - Personalizar preco por barbeiro
- Ator: Funcionario.
- Fluxo principal: ajustar o PrecoPorBarbeiro para procedimento vinculado ao proprio perfil, sem alterar descricao nem PrecoBase.

### UC-11 - Gerenciar barbeiros
- Ator: Administrador.
- Fluxo principal: criar, editar, listar e excluir barbeiros.

## User Stories (resumo)

- Como administrador, quero visualizar toda a agenda para acompanhar a operacao completa.
- Como administrador, quero manter o catalogo de procedimentos para padronizar servicos e precos base.
- Como funcionario, quero ver apenas meus agendamentos para focar no meu atendimento.
- Como funcionario, quero personalizar meu preco por procedimento para refletir meu posicionamento.
- Como funcionario, quero cadastrar clientes vinculados a mim para organizar minha carteira.
- Como cliente, quero agendar rapidamente pela home para reduzir atrito no atendimento.
- Como cliente, quero consultar meus agendamentos para acompanhar meu historico.

## Como Executar Localmente

1. Restaurar e compilar:

~~~bash
dotnet build -v minimal
~~~

2. Executar:

~~~bash
dotnet run
~~~

3. Abrir no navegador pela URL exibida no terminal.

## Credenciais de Desenvolvimento

- admin@navalha.com / 123456 (Administrador)
- funcionario@navalha.com / 123456 (Funcionario)
- 123.456.789-00 / 123456 (Cliente)

**Nota:** O perfil Cliente possui acesso apenas a tela de historico de agendamentos, sem acoes de gerenciamento; a tela exibe o status de cada agendamento em modo somente leitura.

## Observacao

Este projeto prioriza didatica e clareza de regra de negocio para estudo e evolucao incremental. Para ambiente produtivo, recomenda-se evoluir autenticacao, persistencia, auditoria e observabilidade.

## Arquivo de Index na Raiz

Foi adicionado o arquivo `index.html` na raiz do projeto com uma pagina inicial simples de referencia.

PAQ -Padrão Ana Julia de Qualidade :D
Revisado 13/04 15:20

