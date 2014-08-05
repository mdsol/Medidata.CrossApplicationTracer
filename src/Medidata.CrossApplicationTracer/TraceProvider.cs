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
            string headerTraceId = null;
            string headerSpanId = null;
            string headerParentSpanId = null;

            if (httpContext != null)
            {
                // zipkin use the following X-Headers to propagate the trace information
                headerTraceId = httpContext.Request.Headers["X-B3-TraceId"];
                headerSpanId = httpContext.Request.Headers["X-B3-SpanId"];
                headerParentSpanId = httpContext.Request.Headers["X-B3-ParentSpanId"];
            }

            TraceId = !string.IsNullOrWhiteSpace(headerTraceId) ? headerTraceId : GenerateInt64FromNewGuid().ToString();
            SpanId = !string.IsNullOrWhiteSpace(headerSpanId) ? headerSpanId : TraceId;
            ParentSpanId = !string.IsNullOrWhiteSpace(headerParentSpanId) ? headerParentSpanId : string.Empty;
        }

        /// <summary>
        /// Gets a Trace for outgoing HTTP request.
        /// </summary>
        /// <returns>The trace</returns>
        public ITraceProvider GetNext()
        {
            return new TraceProvider
            {
                TraceId = this.TraceId,
                SpanId = GenerateInt64FromNewGuid().ToString(),
                ParentSpanId = this.SpanId,
            };
        }

        /// <summary>
        /// Generate a Int64 from new Guid.
        /// </summary>
        /// <returns>The int64</returns>
        private long GenerateInt64FromNewGuid()
        {
            return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
        }
    }
}
