using Infra.Configuration;
using Infra.Configuration.InBound;
using Infra.Configuration.OutBound;

var builder = WebApplication.CreateBuilder(args);

// dependências da aplicação, pipeline e stages.
builder.Services.AddDomainServices();

// configurações do SQL, conexão, contexto transacional e unit of work.
builder.Services.AddSqlConfig(builder.Configuration);

// documentação da API.
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// endpoints HTTP.
app.AddHttpConfig();

// swagger disponível fora de produção.
app.MapSwagger();

app.Run();