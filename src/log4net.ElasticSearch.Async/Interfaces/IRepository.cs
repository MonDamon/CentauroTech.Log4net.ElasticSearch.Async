namespace CentauroTech.Log4net.ElasticSearch.Async.Interfaces
{
    using System.Collections.Generic;

    using CentauroTech.Log4net.ElasticSearch.Async.Models;

    internal interface IRepository
    {
        void Add(IList<logEvent> logEvents);
    }
}