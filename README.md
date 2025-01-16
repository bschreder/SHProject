# 

##  Choices made
1.  I assume the service uses a public API and can't be modified.
2.  Happy Path with Exception Handling
	*  A ProblemsDetails object (or something similar) wasn't returned from the POST message, I made the assumption the 'Happy Path' pattern is to be used 

##  Opportunities for improvement
1.  Logging service:  
	*  This is a cross cutting concern and can be configured and abstracted as a common service via a private nuget package.
	*  Could be used to have a company standardized structured logging (ie., Serilog), SIEN/XDR (ie, Splunk), or open telemetry (ie., Prometheus, Datadog) implementation
2.  REST Service:
	*  This could be implemented as a private nuget package to abstract the REST API details
	*  Should include Autorization, RequestId, SpanId, ... headers
	*  Many REST APIs return a ProblemDetails object.  The POST controller endpoint could be enhanced to return a ProblemDetails object.
	*  The RestService could be enhanced to handle (or retry), circuit breaker, ...  via Polly