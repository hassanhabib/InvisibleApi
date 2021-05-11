// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using InvisibleApi.Middlewares;
using InvisibleApi.Models.InvisibleApiConfigurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Tynamix.ObjectFiller;
using Xunit;

namespace InvisibleApi.Tests.Unit.Middlewares
{
    public class InvisibleApiMiddlewareTests
    {
        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointIsntConfiguredAsInvisible()
        {
            // given
            var mockedDelegate = new Mock<RequestDelegate>();
            var context = new Mock<HttpContext>();

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>(),
                mockedDelegate.Object);

            await invisibleMiddleware.InvokeAsync(context.Object);

            // then
            mockedDelegate.Verify(requestDelegate =>
                requestDelegate(context.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointIsConfiguredProperly()
        {
            // given
            string randomEndpoint = $"/{GetRandomString()}";
            string randomHeaderName = GetRandomString();
            string randomHeaderValue = GetRandomString();
            string randomHttpVerb = GetRandomString();
            var requestDelegateMock = new Mock<RequestDelegate>();
            var contextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { randomHeaderName, randomHeaderValue}
                }));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>
                {
                    new InvisibleApiConfiguration
                    {
                        HttpVerb = randomHttpVerb,
                        Endpoint = randomEndpoint,
                        Header = randomHeaderName,
                        Value = randomHeaderValue
                    }
                },
                requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldReturnNotFoundIfHeaderValueDontMatchInvisibleConfiguration()
        {
            // given
            string randomEndpoint = $"/{GetRandomString()}";
            string randomHeaderName = GetRandomString();
            string randomHeaderValue = GetRandomString();
            string randomHttpVerb = GetRandomString();
            var requestDelegateMock = new Mock<RequestDelegate>();
            var contextMock = new Mock<HttpContext>();
            var httpRequestMock = new Mock<HttpRequest>();
            var httpResponseMock = new Mock<HttpResponse>();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { randomHeaderName, randomHeaderValue}
                }));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            httpResponseMock.SetupAllProperties();

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>
                {
                    new InvisibleApiConfiguration
                    {
                        HttpVerb = GetRandomString(),
                        Endpoint = randomEndpoint,
                        Header = GetRandomString(),
                        Value = GetRandomString()
                    }
                },
                requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // the
            httpResponseMock.Object.StatusCode.Should()
                .Be(StatusCodes.Status404NotFound);

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Never);
        }

        private static string GetRandomString() =>
            new MnemonicString().GetValue();
    }
}
