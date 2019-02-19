namespace CentauroTech.Log4net.ElasticSearch.Async.Tests.UnitTests.Stubs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using log4net.Core;

    public class ErrorHandlerStub : IErrorHandler
    {
        readonly ConcurrentBag<Exception> exceptions;
        readonly ConcurrentBag<string> messages; 

        public ErrorHandlerStub()
        {
            this.exceptions = new ConcurrentBag<Exception>();
            this.messages = new ConcurrentBag<string>();
        }

        public IEnumerable<Exception> Exceptions
        {
            get { return this.exceptions; }
        }

        public IEnumerable<string> Messages
        {
            get { return this.messages; }
        }

        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            this.Error(message, e);
        }

        public void Error(string message, Exception e)
        {
            this.exceptions.Add(e);
            this.messages.Add(message);
        }

        public void Error(string message)
        {
            this.messages.Add(message);
        }
    }
}