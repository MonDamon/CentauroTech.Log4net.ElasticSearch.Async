namespace log4net.ElasticSearch.Async
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using log4net.Core;
    using log4net.ElasticSearch.Async.Infrastructure;
    using log4net.ElasticSearch.Async.Interfaces;
    using log4net.ElasticSearch.Async.Models;

    /// <summary>ElasticSearch async/background appender.</summary>
    public class ElasticSearchAsyncAppender : AsyncBulkAppenderSkeleton
    {
        /// <summary>Appender type.</summary>
        private static readonly string AppenderType = typeof(ElasticSearchAsyncAppender).Name;

        /// <summary>ElasticSearch data repository.</summary>
        private IRepository repository;

        /// <summary>Log event machine data provider.</summary>
        private MachineDataProvider machineDataProvider;
        
        /// <summary>Initializes a new instance of the <see cref="ElasticSearchAsyncAppender"/> class.</summary>
        public ElasticSearchAsyncAppender()
        {
            this.OnCloseTimeout = TimeSpan.FromSeconds(30);
            this.MaxRetries = 10;
            this.RetrySeedDelay = TimeSpan.FromSeconds(5);
            this.RetryMaxDelay = TimeSpan.FromMinutes(5);
            this.ExternalIpCheckAddress = NetworkDataProvider.DefaultExternalIpCheckAddress;
        }

        /// <summary>Initializes a new instance of the <see cref="ElasticSearchAsyncAppender"/> class. Used for mocking UT</summary>
        /// <param name="mockRepository">Mock repository.</param>
        internal ElasticSearchAsyncAppender(IRepository mockRepository)
            : this()
        {
            this.repository = mockRepository;
        }

        /// <summary>Gets or sets the connection string.</summary>
        public string ConnectionString { get; set; }

        /// <summary>Gets or sets the max retries for ElasticSearch connection (exponential back-off with jitter).</summary>
        public int MaxRetries { get; set; }

        /// <summary>Gets or sets the initial and minimum retry delay.</summary>
        public TimeSpan RetrySeedDelay { get; set; }

        /// <summary>Gets or sets the maximum retry delay.</summary>
        public TimeSpan RetryMaxDelay { get; set; }

        /// <summary>Gets or sets the address for checking machine external IP.</summary>
        public string ExternalIpCheckAddress { get; set; }
        
        /// <summary>Activates appender based on options.</summary>
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            ServicePointManager.Expect100Continue = false;

            try
            {
                Validate(this.ConnectionString);
            }
            catch (Exception ex)
            {
                this.HandleError("Failed to validate ConnectionString in ActivateOptions", ex);
                return;
            }
            
            var options = new RequestOptions(this.ConnectionString.ConnectionStringParts());
            this.ProcessOptions(options);
            this.InitializeProviders(options);
        }

        /// <summary>Appends bulk portion of logging events</summary>
        /// <param name="loggingEvents">Logging events.</param>
        protected override void AppendBulk(IList<LoggingEvent> loggingEvents)
        {
            try
            {
                RetryPolicy.CreateDecorrelatedJitterPolicy(
                        this.MaxRetries,
                        this.RetrySeedDelay,
                        this.RetryMaxDelay,
                        (exception, retryDelay) => this.ErrorHandler.Error($"Adding logEvents to {this.repository.GetType().Name} will be retried after {retryDelay}"))
                    .Execute(() =>
                        {
                            var events = logEvent.CreateMany(loggingEvents, this.machineDataProvider, this.HandleError);
                            this.repository.Add(events);
                        });
            }
            catch (Exception ex)
            {
                this.HandleError("Failed to add logEvents to {0} in AppendBulk".With(this.repository.GetType().Name), ex);
            }
        }

        /// <summary>Validates ElasticSearch connection string.</summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        private static void Validate(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (connectionString.Length == 0)
            {
                throw new ArgumentException("connectionString is empty", nameof(connectionString));
            }
        }

        /// <summary>Processes request options during initialization.</summary>
        /// <param name="options">Request options.</param>
        private void ProcessOptions(RequestOptions options)
        {
            if (options.SkipCertificateValidation)
            {
                try
                { 
#if NETSTANDARD || NETSTANDARD2_0
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls  | SecurityProtocolType.Tls11
                                                                                     | SecurityProtocolType.Tls12;
#elif NET45
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls
                                                                                     | SecurityProtocolType.Tls11
                                                                                     | SecurityProtocolType.Tls12;
#else
                    ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls
                                                                                     | (SecurityProtocolType)768
                                                                                     | (SecurityProtocolType)3072;
#endif
                }
                catch (Exception ex)
                {
                    this.HandleError("Failed to set up SkipCertificateValidation {0} while processing options for appender".With(this.repository.GetType().Name), ex);
                }
            }

            if (options.HttpDefaultConnectionLimit.HasValue)
            {
                ServicePointManager.DefaultConnectionLimit = options.HttpDefaultConnectionLimit.Value;
            }

            if (options.SkipProxy)
            {
                WebRequest.DefaultWebProxy = null;
            }
            else if (options.HttpProxy != null)
            {
                var proxyUri = new Uri(options.HttpProxy);
                WebRequest.DefaultWebProxy = new WebProxy(proxyUri);
            }
        }

        /// <summary>Initializes providers and repositories of data.</summary>
        /// <param name="options">Additional request options.</param>
        private void InitializeProviders(RequestOptions options)
        {
            if (this.repository == null)
            {
                this.repository = Repository.Create(this.ConnectionString, options);
            }
            
            var networkDataProvider = new NetworkDataProvider(this.ErrorHandler);
            this.machineDataProvider = new MachineDataProvider
                                           {
                                               MachineExternalIp = networkDataProvider.GetMachineIp(this.ExternalIpCheckAddress)
                                           };
        }

        /// <summary>Handles error using log4net internal logger.</summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        private void HandleError(string message, Exception ex)
        {
            this.ErrorHandler.Error("{0} [{1}]: {2}.".With(AppenderType, this.Name, message), ex, ErrorCode.GenericFailure);
        }
    }    
}
