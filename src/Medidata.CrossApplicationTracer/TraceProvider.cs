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
        /// Gets Sampled
        /// </summary>
        public string Sampled
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the TraceProvider class.
        /// </summary>
        /// <param name="httpContext">the httpContext</param>
        public TraceProvider(HttpContextBase httpContext = null)
        {
            string headerTraceId = null;
            string headerSpanId = null;
            string headerParentSpanId = null;
            string headerIsSampled = null;

            if (IsValidRequest(httpContext))
            {
                if (httpContext.Items.Contains(KEY))
                {
                    // set properties from context's item.
                    var provider = httpContext.Items[KEY] as ITraceProvider;
                    TraceId = provider.TraceId;
                    SpanId = provider.SpanId;
                    ParentSpanId = provider.ParentSpanId;
                    Sampled = provider.Sampled;
                    return;
                }

                // zipkin use the following X-Headers to propagate the trace information
                headerTraceId = httpContext.Request.Headers["X-B3-TraceId"];
                headerSpanId = httpContext.Request.Headers["X-B3-SpanId"];
                headerParentSpanId = httpContext.Request.Headers["X-B3-ParentSpanId"];
                headerIsSampled = httpContext.Request.Headers["X-B3-Sampled"];
            }

            TraceId = Parse(headerTraceId) ? headerTraceId : GenerateHexEncodedInt64FromNewGuid();
            SpanId = Parse(headerSpanId) ? headerSpanId : TraceId;
            ParentSpanId = Parse(headerParentSpanId) ? headerParentSpanId : string.Empty;
            Sampled = ParseIsSampled(headerIsSampled) ? headerIsSampled : string.Empty;
           
            if (SpanId == ParentSpanId)
            {
                throw new ArgumentException("x-b3-SpanId and x-b3-ParentSpanId must not be the same value.");
            }

            if (IsValidRequest(httpContext))
            {
                httpContext.Items[KEY] = this;
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
                SpanId = GenerateHexEncodedInt64FromNewGuid(),
                ParentSpanId = this.SpanId,
                Sampled = this.Sampled
            };
        }

        /// <summary>
        /// Sets Sampled as isSampled
        /// </summary>
        public void SetSampled(bool isSampled)
        {
            Sampled = isSampled.ToString();
        }

        /// <summary>
        /// Is valid request
        /// </summary>
        /// <returns>true: valid request.</returns>
        private static bool IsValidRequest(HttpContextBase httpContext)
        {
            return httpContext != null && httpContext.Items != null && httpContext.Request != null;
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

        private bool ParseIsSampled(string value)
        {
            bool result;
            return !string.IsNullOrWhiteSpace(value) && Boolean.TryParse(value, out result);
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
