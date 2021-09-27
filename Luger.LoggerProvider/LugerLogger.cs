using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luger.LoggerProvider
{
    using Label = KeyValuePair<string, string>;

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

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
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
            if (!string.IsNullOrEmpty(category)) postableLabels["category"] = category;
            
            batchPoster.AddLog(new LogRecord
            {
                Level = logLevel,
                Message = formatter(state, exception),
                Labels = postableLabels
            });
        }

        private IEnumerable<Label> CreateLogLabels(IEnumerable<object> states)
        {
            return states.SelectMany(ExtractLabelsFromState)
                .Select(NormalizeLabel)
                .Where(l => !UndesiredLabels.Contains(l.Key));
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
    }
}