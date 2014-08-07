using System;
using System.Globalization;
using System.Web;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// TraceProvider class
    /// </summary>
    public class TraceProvider : ITraceProvider
    {
        /// <summary>
        /// Key name for HttpContext.Items
        /// </summary>
        private const string KEY = "Medidata.CrossApplicationTracer.TraceProvider";

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
        private TraceProvider(HttpContextBase httpContext = null)
        {
            string headerTraceId = null;
            string headerSpanId = null;
            string headerParentSpanId = null;

            if (httpContext != null && httpContext.Handler != null)
            {
                // zipkin use the following X-Headers to propagate the trace information
                headerTraceId = httpContext.Request.Headers["X-B3-TraceId"];
                headerSpanId = httpContext.Request.Headers["X-B3-SpanId"];
                headerParentSpanId = httpContext.Request.Headers["X-B3-ParentSpanId"];
            }

            TraceId = Parse(headerTraceId) ? headerTraceId : GenerateHexEncodedInt64FromNewGuid();
            SpanId = Parse(headerSpanId) ? headerSpanId : TraceId;
            ParentSpanId = Parse(headerParentSpanId) ? headerParentSpanId : string.Empty;
           
            if (SpanId == ParentSpanId)
            {
                throw new ArgumentException("x-b3-SpanId and x-b3-ParentSpanId must not be the same value.");
            }
        }

        /// <summary>
        /// GetInstance
        /// </summary>
        /// <param name="httpContext">The httpContext</param>
        /// <returns>Return a provider from HttpContext if httpContext has TraceProvier. Return a new instance if httContext is null or httContext doens't have TraceProvider.</returns>
        public static ITraceProvider GetInstance(HttpContextBase httpContext = null)
        {
            if (httpContext != null && httpContext.Items.Contains(KEY))
            {
                return httpContext.Items[KEY] as ITraceProvider;
            }

            var traceProvider = new TraceProvider(httpContext);

            if (httpContext != null)
            {
                httpContext.Items.Add(KEY, traceProvider);
            }

            return traceProvider;
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
                SpanId = GenerateHexEncodedInt64FromNewGuid(),
                ParentSpanId = this.SpanId,
            };
        }

        /// <summary>
        /// Parse id value
        /// </summary>
        /// <param name="value">header's value</param>
        /// <returns>true: parsed</returns>
        private bool Parse(string value)
        {
            long result;
            return !string.IsNullOrWhiteSpace(value) && Int64.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Generate a hex encoded Int64 from new Guid.
        /// </summary>
        /// <returns>The hex encoded int64</returns>
        private string GenerateHexEncodedInt64FromNewGuid()
        {
            return Convert.ToString(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0), 16);
        }
    }
}
