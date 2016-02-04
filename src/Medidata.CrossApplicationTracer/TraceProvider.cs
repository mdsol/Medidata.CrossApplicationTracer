using System;
using System.Web;

namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// TraceProvider class
    /// </summary>
    public static class TraceProvider
    {
        /// <summary>
        /// Key name for HttpContext.Items
        /// </summary>
        private const string KEY = "Medidata.CrossApplicationTracer.TraceProvider";

        /// <summary>
        /// Generate trace
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="dontSampleListCsv"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public static ITrace GenerateTrace(HttpContextBase httpContext, string dontSampleListCsv, string sampleRate)
        {
            return GenerateTrace(new ZipkinSampler(dontSampleListCsv, sampleRate), httpContext);
        }

        /// <summary>
        /// Generate trace
        /// Able to mock zipkins sampler in unit tests
        /// </summary>
        /// <param name="zipkinSampler"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static ITrace GenerateTrace(ZipkinSampler zipkinSampler, HttpContextBase httpContext)
        {
            string headerTraceId = null;
            string headerSpanId = null;
            string headerParentSpanId = null;
            string headerSampled = null;
            ITrace trace;

            if (IsValidRequest(httpContext))
            {
                if (httpContext.Items.Contains(KEY))
                {
                    // set properties from context's item.
                    trace = httpContext.Items[KEY] as Trace;
                    return trace;
                }

                // zipkin use the following X-Headers to propagate the trace information
                headerTraceId = httpContext.Request.Headers[Trace.HeaderTraceIdKey];
                headerSpanId = httpContext.Request.Headers[Trace.HeaderSpanIdKey];
                headerParentSpanId = httpContext.Request.Headers[Trace.HeaderParentSpanIdKey];
                headerSampled = httpContext.Request.Headers[Trace.HeaderSampledKey];
            }


            var isSampled = zipkinSampler.ShouldBeSampled(httpContext, headerSampled);
            trace = new Trace(headerTraceId, headerSpanId, headerParentSpanId, isSampled);

            if (trace.SpanId == trace.ParentSpanId)
            {
                throw new ArgumentException("SpanId and ParentSpanId must not be the same value.");
            }

            if (IsValidRequest(httpContext))
            {
                httpContext.Items[KEY] = trace;
            }

            return trace;
        }

        /// <summary>
        /// Is valid request
        /// </summary>
        /// <returns>true: valid request.</returns>
        private static bool IsValidRequest(HttpContextBase httpContext)
        {
            return httpContext != null && httpContext.Items != null && httpContext.Request != null;
        }
    }
}
