# Navalha Barbearia

Aplicacao ASP.NET Core MVC (.NET 8) para gestao de barbearia com foco em separacao de responsabilidades, regras de negocio claras e interface simples.

## Resumo da refatoracao desta branch

O README anterior foi limpo e substituido por este documento objetivo.

### Funcionalidade refatorada

Refatoracao do fluxo de edicao de procedimentos para o perfil administrador.

Antes:
- A tela de edicao dependia apenas de rota com tipo especifico.
- Nao havia auto preenchimento dinamico na tela de edicao.
- O controller nao validava explicitamente o acesso de administrador em todas as acoes sensiveis.

Agora:
- O administrador abre a tela de edicao e escolhe o `ProcedimentoEnum` em um seletor.
- Ao selecionar o tipo, a tela busca no backend e preenche automaticamente `Descricao` e `PrecoBase`.
- O POST usa o `ProcedimentoEnum` do proprio model enviado no formulario.
- Create, Edit e Delete validam permissao de administrador com guard clauses.

## Arquivos alterados

- `Controllers/ProcedimentosController.cs`
- `Views/Procedimentos/Edit.cshtml`
- `README.md`

## Detalhes tecnicos da implementacao

### Backend

Em `ProcedimentosController`:
- Foi criado o metodo privado `ValidarAcessoAdministrador()`.
- Foi aplicado o padrao de guard clause nas acoes sensiveis (`Create`, `Edit`, `Delete`) para reduzir aninhamento e deixar o fluxo legivel.
- A acao `Edit` (GET) passou a aceitar `ProcedimentoEnum?` para permitir abrir a tela sem depender obrigatoriamente de query string.
- Foi criado o endpoint `BuscarPorTipo(ProcedimentoEnum procedimentoEnum)` para retornar JSON com os dados do procedimento selecionado.
- A acao `Edit` (POST) foi simplificada para receber `ProcedimentoModel` e atualizar pelo tipo escolhido no formulario.

### Frontend

Em `Views/Procedimentos/Edit.cshtml`:
- O campo de tipo foi convertido para `select` com todos os valores de `ProcedimentoEnum`.
- Foi adicionado JavaScript para buscar os dados em `/Procedimentos/BuscarPorTipo` e preencher automaticamente descricao e preco base.
- Foram incluidos comentarios curtos no codigo explicando:
  - responsabilidade unica de funcoes (SRP),
  - uso de fonte unica da verdade no backend,
  - reducao de duplicacao de dados (DRY),
  - fluxo previsivel e legivel para manutencao.

## Boas praticas aplicadas

- SRP: controller com validacao de acesso centralizada e endpoint dedicado para leitura de dados da tela.
- DRY: reaproveitamento da regra de acesso em um unico metodo privado.
- Guard Clauses: falha rapida em caso de usuario nao autenticado ou sem permissao.
- Fonte unica da verdade: dados de procedimento vem do backend para preencher o formulario.
- Comentarios didaticos: comentarios pontuais para apoiar desenvolvedora junior sem poluir o codigo.
- UI simples e minimalista: formulario limpo, feedback textual objetivo e interacao direta.

## Como validar localmente

1. Restaurar dependencias e compilar:

```bash
dotnet build -v minimal
```

2. Executar a aplicacao e testar como administrador:
- Ir para a tela de procedimentos.
- Abrir edicao de procedimento.
- Alterar o tipo no seletor.
- Confirmar auto preenchimento de descricao e preco base.
- Salvar e validar se os dados foram persistidos no catalogo.

## Observacoes

- O projeto ainda usa autenticacao/autorizacao simulada em memoria para desenvolvimento.
- A validacao de permissao aplicada nesta refatoracao segue o contexto de usuario atual em sessao.
