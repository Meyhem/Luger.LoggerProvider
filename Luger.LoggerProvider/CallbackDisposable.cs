using System;

namespace Luger.LoggerProvider
{
    internal class CallbackDisposable: IDisposable
    {
        private readonly Action callback;

        public CallbackDisposable(Action callback)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public static CallbackDisposable Noop => new(() => { });

        public void Dispose()
        {
            callback();
        }
    }
}
