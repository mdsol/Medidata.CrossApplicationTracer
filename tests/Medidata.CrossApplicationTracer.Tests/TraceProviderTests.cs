﻿using System;
using System.Collections.Fakes;
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
        public void ConstructorWithNullHttpContext()
        {
            // Arrange & Act
            var traceProvider = new TraceProvider();

            // Assert
            Convert.ToInt64(traceProvider.TraceId, 16);
            Assert.AreEqual(traceProvider.TraceId, traceProvider.SpanId);
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(string.Empty, traceProvider.Sampled);
        }

        [TestMethod]
        public void ConstructorWithHttpContextHavingAllIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = Convert.ToString(fixture.Create<bool>());

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", isSampled },
                }
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            // Act
            var traceProvider = new TraceProvider(httpContextFake);

            // Assert
            Assert.AreEqual(traceId, traceProvider.TraceId);
            Assert.AreEqual(spanId, traceProvider.SpanId);
            Assert.AreEqual(parentSpanId, traceProvider.ParentSpanId);
            Assert.AreEqual(isSampled, traceProvider.Sampled);
        }

        [TestMethod]
        public void ConstructorWithHttpContextHavingIdValuesExceptIsSampled()
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

            // Act
            var traceProvider = new TraceProvider(httpContextFake);

            // Assert
            Assert.AreEqual(traceId, traceProvider.TraceId);
            Assert.AreEqual(spanId, traceProvider.SpanId);
            Assert.AreEqual(parentSpanId, traceProvider.ParentSpanId);
            Assert.AreEqual(string.Empty, traceProvider.Sampled);
        }
        [TestMethod]
        public void ConstructorWithHttpContextHavingInvalidIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<string>();
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var isSampled = fixture.Create<string>();

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", isSampled },
                },
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            // Act
            var traceProvider = new TraceProvider(httpContextFake);

            // Assert
            Assert.AreNotEqual(traceId, traceProvider.TraceId);
            Convert.ToInt64(traceProvider.TraceId, 16);
            Assert.AreEqual(traceProvider.TraceId, traceProvider.SpanId);
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(string.Empty, traceProvider.Sampled);
        }

        [TestMethod]
        public void ConstructorWithHavingTraceProviderInHttpContext()
        {
            // Arrange
            var fixture = new Fixture();

            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = Convert.ToString(fixture.Create<bool>());

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", isSampled },
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

            var traceProvider1 = new TraceProvider(httpContextFake1);

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
            var traceProvider2 = new TraceProvider(httpContextFake2);

            // Assert
            Assert.AreEqual(traceProvider2.TraceId, traceProvider1.TraceId);
            Assert.AreEqual(traceProvider2.SpanId, traceProvider1.SpanId);
            Assert.AreEqual(traceProvider2.ParentSpanId, traceProvider1.ParentSpanId);
            Assert.AreEqual(traceProvider2.Sampled, traceProvider1.Sampled);
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
            new TraceProvider(httpContextFake);
        }

        [TestMethod]
        public void GetNext()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var isSampled = Convert.ToString(fixture.Create<bool>());

            var httpRequestFake = new StubHttpRequestBase
            {
                HeadersGet = () => new NameValueCollection
                {
                    { "X-B3-TraceId", traceId },
                    { "X-B3-SpanId", spanId },
                    { "X-B3-ParentSpanId", parentSpanId },
                    { "X-B3-Sampled", isSampled },
                }
            };

            var httpContextFake = new StubHttpContextBase
            {
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var traceProvider = new TraceProvider(httpContextFake);

            // Act
            var nextTraceProvider = traceProvider.GetNext();

            // Assert
            Assert.AreEqual(traceProvider.TraceId, nextTraceProvider.TraceId);
            Convert.ToInt64(nextTraceProvider.SpanId, 16);
            Assert.AreEqual(traceProvider.SpanId, nextTraceProvider.ParentSpanId);
            Assert.AreEqual(traceProvider.Sampled, nextTraceProvider.Sampled);
        }

        [TestMethod]
        public void SetIsSampled()
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

            // Act
            var traceProvider = new TraceProvider(httpContextFake);

            var isSampled = fixture.Create<bool>();

            traceProvider.SetSampled(isSampled);

            Assert.AreEqual(isSampled.ToString(), traceProvider.Sampled);
        }
    }
}
