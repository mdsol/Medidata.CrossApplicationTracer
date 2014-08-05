using System;
using System.Collections.Specialized;
using System.Web.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace Medidata.CrossApplicationTracer.Tests
{
    [TestClass]
    public class TraceProviderTests
    {
        [TestMethod]
        public void ConstructorWithNullValues()
        {
            // Arrange & Act
            var traceProvider = new TraceProvider();

            // Assert
            long.Parse(traceProvider.TraceId);
            Assert.AreEqual(traceProvider.TraceId, traceProvider.SpanId);
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
        }

        [TestMethod]
        public void ConstructorWithAllValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<string>();
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                }
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake
            };

            // Act
            var traceProvider = new TraceProvider(httpContextFake);

            // Assert
            Assert.AreEqual(traceId, traceProvider.TraceId);
            Assert.AreEqual(spanId, traceProvider.SpanId);
            Assert.AreEqual(parentSpanId, traceProvider.ParentSpanId);
        }

        [TestMethod]
        public void GetNext()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<string>();
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                }
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake
            };

            var traceProvider = new TraceProvider(httpContextFake);

            // Act
            var nextTraceProvider = traceProvider.GetNext();

            // Assert
            Assert.AreEqual(traceProvider.TraceId, nextTraceProvider.TraceId);
            long.Parse(nextTraceProvider.SpanId);
            Assert.AreEqual(traceProvider.SpanId, nextTraceProvider.ParentSpanId);
        }
    }
}
