%%{init: {'theme': 'dark'}}%%
graph TD
    %% Configurações globais
    classDef title fill:#1a237e,color:#ffffff,stroke:none,font-size:20px
    classDef bc fill:#1e1e1e,stroke:#aaa,stroke-width:1.5px,color:#90caf9
    classDef entity fill:#2c2c2c,stroke:#90caf9,stroke-width:1px,color:#ffffff
    classDef valueobject fill:#4a4a4a,stroke:#90caf9,stroke-width:1px,color:#ffffff
    classDef relationship fill:#333,stroke:#888,stroke-dasharray:5

    %% Bounded Contexts
    subgraph BC1["Gestão de Conteúdo"]
        direction TB
        style BC1 fill:#2e7d32,stroke:#81c784,color:#ffffff
        C1(("Curso")):::entity
        A1["Aula"]:::entity
        CC1["Conteúdo do Curso"]:::valueobject
        C1 --- A1
        C1 --- CC1
    end

    subgraph BC2["Gestão de Alunos"]
        direction TB
        style BC2 fill:#ef6c00,stroke:#ffb74d,color:#ffffff
        AL(("Aluno")):::entity
        MT[Matrícula]:::entity
        CT[Certificado]:::entity
        HA[Histórico de Aprendizado]:::entity
        PA[Progresso do Curso]:::entity
        AL -->|realiza| MT
        MT -->|gera| CT
        AL -->|possui| HA
        HA -->|contém| PA
    end

    subgraph BC3["Processamento de Pagamento"]
        direction LR
        style BC3 fill:#c62828,stroke:#ef9a9a,color:#ffffff
        PG(("Pagamento")):::entity
        DC["Dados do Cartão"]:::valueobject
        SP["Status do Pagamento"]:::valueobject
        PG -->|contém| DC
        PG -->|possui| SP
    end

    %% Relacionamentos entre BCs
    BC1 --o|"Disponibiliza cursos"| BC2
    BC2 --o|"Gera cobranças"| BC3
    BC3 --o|"Notifica status"| BC2

    %% Elementos auxiliares
    class T title
    linkStyle 0,1 stroke:#90caf9,stroke-width:1.5px
    linkStyle 2,3,4,5 stroke:#ffb74d,stroke-width:1.5px
    linkStyle 6,7 stroke:#ef9a9a,stroke-width:1.5px