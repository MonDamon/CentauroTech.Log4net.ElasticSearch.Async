namespace log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net.Appender;
    using log4net.Core;

    /// <summary>
    /// Asynchronous appender wrapper skeleton for log4net with possibility to generate bulk packages of logging events
    /// </summary>
    public abstract class AsyncBulkAppenderSkeleton : AppenderSkeleton
    {
        /// <summary>Default size of buffer on which events will be flushed to output.</summary>
        private const int DefaultFlushTriggerBuggerSize = 256;

        /// <summary>Collection of queued logging events (producer-consumer)</summary>
        private readonly BlockingCollection<LoggingEvent> eventsQueue;

        /// <summary>Event triggered when all events have been processed and queue has been closed.</summary>
        private readonly ManualResetEvent addingCompletedEvent;

        /// <summary>Cancellation token source for closing event queue.</summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncBulkAppenderSkeleton"/> class.
        /// </summary>
        protected AsyncBulkAppenderSkeleton()
        {
            this.eventsQueue = new BlockingCollection<LoggingEvent>();
            this.addingCompletedEvent = new ManualResetEvent(false);
            this.cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(this.LoggingWorker, TaskCreationOptions.LongRunning);
        }

        /// <summary>Gets or sets the on close timeout.</summary>
        public abstract TimeSpan OnCloseTimeout { get; set; }

        /// <summary>
        /// Appends a new message to the asynchronous buffer
        /// </summary>
        /// <param name="loggingEvent">Logging event with the message</param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                loggingEvent.Fix = FixFlags.All;
                if (!this.eventsQueue.IsAddingCompleted)
                {
                    this.eventsQueue.TryAdd(loggingEvent);
                }
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Exception occurred in AsyncAppender wrapper", ex);
            }
        }

        /// <summary>
        /// Action invoked when closing the logger
        /// </summary>
        protected override void OnClose()
        {
            this.eventsQueue.CompleteAdding();
            this.addingCompletedEvent.WaitOne(this.OnCloseTimeout);
            this.cancellationTokenSource.Cancel();
            base.OnClose();
        }

        /// <summary>
        /// Method to be invoked asynchronously which is responsible for logging (provides a collection of events to be logged)
        /// </summary>
        /// <param name="loggingEvents">Logging event with the message</param>
        protected abstract void AppendBulk(IList<LoggingEvent> loggingEvents);

        /// <summary>
        /// Worker method which acts as consumer thread for the message queue
        /// </summary>
        private void LoggingWorker()
        {
            try
            {
                var buffer = new List<LoggingEvent>();
                foreach (var loggingEvent in this.eventsQueue.GetConsumingEnumerable(this.cancellationTokenSource.Token))
                {
                    buffer.Add(loggingEvent);
                    if (this.eventsQueue.Count == 0 || buffer.Count >= DefaultFlushTriggerBuggerSize)
                    {
                        try
                        {
                            this.AppendBulk(buffer);
                        }
                        catch (Exception ex)
                        {
                            this.ErrorHandler.Error("Exception occurred in AsyncBulkAppenderSkeleton wrapper while adding current batch of events.", ex);
                        }
                        finally
                        {
                            buffer.Clear();
                        }
                    }
                }

                this.addingCompletedEvent.Set();
            }
            catch (Exception ex)
            {
                this.ErrorHandler.Error("Exception occurred in AsyncBulkAppenderSkeleton main worker loop. No more events will be processed.", ex);
            }
        }
    }
}
