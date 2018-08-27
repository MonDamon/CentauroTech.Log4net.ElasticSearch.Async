namespace log4net.ElasticSearch.Async.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using log4net.Core;
    using log4net.ElasticSearch.Async.Infrastructure;

    /// <summary>
    /// Primary object which will get serialized into a json object to pass to ES. Deviating from CamelCase
    /// class members so that we can stick with the build in serializer and not take a dependency on another lib. ES
    /// exepects fields to start with lowercase letters.
    /// </summary>
    internal class logEvent
    {
        public logEvent()
        {
            this.properties = new Dictionary<string, string>();
        }

        public string timeStamp { get; set; }

        public string message { get; set; }

        public object messageObject { get; set; }

        public object exception { get; set; }

        public string loggerName { get; set; }

        public string domain { get; set; }

        public string identity { get; set; }

        public string level { get; set; }

        public string className { get; set; }

        public string fileName { get; set; }

        public string lineNumber { get; set; }

        public string fullInfo { get; set; }

        public string methodName { get; set; }

        public string fix { get; set; }

        public IDictionary<string, string> properties { get; set; }

        public string userName { get; set; }

        public string threadName { get; set; }

        public string hostName { get; set; }

        public string machineIp { get; set; }

        public static IList<logEvent> CreateMany(
            IEnumerable<LoggingEvent> loggingEvents,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
            return loggingEvents.Select(@event => Create(@event, machineDataProvider, errorHandler)).ToArray();
        }

        static logEvent Create(
            LoggingEvent loggingEvent,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
            var logEvent = new logEvent
            {
                loggerName = loggingEvent.LoggerName,
                domain = loggingEvent.Domain,
                identity = loggingEvent.Identity,
                threadName = loggingEvent.ThreadName,
                userName = loggingEvent.UserName,
                timeStamp = loggingEvent.TimeStamp.ToUniversalTime().ToString("O"),
                exception = loggingEvent.ExceptionObject == null ? new object() : JsonSerializableException.Create(loggingEvent.ExceptionObject),
                message = loggingEvent.RenderedMessage,
                fix = loggingEvent.Fix.ToString(),
                hostName = Environment.MachineName,
                level = loggingEvent.Level == null ? null : loggingEvent.Level.DisplayName,
                machineIp = machineDataProvider?.MachineExternalIp
            };

            try
            {
                if (logEvent.domain == Util.SystemInfo.NotAvailableText)
                {
                    logEvent.domain = Assembly.GetEntryAssembly().GetName().Name;
                }

                if (logEvent.userName == Util.SystemInfo.NotAvailableText)
                {
                    logEvent.userName = Environment.UserName;
                }
            }
            catch (Exception ex)
            {
                errorHandler("Exception occurred while adding properties to log event", ex);
            }

            // Added special handling of the MessageObject since it may be an exception. 
            // Exception Types require specialized serialization to prevent serialization exceptions.
            if (loggingEvent.MessageObject != null && loggingEvent.MessageObject.GetType() != typeof(string))
            {
                if (loggingEvent.MessageObject is Exception)
                {
                    logEvent.messageObject = JsonSerializableException.Create((Exception)loggingEvent.MessageObject);
                }
                else
                {
                    logEvent.messageObject = loggingEvent.MessageObject;
                }
            }
            else
            {
                logEvent.messageObject = new object();
            }

            if (loggingEvent.LocationInformation != null)
            {
                logEvent.className = loggingEvent.LocationInformation.ClassName;
                logEvent.fileName = loggingEvent.LocationInformation.FileName;
                logEvent.lineNumber = loggingEvent.LocationInformation.LineNumber;
                logEvent.fullInfo = loggingEvent.LocationInformation.FullInfo;
                logEvent.methodName = loggingEvent.LocationInformation.MethodName;
            }

            AddProperties(loggingEvent, logEvent);

            return logEvent;
        }
        
        static void AddProperties(LoggingEvent loggingEvent, logEvent logEvent)
        {
            loggingEvent.Properties().Union(AppenderPropertiesFor(loggingEvent)).Do(pair => logEvent.properties.Add(pair));
        }

        static IEnumerable<KeyValuePair<string, string>> AppenderPropertiesFor(LoggingEvent loggingEvent)
        {
            yield return Pair.For("@timestamp", loggingEvent.TimeStamp.ToUniversalTime().ToString("O"));
        }
    }
}