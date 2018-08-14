namespace log4net.ElasticSearch.Async.Tests.UnitTests.Stubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using log4net.ElasticSearch.Async.Interfaces;
    using log4net.ElasticSearch.Async.Models;

    internal class RepositoryStub : IRepository
    {
        readonly ConcurrentBag<IEnumerable<logEvent>> logEntries;
        readonly ConcurrentDictionary<int, IEnumerable<logEvent>> logEntriesByThread;
        Exception exception;

        public RepositoryStub()
        {
            this.logEntries = new ConcurrentBag<IEnumerable<logEvent>>();
            this.logEntriesByThread = new ConcurrentDictionary<int, IEnumerable<logEvent>>();
        }

        public void Add(IList<logEvent> logEvents)
        {
            if (this.exception != null)
            {
                throw this.exception;
            }

            var entries = logEvents.ToArray();
            this.logEntries.Add(entries);
            this.logEntriesByThread.AddOrUpdate(Thread.CurrentThread.ManagedThreadId, i => entries,
                                           (i, events) => events.Union(entries));
        }

        public void OnAddThrow<TException>() where TException : Exception, new()
        {
            this.OnAddThrow(new TException());
        }

        public void OnAddThrow(Exception ex)
        {
            this.exception = ex;
        }

        public IEnumerable<IEnumerable<logEvent>> LogEntries { get{ return this.logEntries; } }
        public IEnumerable<KeyValuePair<int, IEnumerable<logEvent>>> LogEntriesByThread { get { return this.logEntriesByThread; } }        
    }
}