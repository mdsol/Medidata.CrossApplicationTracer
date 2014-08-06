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

            ulong result;
            TraceId = !string.IsNullOrWhiteSpace(headerTraceId) && UInt64.TryParse(headerTraceId, out result) ? headerTraceId : GenerateUInt64FromNewGuid().ToString();
            SpanId = !string.IsNullOrWhiteSpace(headerSpanId) && UInt64.TryParse(headerSpanId, out result) ? headerSpanId : TraceId;
            ParentSpanId = !string.IsNullOrWhiteSpace(headerParentSpanId) && UInt64.TryParse(headerParentSpanId, out result) ? headerParentSpanId : string.Empty;
           
            if (SpanId == ParentSpanId)
            {
                throw new ArgumentException("x-b3-SpanId and x-b3-ParentSpanId must not be the same value.");
            }
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
                SpanId = GenerateUInt64FromNewGuid().ToString(),
                ParentSpanId = this.SpanId,
            };
        }

        /// <summary>
        /// Generate a UInt64 from new Guid.
        /// </summary>
        /// <returns>The uint64</returns>
        private ulong GenerateUInt64FromNewGuid()
        {
            return BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 0);
        }
    }
}
