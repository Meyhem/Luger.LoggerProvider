using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luger.LoggerProvider
{
    internal  class LogRecord
    {
        public const string MessageField = "@m";
        public const string LevelField = "@l";
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName(LevelField)]
        public LogLevel Level { get; set; }

        [JsonPropertyName(MessageField)]
        public string Message { get; set; } = default!;
        
        [JsonExtensionData]
        public Dictionary<string, object> Labels { get; set; } = new();
    }
}
