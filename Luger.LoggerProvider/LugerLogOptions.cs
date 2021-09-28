using System;
using System.Net;

namespace Luger.LoggerProvider
{
    public class LugerLogOptions
    {
        /// <summary>
        /// Uri to be used by HttpClient to connect to Luger API
        /// </summary>
        public Uri LugerUrl { get; set; } = default!;
        
        /// <summary>
        /// Bucket identifier to be used to store logs
        /// </summary>
        public string Bucket { get; set; } = default!;
        
        /// <summary>
        /// Timespan interval how often the logs should be flushed to Luger server
        /// </summary>
        public TimeSpan BatchPostInterval { get; set; } = TimeSpan.FromSeconds(3);
        
        /// <summary>
        /// HttpClient pooling option
        /// </summary>
        public int MaxConnectionsPerServer { get; set; } = 3;
        
        /// <summary>
        /// HttpClient proxy options
        /// </summary>
        public IWebProxy? Proxy { get; set; }
        
        /// <summary>
        /// Indicater whether use proxy, <see cref="Proxy"/>
        /// </summary>
        public bool UseProxy { get; set; }
    }
}
