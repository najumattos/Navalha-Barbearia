# Roadmap de Evolução - Navalha Barbearia

Aplicação prática dos pontos do roadmap backend/full-stack no contexto do projeto atual.

---

## 📋 Índice
1. [Internet & Infrastructure](#internet--infrastructure)
2. [Backend Design (MVC)](#backend-design-mvc)
3. [Databases](#databases)
4. [Authentication](#authentication)
5. [Web Security](#web-security)
6. [Testing](#testing)
7. [Software Design & Architecture](#software-design--architecture)
8. [DevOps](#devops)

---

## Internet & Infrastructure

### Como o Internet Funciona
**Status**: ✅ Já integrado no projeto

- [x] HTTP/HTTPS entendido e configurado no `Program.cs`
- [x] DNS resolvido (hospedagem local em desenvolvimento)
- [x] Browsers comunicando corretamente com servidor ASP.NET Core

**Checklist de Validação**:
- [x] HTTPS redirecionamento habilitado (`app.UseHttpsRedirection()`)
- [x] HSTS configurado (`app.UseHsts()`)
- [x] Pipeline HTTP completo: Routing → Session → Authorization

---

### Web Servers
**Status**: ✅ ASP.NET Core (nativo Kestrel)

**Endpoints Atuais**:
- [x] Controller routes mapeadas em `Program.cs`
- [x] Static files servidos (`app.UseStaticFiles()`)
- [x] Views renderizadas corretamente

**Próximas Etapas (se necessário)**:
- [ ] Nginx/IIS para reverse proxy em produção
- [ ] Load balancing (futuro, se escalar)

---

## Backend Design (MVC)

### Arquitetura MVC
**Status**: ✅ Estrutura base implementada

**Objetivo**: manter o projeto orientado a MVC, sem endpoints externos e sem controllers dedicados de integração.

**Checklist de Implementação**:
- [x] Controllers MVC organizados por domínio
- [x] Services concentrando regras de negócio
- [x] Repositories abstraindo acesso a dados
- [ ] Padronizar ViewModels por caso de uso (listagem, criação, edição, detalhes)
- [ ] Revisar validações de entrada com DataAnnotations e ModelState
- [ ] Padronizar mensagens de erro e feedback ao usuário nas Views

### Documentação de Controllers MVC
**Status**: ⚠️ Parcial

**Passos**:
- [ ] Documentar actions críticas com XML comments
- [ ] Descrever regras de autorização por perfil em comentários curtos
- [ ] Atualizar README com fluxos MVC principais (Login, Agendamento, Gestão)

**Checklist**:
- [ ] Controllers críticos com `/// <summary>`
- [ ] Fluxos principais mapeados na documentação
- [ ] Regras de acesso por perfil registradas

---

## Databases

### Relational Database (PostgreSQL ou SQL Server)
**Status**: ❌ Atualmente em-memória (listas estáticas)

**Impacto**: Elimina RNF-12 (perda de dados ao reiniciar). **ALTA PRIORIDADE**.

#### Passo 1: Escolher Banco de Dados
- [ ] PostgreSQL (open-source, recomendado)
- [ ] MS SQL Server (propriedário, pode requerer licença)
- [ ] MySQL (alternativa)

**Recomendação**: PostgreSQL (melhor custo-benefício)

#### Passo 2: Instalar EF Core
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL  # Para PostgreSQL
# OU
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # Para SQL Server
```

#### Passo 3: Criar DbContext
Criar arquivo `Data/NavalhaDbContext.cs`:
```csharp
public class NavalhaDbContext : DbContext
{
    public DbSet<AgendamentoModel> Agendamentos { get; set; }
    public DbSet<BarbeiroModel> Barbeiros { get; set; }
    public DbSet<ClienteModel> Clientes { get; set; }
    public DbSet<ProcedimentoModel> Procedimentos { get; set; }
    
    public NavalhaDbContext(DbContextOptions<NavalhaDbContext> options) 
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configurar relacionamentos e constraints
    }
}
```

#### Passo 4: Configurar Connection String em `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=navalha_barbearia;Username=postgres;Password=senha"
  }
}
```

#### Passo 5: Registrar DbContext em `Program.cs`
```csharp
builder.Services.AddDbContext<NavalhaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

#### Passo 6: Criar Migrations
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Passo 7: Refatorar Repositories
Substituir listas estáticas por queries EF Core:
```csharp
public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly NavalhaDbContext _context;
    
    public AgendamentoRepository(NavalhaDbContext context) => _context = context;
    
    public IEnumerable<AgendamentoModel> ObterTodos()
        => _context.Agendamentos.AsNoTracking().ToList();
    
    public AgendamentoModel ObterPorId(int id)
        => _context.Agendamentos.Find(id);
    
    public void Adicionar(AgendamentoModel agendamento)
    {
        _context.Agendamentos.Add(agendamento);
        _context.SaveChanges();
    }
}
```

**Checklist**:
- [ ] PostgreSQL instalado localmente ou via Docker
- [ ] EF Core packages instalados
- [ ] `NavalhaDbContext` criado
- [ ] `appsettings.json` com connection string
- [ ] DbContext registrado em `Program.cs`
- [ ] Primeira migration criada e aplicada
- [ ] Todos os Repositories refatorados para usar EF Core
- [ ] Testes: aplicação inicia sem erros
- [ ] Testes: dados persistem após reiniciar

---

### ORMs (Entity Framework Core)
**Status**: ⚠️ Parcial (Framework pronto, não configurado)

**Passo a Passo** (após banco de dados acima):

#### Passo 1: Relacionamentos (Fluent Mapping)
Configure relacionamentos em `NavalhaDbContext.OnModelCreating()`:
```csharp
modelBuilder.Entity<AgendamentoModel>()
    .HasOne(a => a.Barbeiro)
    .WithMany(b => b.Agendamentos)
    .HasForeignKey(a => a.BarbeiroId)
    .OnDelete(DeleteBehavior.Restrict);

modelBuilder.Entity<BarbeiroProcedimentoModel>()
    .HasKey(bp => new { bp.BarbeiroId, bp.ProcedimentoId });
```

#### Passo 2: Índices para Performance
```csharp
modelBuilder.Entity<ClienteModel>()
    .HasIndex(c => c.CPF)
    .IsUnique();

modelBuilder.Entity<AgendamentoModel>()
    .HasIndex(a => new { a.BarbeiroId, a.DataHora });
```

#### Passo 3: Seeding (Dados Iniciais)
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<BarbeiroModel>().HasData(
        new BarbeiroModel { Id = 1, Nome = "João Silva", Genero = GeneroEnum.Masculino },
        // ... mais barbeiros
    );
}
```

**Checklist**:
- [ ] Todos os relacionamentos configurados
- [ ] Índices criados para chaves estrangeiras e buscas
- [ ] Dados iniciais com Seeding
- [ ] Migrations após mudanças
- [ ] `_context.SaveChanges()` chamado após modificações

---

### Database Scaling Concepts
**Status**: ⚠️ Não relevante agora, planejar futura

- [ ] N+1 Problem: Usar `.Include()` e `.ThenInclude()` em queries
  ```csharp
  _context.Agendamentos
      .Include(a => a.Barbeiro)
      .Include(a => a.Procedimento)
      .ToList();
  ```
- [ ] Índices: Criar em colunas de busca frequente (CPF, Email)
- [ ] Particionamento: Futuro, se tabelas crescerem muito
- [ ] Replicação: Futuro, para alta disponibilidade

**Checklist para Futuro**:
- [ ] Monitorar queries lentas (SQL Profiler)
- [ ] Adicionar índices conforme performance cai
- [ ] Cache de dados quentes (Redis)

---

## Authentication

### ASP.NET Identity
**Status**: ❌ Atualmente simulado em memória (LoginRepository)

**Impacto**: Remove RNF-13 (senhas em texto plano). Segurança real. **ALTA PRIORIDADE**.

#### Passo 1: Instalar Identity
```bash
dotnet add package Microsoft.AspNetCore.Identity
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

#### Passo 2: Criar IdentityUser customizado
```csharp
public class UsuarioIdentity : IdentityUser<int>
{
    public string CPF { get; set; }
    public string Nome { get; set; }
    public TipoAcessoEnum TipoAcesso { get; set; }
    public DateTime DataCadastro { get; set; }
}
```

#### Passo 3: Atualizar DbContext
```csharp
public class NavalhaDbContext : IdentityDbContext<UsuarioIdentity, IdentityRole<int>, int>
{
    public DbSet<AgendamentoModel> Agendamentos { get; set; }
    // ... outras entidades
    
    public NavalhaDbContext(DbContextOptions<NavalhaDbContext> options) 
        : base(options) { }
}
```

#### Passo 4: Registrar Identity em `Program.cs`
```csharp
builder.Services.AddIdentity<UsuarioIdentity, IdentityRole<int>>()
    .AddEntityFrameworkStores<NavalhaDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
});
```

#### Passo 5: Refatorar AuthController
Substituir `LoginService` por `SignInManager<UsuarioIdentity>`:
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    var usuario = await _userManager.FindByNameAsync(request.CPF);
    var result = await _signInManager.PasswordSignInAsync(
        usuario, request.Senha, false, false);
    
    if (result.Succeeded)
        return RedirectToAction("Home", "Index");
    
    return View();
}
```

#### Passo 6: Adicionar [Authorize] aos Controllers
```csharp
[Authorize(Roles = "Administrador")]
public class BarbeirosController : Controller { }

[Authorize(Roles = "Funcionario")]
public class AgendamentosController : Controller { }
```

**Checklist**:
- [ ] Identity packages instalados
- [ ] `UsuarioIdentity` criado com CPF e TipoAcesso
- [ ] `NavalhaDbContext` extends `IdentityDbContext`
- [ ] Identity registrado em `Program.cs`
- [ ] Senhas com requisitos (min 8 caracteres, maiúsculas, números)
- [ ] `AuthController.Login()` refatorado com `SignInManager`
- [ ] `AuthController.Register()` criado com hashing bcrypt
- [ ] `[Authorize]` adicionado aos Controllers sensíveis
- [ ] `[Authorize(Roles = "...")]` por perfil
- [ ] Logout limpa sessão corretamente
- [ ] Migração: usuários de teste criados com novo sistema
- [ ] Testes: Login/Logout funcionam
- [ ] Testes: Senhas são criptografadas (SHA-256/bcrypt)

---

## Web Security

### HTTPS & SSL/TLS
**Status**: ✅ Já configurado

- [x] `app.UseHttpsRedirection()` ativado
- [x] HSTS habilitado
- [x] Certificados automáticos em dev

**Checklist de Validação**:
- [x] HTTPS obrigatório (`https://localhost:5001`)
- [x] HTTP redireciona para HTTPS
- [x] Header `Strict-Transport-Security` presente

---

### CSRF Protection
**Status**: ✅ Já implementado

- [x] `[ValidateAntiForgeryToken]` em POST/PUT/DELETE
- [x] Token CSRF gerado em formulários

**Verificação**:
- [x] `@Html.AntiForgeryToken()` em cada formulário
- [x] Token CSRF validado em Controllers

---

### OWASP Top 10
**Status**: ⚠️ Parcialmente coberto

#### 1. Injection (SQL Injection, Command Injection)
- [x] Usar EF Core (parameterized queries automáticas)
- [x] Nunca concatenar queries SQL
- [x] Validar entrada do usuário

**Checklist**:
- [ ] Nenhuma query SQL dinâmica (sempre usar LINQ to Entities)
- [ ] Inputs validados (FluentValidation)

#### 2. Broken Authentication
- [x] Hash de senhas (bcrypt com Identity)
- [x] Sem senhas em texto plano

**Checklist**:
- [ ] Senhas hashadas com Identity
- [ ] No browser history (POST sensível)
- [ ] Timeout de sessão (4 horas)

#### 3. Sensitive Data Exposure
- [x] HTTPS obrigatório
- [ ] Dados sensíveis (CPF, email) protegidos

**Checklist**:
- [ ] HTTPS em produção
- [ ] CPF não exposto em logs/responses
- [ ] Senhas nunca retornadas em logs, views ou respostas

#### 4. XML External Entities (XXE)
- [x] Não usa XML parsing de usuários
- [x] Usa JSON (seguro por padrão)

#### 5. Broken Access Control
- [x] Autorização por perfil (RN-01 a RN-08)
- [ ] Adicionar AuditLog

**Checklist**:
- [ ] `[Authorize(Roles = "...")]` em todos Controllers sensíveis
- [ ] Guard clauses em Services validam ownership
- [ ] Testes: Cliente não acessa agendamento de outro

#### 6. Security Misconfiguration
- [ ] Remover stack traces de produção
- [ ] Configurar security headers

**Checklist**:
- [ ] `app.UseExceptionHandler()` ativo
- [ ] `app.UseHsts()` ativo
- [ ] Error pages customizadas (sem stack trace)
- [ ] Headers: X-Content-Type-Options, X-Frame-Options, X-XSS-Protection

#### 7. XSS (Cross-Site Scripting)
- [x] Razor templates escapam HTML automaticamente
- [x] `@Html.DisplayFor()` vs `@Html.Raw()` (nunca Raw com input)

**Checklist**:
- [ ] Nunca usar `Html.Raw()` com input do usuário
- [ ] Content-Security-Policy header
- [ ] Validação de entrada

#### 8. Insecure Deserialization
- [x] EF Core desserializa seguramente
- [ ] Validar DTOs

#### 9. Using Components with Known Vulnerabilities
- [ ] Manter packages atualizados

**Checklist**:
- [ ] `dotnet list package --outdated` regularmente
- [ ] Dependências críticas atualizadas regularmente

#### 10. Insufficient Logging & Monitoring
- [ ] Implementar Serilog (veja Testing & Logging)

---

### Security Headers
**Status**: ⚠️ Básico implementado

Adicionar em `Program.cs`:
```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    
    await next();
});
```

**Checklist**:
- [ ] Security headers adicionados
- [ ] Testado com curl: `curl -i https://localhost:5001`
- [ ] Verificar headers na resposta

---

## Testing

### Unit Testing (xUnit)
**Status**: ❌ Não implementado

#### Passo 1: Criar Projeto de Testes
```bash
dotnet new xunit -n Navalha-Barbearia.Tests
dotnet add Navalha-Barbearia.Tests/Navalha-Barbearia.Tests.csproj reference Navalha-Barbearia/Navalha-Barbearia.csproj
```

#### Passo 2: Estrutura de Pastas
```
Navalha-Barbearia.Tests/
├── Services/
│   ├── AgendamentoServiceTests.cs
│   ├── AgendamentoValidacaoServiceTests.cs
│   ├── ClienteServiceTests.cs
│   └── LoginServiceTests.cs
├── Repositories/
│   ├── ClienteRepositoryTests.cs
│   └── AgendamentoRepositoryTests.cs
└── Fixtures/
    ├── ClienteFixture.cs
    └── AgendamentoFixture.cs
```

#### Passo 3: Instalar Packages
```bash
dotnet add Navalha-Barbearia.Tests package Moq
dotnet add Navalha-Barbearia.Tests package FluentAssertions
```

#### Passo 4: Testar Services (Exemplo)
```csharp
public class AgendamentoServiceTests
{
    private readonly Mock<IAgendamentoRepository> _repositoryMock;
    private readonly AgendamentoService _service;
    
    public AgendamentoServiceTests()
    {
        _repositoryMock = new Mock<IAgendamentoRepository>();
        _service = new AgendamentoService(_repositoryMock.Object);
    }
    
    [Fact]
    public void CriarAgendamento_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var agendamento = new AgendamentoModel { Id = 1, Status = StatusAgendamentoEnum.Agendado };
        
        // Act
        var resultado = _service.Validar(agendamento);
        
        // Assert
        resultado.Should().BeTrue();
    }
    
    [Fact]
    public void CriarAgendamento_ComDataRetroativa_DeveRetornarErro()
    {
        // Arrange
        var agendamento = new AgendamentoModel
        {
            DataHora = DateTime.Now.AddDays(-1),
            Status = StatusAgendamentoEnum.Agendado
        };
        
        // Act
        var resultado = _service.Validar(agendamento);
        
        // Assert
        resultado.Should().BeFalse();
    }
}
```

#### Passo 5: Rodar Testes
```bash
dotnet test Navalha-Barbearia.Tests/Navalha-Barbearia.Tests.csproj
```

**Checklist (Cobertura Mínima)**:
- [ ] 100% das RNs testadas (RN-01 a RN-31)
- [ ] Testes para Services:
  - [ ] `AgendamentoValidacaoService` (RN-24, RN-31)
  - [ ] `AgendamentoService` (RN-15)
  - [ ] `ClienteService` (RN-21)
  - [ ] `BarbeiroService` (procediomentos N:N)
- [ ] Testes para edge cases:
  - [ ] Data retroativa
  - [ ] Barbeiro inválido
  - [ ] Cliente inativo
  - [ ] Procedimento desvinculado
- [ ] Mocks de Repositories
- [ ] FluentAssertions para legibilidade
- [ ] Cobertura >80% do código

---

### Integration Testing
**Status**: ❌ Não implementado

#### Passo 1: Criar WebApplicationFactory
```csharp
public class NavalhaWebApplicationFactory 
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext real
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<NavalhaDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);
            
            // Adicionar DbContext em memória
            services.AddDbContext<NavalhaDbContext>(options =>
                options.UseInMemoryDatabase("NavalhaTest"));
        });
    }
}
```

#### Passo 2: Testar Endpoints
```csharp
public class AgendamentosControllerIntegrationTests
{
    private readonly HttpClient _client;
    
    public AgendamentosControllerIntegrationTests()
    {
        var factory = new NavalhaWebApplicationFactory();
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GetAll_DeveRetornarOk()
    {
        // Act
        var response = await _client.GetAsync("/Agendamentos");
        
        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
```

**Checklist**:
- [ ] WebApplicationFactory criada
- [ ] Testes de endpoints GET/POST/PUT/DELETE
- [ ] Validação de HTTP status codes
- [ ] Testes com dados em banco (EF Core InMemory)
- [ ] Testes de autenticação via cookie/sessão

---

### Functional Testing
**Status**: ⚠️ Manual considerado

Se implementar testes navegador:
- [ ] Selenium (C#)
- [ ] Teste fluxos: Login → Agendamento → Confirmação
- [ ] Validar UI (submit buttons, validações client-side)

---

## Software Design & Architecture

### Design Patterns
**Status**: ⚠️ Parcialmente implementado

#### Repository Pattern ✅
- [x] Interfaces `IAgendamentoRepository`, etc.
- [x] Implementations com lógica de persistência
- [x] Desacoplamento Controllers ↔ Dados

**Checklist**:
- [x] Mesmo padrão em todos Repositories
- [x] Nenhuma lógica de negócio em Repository
- [x] Métodos nomeados semanticamente (ObterTodosPor..., Adicionar, etc.)

#### Service Pattern ✅
- [x] `AgendamentoService`, `ClienteService`, etc.
- [x] Lógica de negócio centralizada
- [x] Validações antes de persistência

**Checklist**:
- [x] Mesmo padrão em todos Services
- [x] Guard clauses para autorização
- [x] Logging em mudanças críticas

#### Dependency Injection ✅
- [x] Configurado em `Program.cs`
- [x] Services/Repositories registrados como Scoped

**Checklist**:
- [x] Nenhuma `new` em Controllers
- [x] Todos registrados em DI

#### Factory Pattern ⚠️
Se precisar criar variações complexas:
```csharp
public interface IAgendamentoFactory
{
    AgendamentoModel CriarPublico(ClienteModel cliente, ...);
    AgendamentoModel CriarPorBarbeiro(BarbeiroModel barbeiro, ...);
}
```

#### Strategy Pattern ⚠️
Se validações variam por tipo:
```csharp
public interface IAgendamentoValidador
{
    bool Validar(AgendamentoModel agendamento);
}

public class AgendamentoPublicoValidador : IAgendamentoValidador { }
```

---

### Domain Driven Design (DDD)
**Status**: ⚠️ Básico implementado

#### Bounded Contexts
- **Agendamento Context**: AgendamentoModel, AgendamentoService
- **Cliente Context**: ClienteModel, ClienteService
- **Barbeiro Context**: BarbeiroModel, BarbeiroService

**Próxima Etapa**:
- [ ] Criar pastas por domínio:
  ```
  Domains/
  ├── Agendamento/
  │   ├── Models/
  │   ├── Services/
  │   └── Repositories/
  ├── Cliente/
  └── Barbeiro/
  ```

---

### CQRS (Command Query Responsibility Segregation)
**Status**: ❌ Não necessário agora

Considerar futura se projeto crescer:
- Separar escrita (Commands) de leitura (Queries)
- Otimizar queries complexas

---

---

## DevOps

### CI/CD
**Status**: ❌ Não implementado

#### Passo 1: Criar GitHub Actions Workflow
Arquivo: `.github/workflows/ci.yml`
```yaml
name: CI

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build -v minimal
    
    - name: Run tests
      run: dotnet test Navalha-Barbearia.Tests/Navalha-Barbearia.Tests.csproj --no-build
```

#### Passo 2: Adicionar ao repositório
```bash
git add .github/workflows/ci.yml
git commit -m "chore: add CI/CD pipeline"
git push
```

**Checklist**:
- [ ] Workflow criado em `.github/workflows/`
- [ ] Build passa automaticamente no push
- [ ] Testes rodam automaticamente
- [ ] PR requer build passar
- [ ] Badges no README.md

---

### Docker
**Status**: ❌ Não implementado (mas recomendado)

#### Passo 1: Criar Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Navalha-Barbearia.csproj", "./"]
RUN dotnet restore "Navalha-Barbearia.csproj"
COPY . .
RUN dotnet build "Navalha-Barbearia.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "Navalha-Barbearia.dll"]
```

#### Passo 2: Criar docker-compose.yml
```yaml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=navalha;Username=postgres;Password=postgres
    depends_on:
      - postgres
  
  postgres:
    image: postgres:15
    environment:
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=navalha
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

#### Passo 3: Build e rodar
```bash
docker-compose up --build
```

**Checklist**:
- [ ] Dockerfile criado
- [ ] docker-compose.yml com app + postgres
- [ ] App consegue conectar ao banco
- [ ] Ou usar como base para cloud deploy (Azure, AWS)

---

### Logging & Observability
**Status**: ❌ Logging melhorado necessário

#### Passo 1: Instalar Serilog
```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.AspNetCore
```

#### Passo 2: Configurar em Program.cs
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

#### Passo 3: Adicionar logs em Services (Exemplo)
```csharp
public class AgendamentoService : IAgendamentoService
{
    private readonly ILogger<AgendamentoService> _logger;
    
    public AgendamentoService(ILogger<AgendamentoService> logger, ...)
    {
        _logger = logger;
    }
    
    public bool Criar(AgendamentoModel agendamento)
    {
        _logger.LogInformation("Criando agendamento para CPF {Cpf} em {Data}", 
            agendamento.ClienteCpf, agendamento.DataHora);
        
        if (!Validar(agendamento))
        {
            _logger.LogWarning("Agendamento inválido: {Motivo}", /*razão*/);
            return false;
        }
        
        _repository.Adicionar(agendamento);
        _logger.LogInformation("Agendamento criado com sucesso, Id: {Id}", 
            agendamento.Id);
        
        return true;
    }
}
```

#### Passo 4: Logs em Controllers MVC
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(AgendamentoCrudViewModel viewModel)
{
    _logger.LogInformation("POST Create agendamento de {Ip}",
        HttpContext.Connection.RemoteIpAddress);

    if (!ModelState.IsValid)
    {
        _logger.LogWarning("Create agendamento invalido para o cliente {ClienteId}",
            viewModel.Agendamento.Cliente?.Id);
        return View(viewModel);
    }

    _service.Criar(viewModel.Agendamento);
    _logger.LogInformation("Agendamento criado com sucesso para o cliente {ClienteId}",
        viewModel.Agendamento.Cliente?.Id);

    return RedirectToAction(nameof(Index));
}
```

**Checklist**:
- [ ] Serilog instalado
- [ ] Logs em arquivo (`logs/`)
- [ ] Logs em console (desenvolvimento)
- [ ] Login/logout registrado
- [ ] Agendamentos criados/alterados registrado
- [ ] Erros with stack trace
- [ ] Informações sensíveis (CPF, senha) não aparecem em logs
- [ ] Ferramentas para análise: ELK Stack ou Application Insights (futuro)

---

## 📊 Resumo de Prioridades

| Fase | Tarefa | Impacto | Esforço |
|---|---|---|---|
| **1** | Banco de Dados (PostgreSQL + EF Core) | ⭐⭐⭐⭐⭐ | 🟠 Médio |
| **1** | Autenticação Real (Identity + cookie/sessão) | ⭐⭐⭐⭐⭐ | 🟠 Médio |
| **2** | Unit Tests (xUnit para Services) | ⭐⭐⭐⭐ | 🟡 Baixo-Médio |
| **2** | Security Headers e hardening MVC | ⭐⭐⭐ | 🟡 Baixo |
| **3** | Logging (Serilog) | ⭐⭐⭐ | 🟡 Baixo |
| **3** | Integration Tests (fluxos MVC) | ⭐⭐ | 🟠 Médio |
| **4** | Docker / docker-compose | ⭐⭐ | 🟡 Baixo |
| **5** | CI/CD (GitHub Actions) | ⭐⭐ | 🟡 Baixo |

---

## 📝 Notas

- **Cada seção deve ser implementada sequencialmente** para evitar retrabalho
- **Atualizar README.md** após cada implementação
- **Rodar `dotnet build` e `dotnet test`** antes de cada commit
- **Solicitar review de código** antes de merge
- **Manter boas práticas de clean code** (comentários, nomenclatura, DRY)

---

**Próximas ações**:
1. Migrar persistência para banco relacional com EF Core e migrations.
2. Substituir autenticação em memória por Identity com sessão/cookie.
3. Criar testes unitários dos Services críticos e regras de negócio.
4. Aplicar hardening de segurança MVC (headers, erros e validações).

**Diretriz do projeto**: manter arquitetura MVC server-side, sem criação de controllers de integração dedicados.

Boa sorte! 🚀
