using System;
using System.Collections.Specialized;
using System.Web.Fakes;
using Microsoft.QualityTools.Testing.Fakes;
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
            Guid.Parse(traceProvider.TraceId);
            Guid.Parse(traceProvider.SpanId);
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
    }
}
