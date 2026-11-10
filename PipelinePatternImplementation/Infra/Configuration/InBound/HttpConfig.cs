using Adapter.InBound.HttpAdapter.Ports;

namespace Infra.Configuration.InBound
{
    public static class HttpConfig
    {
        public static WebApplication AddHttpConfig(
            this WebApplication app)
        {
            app.MapTransacoesEndpoint();

            return app;
        }
    }
}