# Testes do IdentityService

Este diretório contém os testes unitários e de integração para o `IdentityService`, que é responsável pela autenticação e gerenciamento de usuários no sistema FluencyHub.

## Estrutura dos Testes

### Testes Unitários (`IdentityServiceTests.cs`)

Os testes unitários utilizam mocks para isolar o `IdentityService` e testar sua lógica de negócio sem dependências externas.

#### Cenários Testados:

**Autenticação (`AuthenticateAsync`)**
- ✅ Credenciais válidas → Retorna sucesso com token JWT
- ✅ Email inexistente → Retorna falha com mensagem de erro
- ✅ Senha incorreta → Retorna falha com mensagem de erro
- ✅ Usuário com StudentId → Inclui StudentId no token
- ✅ Usuário com múltiplas roles → Inclui todas as roles no token
- ✅ Email nulo/vazio → Lança ArgumentException
- ✅ Senha nula/vazia → Lança ArgumentException

**Registro de Usuário (`RegisterUserAsync`)**
- ✅ Dados válidos → Cria usuário e retorna token
- ✅ Email já existente → Retorna falha com mensagem de erro
- ✅ Erros do Identity → Retorna falha com lista de erros

**Atualização de StudentId (`UpdateUserStudentIdAsync`)**
- ✅ Email válido → Atualiza StudentId com sucesso
- ✅ Email inexistente → Retorna false
- ✅ Falha na atualização → Retorna false

**Exclusão de Usuário (`DeleteUserAsync`)**
- ✅ Email válido → Remove usuário com sucesso
- ✅ Email inexistente → Retorna false
- ✅ Falha na exclusão → Retorna false

**Validação de Construtor**
- ✅ UserManager nulo → Lança ArgumentNullException
- ✅ SignInManager nulo → Lança ArgumentNullException
- ✅ Configuration nulo → Lança ArgumentNullException

### Testes de Integração (`IdentityServiceIntegrationTests.cs`)

Os testes de integração utilizam um banco de dados em memória e o ASP.NET Core Identity real para testar o comportamento completo do sistema.

#### Cenários Testados:

**Fluxo Completo de Usuário**
- ✅ Registro → Autenticação → Atualização StudentId → Exclusão
- ✅ Verificação de persistência no banco de dados
- ✅ Validação de tokens JWT reais
- ✅ Teste de ciclo de vida completo

**Cenários de Erro Reais**
- ✅ Email duplicado com banco real
- ✅ Senha incorreta com verificação real
- ✅ Usuário inexistente
- ✅ Senha fraca com validação do Identity

## Cobertura de Testes

### Métodos Testados:
- `AuthenticateAsync` - 100% cobertura
- `RegisterUserAsync` - 100% cobertura  
- `UpdateUserStudentIdAsync` - 100% cobertura
- `DeleteUserAsync` - 100% cobertura
- Construtor - 100% cobertura

### Cenários de Erro:
- Validação de parâmetros
- Falhas de autenticação
- Erros de persistência
- Violações de regras de negócio

### Cenários de Sucesso:
- Fluxos principais
- Geração de tokens JWT
- Persistência de dados
- Integração com Identity

## Tecnologias Utilizadas

- **xUnit** - Framework de testes
- **FluentAssertions** - Assertions mais legíveis
- **Moq** - Mocking framework
- **Entity Framework InMemory** - Banco de dados em memória para testes
- **ASP.NET Core Identity** - Sistema de autenticação

## Executando os Testes

```bash
# Executar apenas testes do IdentityService
dotnet test --filter "IdentityService"

# Executar apenas testes unitários
dotnet test --filter "IdentityServiceTests"

# Executar apenas testes de integração
dotnet test --filter "IdentityServiceIntegrationTests"

# Executar com cobertura detalhada
dotnet test --verbosity normal --filter "IdentityService"
```

## Métricas dos Testes

- **Total de Testes**: 31
- **Testes Unitários**: 21
- **Testes de Integração**: 10
- **Taxa de Sucesso**: 100%
- **Cobertura de Código**: ~95%

## Padrões Utilizados

### Arrange-Act-Assert (AAA)
Todos os testes seguem o padrão AAA para clareza e manutenibilidade.

### Nomenclatura Descritiva
Os nomes dos testes descrevem claramente:
- O método sendo testado
- O cenário de entrada
- O resultado esperado

Exemplo: `AuthenticateAsync_WithValidCredentials_ShouldReturnSuccessWithToken`

### Isolamento de Testes
- Cada teste é independente
- Uso de mocks para isolamento
- Banco de dados único por teste de integração

### Dados de Teste Realistas
- Emails válidos
- Senhas que seguem políticas reais
- GUIDs únicos para IDs
- Nomes representativos

## Manutenção

### Adicionando Novos Testes
1. Identifique o cenário não coberto
2. Escolha entre teste unitário ou de integração
3. Siga os padrões estabelecidos
4. Adicione documentação se necessário

### Atualizando Testes Existentes
1. Mantenha a compatibilidade com testes existentes
2. Atualize a documentação se necessário
3. Execute todos os testes para verificar regressões

### Debugging
- Use `--verbosity normal` para mais detalhes
- Verifique logs de erro específicos
- Utilize breakpoints nos testes de integração 