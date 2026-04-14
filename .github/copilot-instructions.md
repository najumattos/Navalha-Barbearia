---
description: "Instrucoes permanentes para desenvolvimento no projeto Navalha Barbearia"
applyTo: "**"
---

# Navalha Barbearia - Instrucoes do Workspace

## Regras Gerais

- Preservar a arquitetura ASP.NET Core MVC existente.
- Manter separacao clara entre Controller, Service, Repository, Model e ViewModel.
- Evitar duplicacao de codigo; quando uma regra se repetir, consolidar em metodo privado, helper ou servico apropriado.
- Priorizar guard clauses para reduzir aninhamento e deixar o fluxo mais legivel.
- Manter comentarios curtos e didaticos apenas onde o codigo nao for autoexplicativo.
- Escrever comentarios com foco em uma desenvolvedora junior, explicando o motivo da decisao, nao apenas o que o codigo faz.
- Usar nomes expressivos e consistentes em portugues, seguindo os padroes ja adotados no projeto.
- Manter as telas simples, padronizadas e minimalistas.

## Requisitos de Atualizacao

- Sempre que uma funcionalidade, campo, arquivo ou comportamento relevante mudar, atualizar o README na mesma entrega.
- Se a mudanca afetar regras de desenvolvimento, documentacao ou padroes do projeto, refletir isso tambem nestas instrucoes.
- Quando houver nova regra de negocio, registrar o impacto nos perfis de acesso e nas telas afetadas.

## Frontend

- Reutilizar padroes visuais ja existentes antes de criar novo layout.
- Preferir interface limpa, responsiva e com foco em legibilidade.
- Evitar excesso de cor, animacao ou componentes desnecessarios.
- Garantir consistencia entre formularios, listas, estados vazios e mensagens de erro.

## Backend

- Centralizar validacoes de acesso e regras de negocio em services sempre que possivel.
- Usar exception handling de forma previsivel e consistente com a aplicacao.
- Manter metodos pequenos e com responsabilidade unica.
- Valorizar simplicidade, clareza e manutencao acima de otimizacoes prematuras.

## Qualidade

- Antes de concluir uma mudanca, revisar se a documentacao e os comentarios continuam coerentes com o codigo.
- Se algo nao estiver claro para manutencao futura, explicar com um comentario curto ou com uma pequena refatoracao.
- Evitar criar arquivos novos sem necessidade real.