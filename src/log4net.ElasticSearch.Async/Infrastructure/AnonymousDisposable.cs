namespace log4net.ElasticSearch.Async.Infrastructure
{
    using System;

    internal class AnonymousDisposable : IDisposable
    {
        readonly Action action;

        public AnonymousDisposable(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            this.action();
        }
    }
}