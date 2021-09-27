using System;
using System.Net;

namespace Luger.LoggerProvider
{
    public class LugerLogOptions
    {
        public Uri LugerUrl { get; set; } = default!;
        public string Bucket { get; set; } = default!;
        public TimeSpan BatchPostInterval { get; set; } = TimeSpan.FromSeconds(3);

        public int MaxConnectionsPerServer { get; set; } = 3;
        public IWebProxy? Proxy { get; set; }
        public bool UseProxy { get; set; }
    }
}
