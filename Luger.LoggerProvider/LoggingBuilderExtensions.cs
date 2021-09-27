using System;
using Microsoft.Extensions.Logging;

namespace Luger.LoggerProvider
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggingBuilder AddLuger(this ILoggingBuilder self, Action<LugerLogOptions> config) 
        {
            var configuration = new LugerLogOptions();
            config(configuration);

            if (configuration.LugerUrl is null)
            {
                throw new InvalidOperationException($"{nameof(LugerLogOptions.LugerUrl)} cannot be empty");
            }

            self.AddProvider(new LugerLogProvider(configuration));

            return self;
        }
    }
}
