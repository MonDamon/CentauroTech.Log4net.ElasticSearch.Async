namespace log4net.ElasticSearch.Async.Interfaces
{
    using System.Collections.Generic;

    using log4net.ElasticSearch.Async.Models;

    internal interface IRepository
    {
        void Add(IList<logEvent> logEvents);
    }
}