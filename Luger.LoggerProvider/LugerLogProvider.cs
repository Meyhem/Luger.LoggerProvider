using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Luger.LoggerProvider
{
    public class LugerLogProvider : ILoggerProvider
    {
        private readonly HttpClientHandler clientHandler;
        private readonly LugerLogOptions config;
        private readonly BatchLogPoster batchPoster;

        public LugerLogProvider(LugerLogOptions config)
        {
            if (config.LugerUrl is null) throw new ArgumentNullException($"{nameof(LugerLogOptions)}.{nameof(LugerLogOptions.LugerUrl)} is null");
            if (config.Bucket is null) throw new ArgumentNullException($"{nameof(LugerLogOptions)}.{nameof(LugerLogOptions.Bucket)} is null");

            this.config = config;

            clientHandler = new HttpClientHandler
            {
                MaxConnectionsPerServer = config.MaxConnectionsPerServer,
                Proxy = config.Proxy,
                UseProxy = config.UseProxy
            };
            batchPoster = new BatchLogPoster(config, new HttpClient(clientHandler)
            {
                BaseAddress = config.LugerUrl
            });
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LugerLogger(batchPoster, categoryName);
        }

        public void Dispose()
        {
            clientHandler.Dispose();
            batchPoster.Dispose();
        }
    }
}
