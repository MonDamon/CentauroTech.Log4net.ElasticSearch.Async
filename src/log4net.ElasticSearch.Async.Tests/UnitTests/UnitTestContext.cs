namespace log4net.ElasticSearch.Async.Tests.UnitTests
{
    using System;

    using log4net.ElasticSearch.Async;
    using log4net.ElasticSearch.Async.Tests.UnitTests.Stubs;

    internal class UnitTestContext : IDisposable
    {
        const string ConnectionString = "Server=localhost;Index=log_test;Port=9200;rolling=true";
        const int BufferSize = 100;

        public ElasticSearchAsyncAppender Appender { get; private set; }

        public RepositoryStub Repository { get; private set; }

        public ErrorHandlerStub ErrorHandler { get; private set; }

        public void Dispose()
        {
            if (this.Appender == null) return;

            try
            {
                this.Appender.Close();
            }
            catch{}
        }

        public static UnitTestContext Create(int bufferSize = BufferSize, bool? failSend = null, bool? failClose = null)
        {
            var repository = new RepositoryStub();
            var errorHandler = new ErrorHandlerStub();

            var appender = new TestableAppender(repository)
                {
                    ConnectionString = ConnectionString, 
                    ErrorHandler = errorHandler, 
                    FailSend = failSend, 
                    FailClose = failClose,
                    MaxRetries = 0
                };

            appender.ActivateOptions();

            return new UnitTestContext
                {
                    Repository = repository,
                    ErrorHandler = errorHandler,
                    Appender = appender
                };
        }
    }
}