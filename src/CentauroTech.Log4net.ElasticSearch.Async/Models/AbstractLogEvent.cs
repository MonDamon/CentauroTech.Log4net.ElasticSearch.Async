namespace CentauroTech.Log4net.ElasticSearch.Async.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using log4net.Core;
    using CentauroTech.Log4net.ElasticSearch.Async.Infrastructure;

    /// <summary>
    /// Primary object which will get serialized into a json object to pass to ES. Deviating from CamelCase
    /// class members so that we can stick with the build in serializer and not take a dependency on another lib. ES
    /// expects fields to start with lowercase letters.
    /// </summary>
    internal abstract class AbstractLogEvent
    {
          public virtual IList<logEvent> CreateMany(
            IEnumerable<LoggingEvent> loggingEvents,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
            return loggingEvents.Select(@event => Create(@event, machineDataProvider, errorHandler)).ToArray();
        }

        public abstract  logEvent Create(
            LoggingEvent loggingEvent,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler);
      

         public virtual void AddProperties(LoggingEvent loggingEvent, logEvent logEvent) => loggingEvent.Properties().Union(AppenderPropertiesFor(loggingEvent)).Do(pair => logEvent.properties.Add(pair));

        public virtual IEnumerable<KeyValuePair<string, string>> AppenderPropertiesFor(LoggingEvent loggingEvent)
        {
            yield return Pair.For("@timestamp", loggingEvent.TimeStamp.ToUniversalTime().ToString("O"));
        }
      
    }
}