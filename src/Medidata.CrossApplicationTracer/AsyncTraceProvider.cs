using System;
using System.Globalization;
using System.Web;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// TraceProvider class
    /// </summary>
    public class AsyncTraceProvider : TraceProvider
    {
        /// <summary>
        /// Initializes a new instance of the Async TraceProvider class.
        /// </summary>
        /// <param name="sampleRate">the sampleRate</param>
        public AsyncTraceProvider(string sampleRate)
        {
            TraceId = GenerateHexEncodedInt64FromNewGuid();
            SpanId = TraceId;
            ParentSpanId = string.Empty;
            IsSampled = new Sampler(sampleRate).ShouldBeSampled();
        }
    }
}
