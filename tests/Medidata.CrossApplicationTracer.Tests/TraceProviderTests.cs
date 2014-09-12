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
        public void ConstructorWithNullHttpContext()
        {
            // Arrange & Act
            var traceProvider = new TraceProvider();

            // Assert
            Convert.ToInt64(traceProvider.TraceId, 16);
            Assert.AreEqual(traceProvider.TraceId, traceProvider.SpanId);
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(false, traceProvider.IsSampled);
        }

        [TestMethod]
        public void ConstructorWithHttpContextHavingAllIdValues()
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
            var traceProvider = new TraceProvider(httpContext:httpContextFake);

            // Assert
            Assert.AreEqual(traceId, traceProvider.TraceId);
            Assert.AreEqual(spanId, traceProvider.SpanId);
            Assert.AreEqual(parentSpanId, traceProvider.ParentSpanId);
            Assert.AreEqual(isSampled, traceProvider.IsSampled);
        }

        [TestMethod]
        public void ConstructorWithHttpContextHavingIdValuesExceptIsSampled()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);

            var mockPath = "mockPath/sajfklsajflk";

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                },
                PathGet = () => mockPath
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<ZipkinSampler>(fixture.Create<string>(), fixture.Create<string>());
            sampleFilter.Expect(x => x.ShouldBeSampled(mockPath)).Return(expectedIsSampled);

            // Act
            var traceProvider = new TraceProvider(sampleFilter, httpContextFake);

            // Assert
            Assert.AreEqual(traceId, traceProvider.TraceId);
            Assert.AreEqual(spanId, traceProvider.SpanId);
            Assert.AreEqual(parentSpanId, traceProvider.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, traceProvider.IsSampled);
        }

        [TestMethod]
        public void ConstructorWithHttpContextHavingInvalidIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<string>();
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var sampled = fixture.Create<string>();

            var mockPath = "mockPath/sajfklsajflk";

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", sampled },
                },
                PathGet = () => mockPath
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<ZipkinSampler>(fixture.Create<string>(), fixture.Create<string>());
            sampleFilter.Expect(x => x.ShouldBeSampled(mockPath)).Return(expectedIsSampled);

            // Act
            var traceProvider = new TraceProvider(sampleFilter, httpContextFake);

            // Assert
            Assert.AreNotEqual(traceId, traceProvider.TraceId);
            Convert.ToInt64(traceProvider.TraceId, 16);
            Assert.AreEqual(traceProvider.TraceId, traceProvider.SpanId);
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, traceProvider.IsSampled);
        }

        [TestMethod]
        public void ConstructorWithHavingTraceProviderInHttpContext()
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

            var traceProvider1 = new TraceProvider(httpContext: httpContextFake1);

            var httpContextFake2 = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new StubIDictionary
                {
                    ItemGetObject = (k) => traceProvider1,
                    ItemSetObjectObject = (k, v) => { },
                    ContainsObject = (k) => true
                }
            };

            // Act
            var traceProvider2 = new TraceProvider(httpContext:httpContextFake2);

            // Assert
            Assert.AreEqual(traceProvider2.TraceId, traceProvider1.TraceId);
            Assert.AreEqual(traceProvider2.SpanId, traceProvider1.SpanId);
            Assert.AreEqual(traceProvider2.ParentSpanId, traceProvider1.ParentSpanId);
            Assert.AreEqual(traceProvider2.IsSampled, traceProvider1.IsSampled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorWithHttpContextHavingSameSpanAndParentSpan()
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
            new TraceProvider(httpContext: httpContextFake);
        }

        [TestMethod]
        public void GetNext()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
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

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var traceProvider = new TraceProvider(httpContext: httpContextFake);

            // Act
            var nextTraceProvider = traceProvider.GetNext();

            // Assert
            Assert.AreEqual(traceProvider.TraceId, nextTraceProvider.TraceId);
            Convert.ToInt64(nextTraceProvider.SpanId, 16);
            Assert.AreEqual(traceProvider.SpanId, nextTraceProvider.ParentSpanId);
            Assert.AreEqual(traceProvider.IsSampled, nextTraceProvider.IsSampled);
        }
    }
}
