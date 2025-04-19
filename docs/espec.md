## Plataforma de Educação Online

- Título do Projeto
- Objetivo
- Descrição Geral
   - Bounded Context 1: Gestão de Conteúdo
   - Bounded Context 2: Gestão de Alunos
   - Bounded Context 3: Pagamento e Faturamento
   - Modelo de Autenticação e Usuários
   - Quem Executa as Ações
- Casos de Uso Detalhados
   - Cadastro de Curso
   - Cadastro de Aula
   - Matrícula do Aluno
   - Realização do Pagamento
   - Realização da Aula
   - Finalização do Curso.....................................................................................
- Requisitos Técnicos
- Critérios de Sucesso
- Prazos e Tipo de Entrega
- Instruções Importantes
- Entrega
- Matriz de avaliação.........................................................................................


## Título do Projeto

**Plataforma de Educação Online**

## Objetivo

Desenvolver uma plataforma educacional online com múltiplos bounded contexts
(BC), aplicando DDD, TDD, CQRS e padrões arquiteturais para gestão eficiente de
conteúdos educacionais, alunos e processos financeiros.

## Descrição Geral

A plataforma de educação e treinamento é uma aplicação disponibilizada via API
para gerir cursos/matriculas/alunos/pagamentos e prover meios para que os
alunos realizem os cursos.

O sistema será dividido em duas grandes responsabilidades:

1. Backend (API RESTful): Desenvolvido com ASP.NET Core WebAPI,
    responsável pelo processamento de dados e lógica de negócios e
    exposição de todos os endpoints do negócio em uma única API.
2. Bounded Contexts (BC): Cada BC deve possuir as camadas necessárias
    para implementar as soluções de cada problema de negócio.
       - Os BC’s não precisam ser iguais (mesma complexidade, precisar
          usar CQRS, aplicar mesmos padrões) use conforme seus critérios.
       - TDD para modelagem e ações de negócios
       - Testes de integração para validar os casos de uso
       - Banco de Dados: SQL Server com EF Core para persistência e
          gerenciamento de dados,
       - Deve haver suporte para SQLite para validação sem dependência de
          infra.
       - O usuário logado (interativo) deve corresponder a persona do
          negócio (Aluno, Administrador)


### Bounded Context 1: Gestão de Conteúdo

- **Aggregate Roots:**
    o **Curso** (agrega Aulas)
- **Entities e Value Objects:**
    o **Aula** (Entity)
    o **ConteudoProgramatico** (Value Object)
- **Manipulação:** Curso gerencia diretamente suas Aulas e Conteúdo
    Programático.

### Bounded Context 2: Gestão de Alunos

- **Aggregate Roots:**
    o **Aluno** (agrega Matrículas)
- **Entities e Value Objects:**
    o **Matricula** (Entity)
    o **Certificado** (Entity)
    o **HistoricoAprendizado** (Value Object)
- **Manipulação:** Entidade Aluno gerencia diretamente suas Matrículas e
    Certificados.

### Bounded Context 3: Pagamento e Faturamento

- **Aggregate Roots:**
    o **Pagamento**
- **Entities e Value Objects:**
    o **DadosCartao** (Value Object)
    o **StatusPagamento** (Value Object)
- **Manipulação:** Pagamento dispara eventos confirmando ou rejeitando
    pagamentos.


### Modelo de Autenticação e Usuários

- API RESTful protegida com autenticação JWT.
- O usuário logado que executa a ação deve ser representado pela persona
    do negócio (Aluno, Admin), ou seja deve haver o registro da Persona e o
    registro do usuário de forma independente, porém compartilhando o
    mesmo ID.
- Tipos de usuário:
    o **Administrador:** Controle total para cadastrar cursos, aulas, gerir
       assinaturas, pagamentos, alunos.
    o **Aluno:** Acesso restrito para matrícula em cursos, visualização de
       aulas/conteúdos e gerenciamento das suas próprias informações e
       pagamentos.

### Quem Executa as Ações

- **Aluno autenticado:** Realiza suas próprias matrículas, consulta
    cursos/aulas disponíveis, realiza pagamentos e acessa materiais das aulas.
- **Administrador autenticado:** Realiza operações administrativas (criação e
    manutenção dos cursos, gestão financeira e monitoramento dos alunos).


## Casos de Uso Detalhados

### Cadastro de Curso

- **Ator:** Administrador
- **Pré-condição:** Autenticado
- **Fluxo Principal:**
    1. Administrador cadastra curso informando nome e conteúdo
       programático.
    2. Curso validado e salvo.
- **Fluxo Alternativo:** Dados inválidos geram mensagens de erro.
- **Pós-condição:** Curso disponível para matrícula.

### Cadastro de Aula

- **Ator:** Administrador
- **Pré-condição:** Curso existente
- **Fluxo Principal:**
    1. Administrador seleciona curso.
    2. Insere título, conteúdo da aula e material (se houver).
    3. Aula validada e vinculada ao curso.
- **Fluxo Alternativo:** Dados inválidos retornam erros.
- **Pós-condição:** Aula associada ao curso.


### Matrícula do Aluno

- **Ator:** Aluno
- **Pré-condição:** Aluno autenticado, curso disponível
- **Fluxo Principal:**
    1. Seleciona curso e inicia matrícula.
    2. Matrícula criada com status pendente de pagamento.
- **Fluxo Alternativo:** Erro na matrícula informado.
- **Pós-condição:** Matrícula criada e aguardando pagamento.

### Realização do Pagamento

- **Ator:** Aluno
- **Pré-condição:** Matrícula pendente
- **Fluxo Principal:**
    1. Aluno realiza pagamento informando dados do cartão.
    2. Pagamento confirmado altera status da matrícula para ativa.
- **Fluxo Alternativo:** Pagamento rejeitado notifica aluno.
- **Pós-condição:** Pagamento registrado e matrícula ativada.

### Realização da Aula

- **Ator:** Aluno
- **Pré-condição:** Matrícula ativa
- **Fluxo Principal:**
    1. Acessa aula e realiza estudo.
    2. Progresso registrado automaticamente.
- **Fluxo Alternativo:** Problema de acesso notifica aluno.
- **Pós-condição:** Aula realizada e progresso registrado.


### Finalização do Curso.....................................................................................

- **Ator:** Aluno
- **Pré-condição:** Todas as aulas concluídas
- **Fluxo Principal:**
    1. Solicita finalização.
    2. Matrícula atualizada para status concluído e certificado gerado.
- **Fluxo Alternativo:** Aulas incompletas impedem finalização.
- **Pós-condição:** Certificado gerado e disponível.

## Requisitos Técnicos

- Linguagem: C#
- Backend: ASP.NET Core WebAPI
- Autenticação: JWT com ASP.NET Core Identity
- Banco de Dados: SQL Server e SQLite com EF Core (o uso de SQLite deve
    estar sempre configurado com o Seed para que qualquer avaliador do
    projeto possa executar sem a infra do BD).
- Documentação: Swagger
- Controle de Versão: Github
- Testes:
    o Testes de unidades (via TDD) com cobertura mínima de 80%
    o Testes de integração completos simulando todos processos críticos
       (casos de uso).


## Critérios de Sucesso

- Implementação correta dos bounded contexts e relações.
- Funcionalidades completas e operando dentro dos requisitos
- Código claro e coeso conforme práticas DDD, TDD e CQRS.
- Rodar todos os testes (e passar)
- Cobertura de testes de 80%
- Segurança robusta via autenticação JWT.
- Configuração ( **obrigatório** ):
    o O projeto deve rodar com a menor configuração de infra possível,
       para isso utilize a prática ensinada no vídeo a seguir:
       https://desenvolvedor.io/plus/criando-e-populando-
       automaticamente-qualquer-banco-de-dados