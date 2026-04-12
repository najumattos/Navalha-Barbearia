# Navalha Barbearia

Aplicacao ASP.NET Core MVC organizada com foco em SOLID, separacao de responsabilidades, clean code e uma apresentacao minimalista.

## O que foi ajustado agora

- Normalizacao do genero para o dominio de cliente: `GeneroEnum` pertence a `ClienteModel`.
- Views principais atualizadas para respeitar a modelagem normalizada e o visual simples.
- Layout compartilhado e CSS global refinados para uma aparencia mais limpa e consistente.
- Comentarios adicionados em pontos chave para explicar intencao, responsabilidade e boas praticas para uma desenvolvedora junior.
- Paginas institucionais e de erro simplificadas para nao competir com as telas principais.

## Atualizacao recente (Agendamento e home)

- Adicionado o dominio de agendamento com model, enum e camadas separadas:
  - [Models/AgendamentoModel.cs](Models/AgendamentoModel.cs)
  - [Enums/StatusAgendamentoEnum.cs](Enums/StatusAgendamentoEnum.cs)
  - [Enums/GeneroEnum.cs](Enums/GeneroEnum.cs)
  - [Repositories/Interfaces/IAgendamentoRepository.cs](Repositories/Interfaces/IAgendamentoRepository.cs)
  - [Repositories/AgendamentoRepository.cs](Repositories/AgendamentoRepository.cs)
  - [Services/Interfaces/IAgendamentoService.cs](Services/Interfaces/IAgendamentoService.cs)
  - [Services/AgendamentoService.cs](Services/AgendamentoService.cs)
  - [Controllers/AgendamentoController.cs](Controllers/AgendamentoController.cs)
- O campo de barbeiro no agendamento passou a ser uma relacao direta com `BarbeiroModel`, reduzindo texto solto e melhorando a integridade do dado.
- O cliente nao visualiza mais o status inicial na tela de agendamento.
- O campo de preco da Home agora e preenchido automaticamente pelo `PrecoPorBarbeiro` de acordo com barbeiro + procedimento selecionados.
- O funcionario pode visualizar e alterar apenas os agendamentos vinculados ao proprio id.
- O administrador pode visualizar a agenda completa de todos os barbeiros.
- Os campos de cliente foram normalizados: `NomeCliente`, `Telefone` e `CPF` sairam do agendamento e passaram para `ClienteModel`.
- O CPF na Home aciona auto preenchimento do nome do cliente por endpoint publico, sem exigir login.
- Home inicial virou uma pagina minimalista com:
  - apresentacao da barbearia
  - botao de login para barbeiros
  - formulario visual de agendamento com selecao de barbeiro
- O layout compartilhado foi simplificado para ficar mais leve e limpo.

## Atualizacao recente (Login e telas)

- Home inicial simplificada em [Views/Home/Index.cshtml](Views/Home/Index.cshtml) com mensagem de boas-vindas e botao de login.
- Nova tela de login em [Views/Auth/Login.cshtml](Views/Auth/Login.cshtml).
- Nova tela de administracao em [Views/Home/HomeAdministrador.cshtml](Views/Home/HomeAdministrador.cshtml) com lista de barbeiros.
- Nova tela de funcionario em [Views/Home/HomeFuncionario.cshtml](Views/Home/HomeFuncionario.cshtml) com lista de procedimentos do funcionario logado.
- Nova tela de cliente em [Views/Home/HomeCliente.cshtml](Views/Home/HomeCliente.cshtml) com dados cadastrais e agendamentos por CPF.
- Fluxo de autenticacao simples em camadas:
  - Controller: [Controllers/AuthController.cs](Controllers/AuthController.cs)
  - Service: [Services/LoginService.cs](Services/LoginService.cs)
  - Repository: [Repositories/LoginRepository.cs](Repositories/LoginRepository.cs)
- Novos ViewModels para manter SRP no MVC:
  - [Models/ViewModels/LoginRequestViewModel.cs](Models/ViewModels/LoginRequestViewModel.cs)
  - [Models/ViewModels/HomeFuncionarioViewModel.cs](Models/ViewModels/HomeFuncionarioViewModel.cs)

## O que foi estruturado

- Centralizacao dos enums em uma pasta dedicada `Enums`.
- Relacao entre `BarbeiroModel` e `ProcedimentoModel` por meio da lista `Procedimentos` no barbeiro.
- Relacao entre `BarbeiroModel` e `ClienteModel` por meio da lista `Clientes` no barbeiro.
- Relacao entre `AgendamentoModel` e `ClienteModel` por entidade, sem dados duplicados de nome/telefone/cpf.
- Service para orquestrar regras de negocio:
  - `BarbeiroService`
  - `ProcedimentoService`
  - `ClienteService`
  - `AgendamentoService`
- Controllers enxutos, sem regra de negocio pesada.
- Repositories em memoria para persistencia simples durante o desenvolvimento.
- Validacao de acesso por perfil no fluxo de navegacao web.
- Agendamento separado em service/repository para manter a responsabilidade unica.

## Regras de negocio implementadas

- `TipoAcessoEnum.Administrador` pode atualizar `Descricao` e `PrecoBase` do catalogo de procedimentos.
- `TipoAcessoEnum.Funcionario` pode adicionar e remover procedimentos da propria lista.
- `TipoAcessoEnum.Funcionario` pode atualizar `PrecoPorBarbeiro` dos seus procedimentos.
- O administrador pode visualizar o preco praticado por cada barbeiro por meio do retorno do barbeiro com sua lista de procedimentos.
- `Descricao` e `PrecoBase` representam o cadastro global do procedimento e sao sincronizados em todos os barbeiros.
- No login: Funcionario vai para HomeFuncionario e Administrador vai para HomeAdministrador.
- No login: Cliente vai para HomeCliente.
- Agendamento inicia com `StatusAgendamentoEnum.Pendente` por padrao.
- O barbeiro escolhido no formulario e persistido como entidade relacionada, nao como string.
- O preco do agendamento e recalculado na camada de service para garantir consistencia, mesmo que o cliente altere o formulario no navegador.
- O funcionario altera apenas seus proprios agendamentos; o administrador apenas visualiza todos.
- Cliente visualiza agendamentos vinculados ao proprio CPF e pode alterar somente o `StatusAgendamentoEnum` dos seus agendamentos.
- Cliente apenas visualiza os dados cadastrais na HomeCliente.
- Barbeiro pode cadastrar, visualizar, editar e desativar clientes que cadastrou.
- Administrador pode visualizar, atualizar, desativar e excluir clientes de todos os barbeiros.

## Principios SOLID aplicados

- SRP: controllers, services e repositories tem responsabilidades separadas.
- OCP: a regra de negocio esta concentrada nos services, facilitando extensao.
- DIP: controllers dependem de interfaces de services, e services dependem de interfaces de repositories.
- ISP: as interfaces expostas sao pequenas e focadas no que cada camada precisa.

## Como o codigo foi organizado

- Controllers apenas orquestram entrada, saida e autorizacao basica.
- Services concentram validacoes, regras de acesso e calculos de negocio.
- Repositories cuidam da persistencia em memoria e do acesso aos dados.
- ViewModels entregam somente o necessario para a tela.
- Views exibem informacao e UX simples; nao carregam regra de negocio.

## Boas praticas de Clean Code aplicadas

- Nomes claros para entidades, services e metodos.
- Guard clauses para validar acesso e evitar aninhamento desnecessario.
- Copia explicita do procedimento do catalogo para o barbeiro, evitando acoplamento indevido entre instancias.
- Normalizacao de dados: informacoes do cliente ficaram em uma entidade dedicada para evitar redundancia no agendamento.
- Comentarios pontuais explicando a intencao da regra de negocio para facilitar manutencao por uma desenvolvedora junior.
- ViewModels dedicados para separar entrada de tela e entidades de dominio.
- Formularios da Home usam markup simples, sem JS de envio, para manter a interface previsivel.
- Relacoes entre entidades sao expostas de forma clara para a tela via ViewModel, preservando SRP.
- Regra critica de preco validada em duas camadas (UI para experiencia e Service para regra de negocio), reduzindo risco de inconsistencia.
- Nomenclatura explicitamente orientada ao dominio: `ClienteModel`, `BarbeiroModel`, `AgendamentoModel` e `ProcedimentoModel`.
- Layout visual minimalista, com hierarquia clara e poucos elementos por tela.

## Verificacao realizada

- O projeto foi validado com `dotnet build -v minimal`.
- A configuracao de DI em `Program.cs` continua apontando para as interfaces corretas.
- As views foram conferidas para manter os bindings alinhados com os models atuais.

## Endpoints principais

### Procedimentos

- `GET /api/Procedimento`
- `GET /api/Procedimento/{procedimentoEnum}`
- `PUT /api/Procedimento/{procedimentoEnum}?tipoAcessoSolicitante=Administrador`

### Barbeiros

- `GET /api/Barbeiro`
- `GET /api/Barbeiro/{id}`
- `POST /api/Barbeiro`
- `POST /api/Barbeiro/{barbeiroId}/procedimentos/{procedimentoEnum}?tipoAcessoSolicitante=Funcionario`
- `DELETE /api/Barbeiro/{barbeiroId}/procedimentos/{procedimentoEnum}?tipoAcessoSolicitante=Funcionario`
- `PUT /api/Barbeiro/{barbeiroId}/procedimentos/{procedimentoEnum}/preco?precoPorBarbeiro=50&tipoAcessoSolicitante=Funcionario`

### Clientes

- `GET /api/Cliente/administrador?tipoAcessoSolicitante=Administrador`
- `GET /api/Cliente/barbeiro/{barbeiroId}?tipoAcessoSolicitante=Funcionario`
- `GET /api/Cliente/por-cpf?cpf=...`
- `POST /api/Cliente/barbeiro/{barbeiroId}?tipoAcessoSolicitante=Funcionario`
- `PUT /api/Cliente/barbeiro/{barbeiroId}/{clienteId}?tipoAcessoSolicitante=Funcionario`
- `PATCH /api/Cliente/barbeiro/{barbeiroId}/{clienteId}/desativar?tipoAcessoSolicitante=Funcionario`
- `PUT /api/Cliente/administrador/{clienteId}?tipoAcessoSolicitante=Administrador`
- `PATCH /api/Cliente/administrador/{clienteId}/desativar?tipoAcessoSolicitante=Administrador`
- `DELETE /api/Cliente/administrador/{clienteId}?tipoAcessoSolicitante=Administrador`

## Observacao importante

Ainda nao existe autenticao/autorizacao real integrada no projeto. Por isso, os endpoints que precisam de regra de acesso recebem `tipoAcessoSolicitante` como parametro para simular a validacao ate a integracao com Identity/JWT.

## Credenciais de exemplo para desenvolvimento

- Administrador: `admin@navalha.com` / `123456`
- Funcionario: `funcionario@navalha.com` / `123456`
- Cliente: `ana@cliente.com` / `123456`
- Cliente: `bianca@cliente.com` / `123456`

## Novas telas

- [Views/Home/Index.cshtml](Views/Home/Index.cshtml) apresenta a barbearia e o formulario de agendamento.
- [Views/Auth/Login.cshtml](Views/Auth/Login.cshtml) faz o login do barbeiro.
- [Views/Home/HomeAdministrador.cshtml](Views/Home/HomeAdministrador.cshtml) lista os barbeiros.
- [Views/Home/HomeFuncionario.cshtml](Views/Home/HomeFuncionario.cshtml) lista os procedimentos do funcionario logado.
- [Views/Home/HomeCliente.cshtml](Views/Home/HomeCliente.cshtml) mostra dados cadastrais do cliente e seus agendamentos.

## Endpoints de agendamento atualizados

- `GET /api/Agendamento/administrador?tipoAcessoSolicitante=Administrador` retorna todos os agendamentos.
- `GET /api/Agendamento/funcionario/{barbeiroId}?tipoAcessoSolicitante=Funcionario` retorna os agendamentos do barbeiro logado.
- `GET /api/Agendamento/cliente?cpf=...&tipoAcessoSolicitante=Cliente` retorna os agendamentos do cliente logado.
- `GET /api/Agendamento/{idAgendamento}?barbeiroIdSolicitante=...&tipoAcessoSolicitante=...` retorna um agendamento respeitando a regra de acesso.
- `PUT /api/Agendamento/funcionario/{idAgendamento}?barbeiroIdSolicitante=...` atualiza um agendamento vinculado ao barbeiro logado.
- `PUT /api/Agendamento/cliente/{idAgendamento}/status?cpfClienteSolicitante=...&status=...` atualiza apenas o status de agendamento do cliente.