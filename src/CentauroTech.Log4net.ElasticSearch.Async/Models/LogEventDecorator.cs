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
    internal abstract class LogEventDecorator : AbstractLogEvent
    {
        protected AbstractLogEvent _logEvent ;

        public LogEventDecorator(AbstractLogEvent log)
        {
            this._logEvent = log;
        }

        public override  logEvent Create(
            LoggingEvent loggingEvent,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
           
            return _logEvent.Create(loggingEvent,machineDataProvider,errorHandler);
        }

    }
}