using System;
using Microsoft.Extensions.Logging;

namespace Luger.LoggerProvider
{
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Adds ILoggingProvider that implements sending logs to Luger server
        /// Internally calls ILoggingBuilder.AddProvider(new LugerLogProvider(config))
        /// </summary>
        /// <param name="self"></param>
        /// <param name="config">Luger config</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static ILoggingBuilder AddLuger(this ILoggingBuilder self, Action<LugerLogOptions> config) 
        {
            var configuration = new LugerLogOptions();
            config(configuration);
            if (string.IsNullOrEmpty(configuration.Bucket)) throw new ArgumentNullException(nameof(configuration.Bucket));

            if (configuration.LugerUrl is null)
            {
                throw new InvalidOperationException($"{nameof(LugerLogOptions.LugerUrl)} cannot be empty");
            }

            self.AddProvider(new LugerLogProvider(configuration));

            return self;
        }
    }
}
