using System;
using System.Globalization;
using System.Net;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// Trace class
    /// </summary>
    public class Trace : ITrace
    {
        /// <summary>
        /// Header Trace Id Key
        /// </summary>
        public const string HeaderTraceIdKey = "X-B3-TraceId";

        /// <summary>
        /// Header Spam Id Key
        /// </summary>
        public const string HeaderSpanIdKey = "X-B3-SpanId";

        /// <summary>
        /// Header Parent Span Id Key
        /// </summary>
        public const string HeaderParentSpanIdKey = "X-B3-ParentSpanId";

        /// <summary>
        /// Header Sampled Key
        /// </summary>
        public const string HeaderSampledKey = "X-B3-Sampled";

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
        /// Gets IsSampled
        /// </summary>
        public bool IsSampled { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="spanId"></param>
        /// <param name="parentSpanId"></param>
        /// <param name="isSampled"></param>
        internal Trace(string traceId, string spanId, string parentSpanId, bool isSampled)
        {
            TraceId = Parse(traceId) ? traceId : GenerateHexEncodedInt64FromNewGuid();
            SpanId = Parse(spanId) ? spanId : TraceId;
            ParentSpanId = Parse(parentSpanId) ? parentSpanId : string.Empty;
            IsSampled = isSampled;
        }
        
        /// <summary>
        /// Gets a Trace for outgoing HTTP request.
        /// </summary>
        /// <returns>The trace</returns>
        public ITrace GetNext()
        {
            return new Trace(
                TraceId,
                GenerateHexEncodedInt64FromNewGuid(),
                SpanId,
                IsSampled);
        }

        /// <summary>
        /// Adds trace information to headers
        /// </summary>
        /// <param name="headers"></param>
        public void AddTraceHeaders(WebHeaderCollection headers)
        {
            headers.Add(HeaderTraceIdKey, TraceId);
            headers.Add(HeaderSpanIdKey, SpanId);
            headers.Add(HeaderParentSpanIdKey, ParentSpanId);
            headers.Add(HeaderSampledKey, IsSampled.ToString().ToLower());
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
