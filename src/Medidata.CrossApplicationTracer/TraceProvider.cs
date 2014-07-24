using System;
using System.Web;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// TraceProvider class
    /// </summary>
    public class TraceProvider : ITraceProvider
    {
        /// <summary>
        /// Gets a TraceId
        /// </summary>
        public string TraceId { get; private set; }

        /// <summary>
        /// Gets a SpanId
        /// </summary>
        public string SpanId { get; private set; }

        /// <summary>
        /// Gets a ParentSpanId
        /// </summary>
        public string ParentSpanId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TraceProvider class.
        /// </summary>
        /// <param name="httpContext">the httpContext</param>
        public TraceProvider(HttpContextBase httpContext = null)
        {
            if (httpContext != null)
            {
                // zipkin use the following X-Headers to propagate the trace information
                TraceId = httpContext.Request.Headers["X-B3-TraceId"];
                SpanId = httpContext.Request.Headers["X-B3-SpanId"];
                ParentSpanId = httpContext.Request.Headers["X-B3-ParentSpanId"];
            }

            TraceId = TraceId ?? Guid.NewGuid().ToString();
            SpanId = SpanId ?? Guid.NewGuid().ToString();
            if (string.IsNullOrWhiteSpace(ParentSpanId))
            {
                ParentSpanId = string.Empty;    
            }
        }
    }
}
