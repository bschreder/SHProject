# Synapse project
See the [Synapse project](./Synapse Health Technical Assessment.doc)
The original project file is in src/Original 

##  Choices made
1.  I assume the service uses a public API that can't be modified.
2.  I broke the code into 3 parts:  REST API Service, Orders Service, and Alert Service
	*  Each service is responsible for a single concern:  REST for the API functions, Alert for notifying the user, and Orders for the order management business logic.

##  Opportunities for improvement
1.  Logging service:  
	*  This is a cross cutting concern and can be configured and abstracted as a common service via a private nuget package.
	*  Could be used to have a company standardized structured logging (ie., Serilog), SIEN/XDR (ie, Splunk), or open telemetry (ie., Prometheus, Datadog) implementation
2.  REST Service:
	*  This could be implemented as a private nuget package to abstract the REST API details
	*  Should include Autorization, RequestId, SpanId, ... headers
	*  Many REST APIs return a ProblemDetails object.  The POST controller endpoint could be enahnced to support a ProblemDetails object.
	*  The RestService could be enhanced to handle (or retry), circuit breaker, ...  via Polly