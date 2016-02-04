using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace Medidata.CrossApplicationTracer.Tests
{
    [TestClass]
    public class TraceTests
    {
        [TestMethod]
        public void ConstructorWithNullParameters()
        {
            // Arrange & Act
            var trace = new Trace(null, null, null, false);

            // Assert
            Convert.ToInt64(trace.TraceId, 16);
            Assert.AreEqual(trace.TraceId, trace.SpanId);
            Assert.AreEqual(string.Empty, trace.ParentSpanId);
            Assert.AreEqual(false, trace.IsSampled);
        }

        [TestMethod]
        public void ConstructorWithPositiveParameters()
        {
            // Arrance
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = true;

            // Act
            var trace = new Trace(traceId, spanId, parentSpanId, isSampled);

            // Assert
            Assert.AreEqual(traceId, trace.TraceId);
            Assert.AreEqual(spanId, trace.SpanId);
            Assert.AreEqual(parentSpanId, trace.ParentSpanId);
            Assert.AreEqual(isSampled, trace.IsSampled);
        }

        [TestMethod]
        public void GetNext()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var isSampled = fixture.Create<bool>();

            var trace = new Trace(traceId, spanId, parentSpanId, isSampled);

            // Act
            var nextTrace = trace.GetNext();

            // Assert
            Assert.AreEqual(trace.TraceId, nextTrace.TraceId);
            Convert.ToInt64(nextTrace.SpanId, 16);
            Assert.AreEqual(trace.SpanId, nextTrace.ParentSpanId);
            Assert.AreEqual(trace.IsSampled, nextTrace.IsSampled);
        }

        [TestMethod]
        public void AddTraceHeaders()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var isSampled = fixture.Create<bool>();

            var trace = new Trace(traceId, spanId, parentSpanId, isSampled);
            var headers = new WebHeaderCollection();

            // Act
            trace.AddTraceHeaders(headers);

            // Assert
            Assert.AreEqual(trace.TraceId, headers[Trace.HeaderTraceIdKey]);
            Assert.AreEqual(trace.SpanId, headers[Trace.HeaderSpanIdKey]);
            Assert.AreEqual(trace.ParentSpanId, headers[Trace.HeaderParentSpanIdKey]);
            Assert.AreEqual(trace.IsSampled.ToString().ToLower(), headers[Trace.HeaderSampledKey]);
        }
    }
}
