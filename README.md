# Pipeline Pattern Implementation

Projeto backend desenvolvido em **ASP.NET Core / .NET 8**, com processamento de pedidos organizado em um pipeline de etapas: validação, pagamento e envio.

A aplicação separa regras de negócio, casos de uso e integrações por meio de portas e adapters. O acesso ao SQL Server utiliza Entity Framework Core e Unit of Work para coordenar transações.

## Estrutura

- `Domain`: modelos, regras de negócio, interfaces, UseCases e pipeline.
- `Adapter`: endpoints HTTP, DTOs, mappers e implementações de acesso ao banco.
- `Infra`: configurações de injeção de dependência, banco, HTTP e Swagger.
- Projeto de testes: testes unitários com xUnit e Moq, além de testes de integração com SQL Server temporário via Testcontainers.

## Pré-requisitos

- SDK do .NET 8.
- SQL Server acessível para executar a aplicação.
- Docker em execução, configurado para containers Linux, para executar os testes de integração com Testcontainers.

Os testes unitários não precisam de SQL Server nem de Docker.

## Configuração do projeto principal

Na raiz do projeto principal, ao lado do arquivo `.csproj`, crie `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DBConnection": {
    "Cluster": "<SERVIDOR_SQL_SERVER>",
    "Username": "<USUARIO_SQL_SERVER>",
    "Password": "<SENHA_CRIPTOGRAFADA>",
    "Database": "<NOME_DO_BANCO>",
    "Chave": "<CHAVE_DE_CRIPTOGRAFIA>",
    "CommandTimeout": 30,
    "ConnectTimeout": 10,
    "ConnectionPool": 0,
    "IsTraceExecActive": false
  }
}
```

Substitua os valores indicados pelas configurações do seu ambiente. Os números e o booleano são valores iniciais de exemplo: ajuste-os mantendo os tipos JSON.

| Campo | Descrição |
|---|---|
| `Cluster` | Endereço ou instância do SQL Server. Inclua a porta quando necessário. |
| `Username` | Usuário utilizado na conexão. |
| `Password` | Senha criptografada em formato compatível com `Crypt.decryptDES`. Não informe a senha em texto puro. |
| `Database` | Nome do banco da aplicação. |
| `Chave` | Chave correspondente à usada para criptografar a senha. |
| `CommandTimeout` | Timeout dos comandos SQL, em segundos. |
| `ConnectTimeout` | Timeout de abertura da conexão, em segundos. |
| `ConnectionPool` | Valor de `Max Pool Size` quando maior que zero. Com `0`, essa opção não é adicionada à connection string. |
| `IsTraceExecActive` | Indicador de rastreamento conforme o suporte implementado na aplicação. Use `true` ou `false`. |

Gere a senha com o mecanismo correspondente ao utilizado pelo projeto. Para o carregamento padrão desse arquivo na aplicação, execute no ambiente `Development`.

## Configuração do projeto de testes

Na raiz do projeto de testes, ao lado do respectivo `.csproj`, crie outro `appsettings.Development.json`. Ele é independente do arquivo do projeto principal:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "SqlServerTests": {
    "Image": "<IMAGEM_SQL_SERVER:TAG>",
    "Database": "<NOME_DO_BANCO_TEMPORARIO>",
    "CommandTimeout": 30
  }
}
```

| Campo | Descrição |
|---|---|
| `Image` | Imagem Docker do SQL Server e sua tag, compatíveis com o ambiente de testes. |
| `Database` | Nome do banco criado dentro do container temporário. |
| `CommandTimeout` | Timeout dos comandos SQL dos testes, em segundos. |

Não informe credenciais do banco de desenvolvimento: o helper obtém a connection string do container iniciado pelo Testcontainers. O helper carrega explicitamente esse JSON e utiliza apenas a seção `SqlServerTests`; `Logging` pode ser mantida para a configuração dos testes que a utilizarem.

### Copiar o arquivo para a saída dos testes

Adicione ao `.csproj` do projeto de testes:

```xml
<ItemGroup>
  <None Update="appsettings.Development.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

## Executar a aplicação

Após preencher a configuração:

```bash
dotnet restore
dotnet run --project <CAMINHO_DO_PROJETO_PRINCIPAL.csproj>
```

O banco deve estar acessível e possuir o schema necessário. O Unit of Work não cria as tabelas da aplicação automaticamente.

## Executar os testes

Todos:

```bash
dotnet test
```

Somente unitários:

```bash
dotnet test --filter "Category=Unit"
```

Somente integração:

```bash
dotnet test --filter "Category=Integration"
```

Nos testes transacionais, o helper inicia um SQL Server temporário, cria as tabelas necessárias e remove o container com o banco ao finalizar a fixture. A primeira execução pode demorar mais devido ao download da imagem.

A imagem pode permanecer em cache. Encerramentos forçados ou indisponibilidade do Docker podem impedir a remoção imediata do container.

## Cuidados com as configurações

- Substitua todos os valores entre `<...>` antes de executar.
- Mantenha números sem aspas e booleanos como `true` ou `false`.
- Não adicione comentários aos exemplos JSON.
- Não versione arquivos preenchidos com credenciais, senhas ou chaves reais.
- Prefira versionar modelos sem dados sensíveis, como `appsettings.Development.example.json`.
