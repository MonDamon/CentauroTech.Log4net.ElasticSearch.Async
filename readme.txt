log4net.ElasticSearch.Async is a log4net appender, based on log4net.ElasticSearch package, for easy logging of exceptions and messages to Elasticsearch indices. The main improvement over log4net.ElasticSearch is background/async logging based on producer-consumer pattern, automatically utilizing bulk API in case of log event bursts. Currently the package provides:
- Background/Async logging based on producer-consumer pattern (non-blocking for main application thread)
- Configurable exponential backoff retry policy for communication with ElasticSearch
- Configurable buffer sizes with rolling buffer option (both general producer-consumer buffer and intermediate flush buffer)
- External machine IP added to log events (if possible)
- Skipping TLS certificate validation for ElasticSearch endpoint
- Setting custom HTTP(s) proxy
- Disabling system HTTP(S) proxy
- Using custom ElasticSearch processing pipeline


---

The package is based on the following repository:

Documentation available at http://jptoto.github.io/log4net.ElasticSearch/

You can always find the latest version of Elasticsearch for all platforms at https://www.elastic.co/downloads/elasticsearch (requires a recent version of the JRE)
