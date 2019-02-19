namespace CentauroTech.Log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net.Appender;
    using log4net.Core;
    using log4net.Util;

    /// <summary>
    /// Asynchronous appender wrapper skeleton for log4net with possibility to generate bulk packages of logging events
    /// </summary>
    public abstract class AsyncBulkAppenderSkeleton : AppenderSkeleton
    {
        /// <summary>Default maximum size of buffer on which events will be flushed to output instantly.</summary>
        private const int DefaultFlushTriggerBufferSize = 256;

        /// <summary>Default size of rolling buffer. Zero means that the buffer will have no upper bound</summary>
        private const int DefaultRollingBufferSize = 0;

        /// <summary>Event triggered when all events have been processed and queue has been closed.</summary>
        private readonly ManualResetEvent addingCompletedEvent;

        /// <summary>Cancellation token source for closing event queue.</summary>
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>Collection of queued logging events (producer-consumer)</summary>
        private BlockingCollection<LoggingEvent> eventsQueue;

        /// <summary>Initializes a new instance of the <see cref="AsyncBulkAppenderSkeleton"/> class.</summary>
        protected AsyncBulkAppenderSkeleton()
        {
            this.RollingBufferSize = DefaultRollingBufferSize;
            this.FlushTriggerBufferSize = DefaultFlushTriggerBufferSize;

            this.addingCompletedEvent = new ManualResetEvent(false);
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>Gets or sets the on close timeout.</summary>
        public TimeSpan OnCloseTimeout { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of rolling buffer of log events.
        /// If the size is exceeded, old events will be discarded.
        /// Default value (0) creates a buffer without upper bound
        /// </summary>
        public int RollingBufferSize { get; set; }

        /// <summary>Gets or sets the maximum size of buffer which will trigger flushing log events instantly.</summary>
        public int FlushTriggerBufferSize { get; set; }

        /// <inheritdoc />
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            this.eventsQueue = this.RollingBufferSize > 0 ? new BlockingCollection<LoggingEvent>(this.RollingBufferSize) : new BlockingCollection<LoggingEvent>();
            Task.Factory.StartNew(this.LoggingWorker, TaskCreationOptions.LongRunning);
        }

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
                    int droppedCount = 0;

                    // Add new events in a rolling buffer fashion
                    while (!this.eventsQueue.TryAdd(loggingEvent))
                    {
                        this.eventsQueue.TryTake(out _);
                        droppedCount++;
                    }

                    if (droppedCount > 0)
                    {
                        LogLog.Warn(this.GetType(), $"{droppedCount} log events have been dropped, RollingBufferSize={this.RollingBufferSize} has been reached");
                    }
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

                    // Either current producer consumer buffer is empty or we have reached maximum size which triggers flush instantly
                    // This approach creates log event batches which are variable in size, but if your concrete appender has much better
                    // performance when performing bulk operations, this approach gives very good results, i.e. minimizing number of processing requests
                    // while having almost real-time log events flow at the same time
                    if (this.eventsQueue.Count == 0 || buffer.Count >= this.FlushTriggerBufferSize)
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
