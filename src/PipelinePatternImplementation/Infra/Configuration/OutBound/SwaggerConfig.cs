namespace Infra.Configuration.InBound
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerConfiguration(
            this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new()
                {
                    Title = "Order Processing API",
                    Version = "v1",
                    Description =
                        "API de processamento de pedidos com pipeline de etapas."
                });
            });

            return services;
        }

        public static WebApplication MapSwagger(
            this WebApplication app)
        {
            ArgumentNullException.ThrowIfNull(app);

            if (!app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }
    }
}
