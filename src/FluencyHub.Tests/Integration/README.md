# Testes de Integração - FluencyHub

Este diretório contém testes de integração para a plataforma FluencyHub, validando os fluxos completos de cada bounded context.

## Estrutura

- `IntegrationTestFixture.cs` - Classe base para configuração do ambiente de teste usando SQLite em memória
- `Helpers/` - Classes auxiliares para autenticação e outras operações comuns
- `ContentManagement/` - Testes para o bounded context de gestão de conteúdo
- `StudentManagement/` - Testes para o bounded context de gestão de alunos
- `PaymentProcessing/` - Testes para o bounded context de pagamento e faturamento

## Execução dos Testes

Para executar os testes de integração, use o seguinte comando:

```bash
dotnet test FluencyHub.Tests --filter Category=Integration
```

Para executar todos os testes incluindo testes unitários:

```bash
dotnet test FluencyHub.Tests
```

## Bounded Contexts Testados

### 1. Gestão de Conteúdo
- Criação de cursos por administradores
- Adição de aulas aos cursos
- Listagem e busca de cursos

### 2. Gestão de Alunos
- Matrícula de alunos em cursos
- Acompanhamento do progresso do aluno
- Finalização de curso e geração de certificados

### 3. Pagamento e Faturamento
- Processamento de pagamentos
- Validação de cartões
- Reembolsos e cancelamentos

## Fluxos Integrados
Os testes validam fluxos completos do sistema, incluindo:

1. **Fluxo de Cadastro de Curso**:
   - Administrador cria curso
   - Administrador adiciona aulas ao curso
   - Curso fica disponível para alunos

2. **Fluxo de Matrícula e Pagamento**:
   - Aluno se matricula em um curso
   - Aluno realiza pagamento
   - Matrícula é ativada

3. **Fluxo de Conclusão e Certificação**:
   - Aluno completa todas as aulas do curso
   - Aluno solicita finalização do curso
   - Certificado é gerado automaticamente 