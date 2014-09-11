﻿namespace Medidata.CrossApplicationTracer
{
    /// <summary>
    /// TraceProvider interface
    /// </summary>
    public interface ITraceProvider
    {
        /// <summary>
        /// Gets a TraceId
        /// </summary>
        string TraceId { get; }

        /// <summary>
        /// Gets a SpanId
        /// </summary>
        string SpanId { get; }

        /// <summary>
        /// Gets a ParentSpanId
        /// </summary>
        string ParentSpanId { get; }

        /// <summary>
        /// Gets and Sets IsSampled
        /// </summary>
        bool? IsSampled { get; set; }

        /// <summary>
        /// Gets a Trace for outgoing HTTP request.
        /// </summary>
        /// <returns>The trace</returns>
        ITraceProvider GetNext();
    }
}
