using Infra.Configuration;
using Infra.Configuration.InBound;

var builder = WebApplication.CreateBuilder(args);

// dependências da aplicação, pipeline e stages.
builder.Services.AddDomainServices();

// documentação da API.
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// endpoints HTTP.
app.AddHttpConfig();

// swagger disponível fora de produção.
app.MapSwagger();

app.Run();