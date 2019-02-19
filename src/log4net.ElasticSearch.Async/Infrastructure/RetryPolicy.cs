namespace CentauroTech.Log4net.ElasticSearch.Async.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using Polly;

    /// <summary>Retry policy helpers.</summary>
    internal class RetryPolicy
    {
        /// <summary>Thread-safe random generator</summary>
        private static readonly ThreadLocal<Random> Rand = new ThreadLocal<Random>(() => new Random(GetSeed()));

        /// <summary>Creates decorrelated jitter policy (as described in https://github.com/App-vNext/Polly/wiki/Retry-with-jitter ).</summary>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="seedDelay">Initial and minimum retry delay.</param>
        /// <param name="maxDelay">Maximum possible delay</param>
        /// <param name="logAction">Error log action</param>
        /// <returns>The <see cref="Policy"/>.</returns>
        public static Policy CreateDecorrelatedJitterPolicy(int maxRetries, TimeSpan seedDelay, TimeSpan maxDelay, Action<Exception, TimeSpan> logAction)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetry(DecorrelatedJitter(maxRetries, seedDelay, maxDelay), logAction);
        }

        /// <summary>Gets thread-safe random seed.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        private static int GetSeed()
        {
            return Environment.TickCount * Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>Generates decorrelated jitter delays</summary>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="seedDelay">Initial and minimum retry delay.</param>
        /// <param name="maxDelay">Maximum possible delay</param>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        private static IEnumerable<TimeSpan> DecorrelatedJitter(int maxRetries, TimeSpan seedDelay, TimeSpan maxDelay)
        {
            int retries = 0;

            double seed = seedDelay.TotalMilliseconds;
            double max = maxDelay.TotalMilliseconds;
            double current = seed;

            while (++retries <= maxRetries)
            {
                current = Math.Min(max, Math.Max(seed, current * 3 * Rand.Value.NextDouble())); // adopting the 'Decorrelated Jitter' formula from https://www.awsarchitectureblog.com/2015/03/backoff.html.  Can be between seed and previous * 3.  Mustn't exceed max.
                yield return TimeSpan.FromMilliseconds(current);
            }
        }
    }
}
