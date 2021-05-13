// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using FluentAssertions;
using InvisibleApi.Attributes;
using InvisibleApi.Middlewares;
using InvisibleApi.Models.InvisibleApiConfigurations;
using InvisibleApi.Models.InvisibleApiProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tynamix.ObjectFiller;
using Xunit;

namespace InvisibleApi.Tests.Unit.Middlewares
{
    public class InvisibleApiMiddlewareTests
    {
        // NO CONFIGURATIONS OR ATTRIBUTES USED

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointIsntMarkedAsInvisible()
        {
            // given
            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();

            // when
            InvisibleApiMiddleware invisibleMiddleware = new(null, null, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        // ONLY CONFIGURATIONS - NO ATTRIBUTES USED

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointIsConfiguredProperly()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            InvisibleApiConfiguration configuration = new()
            {
                HttpVerb = randomHttpVerb,
                Endpoint = randomEndpoint,
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            List<InvisibleApiConfiguration> configurations = new() { configuration };

            InvisibleApiMiddleware invisibleMiddleware = new(configurations, null, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .NotBe(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldReturnNotFoundIfHeaderValueDontMatchInvisibleConfiguration()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            httpResponseMock.SetupAllProperties();

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            InvisibleApiConfiguration configuration = new()
            {
                HttpVerb = randomHttpVerb,
                Endpoint = randomEndpoint,
                Header = GetRandomString(),
                Value = GetRandomString()
            };

            List<InvisibleApiConfiguration> configurations = new() { configuration };

            InvisibleApiMiddleware invisibleMiddleware = new(configurations, null, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .Be(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Never);
        }

        // ONLY ATTRIBUTES - NO CONFIGURATIONS USED

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointHasInvisibleAttributeSetProperly()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();
            var randomProfileName = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<IFeatureCollection> featureCollectionMock = new();
            Mock<IEndpointFeature> endpointFeatureMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(GetEndpointWithAttribute(requestDelegateMock.Object, randomProfileName));

            featureCollectionMock.Setup(featureCollection => featureCollection.Get<IEndpointFeature>())
                .Returns(endpointFeatureMock.Object);

            contextMock.SetupGet(context => context.Features)
                .Returns(featureCollectionMock.Object);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            InvisibleApiProfile profile = new()
            {
                Name = randomProfileName,
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            List<InvisibleApiProfile> profiles = new() { profile };

            InvisibleApiMiddleware invisibleMiddleware = new(null, profiles, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .NotBe(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointHasOneOfManyInvisibleAttributeSetProperly()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();
            var randomProfileName = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<IFeatureCollection> featureCollectionMock = new();
            Mock<IEndpointFeature> endpointFeatureMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(GetEndpointWithAttribute(requestDelegateMock.Object, randomProfileName));

            featureCollectionMock.Setup(featureCollection => featureCollection.Get<IEndpointFeature>())
                .Returns(endpointFeatureMock.Object);

            contextMock.SetupGet(context => context.Features)
                .Returns(featureCollectionMock.Object);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            InvisibleApiProfile matchingProfile = new()
            {
                Name = randomProfileName,
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            InvisibleApiProfile nonMatchingProfile = new()
            {
                Name = GetRandomString(),
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            List<InvisibleApiProfile> profiles = new() { nonMatchingProfile, matchingProfile };

            InvisibleApiMiddleware invisibleMiddleware = new(null, profiles, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .NotBe(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldReturnNotFoundIfHeaderValueDoesntMatchInvisibleAttributeProfile()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();
            var randomProfileName = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<IFeatureCollection> featureCollectionMock = new();
            Mock<IEndpointFeature> endpointFeatureMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(GetEndpointWithAttribute(requestDelegateMock.Object, randomProfileName));

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
            InvisibleApiProfile profile = new()
            {
                Name = randomProfileName,
                Header = GetRandomString(),
                Value = GetRandomString()
            };

            List<InvisibleApiProfile> profiles = new() { profile };

            InvisibleApiMiddleware invisibleMiddleware = new(null, profiles, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .Be(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Never);
        }

        // BOTH CONFIGURATIONS AND ATTRIBUTES USED

        [Fact]
        public async Task ShouldHitApiEndpointIfEndpointIsSetProperlyFromConfigurationAndAttributeProfile()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();
            var randomProfileName = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<IFeatureCollection> featureCollectionMock = new();
            Mock<IEndpointFeature> endpointFeatureMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(GetEndpointWithAttribute(requestDelegateMock.Object, randomProfileName));

            featureCollectionMock.Setup(featureCollection => featureCollection.Get<IEndpointFeature>())
                .Returns(endpointFeatureMock.Object);

            contextMock.SetupGet(context => context.Features)
                .Returns(featureCollectionMock.Object);

            contextMock.SetupGet(context => context.Request)
                .Returns(httpRequestMock.Object);

            contextMock.SetupGet(context => context.Response)
                .Returns(httpResponseMock.Object);

            // when
            InvisibleApiConfiguration configuration = new()
            {
                HttpVerb = randomHttpVerb,
                Endpoint = randomEndpoint,
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            List<InvisibleApiConfiguration> configurations = new() { configuration };

            InvisibleApiProfile profile = new()
            {
                Name = randomProfileName,
                Header = randomHeaderName,
                Value = randomHeaderValue
            };

            List<InvisibleApiProfile> profiles = new() { profile };

            InvisibleApiMiddleware invisibleMiddleware = new(configurations, profiles, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .NotBe(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Once);
        }

        [Fact]
        public async Task ShouldReturnNotFoundIfHeaderValueDoesntMatchInvisibleConfigurationOrAttributeProfile()
        {
            // given
            var randomEndpoint = $"/{GetRandomString()}";
            var randomHeaderName = GetRandomString();
            var randomHeaderValue = GetRandomString();
            var randomHttpVerb = GetRandomString();
            var randomProfileName = GetRandomString();

            Mock<RequestDelegate> requestDelegateMock = new();
            Mock<HttpContext> contextMock = new();
            Mock<IFeatureCollection> featureCollectionMock = new();
            Mock<IEndpointFeature> endpointFeatureMock = new();
            Mock<HttpRequest> httpRequestMock = new();
            Mock<HttpResponse> httpResponseMock = new();

            httpRequestMock.SetupGet(request => request.Path)
                .Returns(randomEndpoint);

            httpRequestMock.SetupGet(request => request.Headers)
                .Returns(GetHeaderDictionary(randomHeaderName, randomHeaderValue));

            httpRequestMock.SetupGet(request => request.Method)
                .Returns(randomHttpVerb);

            endpointFeatureMock.SetupGet(endpointFeature => endpointFeature.Endpoint)
                .Returns(GetEndpointWithAttribute(requestDelegateMock.Object, randomProfileName));

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
            InvisibleApiConfiguration configuration = new()
            {
                HttpVerb = randomHttpVerb,
                Endpoint = randomEndpoint,
                Header = GetRandomString(),
                Value = GetRandomString()
            };

            List<InvisibleApiConfiguration> configurations = new() { configuration };

            InvisibleApiProfile profile = new()
            {
                Name = randomProfileName,
                Header = GetRandomString(),
                Value = GetRandomString()
            };

            List<InvisibleApiProfile> profiles = new() { profile };

            InvisibleApiMiddleware invisibleMiddleware = new(configurations, profiles, requestDelegateMock.Object);

            await invisibleMiddleware.InvokeAsync(contextMock.Object);

            // then
            httpResponseMock.Object.StatusCode.Should()
                .Be(StatusCodes.Status404NotFound, "");

            requestDelegateMock.Verify(requestDelegate =>
                requestDelegate(contextMock.Object),
                    Times.Never);
        }

        // HELPERS

        private static HeaderDictionary GetHeaderDictionary(string header, string value)
        {
            Dictionary<string, StringValues> dictionaryForHeaders = new()
            {
                { header, value }
            };

            return new(dictionaryForHeaders);
        }

        private static Endpoint GetEndpointWithAttribute(RequestDelegate requestDelegate, string profileName)
        {
            string[] profileNames = { profileName };
            InvisibleApiAttribute attribute = new(profileNames);

            EndpointMetadataCollection endpointMetadata = new(attribute);

            return new(requestDelegate, endpointMetadata, nameof(InvisibleApiAttribute));
        }

        private static string GetRandomString()
        {
            MnemonicString mnemonicString = new();
            return mnemonicString.GetValue();
        }
    }
}
