graph TD
    %% Configurações globais
    classDef title fill:#1a237e,color:white,stroke:none,font-size:20px
    classDef bc fill:#f5f5f5,stroke:#333,stroke-width:1.5px,color:#1a237e
    classDef entity fill:#ffffff,stroke:#1a237e,stroke-width:1px,color:#1a237e
    classDef relationship fill:#fafafa,stroke:#666,stroke-dasharray:5

    %% Bounded Contexts
    subgraph BC1["Gestão de Conteúdo"]
        direction TB
        style BC1 fill:#e8f5e9,stroke:#2e7d32
        C1(("Curso")):::entity
        A1["▪ Aula - Vídeos Materiais"]:::entity
        CP["▫ Conteúdo Programático- Objetivos - Ementa"]:::entity
        C1 --- A1
        C1 --- CP
    end

    subgraph BC2["Gestão de Alunos"]
        direction TB
        style BC2 fill:#fff3e0,stroke:#ef6c00
        AL(("Aluno")):::entity
        MT[Matrícula]:::entity
        CT[Certificado]:::entity
        HA[Histórico Acadêmico]:::entity
        AL -->|realiza| MT
        MT -->|gera| CT
        MT -->|registra| HA
    end

    subgraph BC3["Pagamento"]
        direction LR
        style BC3 fill:#fbe9e7,stroke:#d32f2f
        PG(("Pagamento")):::entity
        DC[[Dados do Cartão]]:::entity
        SP{Status}:::entity
        PG -->|processa| DC
        PG -->|atualiza| SP
    end

    %% Relacionamentos entre BCs
    BC1 --o|"Disponibiliza cursos"| BC2
    BC2 --o|"Gera cobranças"| BC3
    BC3 --o|"Notifica status"| BC2

    %% Elementos auxiliares
    class T title
    linkStyle 0,1,2 stroke:#1a237e,stroke-width:1.5px
    linkStyle 3,4,5 stroke:#ef6c00,stroke-width:1.5px
    linkStyle 6,7,8 stroke:#d32f2f,stroke-width:1.5px