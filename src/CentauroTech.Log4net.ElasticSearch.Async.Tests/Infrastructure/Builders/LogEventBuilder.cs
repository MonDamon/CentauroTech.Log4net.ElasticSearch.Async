namespace CentauroTech.Log4net.ElasticSearch.Async.Tests.Infrastructure.Builders
{
    using System;
    using System.Globalization;
    using System.Linq;

    internal class LogEventBuilder
    {
        string className;
        string domain;
        object exception;
        string fileName;
        string fix;
        string fullInfo;
        string identity;
        string level;
        string lineNumber;
        string message;
        string timeStamp;

        LogEventBuilder() {}

        public static LogEventBuilder Default
        {
            get { return new LogEventBuilder().WithDefaults(); }
        }

        public Models.logEvent LogEvent
        {
            get { return this; }
        }

        LogEventBuilder WithDefaults()
        {            
            this.className = Faker.Lorem.Words(1).Single();
            this.domain = Faker.Lorem.Words(1).Single();
            this.exception = new object();
            this.fileName = Faker.Lorem.Sentence(5);
            this.fix = Faker.Lorem.Words(1).Single();
            this.fullInfo = Faker.Lorem.Sentence(100);
            this.identity = Faker.Lorem.Sentence(2);
            this.level = Faker.RandomNumber.Next(10).ToString(CultureInfo.InvariantCulture);
            this.lineNumber = Faker.RandomNumber.Next(1000).ToString(CultureInfo.InvariantCulture);
            this.timeStamp = DateTime.UtcNow.ToString("O");
            this.message = Faker.Lorem.Sentence(20);

            return this;
        }

        public static implicit operator Models.logEvent(LogEventBuilder builder)
        {
            return new Models.logEvent
                {
                    className = builder.className,
                    domain = builder.domain,
                    exception = builder.exception,
                    fileName = builder.fileName,
                    fix = builder.fix,
                    fullInfo = builder.fullInfo,
                    identity = builder.identity,
                    level = builder.level,
                    lineNumber = builder.lineNumber,
                    timeStamp = builder.timeStamp.ToString(CultureInfo.InvariantCulture),
                    message = builder.message
                };
        }
    }
}