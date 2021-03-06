using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Luger.LoggerProvider
{
    using Label = KeyValuePair<string, string>;

    internal record SerializableException(string Message, string? Trace, SerializableException? InnerException);

    internal class LugerLogger : ILogger
    {
        private readonly IList<object> scopes;
        private readonly BatchLogPoster batchPoster;
        private readonly string category;
        private readonly string[] UndesiredLabels = {"OriginalFormat"};

        public LugerLogger(BatchLogPoster batchPoster, string category)
        {
            scopes = new List<object>();
            this.batchPoster = batchPoster;
            this.category = category;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state is null) return CallbackDisposable.Noop;

            scopes.Add(state);

            return new CallbackDisposable(() => scopes.Remove(state));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception, string> formatter
        )
        {
            IEnumerable<object> allLabels = scopes;
            if (state is not null)
            {
                allLabels = allLabels.Append(state);
            }

            var stringLabels = CreateLogLabels(allLabels);

            var postableLabels = new Dictionary<string, object>(
                stringLabels.Select(l => KeyValuePair.Create(l.Key, l.Value as object))
            );

            if (exception is not null)
            {
                postableLabels["@exception"] = JsonSerializer.Serialize(
                    MapException(exception),
                    new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        IgnoreNullValues = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                );
            }

            if (!string.IsNullOrEmpty(category)) postableLabels["@category"] = category;

            batchPoster.AddLog(new LogRecord
            {
                Level = logLevel,
                Message = formatter(state, exception!),
                Labels = postableLabels
            });
        }

        private IEnumerable<Label> CreateLogLabels(IEnumerable<object> states)
        {
            return states.SelectMany(ExtractLabelsFromState) // extract
                .Select(NormalizeLabel) // map
                .Where(l => !UndesiredLabels.Contains(l.Key)) // filter
                .GroupBy(l => l.Key) // deduplicate 
                .Select(g => g.First());
        }

        private IEnumerable<Label> ExtractLabelsFromState(object? state)
        {
            if (state is null) return Enumerable.Empty<Label>();

            if (state is IEnumerable<KeyValuePair<string, object>> labels)
            {
                return labels.Select(l =>
                    NormalizeLabel(
                        KeyValuePair.Create(l.Key, l.Value?.ToString() ?? string.Empty)));
            }

            return Enumerable.Empty<Label>();
        }

        private Label NormalizeLabel(Label l)
        {
            var key = (l.Key ?? string.Empty).Trim().Trim('{', '}');
            var value = (l.Value ?? string.Empty);

            return KeyValuePair.Create(key, value);
        }

        private SerializableException MapException(Exception ex)
        {
            return new SerializableException(
                Message: ex.Message,
                Trace: ex.StackTrace,
                InnerException: ex.InnerException is not null ? MapException(ex.InnerException) : null
            );
        }
    }
}