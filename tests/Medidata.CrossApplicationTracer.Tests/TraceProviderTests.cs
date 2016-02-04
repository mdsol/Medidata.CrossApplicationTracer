using System;
using System.Collections.Fakes;
using System.Collections.Specialized;
using System.Web.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.CrossApplicationTracer.Tests
{
    [TestClass]
    public class TraceProviderTests
    {
        [TestMethod]
        public void GenerateTraceWithNullHttpContext()
        {
            // Arrange & Act
            var trace = TraceProvider.GenerateTrace(null, null, null);

            // Assert
            Convert.ToInt64(trace.TraceId, 16);
            Assert.AreEqual(trace.TraceId, trace.SpanId);
            Assert.AreEqual(string.Empty, trace.ParentSpanId);
            Assert.AreEqual(false, trace.IsSampled);
        }

        [TestMethod]
        public void GenerateTraceWithHttpContextHavingAllIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = fixture.Create<bool>();

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", isSampled.ToString() },
                }
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            // Act
            var trace = TraceProvider.GenerateTrace(httpContextFake, null, null);

            // Assert
            Assert.AreEqual(traceId, trace.TraceId);
            Assert.AreEqual(spanId, trace.SpanId);
            Assert.AreEqual(parentSpanId, trace.ParentSpanId);
            Assert.AreEqual(isSampled, trace.IsSampled);
        }

        [TestMethod]
        public void GenerateTraceWithHttpContextHavingIdValuesExceptIsSampled()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);

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
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<ZipkinSampler>(fixture.Create<string>(), fixture.Create<string>());
            sampleFilter.Expect(x => x.ShouldBeSampled(httpContextFake, null)).Return(expectedIsSampled);

            // Act
            var trace = TraceProvider.GenerateTrace(sampleFilter, httpContextFake);

            // Assert
            Assert.AreEqual(traceId, trace.TraceId);
            Assert.AreEqual(spanId, trace.SpanId);
            Assert.AreEqual(parentSpanId, trace.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, trace.IsSampled);
        }

        [TestMethod]
        public void GenerateTraceWithHttpContextHavingInvalidIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<string>();
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var sampled = fixture.Create<string>();

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", sampled },
                },
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<ZipkinSampler>(fixture.Create<string>(), fixture.Create<string>());
            sampleFilter.Expect(x => x.ShouldBeSampled(httpContextFake, sampled)).Return(expectedIsSampled);

            // Act
            var trace = TraceProvider.GenerateTrace(sampleFilter, httpContextFake);

            // Assert
            Assert.AreNotEqual(traceId, trace.TraceId);
            Convert.ToInt64(trace.TraceId, 16);
            Assert.AreEqual(trace.TraceId, trace.SpanId);
            Assert.AreEqual(string.Empty, trace.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, trace.IsSampled);
        }

        [TestMethod]
        public void GenerateTraceWithHavingTraceProviderInHttpContext()
        {
            // Arrange
            var fixture = new Fixture();

            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var sampled = Convert.ToString(fixture.Create<bool>());

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", sampled },
                }
            };

            var httpContextFake1 = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new StubIDictionary
                {
                    ItemGetObject = (k) => null,
                    ItemSetObjectObject = (k, v) => { },
                    ContainsObject = (k) => false
                }
            };

            var trace1 = TraceProvider.GenerateTrace(httpContextFake1, null, null);

            var httpContextFake2 = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new StubIDictionary
                {
                    ItemGetObject = (k) => trace1,
                    ItemSetObjectObject = (k, v) => { },
                    ContainsObject = (k) => true
                }
            };

            // Act
            var trace2 = TraceProvider.GenerateTrace(httpContextFake2, null, null);

            // Assert
            Assert.AreEqual(trace2.TraceId, trace1.TraceId);
            Assert.AreEqual(trace2.SpanId, trace1.SpanId);
            Assert.AreEqual(trace2.ParentSpanId, trace1.ParentSpanId);
            Assert.AreEqual(trace2.IsSampled, trace1.IsSampled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GenerateTraceWithHttpContextHavingSameSpanAndParentSpan()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = spanId;

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
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            // Act
            TraceProvider.GenerateTrace(httpContextFake, null, null);
        }
    }
}
