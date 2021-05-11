// ---------------------------------------------------------------
// Copyright (c) Hassan Habib All rights reserved.
// Licensed under the MIT License.
// See License.txt in the project root for license information.
// ---------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvisibleApi.Models.InvisibleApiConfigurations;
using Microsoft.AspNetCore.Http;

namespace InvisibleApi.Middlewares
{
    public class InvisibleApiMiddleware
    {
        private readonly RequestDelegate next;
        private readonly List<InvisibleApiConfiguration> invisibleApiConfigurations;

        public InvisibleApiMiddleware(
            List<InvisibleApiConfiguration> invisibleApiConfigurations,
            RequestDelegate next)
        {
            this.invisibleApiConfigurations = invisibleApiConfigurations;
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsApiConfigurationMatchOrNotConfigured(context.Request) is false)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            else
            {
                await next(context);
            }
        }

        private bool IsApiConfigurationMatchOrNotConfigured(HttpRequest request)
        {
            InvisibleApiConfiguration invisibleEndpointConfiguration =
                this.invisibleApiConfigurations.FirstOrDefault(apiDetails =>
                    apiDetails.Endpoint == request.Path
                    && apiDetails.HttpVerb == request.Method);

            if (invisibleEndpointConfiguration is { Endpoint: string endpoint })
            {
                return this.invisibleApiConfigurations.Any(configuration =>
                    configuration.HttpVerb == request.Method
                    && configuration.Endpoint == endpoint
                    && request.Headers[configuration.Header] == configuration.Value);
            }

            return true;
        }
    }
}
