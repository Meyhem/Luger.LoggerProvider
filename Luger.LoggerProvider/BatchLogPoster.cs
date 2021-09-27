using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Luger.LoggerProvider
{
    internal class BatchLogPoster : IDisposable, IAsyncDisposable
    {
        private readonly LugerLogOptions config;
        private readonly HttpClient httpClient;
        private readonly ConcurrentQueue<LogRecord> queue;
        private readonly object thisLock;
        private readonly Task backgroundTask;
        private readonly CancellationTokenSource cancellationTokenSource;

        public BatchLogPoster(LugerLogOptions config, HttpClient httpClient)
        {
            this.config = config;
            this.httpClient = httpClient;
            
            thisLock = new object();
            queue = new ConcurrentQueue<LogRecord>();
            
            cancellationTokenSource = new CancellationTokenSource();
            backgroundTask = BatchPostingBackgroundTask(cancellationTokenSource.Token);
        }

        public void AddLog(LogRecord log)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            queue.Enqueue(log);
        }

        public async ValueTask DisposeAsync()
        {
            cancellationTokenSource.Cancel();
            await backgroundTask;
        }
        
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        private async Task BatchPostingBackgroundTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(
                        TimeSpan.FromMilliseconds(config.BatchPostInterval.TotalMilliseconds),
                        cancellationToken
                    );

                    LogRecord[] logs;
                    lock (thisLock)
                    {
                        logs = queue.ToArray();
                        queue.Clear();
                    }

                    if (!logs.Any()) return;

                    await httpClient.PostAsync(
                        $"/api/collect/{config.Bucket}",
                        JsonContent.Create(logs),
                        cancellationToken
                    );
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // silencing exception ?
                }
            }
        }


    }
}