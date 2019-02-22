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
    /// exepects fields to start with lowercase letters.
    /// </summary>
    internal class logEventPTBR:logEvent
    {
     
        public string horario { get {return base.timeStamp;}  set{;} }

        public string mensagem { get{return base.message;} set{;} }

        public object objeto { get{return base.messageObject;} set{;} }

        public object excecao { get{return base.exception;} set{;} }

        public string nomeLog { get; set; }

        public string dominio { get; set; }

        public string identidade { get; set; }

        public string nivel { get; set; }

        public string nomeClasse { get; set; }

        public string nomeArquivo { get; set; }

        public string numeroLinha { get; set; }

        public string informacaoCompleta { get; set; }

        public string nomeMetodo { get; set; }

        public IDictionary<string, string> propriedades { get; set; }

        public string nomeUsuario { get; set; }

        public string nomeThread { get; set; }

        public string nomeHost { get; set; }

        public string ipMaquina { get; set; }

         public static IList<logEvent> CreateMany(
            IEnumerable<LoggingEvent> loggingEvents,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
            return loggingEvents.Select(@event => Create(@event, machineDataProvider, errorHandler)).ToArray();
        }

        protected static logEvent Create(
            LoggingEvent loggingEvent,
            MachineDataProvider machineDataProvider,
            Action<string, Exception> errorHandler)
        {
            var logEvent = new logEventPTBR
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
                if (logEvent.domain == log4net.Util.SystemInfo.NotAvailableText)
                {
                    logEvent.domain = Assembly.GetEntryAssembly().GetName().Name;
                }

                if (logEvent.userName == log4net.Util.SystemInfo.NotAvailableText)
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

        protected static void AddProperties(LoggingEvent loggingEvent, logEvent logEvent) => loggingEvent.Properties().Union(AppenderPropertiesFor(loggingEvent)).Do(pair => logEvent.properties.Add(pair));

        protected static IEnumerable<KeyValuePair<string, string>> AppenderPropertiesFor(LoggingEvent loggingEvent)
        {
            yield return Pair.For("@timestamp", loggingEvent.TimeStamp.ToUniversalTime().ToString("O"));
        }
        
    }
}