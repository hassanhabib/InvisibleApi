// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using InvisibleApi.Attributes;
using InvisibleApi.Middlewares;
using InvisibleApi.Models.InvisibleApiConfigurations;
using InvisibleApi.Models.InvisibleApiProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
                new List<InvisibleApiProfile>(),
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
                new List<InvisibleApiProfile>(),
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
                        HttpVerb = randomHttpVerb,
                        Endpoint = randomEndpoint,
                        Header = GetRandomString(),
                        Value = GetRandomString()
                    }
                },
                new List<InvisibleApiProfile>(),
                requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .Be(StatusCodes.Status404NotFound);

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Never);
        }

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointDoesntHaveInvisibleAttribute()
        {
            // given
            var mockedDelegate = new Mock<RequestDelegate>();
            var context = new Mock<HttpContext>();

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>(),
                new List<InvisibleApiProfile>(),
                mockedDelegate.Object);

            await invisibleMiddleware.InvokeAsync(context.Object);

            // then
            mockedDelegate.Verify(requestDelegate =>
                requestDelegate(context.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointHasInvisibleAttributeSetProperly()
        {
            // given
            string randomEndpoint = $"/{GetRandomString()}";
            string randomHeaderName = GetRandomString();
            string randomHeaderValue = GetRandomString();
            string randomHttpVerb = GetRandomString();
            string randomProfileName = GetRandomString();
            var requestDelegateMock = new Mock<RequestDelegate>();
            var contextMock = new Mock<HttpContext>();
            var featureCollectionMock = new Mock<IFeatureCollection>();
            var endpointFeatureMock = new Mock<IEndpointFeature>();
            var endpointMetadataCollectionMock = new Mock<EndpointMetadataCollection>();
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

            endpointMetadataCollectionMock.Setup(metadata => metadata.GetMetadata<InvisibleApiAttribute>())
                .Returns(new InvisibleApiAttribute
                {
                    Profiles = new[] { randomProfileName }
                });

            var endpoint = new Endpoint(requestDelegateMock.Object, endpointMetadataCollectionMock.Object, nameof(InvisibleApiAttribute));

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(endpoint);

            featureCollectionMock.Setup(featureCollection => featureCollection.Get<IEndpointFeature>())
                .Returns(endpointFeatureMock.Object);

            contextMock.SetupGet(context => context.Features)
                .Returns(featureCollectionMock.Object);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>(),
                new List<InvisibleApiProfile>
                {
                    new InvisibleApiProfile
                    {
                        Name = randomProfileName,
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
        public async Task ShouldReturnNotFoundIfHeaderValueDoesntMatchInvisibleAttributeProfile()
        {
            // given
            string randomEndpoint = $"/{GetRandomString()}";
            string randomHeaderName = GetRandomString();
            string randomHeaderValue = GetRandomString();
            string randomHttpVerb = GetRandomString();
            string randomProfileName = GetRandomString();
            var requestDelegateMock = new Mock<RequestDelegate>();
            var contextMock = new Mock<HttpContext>();
            var featureCollectionMock = new Mock<IFeatureCollection>();
            var endpointFeatureMock = new Mock<IEndpointFeature>();
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

            var endpointMetadata = new EndpointMetadataCollection(
                new InvisibleApiAttribute
                {
                    Profiles = new[] { randomProfileName }
                });

            var endpoint = new Endpoint(requestDelegateMock.Object, endpointMetadata, nameof(InvisibleApiAttribute));

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(endpoint);

            featureCollectionMock.Setup(featureCollection => featureCollection.Get<IEndpointFeature>())
                .Returns(endpointFeatureMock.Object);

            contextMock.SetupGet(context => context.Features)
                .Returns(featureCollectionMock.Object);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            httpResponseMock.SetupAllProperties();

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            var invisibleMiddleware = new InvisibleApiMiddleware(
                new List<InvisibleApiConfiguration>(),
                new List<InvisibleApiProfile>
                {
                    new InvisibleApiProfile
                    {
                        Name = randomProfileName,
                        Header = GetRandomString(),
                        Value = GetRandomString()
                    }
                },
                requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
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
